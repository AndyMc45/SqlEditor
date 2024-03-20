using System.Data;
using System.Text;

namespace SqlEditor.TranscriptPlugin
{
    internal static class TranscriptHelper
    {
        internal static DataTable GetOneRowDataTable(string tableName, int PkValue, ref StringBuilder sbErrors)
        {
            SqlFactory sqlFactory = new SqlFactory(tableName, 0, 0, false);
            field fld = dataHelper.getTablePrimaryKeyField(tableName);
            where wh = new where(fld, PkValue.ToString());
            sqlFactory.myWheres.Add(wh);
            // Get data row
            string sqlString = sqlFactory.returnSql(command.selectAll);
            DataTable dt = new DataTable();
            string strError = MsSql.FillDataTable(dt, sqlString);
            if (strError != string.Empty) { sbErrors.AppendLine(strError); }
            return dt;
        }

        internal static Dictionary<string, string> GetPkRowColumnValues(string tableName, int pkValue, List<string> columnNames, ref StringBuilder sbErrors)
        {
            var columnValues = new Dictionary<string, string>();
            DataTable dt = GetOneRowDataTable(tableName, pkValue, ref sbErrors);
            SqlFactory sqlFactory = new SqlFactory(tableName, 0, 0);
            // Should be exactly one row in requirementAreaDaDt.dt
            if (dt.Rows.Count != 1)
            {
                sbErrors.AppendLine(String.Format("Error: {0} rows in {1} with primary {2}", dt.Rows.Count.ToString(), tableName, pkValue.ToString()));
            }
            if (dt.Rows.Count > 0)
            {
                foreach (string colName in columnNames)
                {
                    string colValue = dataHelper.getColumnValueinDR(dt.Rows[0], colName);
                    columnValues.Add(colName, colValue);
                }
            }
            return columnValues;
        }

        internal static void fillStudentDegreeDataRow(int studentDegreeID, ref StringBuilder sbErrors)
        {
            PrintToWord.studentDegreeInfoDT = TranscriptHelper.GetOneRowDataTable(TableName.studentDegrees, studentDegreeID, ref sbErrors);
        }

        internal static void fillCourseTermDataRow(int courseTermID, ref StringBuilder sbErrors)
        {
            PrintToWord.courseTermInfoDT = TranscriptHelper.GetOneRowDataTable(TableName.courseTerms, courseTermID, ref sbErrors);
        }

        internal static void fillStudentTranscriptTable(int studentDegreeID, ref StringBuilder sbErrors)
        {
            // Filter transcript table on studentDegreeID
            fillTranscriptTable(TableName.studentDegrees, studentDegreeID, ref sbErrors);
        }

        internal static void fillCourseRoleTable(int courseTermID, ref StringBuilder sbErrors)
        {
            // Filter transcript table on courseTermID
            fillTranscriptTable(TableName.courseTerms, courseTermID, ref sbErrors);
        }

        private static void fillTranscriptTable(string referenceTable, int pkRefTable, ref StringBuilder sbErrors)  // Used for print transcript and print role
        {
            // 0, 0 means no paging - false means don't include all columns of all foreign keys - would be 89 if we did
            SqlFactory sqlTranscript = new SqlFactory(TableName.transcript, 0, 0, false);
            if (referenceTable == TableName.studentDegrees)
            {
                field fkField = dataHelper.getForeignKeyFromRefTableName(TableName.transcript, referenceTable);
                where wh = new where(fkField, pkRefTable.ToString());
                sqlTranscript.myWheres.Add(wh);
                field term = new field(TableName.terms, "term", DbType.Int32, 4);
                orderBy ob = new orderBy(term, System.Windows.Forms.SortOrder.Ascending);
                sqlTranscript.myOrderBys.Add(ob);
            }
            else if (referenceTable == TableName.courseTerms)
            {
                field fkField = dataHelper.getForeignKeyFromRefTableName(TableName.courseTermSection, referenceTable);
                where wh = new where(fkField, pkRefTable.ToString());
                sqlTranscript.myWheres.Add(wh);
                field section = new field(TableName.section, "sectionID", DbType.Int32, 4);
                orderBy ob = new orderBy(section, System.Windows.Forms.SortOrder.Ascending);  // Could order by deliveryMethodID
                sqlTranscript.myOrderBys.Add(ob);
            }
            string sqlString = sqlTranscript.returnSql(command.selectAll);
            PrintToWord.transcriptDT = new System.Data.DataTable();
            // I fill the transcript table into a datatable, and show it in the "transcript" tab
            string strError = MsSql.FillDataTable(PrintToWord.transcriptDT, sqlString);
            if (strError != string.Empty)
            {
                sbErrors.AppendLine(String.Format("ERROR filling transcript table: {0}", strError));
            }
        }

