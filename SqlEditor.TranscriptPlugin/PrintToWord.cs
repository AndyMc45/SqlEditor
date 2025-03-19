using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace SqlEditor.TranscriptPlugin
{
    public static class PrintToWord
    {

        // There are 4 dataTables for transcript and one extra for course role 
        public static System.Data.DataTable studentDegreeInfoDT { get; set; } // No editing - 1 data row only for this studentDegree 
        public static System.Data.DataTable studentDegreeStatusInfoDT { get; set; } // No editing - 1 data row only for this studentDegree 

        public static System.Data.DataTable transcriptDT { get; set; }  // No editing. Transcripts filtered on this studentDegree

        // StudentReqDT is created from scratch -columns added to mainform dataHelper.fieldDT to allows sqlStudentReq factory
        public static System.Data.DataTable studentReqDT { get; set; } // No editing.  

        // A 4th Datatable and Sql for course role - I also use transcriptDT / sql for course role but filter on course
        // No editing - 1 data row only for this studentDegree
        public static System.Data.DataTable courseTermInfoDT { get; set; } // No editing - 1 data row only for this studentDegree 

        private static void InsertTextInTable(Table table, int intRow, int intCell, string text)
        {
            InsertTextInTable(table, intRow, intCell, text, JustificationValues.Center);
        }
        private static void InsertTextInTable(Table table, int intRow, int intCell, string text, JustificationValues jv)
        {
            // Find the third cell in the row.
            TableRow row = table.Elements<TableRow>().ElementAt(intRow);
            TableCell cell = row.Elements<TableCell>().ElementAt(intCell);
            // Find the first paragraph in the table cell.
            if (cell.Elements<Paragraph>().Count() == 0)
            {
                Paragraph newPara = new Paragraph();
                cell.AddChild(newPara);
            }
            Paragraph p = cell.Elements<Paragraph>().First();
            ParagraphProperties pp = GetPP(ref p);
            Justification just = new Justification() { Val = jv };
            pp.Append(just);

            // Find the first run in the paragraph.
            if (p.Elements<Run>().Count() == 0)
            {
                Run run = new Run();
                p.AddChild(run);
            }
            Run r = p.Elements<Run>().First();
            if (r.Elements<Text>().Count() == 0)
            {
                Text eText = new Text();
                r.AddChild(eText);
            }
            // Set the text for the run.
            Text t = r.Elements<Text>().First();
            t.Text = text;
        }
        private static void RemoveInnerCellBorders(Table table, int intRow, int intStartCell, int intEndCell)
        {
            TableRow row = table.Elements<TableRow>().ElementAt(intRow);
            for (int i = intStartCell; i <= intEndCell; i++)
            {
                TableCell cell = row.Elements<TableCell>().ElementAt(i);
                // Find the tableCellProperty
                TableCellProperties tcp = GetTCP(ref cell);
                RightBorder rb = new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Nil) };
                LeftBorder lb = new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Nil) };
                TableCellBorders tableCellBorders = new TableCellBorders();
                if (i == intStartCell) { tableCellBorders.Append(rb); }
                else if (i == intEndCell) { tableCellBorders.Append(lb); }
                else { tableCellBorders.Append(lb); tableCellBorders.Append(rb); }
                tcp.AppendChild(tableCellBorders);
            }
        }
        private static void MergeTableCells(Table table, int intRow, int intStartCell, int intEndCell)
        {
            TableRow row = table.Elements<TableRow>().ElementAt(intRow);
            for (int i = intStartCell; i <= intEndCell; i++)
            {
                TableCell currentCell = row.Elements<TableCell>().ElementAt(i);
                TableCellProperties tcp = GetTCP(ref currentCell);
                if (i == intStartCell)
                {
                    HorizontalMerge startMerge = new HorizontalMerge() { Val = MergedCellValues.Restart };
                    currentCell.AppendChild(startMerge);
                }
                else
                {
                    HorizontalMerge continueMerge = new HorizontalMerge() { Val = MergedCellValues.Continue };
                    currentCell.AppendChild(continueMerge);

                }
            }
            //    // Find Start tableCellProperty and append
            //    TableCellProperties start_tcp = GetTCP(ref startCell);
            //start_tcp.AppendChild(startMerge);

            //// Repeat for end
            //TableCellProperties end_tcp = GetTCP(ref endCell);
            //HorizontalMerge endMerge = new HorizontalMerge() { Val = MergedCellValues.Continue };
            //end_tcp.AppendChild(endMerge);
        }
        private static TableCellProperties GetTCP(ref TableCell cell)
        {
            TableCellProperties tcp;
            if (cell.Elements<TableCellProperties>().Count() > 0)
            {
                tcp = cell.Elements<TableCellProperties>().ElementAt(0);
            }
            else
            {
                tcp = new TableCellProperties();
                cell.InsertAt(tcp, 0);
            }
            return tcp;
        }
        private static ParagraphProperties GetPP(ref Paragraph para)
        {
            ParagraphProperties pp;
            if (para.Elements<ParagraphProperties>().Count() > 0)
            {
                pp = para.Elements<ParagraphProperties>().ElementAt(0);
            }
            else
            {
                pp = new ParagraphProperties();
                para.InsertAt(pp, 0);
            }
            return pp;
        }

        public static void printTranscript(PrintJob printJob, bool includeAuditedCourses, ref StringBuilder sbErrors)
        {
            string transTemplate = AppData.GetKeyValue("TranscriptTemplate");
            if (printJob == PrintJob.printEnglishTranscript) { transTemplate = AppData.GetKeyValue("EnglishTranscriptTemplate"); }
            if (File.Exists(transTemplate))
            {
                if (studentDegreeInfoDT != null && studentDegreeInfoDT.Rows.Count == 1)
                {
                    string studentName = string.Empty;  // Used in file name
                    // Write file to byteArray and read it into a memory stream
                    byte[] byteArray = File.ReadAllBytes(transTemplate);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        stream.Write(byteArray, 0, (int)byteArray.Length);
                        // Get the wordDoc and fill the tables
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
                        {
                            // A. Assign a reference to the existing document body.
                            MainDocumentPart myMainDocumentPart = wordDoc.MainDocumentPart ?? wordDoc.AddMainDocumentPart();
                            Body wordDocBody = myMainDocumentPart.Document.Body;

                            //StudentDegree table
                            studentName = dataHelper.getColumnValueinDR(studentDegreeInfoDT.Rows[0], "studentName");
                            string studentDegree = dataHelper.getColumnValueinDR(studentDegreeInfoDT.Rows[0], "degreeName");

                            //StudentDegreeStatus table
                            string strCreditsEarned = dataHelper.getColumnValueinDR(studentDegreeStatusInfoDT.Rows[0], "creditsEarned");
                            string strQPA = dataHelper.getColumnValueinDR(studentDegreeStatusInfoDT.Rows[0], "QPA");
                            int sdTermID = Int32.Parse(dataHelper.getColumnValueinDR(studentDegreeStatusInfoDT.Rows[0], "firstTermID"));

                            List<string> sdTermsColNames = new List<string> { "startYear", "startMonth" };
                            Dictionary<string, string> sdTermsColValues = TranscriptHelper.GetPkRowColumnValues(
                                    TableName.terms, sdTermID, sdTermsColNames, ref sbErrors);
                            string startDate = String.Format("{0} / {1}", sdTermsColValues["startMonth"], sdTermsColValues["startYear"]);

                            int fdTermID = Int32.Parse(dataHelper.getColumnValueinDR(studentDegreeStatusInfoDT.Rows[0], "lastTermID"));
                            List<string> fdTermsColNames = new List<string> { "endYear", "endMonth" };
                            Dictionary<string, string> fdTermsColValues = TranscriptHelper.GetPkRowColumnValues(
                                    TableName.terms, fdTermID, fdTermsColNames, ref sbErrors);
                            string endDate = String.Format("{0} / {1}", fdTermsColValues["endMonth"], fdTermsColValues["endYear"]);

                            // Add student status to end date
                            int academicStatusID = Int32.Parse(dataHelper.getColumnValueinDR(studentDegreeStatusInfoDT.Rows[0], "academicStatusID"));
                            List<string> academicStatusColNames = new List<string> { "statusName", "eStatusName" };
                            Dictionary<string, string> academicStatusColValues = TranscriptHelper.GetPkRowColumnValues(
                                    TableName.academicStatus, academicStatusID, academicStatusColNames, ref sbErrors);
                            string endDateWithStatus = String.Format("{0} ({1})", endDate, academicStatusColValues["statusName"]);

                            // B. Translate to English
                            if (printJob == PrintJob.printEnglishTranscript)
                            {
                                int studentID = Int32.Parse(dataHelper.getColumnValueinDR(studentDegreeInfoDT.Rows[0], "studentID"));
                                int studentDegreeID = Int32.Parse(dataHelper.getColumnValueinDR(studentDegreeInfoDT.Rows[0], "degreeID"));
                                //Get English name
                                List<string> studentInfoColNames = new List<string> { "eStudentName" };
                                Dictionary<string, string> studentColValues = TranscriptHelper.GetPkRowColumnValues(
                                        TableName.students, studentID, studentInfoColNames, ref sbErrors);
                                string eStudentName = studentColValues["eStudentName"];
                                if (!String.IsNullOrEmpty(eStudentName)) { studentName = eStudentName; }
                                //Get English Degree name
                                List<string> degreeColNames = new List<string> { "eDegreeName" };
                                Dictionary<string, string> degreeColValues = TranscriptHelper.GetPkRowColumnValues(
                                        TableName.degrees, studentDegreeID, degreeColNames, ref sbErrors);
                                string eDegreeName = degreeColValues["eDegreeName"];
                                if (!String.IsNullOrEmpty(eDegreeName)) { studentDegree = eDegreeName; }
                                string eAcademicStatus = academicStatusColValues["eStatusName"];
                                if (!String.IsNullOrEmpty(eAcademicStatus))
                                {
                                    endDateWithStatus = String.Format("{0} ({1})", endDate, eAcademicStatus);
                                }
                            }


                            // C. Find the first table in the document and insert values.
                            Table table = wordDocBody.Elements<Table>().First();

                            InsertTextInTable(table, 0, 1, studentName);
                            InsertTextInTable(table, 1, 1, studentDegree);
                            InsertTextInTable(table, 0, 3, strCreditsEarned);
                            InsertTextInTable(table, 1, 3, strQPA);
                            InsertTextInTable(table, 0, 5, startDate);
                            InsertTextInTable(table, 1, 5, endDateWithStatus);

                            // D. Find second table and fill it
                            table = wordDocBody.Elements<Table>().ElementAt(1);
                            int currentTermID = -1;
                            int currentRow = 1;  // Starts at 1; first row is the title row
                            foreach (DataRow transDR in transcriptDT.Rows)
                            {
                                // Skip audited courses
                                string statusKey = dataHelper.getColumnValueinDR(transDR, "statusKey");
                                if (statusKey != "audit" || includeAuditedCourses)
                                {
                                    // Get information about current term
                                    int termID = Int32.Parse(dataHelper.getColumnValueinDR(transDR, "termID"));
                                    List<string> tTermsColNames = new List<string> { "term", "termName", "eTermName", "startYear", "startMonth", "endYear", "endMonth" };
                                    Dictionary<string, string> tTermsColValues = TranscriptHelper.GetPkRowColumnValues(
                                            TableName.terms, termID, tTermsColNames, ref sbErrors);
                                    string term = tTermsColValues["term"];
                                    if (termID != currentTermID)
                                    {
                                        string termName = tTermsColValues["termName"];
                                        string startYear = tTermsColValues["startYear"];
                                        string endYear = tTermsColValues["endYear"];
                                        string termDate = startYear;
                                        if (startYear != endYear)
                                        {
                                            termDate = string.Format("{0} - {1}", startYear, endYear);
                                        }
                                        // Translate to English
                                        if (printJob == PrintJob.printTranscript) { termDate = termDate + "年 "; }
                                        else if (printJob == PrintJob.printEnglishTranscript)
                                        {
                                            termDate = termDate + " ";
                                            termName = tTermsColValues["eTermName"];
                                        }
                                        //Make this a new Term row and insert term in cell 2

                                        RemoveInnerCellBorders(table, currentRow, 2, 6);
                                        MergeTableCells(table, currentRow, 0, 1);
                                        InsertTextInTable(table, currentRow, 2, termDate + termName, JustificationValues.Left);

                                        // Prepare for next row    
                                        TableRow newTermRow = new TableRow();
                                        for (int i = 0; i < table.Elements<TableRow>().First().Elements<TableCell>().Count(); i++)
                                        {
                                            TableCell newCell = new TableCell(new Paragraph(new Run(new Text(string.Empty))));
                                            newTermRow.Append(newCell);
                                        }
                                        table.Append(newTermRow);
                                        currentRow = currentRow + 1;
                                        currentTermID = termID;
                                    }

                                    string courseName = dataHelper.getColumnValueinDR(transDR, "courseName");
                                    string facultyName = dataHelper.getColumnValueinDR(transDR, "facultyName");
                                    string department = dataHelper.getColumnValueinDR(transDR, "depName");
                                    string credits = dataHelper.getColumnValueinDR(transDR, "credits");
                                    string grade = dataHelper.getColumnValueinDR(transDR, "grade");
                                    int requirementAreaID = Int32.Parse(dataHelper.getColumnValueinDR(transDR, "requirementAreaID"));
                                    List<string> reqAreaColNames = new List<string> { "reqArea", "eReqArea" };
                                    Dictionary<string, string> reqAreaColValues = TranscriptHelper.GetPkRowColumnValues(
                                            TableName.requirementArea, requirementAreaID, reqAreaColNames, ref sbErrors);
                                    string reqArea = reqAreaColValues["reqArea"];
                                    string eReqArea = reqAreaColValues["eReqArea"];
                                    // Round off the credits
                                    decimal dCredits = Decimal.Parse(credits);
                                    dCredits = Decimal.Round(dCredits, 2);
                                    credits = dCredits.ToString();
                                    // Translate to English
                                    if (printJob == PrintJob.printEnglishTranscript)
                                    {
                                        if (!String.IsNullOrEmpty(eReqArea)) { reqArea = eReqArea; }
                                        int courseNameID = Int32.Parse(dataHelper.getColumnValueinDR(transDR, "courseNameID"));
                                        int facultyID = Int32.Parse(dataHelper.getColumnValueinDR(transDR, "facultyID"));
                                        //Get English for courseName and facultyName
                                        List<string> courseColNames = new List<string> { "eCourseName" };
                                        Dictionary<string, string> courseColValues = TranscriptHelper.GetPkRowColumnValues(
                                                TableName.courseNames, courseNameID, courseColNames, ref sbErrors);
                                        string eCourseName = courseColValues["eCourseName"];
                                        if (!String.IsNullOrEmpty(eCourseName)) { courseName = eCourseName; }
                                        List<string> facultyColNames = new List<string> { "eFacultyName" };
                                        Dictionary<string, string> facultyColValues = TranscriptHelper.GetPkRowColumnValues(
                                                TableName.faculty, facultyID, facultyColNames, ref sbErrors);
                                        string eFacultyName = facultyColValues["eFacultyName"];
                                        if (!String.IsNullOrEmpty(eFacultyName)) { facultyName = eFacultyName; }
                                    }
                                    InsertTextInTable(table, currentRow, 0, term);
                                    InsertTextInTable(table, currentRow, 1, department);
                                    InsertTextInTable(table, currentRow, 2, courseName, JustificationValues.Left);
                                    InsertTextInTable(table, currentRow, 3, facultyName, JustificationValues.Left);
                                    InsertTextInTable(table, currentRow, 4, credits);
                                    InsertTextInTable(table, currentRow, 5, grade);
                                    if (reqArea != department)
                                    {
                                        InsertTextInTable(table, currentRow, 6, reqArea, JustificationValues.Left);
                                    }
                                    // Prepare for next row - clone this before adding information
                                    TableRow newRow = new TableRow();
                                    for (int i = 0; i < table.Elements<TableRow>().First().Elements<TableCell>().Count(); i++)
                                    {
                                        TableCell newCell = new TableCell(new Paragraph(new Run(new Text(string.Empty))));
                                        newRow.Append(newCell);
                                    }
                                    table.Append(newRow);
                                    currentRow = currentRow + 1;
                                } // End if statusKey != "audit"
                            }

                            // E. Find Requirement table and fill it
                            if (PrintToWord.studentReqDT != null)
                            {
                                currentRow = 1; // Skip first row
                                table = wordDocBody.Elements<Table>().ElementAt(2);
                                foreach (DataRow reqDR in PrintToWord.studentReqDT.Rows)
                                {
                                    //E.1 Get requirement details
                                    string reqType = dataHelper.getColumnValueinDR(reqDR, "ReqType");
                                    string reqArea = dataHelper.getColumnValueinDR(reqDR, "ReqArea");
                                    string delMethName = dataHelper.getColumnValueinDR(reqDR, "DelMethName");
                                    string required = dataHelper.getColumnValueinDR(reqDR, "Required");
                                    string earned = dataHelper.getColumnValueinDR(reqDR, "Earned");
                                    string needed = dataHelper.getColumnValueinDR(reqDR, "Needed");
                                    string inProgress = dataHelper.getColumnValueinDR(reqDR, "InProgress");
                                    // Prepare for next row
                                    TableRow newTermRow = new TableRow();
                                    for (int i = 0; i < table.Elements<TableRow>().First().Elements<TableCell>().Count(); i++)
                                    {
                                        TableCell newCell = new TableCell(new Paragraph(new Run(new Text(string.Empty))));
                                        newTermRow.Append(newCell);
                                    }
                                    table.Append(newTermRow);
                                    // Write to 3rd table
                                    InsertTextInTable(table, currentRow, 0, reqType);
                                    InsertTextInTable(table, currentRow, 1, reqArea, JustificationValues.Left);
                                    InsertTextInTable(table, currentRow, 2, delMethName, JustificationValues.Left);
                                    InsertTextInTable(table, currentRow, 3, required);
                                    InsertTextInTable(table, currentRow, 4, earned);
                                    InsertTextInTable(table, currentRow, 5, needed);
                                    InsertTextInTable(table, currentRow, 6, inProgress);
                                    currentRow = currentRow + 1;
                                }
                            }
                        }
                        // Save the stream to the disk
                        string myPath = AppData.GetKeyValue("DocumentFolder");
                        myPath = myPath + "\\" + studentName.Replace(" ", "_") + "." + DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + ".docx";
                        try
                        {
                            byte[] newByteArray = UseBinaryReader(stream);
                            File.WriteAllBytes(myPath, newByteArray);
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); return; }

                        // Open Microsoft word
                        DialogResult infoResult = MessageBox.Show(Properties.PluginResources.doYouWantToOpenInWord, Properties.PluginResources.openInWord, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (infoResult == DialogResult.Yes)
                        {
                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo(myPath)
                            {
                                UseShellExecute = true
                            };
                            // process.StartInfo.Arguments = "-n";
                            // process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            process.Start();
                            // process.WaitForExit();// Waits here for the process to exit.
                        }
                    }
                }
            }
        }

        public static void printCourseStudentList(PrintJob printJob, ref StringBuilder sbErrors)
        {
            // Only major difference between printing class role and printing course role is template
            // Only minor difference is number of columns to merge in row with section name
            string studentListTemplate = AppData.GetKeyValue("ClassRoleTemplate");
            int endMergeColumn = 13;
            if (printJob == PrintJob.printCourseGradeSheet)
            {
                studentListTemplate = AppData.GetKeyValue("CourseGradeSheetTemplate");
                endMergeColumn = 8;
            }
            if (File.Exists(studentListTemplate))
            {
                if (courseTermInfoDT != null && courseTermInfoDT.Rows.Count == 1)
                {
                    string courseTermName = string.Empty;  // Used in file name
                    // Write file to byteArray and read it into a memory stream
                    byte[] byteArray = File.ReadAllBytes(studentListTemplate);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        stream.Write(byteArray, 0, (int)byteArray.Length);
                        // Get the wordDoc and fill the tables
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
                        {
                            // Assign a reference to the existing document body.
                            MainDocumentPart myMainDocumentPart = wordDoc.MainDocumentPart ?? wordDoc.AddMainDocumentPart();
                            Body wordDocBody = myMainDocumentPart.Document.Body;

                            string courseName = dataHelper.getColumnValueinDR(courseTermInfoDT.Rows[0], "courseName");
                            string termNumber = dataHelper.getColumnValueinDR(courseTermInfoDT.Rows[0], "term");
                            string facultyName = dataHelper.getColumnValueinDR(courseTermInfoDT.Rows[0], "facultyName");
                            courseTermName = String.Format("{0}.{1}.{2}", termNumber, facultyName, courseName);
                            int termID = Int32.Parse(dataHelper.getColumnValueinDR(courseTermInfoDT.Rows[0], "termID"));

                            List<string> termColNames = new List<string> { "term", "termName", "eTermName", "startYear", "startMonth", "endYear", "endMonth" };
                            Dictionary<string, string> termColValues = TranscriptHelper.GetPkRowColumnValues(
                                    TableName.terms, termID, termColNames, ref sbErrors);
                            string termName = termColValues["termName"];
                            string startYear = termColValues["startYear"];
                            string endYear = termColValues["endYear"];
                            string termDate = startYear;
                            if (startYear != endYear)
                            {
                                termDate = string.Format("{0} - {1}", startYear, endYear);
                            }
                            termDate = termDate + "年 ";
                            termName = termDate + termName;

                            // Find the first table in the document and insert values.
                            Table table = wordDocBody.Elements<Table>().First();

                            InsertTextInTable(table, 0, 1, courseName);
                            InsertTextInTable(table, 0, 3, termNumber);
                            InsertTextInTable(table, 0, 4, termName);
                            InsertTextInTable(table, 0, 6, facultyName);

                            // Find second table and fill it
                            table = wordDocBody.Elements<Table>().ElementAt(1);
                            int currentSectionID = -1;
                            int currentRow = 1;  // Starts at 1; first row is the title row
                            foreach (DataRow transDR in transcriptDT.Rows)
                            {
                                int sectionID = Int32.Parse(dataHelper.getColumnValueinDR(transDR, "sectionID"));
                                if (sectionID != currentSectionID)
                                {
                                    // Get information about current section - not using courseInfo because had two DeleteMethodID columns
                                    List<string> sectionColNames = new List<string> { "degreeLevelID", "deliveryMethodID", "credits" };
                                    Dictionary<string, string> sectionColValues = TranscriptHelper.GetPkRowColumnValues(
                                            TableName.section, sectionID, sectionColNames, ref sbErrors);

                                    string credits = sectionColValues["credits"];
                                    // Round off the credits
                                    decimal dCredits = Decimal.Parse(credits);
                                    dCredits = Decimal.Round(dCredits, 2);
                                    credits = dCredits.ToString();
                                    credits = credits + "學分";

                                    int deliveryMethodID = Int32.Parse(sectionColValues["deliveryMethodID"]);
                                    List<string> delMethodColNames = new List<string> { "delMethName", "eDelMethName" };
                                    Dictionary<string, string> delMethodColValues = TranscriptHelper.GetPkRowColumnValues(
                                            TableName.deliveryMethod, deliveryMethodID, delMethodColNames, ref sbErrors);
                                    string delMethodName = delMethodColValues["delMethName"];

                                    int degreeLevelID = Int32.Parse(sectionColValues["degreeLevelID"]);
                                    List<string> degreeLevelColNames = new List<string> { "degreeLevelName" };
                                    Dictionary<string, string> degreeLevelColValues = TranscriptHelper.GetPkRowColumnValues(
                                            TableName.degreeLevel, degreeLevelID, degreeLevelColNames, ref sbErrors);
                                    string degreeLevelName = degreeLevelColValues["degreeLevelName"];

                                    // Prepare for next row    
                                    TableRow newSectionRow = new TableRow();
                                    for (int i = 0; i < table.Elements<TableRow>().First().Elements<TableCell>().Count(); i++)
                                    {
                                        TableCell newCell = new TableCell(new Paragraph(new Run(new Text(string.Empty))));
                                        newSectionRow.Append(newCell);
                                    }
                                    table.Append(newSectionRow);

                                    //Merge cells and insert section name
                                    MergeTableCells(table, currentRow, 1, endMergeColumn);
                                    string fullSectionName = String.Format("{0}, {1}, {2}", degreeLevelName, delMethodName, credits);
                                    InsertTextInTable(table, currentRow, 1, fullSectionName, JustificationValues.Left);

                                    //Prepare for next loop
                                    currentRow = currentRow + 1;
                                    currentSectionID = sectionID;
                                }

                                string studentName = dataHelper.getColumnValueinDR(transDR, "studentName");
                                string studentDegree = dataHelper.getColumnValueinDR(transDR, "degreeName");

                                InsertTextInTable(table, currentRow, 0, studentName, JustificationValues.Left);
                                InsertTextInTable(table, currentRow, 1, studentDegree, JustificationValues.Left);

                                // Prepare for next row - clone this before adding information
                                TableRow newRow = new TableRow();
                                for (int i = 0; i < table.Elements<TableRow>().First().Elements<TableCell>().Count(); i++)
                                {
                                    TableCell newCell = new TableCell(new Paragraph(new Run(new Text(string.Empty))));
                                    newRow.Append(newCell);
                                }
                                table.Append(newRow);

                                currentRow = currentRow + 1;
                            }
                        }
                        // Save the stream to the disk
                        string myPath = AppData.GetKeyValue("DocumentFolder");
                        myPath = myPath + "\\" + courseTermName.Replace(" ", "_") + "." + DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + ".docx";
                        try
                        {
                            byte[] newByteArray = UseBinaryReader(stream);
                            File.WriteAllBytes(myPath, newByteArray);
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); return; }

                        // Open Microsoft word
                        DialogResult infoResult = MessageBox.Show(Properties.PluginResources.doYouWantToOpenInWord, Properties.PluginResources.openInWord, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (infoResult == DialogResult.Yes)
                        {
                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo(myPath)
                            {
                                UseShellExecute = true
                            };
                            // process.StartInfo.Arguments = "-n";
                            // process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            process.Start();
                            // process.WaitForExit();// Waits here for the process to exit.
                        }
                    }
                }
            }
        }

        public static byte[] UseBinaryReader(Stream stream)
        {
            byte[] bytes;
            stream.Position = 0;
            using (var binaryReader = new BinaryReader(stream))
            {
                bytes = binaryReader.ReadBytes((int)stream.Length);
            }
            return bytes;
        }

    }

    public enum PrintJob
    {
        printTranscript,
        printEnglishTranscript,
        printCourseGradeSheet,
        printClassRole
    }



}