        public static void fillGradRequirementsDT(ref StringBuilder sbErrors)
        {
            // 1. Create a table "StudentReq" - this table is not in the database.
            // Add this table to dataHelper.fieldsDT so that I can use an sqlFactory to style the table 
            // Add rows to dataHelper.fieldsDT. Only do it once in a session
            string filter = "TableName = 'StudentReq'";
            DataRow[] drs = dataHelper.fieldsDT.Select(filter);
            // If no rows in above, the rows have already been added to dataHelper.fieldsDT.  14 columns as follows:
            // public static void AddRowToFieldsDT(string TableName, int ColNum, string ColumnName, string ColumnDisplayName
            //      , string DataType, bool Nullable, bool _identity, bool is_PK, bool is_FK, bool is_DK, short MaxLength
            //      , string RefTable, string RefPkColumn, int Width)
            if (drs.Count() == 0)
            {
                dataHelper.AddRowToFieldsDT("StudentReq", 1, "StudentReqID", "StudentReqID", "int", false, true, true, false, false, 4, String.Empty, String.Empty, 0);
                //RequirementType table -
                dataHelper.AddRowToFieldsDT("StudentReq", 2, "ReqType", "ReqType", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 10, "eReqType", "eReqType", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                //RequirementArea table - 
                dataHelper.AddRowToFieldsDT("StudentReq", 3, "ReqArea", "ReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 12, "eReqArea", "eReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                // DeliveryMethodTable
                dataHelper.AddRowToFieldsDT("StudentReq", 4, "DelMethName", "DelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 14, "eDelMethName", "eDelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 15, "rDeliveryLevel", "rDeliveryLevel", "int", false, false, false, false, true, 4, String.Empty, String.Empty, 0);
                // GradRequirement Table itself
                dataHelper.AddRowToFieldsDT("StudentReq", 5, "Required", "Required", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 16, "Limit", "Limit", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                // Need to calucate the following from transcript
                dataHelper.AddRowToFieldsDT("StudentReq", 6, "Earned", "Earned", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 7, "Needed", "Needed", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 8, "InProgress", "InProgress", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 17, "Icredits", "Icredits", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
            }

            // 2. Get Grad Requirements for this degree and handbook
            int intHandbookID = Int32.Parse(dataHelper.getColumnValueinDR(PrintToWord.studentDegreeInfoDT.Rows[0], "handbookID"));
            int intDegreeID = Int32.Parse(dataHelper.getColumnValueinDR(PrintToWord.studentDegreeInfoDT.Rows[0], "degreeID"));
            SqlFactory sqlGradReq = new SqlFactory(TableName.gradRequirements, 0, 0);
            where wh1 = new where(TableField.GradRequirements_DegreeID, intDegreeID.ToString());
            where wh2 = new where(TableField.GradRequirements_handbookID, intHandbookID.ToString());
            sqlGradReq.myWheres.Add(wh1);
            sqlGradReq.myWheres.Add(wh2);
            string sqlString = sqlGradReq.returnSql(command.selectAll);
            // Put degree requirements in a new DataTable grDaDt.dt
            MsSqlWithDaDt grDaDt = new MsSqlWithDaDt(sqlString);

            // 3. Put all the basic information about this degree, degreeLevel, degreeDeliveryMethod into 3 dictionaries.
            //    Use "GetPkRowColumnValues" to create the dictionary.
            //    Use the dictionary to get any value you want about this degree.  For example, sDegreeLevelColValue["degreeLevel"]
            // Degree - "degreeName", "deliveryMethodID", "eDegreeName", "degreeLevelID"
            List<string> sDegreeColNames = new List<string> { "degreeName", "eDegreeName", "degreeLevelID" };
            Dictionary<string, string> sDegreeColValues = TranscriptHelper.GetPkRowColumnValues(
                    TableName.degrees, intDegreeID, sDegreeColNames, ref sbErrors);

            // degreeLevel - "degreeLevelName", "degreeLevel"
            int sDegreeLevelID = Int32.Parse(sDegreeColValues["degreeLevelID"]);
            List<string> sDegreeLevelColNames = new List<string> { "degreeLevelName", "degreeLevel" };
            Dictionary<string, string> sDegreeLevelColValues = TranscriptHelper.GetPkRowColumnValues(
                    TableName.degreeLevel, sDegreeLevelID, sDegreeLevelColNames, ref sbErrors);
            int sDegreeLevel = Int32.Parse(sDegreeLevelColValues["degreeLevel"]);
            string sDegreeLevelName = sDegreeLevelColValues["degreeLevelName"];

            // 4. Create sqlFactory for studentReq - this will give you sqlStudentReq.myFields()
            SqlFactory sqlStudentReq = new SqlFactory("StudentReq", 0, 0);

            // 5. Create studentReqDT and add a column for each field.
            //    We will add a row to this table for each requirement and fill it. 
            PrintToWord.studentReqDT = new System.Data.DataTable();
            foreach (field f in sqlStudentReq.myFields)
            {
                DataColumn dc = new DataColumn(f.fieldName, dataHelper.ConvertDbTypeToType(f.dbType));
                // Make StudentReqID the primary key
                if (f.fieldName == "StudentReqID")
                {
                    dc.AutoIncrement = true;
                    dc.AutoIncrementSeed = 1;
                    dc.AutoIncrementStep = 1;
                }
                PrintToWord.studentReqDT.Columns.Add(dc);
            }

            //------------------------------------------------------------------------------------//
            // Outer loop - graduation requirements
            //------------------------------------------------------------------------------------//

            decimal totalQpaCredits = 0;  // Only update on the first inner loop through transcripts
            decimal totalQpaPoints = 0;
            decimal totalCreditsEarned = 0;
            bool firstLoop = true;
            // 6. Main routine: Fill studentReqDT 
            // Old degrees might not have any requirements, but I need one to get the QPA.
            // So I get the first row in the gradReq table along with a 'note' that this is a fakeRow
            bool fakeRow = false;
            if (grDaDt.dt.Rows.Count == 0)
            {
                // These values are not used, so I just use the first row in the gradRequirement Table.
                sqlGradReq.myWheres.Clear();
                sqlString = sqlGradReq.returnSql(command.selectAll);
                grDaDt = new MsSqlWithDaDt(sqlString);
                fakeRow = true;
            }

            foreach (DataRow drGradReq in grDaDt.dt.Rows)
            {
                //6a. Get information we need from drGradReq - use "r" for requirement.
                Decimal rCreditLimit = Decimal.Parse(dataHelper.getColumnValueinDR(drGradReq, "creditLimit"));
                Decimal rReqCredits = Decimal.Parse(dataHelper.getColumnValueinDR(drGradReq, "reqUnits"));
                int rGradReqTypeID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "gradReqTypeID"));
                int rRequirementAreaID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "reqAreaID"));
                int rDeliveryMethodID = Int32.Parse(dataHelper.getColumnValueinDR(drGradReq, "rDeliveryMethodID"));

                //6b. requirementAreaID - Get information from gradRequirmentType Table
                List<string> rReqAreaTableColNames = new List<string> { "reqArea", "eReqArea" };
                Dictionary<string, string> rReqAreaTableColValues = TranscriptHelper.GetPkRowColumnValues(
                    TableName.requirementArea, rRequirementAreaID, rReqAreaTableColNames, ref sbErrors);
                string rReqArea = rReqAreaTableColValues["reqArea"];
                string rReqAreaEng = rReqAreaTableColValues["eReqArea"];

                //6c. reqTypeID - Get information from requirementType table
                List<string> rGradTypeColNames = new List<string> { "reqTypeDK"};
                Dictionary<string, string> rGradTypeColValues = TranscriptHelper.GetPkRowColumnValues(
                    TableName.gradRequirementType, rGradReqTypeID, rGradTypeColNames, ref sbErrors);
                string rGradReqTypeName = rGradTypeColValues["reqTypeDK"];

                //6d. deliveryMethodID - Get the requirement deliveryMethod table
                List<string> rDeliveryMethodColNames = new List<string> { "delMethName", "eDelMethName", "deliveryLevel" };
                Dictionary<string, string> rDeliveryMethodColValues = TranscriptHelper.GetPkRowColumnValues(
                    TableName.deliveryMethod, rDeliveryMethodID, rDeliveryMethodColNames, ref sbErrors);
                string rDelMethName = rDeliveryMethodColValues["delMethName"];
                string rDelMethNameEng = rDeliveryMethodColValues["eDelMethName"];
                int rDeliveryLevel = Int32.Parse(rDeliveryMethodColValues["deliveryLevel"]);
                // For level zero, don't give user the name; just leave it empty
                if (rDeliveryLevel == 0)
                {
                    rDelMethName = string.Empty; ;
                    rDelMethNameEng = string.Empty; ;
                }

                //6e. Create a row for studentReqDT, and partially fill it with drGradReq info. extracted above.
                //   (Except for credits earned / inProgress / needed, we have all that we need to fill this row )

                DataRow dr = PrintToWord.studentReqDT.NewRow();

                // "StudentReqID" is the Primary key and column set to autoincrement.
                dataHelper.setColumnValueInDR(dr, "Required", rReqCredits);
                dataHelper.setColumnValueInDR(dr, "Limit", rCreditLimit);
                dataHelper.setColumnValueInDR(dr, "ReqArea", rReqArea);
                dataHelper.setColumnValueInDR(dr, "eReqArea", rReqAreaEng);

                dataHelper.setColumnValueInDR(dr, "DelMethName", rDelMethName);
                dataHelper.setColumnValueInDR(dr, "eDelMethName", rDelMethNameEng);
                dataHelper.setColumnValueInDR(dr, "rDeliveryLevel", rDeliveryLevel);

                dataHelper.setColumnValueInDR(dr, "ReqType", rGradReqTypeName);
                dataHelper.setColumnValueInDR(dr, "Earned", 0);
                dataHelper.setColumnValueInDR(dr, "InProgress", 0);
                dataHelper.setColumnValueInDR(dr, "Needed", 0);
                dataHelper.setColumnValueInDR(dr, "Icredits", 0);

                //------------------------------------------------------------------------------------//
                //   Inner Loop - Transcript rows
                //------------------------------------------------------------------------------------//

                // 7. Loop through the transcript rows - updating "earned", "InProgress", Icredits"
                //    Also keep track of QPA credits and QPA pointsEarned
                //    After loop is complete, fill in the "Needed" (= required minus earned) 
                decimal qpaCredits = 0;
                decimal qpaPoints = 0;
                decimal earned = 0;
                decimal inProgress = 0;
                decimal iCredits = 0;

                foreach (DataRow drTrans in PrintToWord.transcriptDT.Rows)
                {
                    // Get all required information about the transcript row course - add "c" before variable for "course"

                    // Information from transcript datarow itself - a lot
                    int cGradeID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "gradeID"));
                    int cGradeStatusID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "gradeStatusID"));
                    int cCourseTermSectionID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseTermSectionID"));
                    int cCourseTermID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseTermID"));
                    int cCourseID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseID"));
                    int cCourseNameID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "courseNameID"));
                    int cSectionID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "sectionID"));
                    int cDegreeLevelID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "degreeLevelID"));
                    int cDeliveryMethodID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "deliveryMethodID"));
                    int cRequirementAreaID = Int32.Parse(dataHelper.getColumnValueinDR(drTrans, "requirementAreaID"));


                    // Information from Grades table
                    List<string> cGradesColNames = new List<string> { "grade", "QP", "earnedCredits", "creditsInQPA" };
                    Dictionary<string, string> cGradesColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.grades, cGradeID, cGradesColNames, ref sbErrors);
                    string cGrade = cGradesColValues["grade"];
                    Decimal cGradeQP = Decimal.Parse(cGradesColValues["QP"]);
                    Boolean cEarnedCredits = Boolean.Parse(cGradesColValues["earnedCredits"]);
                    Boolean cCreditsInQPA = Boolean.Parse(cGradesColValues["creditsInQPA"]);

                    // Information from GradeStatus table
                    List<string> cGradeStatusColNames = new List<string> { "statusKey", "statusName", "eStatusName", "forCredit" };
                    Dictionary<string, string> cGradesStatusColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.gradeStatus, cGradeStatusID, cGradeStatusColNames, ref sbErrors);
                    string cStatusKey = cGradesStatusColValues["statusKey"];  // Used in error msg
                    bool forCredit = bool.Parse(cGradesStatusColValues["forCredit"]);

                    // Information from Section table
                    List<string> cSectionColNames = new List<string> { "credits" };
                    Dictionary<string, string> cSectionColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.section, cSectionID, cSectionColNames, ref sbErrors);
                    Decimal cCredits = Decimal.Parse(cSectionColValues["credits"]);

                    // CourseName - 
                    // int cCourseID = Int32.Parse(cCourseTermColValues["courseID"]);
                    List<string> cCourseNamesColNames = new List<string> { "courseName", "eCourseName" };
                    Dictionary<string, string> cCourseNamesColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.courseNames, cCourseNameID, cCourseNamesColNames, ref sbErrors);
                    string cCourseName = cCourseNamesColValues["courseName"];  // Used in error messages only, i.e. doesn't affect requirements

                    // DegreeLevel - "degreeLevelName", "degreeLevel"
                    List<string> cDegreeLevelColNames = new List<string> { "degreeLevelName", "degreeLevel" };
                    Dictionary<string, string> cDegreeLevelColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.degreeLevel, cDegreeLevelID, cDegreeLevelColNames, ref sbErrors);
                    int cDegreeLevel = Int32.Parse(cDegreeLevelColValues["degreeLevel"]);
                    string cDegreeLevelName = cDegreeLevelColValues["degreeLevelName"];  // Used in error msg

                    // deliveryMethod - "delMethName", "eDelMethName", "deliveryLevel"
                    List<string> cDelMethColNames = new List<string> { "delMethName", "eDelMethName", "deliveryLevel" };
                    Dictionary<string, string> cDelMethColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.deliveryMethod, cDeliveryMethodID, cDelMethColNames, ref sbErrors);
                    int cDeliveryLevel = Int32.Parse(cDelMethColValues["deliveryLevel"]);

                    // Requirements - "reqArea", "eReqArea"
                    List<string> cReqColNames = new List<string> { "reqArea", "eReqArea", "Ancestors" };
                    Dictionary<string, string> cReqColValues = TranscriptHelper.GetPkRowColumnValues(
                        TableName.requirementArea, cRequirementAreaID, cReqColNames, ref sbErrors);
                    string cReqArea = cReqColValues["reqArea"];
                    string cAncestors = cReqColValues["Ancestors"];

                    //Error message
                    if (cEarnedCredits && !forCredit && !fakeRow)
                    {
                        // cStatusKey is not "forCredit" but the grade indicates student has earned credit
                        string strWarn = String.Format("{0} has a grade but its statusKey is {1}. ", cCourseName, cStatusKey);
                        sbErrors.AppendLine(strWarn);
                    }

                    // Update "Earned", "InProgress", "Icredit" for this requirement row
                    // First check that the degreeLevel is correct
                    if (forCredit || fakeRow)  // This is needed for every type of requirement
                    {
                        // Error message
                        if (cDegreeLevel < sDegreeLevel && !fakeRow)
                        {
                            sbErrors.AppendLine(String.Format("Degree level of '{0} ({1})' is lower than student degree level ({2}). No credit granted.",
                                        cCourseName, cDegreeLevelName, sDegreeLevelName));
                        }
                        else
                        {
                            // Handle total QPA points and credits earned
                            // This is the only thing that we need in a fake row
                            if (cCreditsInQPA && firstLoop)
                            {
                                totalQpaPoints = totalQpaPoints + (cCredits * cGradeQP);
                                totalQpaCredits = totalQpaCredits + cCredits;
                            }
                            if (cEarnedCredits && firstLoop)
                            {
                                totalCreditsEarned = totalCreditsEarned + cCredits;
                            }
                            if (!fakeRow)
                            {
                                if (rGradReqTypeName == "QPA" || fakeRow)
                                {
                                    if (cCreditsInQPA)
                                    {
                                        qpaPoints = qpaPoints + (cCredits * cGradeQP);
                                        qpaCredits = qpaCredits + cCredits;
                                    }
                                }
                                else  // Credits or Hours or Times requirement
                                {
                                    // Get the number of "credits" or "units" earned
                                    if (rGradReqTypeName == "times")
                                    {
                                        cCredits = 1;   // The main point of "Times"
                                    }  

                                    // No credit for this requirement if cDeliveryMethod < rDeliveryMethod
                                    if (cDeliveryLevel < rDeliveryLevel)
                                    {
                                        cCredits = 0;
                                    }

                                    // Check if this drTrans dataRow meets this requirement
                                    List<string> cAncestorsList = cAncestors.Split(",").ToList();
                                    if (rGradReqTypeName == "credits" || rGradReqTypeName == "hours" || rGradReqTypeName == "times")
                                    {
                                        // Meets requirement
                                        if (rReqArea == cReqArea || cAncestorsList.Contains(rReqArea))
                                        {
                                            // Update earned and inProgress (for this requirement)
                                            if (cEarnedCredits)
                                            {
                                                earned = earned + cCredits;
                                            }
                                            else if (cGrade == "NG")
                                            {
                                                inProgress = inProgress + cCredits;
                                            }
                                            else if (cGrade == "I")
                                            {
                                                iCredits = inProgress + cCredits;
                                            }
                                        }
                                        else
                                        {
                                            cCredits = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Fill in the requirement row Earned, InProgress, Needed
                if (!fakeRow)
                {
                    if (rGradReqTypeName == "QPA")
                    {
                        decimal QPA = 0;
                        if (qpaCredits > 0)
                        {
                            QPA = Math.Round(qpaPoints / qpaCredits, 2);
                        }
                        dataHelper.setColumnValueInDR(dr, "Earned", QPA);
                        dataHelper.setColumnValueInDR(dr, "InProgress", 0);
                        if (QPA < rReqCredits)
                        {
                            dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - QPA);
                        }
                    }
                    else
                    {
                        // Fill in columns 
                        dataHelper.setColumnValueInDR(dr, "Earned", earned);
                        dataHelper.setColumnValueInDR(dr, "InProgress", inProgress);
                        if (rReqCredits > earned)
                        {
                            dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - earned);
                        }
                        else
                        {
                            dataHelper.setColumnValueInDR(dr, "Needed", 0);
                        }
                        dataHelper.setColumnValueInDR(dr, "Icredits", iCredits);
                    }
                    // Add this row to studentReqDT
                    PrintToWord.studentReqDT.Rows.Add(dr);
                    firstLoop = false;
                }
                if (fakeRow) { break; }
            }
            // Update student QPA and total Credits - not yet committed to database
            DataRow studentDegreeInfoRow = PrintToWord.studentDegreeInfoDT.Rows[0];
            decimal totalQPA = 0;
            if (totalQpaCredits > 0)
            {
                totalQPA = Math.Round(totalQpaPoints / totalQpaCredits, 2);
            }
            dataHelper.setColumnValueInDR(studentDegreeInfoRow, "creditsEarned", totalCreditsEarned);
            dataHelper.setColumnValueInDR(studentDegreeInfoRow, "QPA", totalQPA);
            dataHelper.setColumnValueInDR(studentDegreeInfoRow, "lastUpdated", DateTime.Now.ToShortDateString());

            // Save these three changes down to the database - start over from scratch.
            field pkField = dataHelper.getTablePrimaryKeyField(TableName.studentDegrees);
            string pk = dataHelper.getColumnValueinDR(studentDegreeInfoRow, pkField.fieldName);
            sqlString = String.Format("Select * from {0} where {1} = '{2}'", TableName.studentDegrees, pkField.fieldName, pk);
            MsSqlWithDaDt dadt = new MsSqlWithDaDt(sqlString);
            List<field> fieldsToUpdate = new List<field>();
            fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "creditsEarned"));
            fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "QPA"));
            fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "lastUpdated"));
            MsSql.SetUpdateCommand(fieldsToUpdate, dadt.da);
            dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "creditsEarned", totalCreditsEarned);
            dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "QPA", totalQPA);
            dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "lastUpdated", DateTime.Now.ToShortDateString());
            try { dadt.da.Update(dadt.dt); } catch { }
        }

        public static Dictionary<string, string> FillColumnHeaderTranslationDictionary()
        {
            // Use lowercase english.  "zzz" will find "zzzID" - so "zzzID" can be skipped
            // Maintain alphabetical order
            Dictionary<string, string> columnHeaderTranslations = new Dictionary<string, string>(); ;
            columnHeaderTranslations.Add("ancestors", "祖宗");
            columnHeaderTranslations.Add("auditing", "潘婷");
            columnHeaderTranslations.Add("cdepname", "c學科名字");
            columnHeaderTranslations.Add("country", "國家");
            columnHeaderTranslations.Add("countries", "國家");
            columnHeaderTranslations.Add("countryname", "國家名字");
            columnHeaderTranslations.Add("countryprovince", "國家省市");
            columnHeaderTranslations.Add("countryprovinces", "國家省市");
            columnHeaderTranslations.Add("courses", "課程");
            columnHeaderTranslations.Add("course", "課程");
            columnHeaderTranslations.Add("coursename", "課程名字");
            columnHeaderTranslations.Add("coursenames", "課程名字");
            columnHeaderTranslations.Add("courseterms", "學季課程");
            columnHeaderTranslations.Add("courseterm", "課程學季");
            columnHeaderTranslations.Add("coursetermsection", "學季課程班");
            columnHeaderTranslations.Add("createdate", "創建日期");
            columnHeaderTranslations.Add("creditlimit", "學分限制");
            columnHeaderTranslations.Add("credits", "學分");
            columnHeaderTranslations.Add("creditsearned", "總學分");
            columnHeaderTranslations.Add("creditsinqpa", "學分在QPA");
            columnHeaderTranslations.Add("creditsource", "學分來源");
            columnHeaderTranslations.Add("degree", "學位");
            columnHeaderTranslations.Add("degrees", "學位");
            columnHeaderTranslations.Add("degreelevel", "學位程度");
            columnHeaderTranslations.Add("degreelevelname", "學位程度名字");
            columnHeaderTranslations.Add("degreename", "學位名字");
            columnHeaderTranslations.Add("degreenamelong", "學位名字長");
            columnHeaderTranslations.Add("degreesubarea", "學位分割");
            columnHeaderTranslations.Add("subareaname", "輔修分割名字");
            columnHeaderTranslations.Add("departments", "課系");
            columnHeaderTranslations.Add("deliverylevel", "上課方法程度");
            columnHeaderTranslations.Add("deliverymethod", "上課方法");
            columnHeaderTranslations.Add("delmethname", "上課方法名字");
            columnHeaderTranslations.Add("depname", "學科名字");
            columnHeaderTranslations.Add("department", "學科");
            columnHeaderTranslations.Add("earnedcredits", "得到學分");
            columnHeaderTranslations.Add("ecountryname", "e國家名字");
            columnHeaderTranslations.Add("ecoursename", "e課程名字");
            columnHeaderTranslations.Add("ecreditsource", "e學分來源");
            columnHeaderTranslations.Add("edegreename", "e學位名字");
            columnHeaderTranslations.Add("edegreenamelong", "e學位名字長");
            columnHeaderTranslations.Add("edepname", "e學科名字");
            columnHeaderTranslations.Add("edelmethname", "e上課方法名字");
            columnHeaderTranslations.Add("efacultyname", "e老師名字");
            columnHeaderTranslations.Add("enddate", "結束日期");
            columnHeaderTranslations.Add("endmonth", "結束月");
            columnHeaderTranslations.Add("endyear", "結束年");
            columnHeaderTranslations.Add("eprovincename", "e省市名字");
            columnHeaderTranslations.Add("ereqarea", "e規則範圍");
            columnHeaderTranslations.Add("ereqtype", "e規則Type");
            columnHeaderTranslations.Add("esubareaname", "e輔修分割名字");
            columnHeaderTranslations.Add("estatusname", "e狀態名字");
            columnHeaderTranslations.Add("estudentname", "e學生名字");
            columnHeaderTranslations.Add("etermname", "e學季名字");
            columnHeaderTranslations.Add("etypename", "e類別名字");
            columnHeaderTranslations.Add("faculty", "老師");
            columnHeaderTranslations.Add("facultyname", "老師名字");
            columnHeaderTranslations.Add("facultyunique", "老師Unique");
            columnHeaderTranslations.Add("firstterm", "入學學季");
            columnHeaderTranslations.Add("forcredit", "可得到學分");
            columnHeaderTranslations.Add("fulfillrequirementnocredit", "滿足畢業要求_沒學分");
            columnHeaderTranslations.Add("gender", "性別"); 
            columnHeaderTranslations.Add("grade", "成績");
            columnHeaderTranslations.Add("grades", "成績");
            columnHeaderTranslations.Add("gradestatus", "成績狀態");
            columnHeaderTranslations.Add("gradrequirement", "畢業要求");
            columnHeaderTranslations.Add("gradrequirements", "畢業要求");
            columnHeaderTranslations.Add("gradrequirementtype", "畢業要求類別");
            columnHeaderTranslations.Add("gradreqtype", "畢業要求類別");
            columnHeaderTranslations.Add("graduated", "已畢業");
            columnHeaderTranslations.Add("handbook", "手冊");
            columnHeaderTranslations.Add("handbooks", "手冊");
            columnHeaderTranslations.Add("lastupdatedqpa", "QPA更新日期");
            columnHeaderTranslations.Add("lastupdatedstudentstatus", "狀態更新日期");
            columnHeaderTranslations.Add("lastterm", "最近上課學季");
            columnHeaderTranslations.Add("lastupdated", "更新日期"); 
            columnHeaderTranslations.Add("note", "說明");
            columnHeaderTranslations.Add("province", "省市");
            columnHeaderTranslations.Add("provincename", "省市名字");
            columnHeaderTranslations.Add("rdeliverymethod", "r上課方法");
            columnHeaderTranslations.Add("repeatspermitted", "可重複");
            columnHeaderTranslations.Add("reqarea", "規則範圍");
            columnHeaderTranslations.Add("reqtype", "規則類別");
            columnHeaderTranslations.Add("requirementarea", "要求名稱");
            columnHeaderTranslations.Add("requirementType", "規則Type");
            columnHeaderTranslations.Add("reqUnits", "必獲數目");
            columnHeaderTranslations.Add("section", "班");
            columnHeaderTranslations.Add("selfstudycourse", "自修課程");
            columnHeaderTranslations.Add("startdate", "錄取日期");
            columnHeaderTranslations.Add("startmonth", "錄取月");
            columnHeaderTranslations.Add("startyear", "錄取年");
            columnHeaderTranslations.Add("statuskey", "狀態Key");
            columnHeaderTranslations.Add("statusname", "狀態名字");
            columnHeaderTranslations.Add("studentdegree", "學生學位");
            columnHeaderTranslations.Add("studentdegrees", "學位學生");
            columnHeaderTranslations.Add("student", "學生");
            columnHeaderTranslations.Add("students", "學生");
            columnHeaderTranslations.Add("studentname", "學生名字");
            columnHeaderTranslations.Add("studentstatus", "學生狀態");
            columnHeaderTranslations.Add("studentstatushistory", "學生狀態紀錄");
            columnHeaderTranslations.Add("studentunique", "學生Unique");
            columnHeaderTranslations.Add("table", "表格");
            columnHeaderTranslations.Add("term", "學季");
            columnHeaderTranslations.Add("terms", "學期");
            columnHeaderTranslations.Add("termname", "學季名字");
            columnHeaderTranslations.Add("transcript", "成績單");
            columnHeaderTranslations.Add("transfercredits", "轉學分");
            columnHeaderTranslations.Add("typename", "類別名字");

            // My plugin menu items    
            columnHeaderTranslations.Add("transcripts", "成績單");
            columnHeaderTranslations.Add("print transcript", "列印成績單");
            columnHeaderTranslations.Add("print class list", "列印課程學生");
            columnHeaderTranslations.Add("update studentdegrees table", "更新學生學位表");
            columnHeaderTranslations.Add("options", "選類");

            return columnHeaderTranslations;
        }
    }

    internal static class TableName
    {
        // Names of the tables <string>
        internal static string countries { get => "Countries"; }
        internal static string countryProvinces { get => "CountryProvinces"; }

        internal static string courseNames { get => "CourseNames"; }
        internal static string courses { get => "Courses"; }
        internal static string courseTerms { get => "CourseTerms"; }
        internal static string courseTermSection { get => "CourseTermSection"; }
        internal static string degreeLevel { get => "DegreeLevel"; }
        internal static string degrees { get => "Degrees"; }
        internal static string deliveryMethod { get => "DeliveryMethod"; }
        internal static string departments { get => "Departments"; }
        internal static string faculty { get => "Faculty"; }
        internal static string grades { get => "Grades"; }
        internal static string gradRequirements { get => "GradRequirements"; }
        internal static string gradRequirementType { get => "GradRequirementType"; }
        internal static string gradeStatus { get => "GradeStatus"; }
        internal static string handbooks { get => "Handbooks"; }
        internal static string studentDegrees { get => "StudentDegrees"; }
        internal static string requirementArea { get => "RequirementArea"; }
        internal static string section { get => "Section"; }
        internal static string studentGradReq { get => "StudentGradReq"; }
        internal static string studentStatus { get => "StudentStatus"; }

        internal static string studentStatusHistory { get => "StudentStatusHistory"; }

        internal static string students { get => "Students"; }
        internal static string terms { get => "Terms"; }
        internal static string transcript { get => "Transcript"; }
        internal static string transferCredits { get => "TransferCredits"; }


    }
    internal static class TableField
    {
        // Fields used in where statement
        internal static field GradRequirements_DegreeID { get => dataHelper.getFieldFromFieldsDT(TableName.gradRequirements, "degreeID"); }
        internal static field GradRequirements_handbookID { get => dataHelper.getFieldFromFieldsDT(TableName.gradRequirements, "handbookID"); }

    }




}
