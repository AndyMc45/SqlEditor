using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlTypes;
// using Windows.Media.Core;
using System.Reflection;


namespace SqlEditor
{
    public partial class frmImportExcelEvals : Form
    {
        private bool loadingCmbTerms = true;
        public frmImportExcelEvals()
        {
            InitializeComponent();
        }

        private void frmImportExcelEvals_Load(object sender, EventArgs e)
        {
            string myDocumentFolder = AppData.GetKeyValue("DocumentFolder");
            if (String.IsNullOrEmpty(myDocumentFolder))
            {
                MessageBox.Show("First use 'transcript options' to select a document folder. Place folders with excel files in this document folder.");
                return;
            }
            if (!Directory.Exists(myDocumentFolder))
            {
                string errMessage = String.Format("The folder {0} does not exist! Use 'transcript options' to select a different document folder. ", myDocumentFolder);
                MessageBox.Show(errMessage);
                return;
            }
            dataGridViewExcelFiles.AllowUserToDeleteRows = true;
            // Bind cmbTerms combobox
            // Bind Term ComboBox
            if (MsSql.cn != null && MsSql.cn.State == ConnectionState.Open)
            {
                try
                {
                    DataTable dt = new DataTable();
                    string sqlString = "SELECT termID, CONCAT_WS(' - ',CONCAT('T',term), startYear, termName) as termString FROM Terms where term > 90 ORDER BY Term DESC";
                    MsSql.FillDataTable(dt, sqlString);
                    cmbBoxTerm.DataSource = dt;
                    cmbBoxTerm.ValueMember = "termID";
                    cmbBoxTerm.DisplayMember = "termString";
                    loadingCmbTerms = false;  //Loading only done once
                }
                catch { }
            }
            btnSelectFolder.Enabled = false;
            btnSelectFolder.Visible = false;
            btnRun.Enabled = false;
            progressBar1.Visible = false;
        }

        public DialogResult result { get; set; }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            string myDocumentFolder = AppData.GetKeyValue("DocumentFolder");
            folderBrowserDialog1.InitialDirectory = myDocumentFolder;
            folderBrowserDialog1.ShowNewFolderButton = false;
            result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Check that this folder path is not a higher path
                string folderPath = folderBrowserDialog1.SelectedPath;
                if (!folderPath.StartsWith(myDocumentFolder))
                {
                    MessageBox.Show(String.Format("Folder must be a subfolder of {0} folder;", myDocumentFolder));
                    return;
                }
                lblSelectFolder.Text = folderPath;
                btnSelectFolder.Enabled = false;
                btnSelectFolder.Visible = false;
                btnRun.Enabled = true;

                FillDataGridView(GetDataGridViewExcelFiles());
            }
        }

        private DataGridView GetDataGridViewExcelFiles()
        {
            return dataGridViewExcelFiles;
        }

        private void FillDataGridView(DataGridView dataGridViewExcelFiles)
        {
            // Prepare datatable to load into DataGridViewComboBoxColumn
            string sqlString = @"SELECT courseTermID, cn.courseName, CONCAT_WS(' - ',CONCAT('T',t.term), f.facultyName, cn.courseName) as strCourse FROM
                        CourseTerms ct inner join Faculty f
                        on ct.facultyID = f.facultyID
                        inner join Courses c
                        on c.courseID = ct.courseID
                        inner join CourseNames cn
                        on c.courseNameID = cn.courseNameID
						inner join Terms t
						on t.termID = ct.termID
                        Where ct.termID = " + cmbBoxTerm.SelectedValue.ToString();
            DataTable dt = new DataTable();
            MsSql.FillDataTable(dt, sqlString);
            // Add blank row to dataTable (not the dropDown yet)
            DataRow blankRow = dt.NewRow();
            blankRow["courseTermID"] = 0;
            blankRow["courseName"] = string.Empty;
            blankRow["strCourse"] = "Course not found";
            dt.Rows.Add(blankRow);

            // Add a file name column to the DataGridView
            dataGridViewExcelFiles.Columns.Add("FileName", "Excel Files");

            // Add a ComboBox column to the DataGridView with dt as the Datasource for the drop down
            DataGridViewComboBoxColumn courseTermColumn = new DataGridViewComboBoxColumn();
            courseTermColumn.Name = "CourseTerm";
            courseTermColumn.HeaderText = "Teacher-Course";
            courseTermColumn.ValueMember = "courseTermID";
            courseTermColumn.DisplayMember = "strCourse";
            courseTermColumn.DataSource = dt;
            courseTermColumn.MinimumWidth = 250;
            courseTermColumn.DataPropertyName = "courseTermID";
            dataGridViewExcelFiles.Columns.Add(courseTermColumn);

            //Fill DataGridView - adding a row for each excel file in the selected folder
            string selectedFolderPath = lblSelectFolder.Text;
            string[] excelFiles = Directory.GetFiles(selectedFolderPath, "*.xlsx");
            if (excelFiles.Length == 0)
            {
                MessageBox.Show("No Excel files found in the selected folder.");
                return;
            }

            // Process each Excel file
            foreach (string filePath in excelFiles)
            {
                string myDocumentFolder = AppData.GetKeyValue("DocumentFolder");
                string shortFilePath = filePath;
                if (filePath.StartsWith(myDocumentFolder))
                {
                    // Remove the myDocument directory to get relative Path
                    shortFilePath = filePath.Substring(myDocumentFolder.Length);
                }
                else
                {
                    MessageBox.Show("Something is wrong. A file path does not begin with myDocument path");
                }
                int newRowIndex = dataGridViewExcelFiles.Rows.Add();
                DataGridViewRow newRow = dataGridViewExcelFiles.Rows[newRowIndex];
                newRow.Cells[0].Value = shortFilePath;
                newRow.Cells[1].Value = 0; // The last element of the dropdown
            }
            GuessAtTeacherCourseItem(dt);
        }

        private void GuessAtTeacherCourseItem(DataTable dt)
        {
            foreach (DataGridViewRow dgvRow in dataGridViewExcelFiles.Rows)
            {
                string filePath = dgvRow.Cells[0].Value.ToString();

                IEnumerable<DataRow> foundRows = dt.AsEnumerable()
                .Where(row => filePath.Contains(row.Field<string>("courseName")) && row.Field<int>("courseTermID") > 0);

                //// Convert to an array if needed
                DataRow[] foundRowsArray = foundRows.ToArray();
                if (foundRowsArray.Count() > 0)
                {
                    dgvRow.Cells[1].Value = (int)foundRowsArray[0].ItemArray[0];
                }
                else
                {
                    dgvRow.Cells[1].Value = 0;  // Blank row
                }
            }
        }
        
        private void cmbBoxTerm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loadingCmbTerms)  // True except on loading form
            {
                lblSelectFolder.Text = "Select folder:";
                btnSelectFolder.Enabled = true;
                btnSelectFolder.Visible = true;
                btnRun.Enabled = false;
                // Clear old values - (first clause not used, but just in case)
                dataGridViewExcelFiles.DataSource = null;
                dataGridViewExcelFiles.Rows.Clear();
                dataGridViewExcelFiles.Columns.Clear();
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            var progress = new Progress<int>(value =>
            {
                progressBar1.PerformStep();
                // progressBar1.Value = value; // Runs on UI thread automatically
                // lblStatus.Text = $"Processing: {value}%";
            });
            btnRun.Enabled = false;
            // Set up variables
            progressBar1.Value = 0;
            int dvgRows = dataGridViewExcelFiles.Rows.Count;
            if (dvgRows > 0)
            {
                progressBar1.Maximum = dvgRows;
            }
            else
            { 
                progressBar1.Maximum = 1; 
            }
            progressBar1.Step = 1;
            string strTerm = cmbBoxTerm.SelectedValue.ToString();
            string sqlQuery7 = string.Format("Select Top (1) strYear from Terms where termID = {0}", strTerm);
            string strEvalForm = MsSql.ExecuteScalar(sqlQuery7);
            int inserted = 0;
            int updated = 0;
            StringBuilder sbErr = new StringBuilder();
            progressBar1.Visible = true;

            foreach (DataGridViewRow dvgRow in dataGridViewExcelFiles.Rows)
            {
                int ctID = (int)dvgRow.Cells["CourseTerm"].Value;
                string relPath = dvgRow.Cells["FileName"].Value.ToString();
                // Process rows that have a FileName and CourseTerm
                bool AlreadyPresent = AlreadyInFilesTable(ctID);
                bool processFile = false; // By default do not processFile
                if (ctID > 0)
                {
                    if (!AlreadyPresent)
                    {
                        InsertInFilesTable(dvgRow);
                        inserted++;
                        dvgRow.Cells["FileName"].Value = string.Empty;
                        processFile = true;
                    }
                    else if (ckbUpdateCourses.Checked)
                    {
                        UpdateFilesTable(dvgRow);
                        updated++;
                        dvgRow.Cells["FileName"].Value = string.Empty;
                        processFile = true;
                    }
                }
                // Run the program - do not invoke the UI, because it is running on a different thread (but dvg Rows are still available. Not sure why)
                await Task.Run(() => DoWork(progress, processFile, ctID, relPath, strEvalForm, ref sbErr));
            }

            // Messages to user
            string msg = String.Format("{0} Evaluation files inserted; {1} files updated;", inserted.ToString(), updated.ToString());
            if (sbErr.Length > 0)
            { 
                msg = Environment.NewLine + sbErr.ToString();
            } 
            MessageBox.Show(msg);
            progressBar1.Visible = false;
        }

        private void DoWork(IProgress<int> progress, bool processFile, int ctID, string relPath, string strEvalForm, ref StringBuilder sbErr)
        {
            try
            {
                // Make the process files divide here in order to allow the progress bar to progress on unprocessed files as well.
                if (processFile)
                {
                    // Insert new rows in Answers tables

                    // 1. Load columns into a headers DataTable
                    ExcelReader.headers = new DataTable();
                    string strSqlHeaders = @"
                        SELECT [ExcelColumnLetter],[ExcelColumn],eq.[QuestionName], eq.[NumericQuestion],
                            eq.[HighestScore], eq.[LowestScore], eq.[HeaderMustContain]
                        FROM [dbo].[evalColumnMap] ecm inner join evalQuestions eq
                        on ecm.evalQuestionID = eq.evalQuestionsID 
                        WHERE strYear = '{0}'";
                    strSqlHeaders = string.Format(strSqlHeaders, strEvalForm);
                    MsSql.FillDataTable(ExcelReader.headers, strSqlHeaders);

                    // 2. For each row in headers, add a corresponding column to coursesEvaluations
                    ExcelReader.courseEvaluations = new DataTable();
                    foreach (DataRow headerRow in ExcelReader.headers.Rows)
                    {
                        DataColumn dc = new DataColumn();
                        dc.ColumnName = headerRow["QuestionName"].ToString();
                        if (headerRow["NumericQuestion"].ToString() == "1")
                        {
                            dc.DataType = typeof(int);
                        }
                        else
                        {
                            dc.DataType = typeof(string);
                        }
                        dc.ExtendedProperties.Add("ExcelColumn", headerRow["ExcelColumn"].ToString());
                        dc.ExtendedProperties.Add("ExcelColumnLetter", headerRow["ExcelColumnLetter"].ToString());
                        dc.ExtendedProperties.Add("NumericQuestion", headerRow["NumericQuestion"].ToString());
                        dc.ExtendedProperties.Add("HighestScore", headerRow["HighestScore"].ToString());
                        dc.ExtendedProperties.Add("LowestScore", headerRow["LowestScore"].ToString());
                        dc.ExtendedProperties.Add("HeaderMustContain", headerRow["HeaderMustContain"].ToString());

                        ExcelReader.courseEvaluations.Columns.Add(dc);
                    }
                    // 3. Load excel file into DataTable courseEvaluations
                    string myDocumentFolder = AppData.GetKeyValue("DocumentFolder");
                    string filePath = myDocumentFolder + relPath;
                    if (File.Exists(filePath))
                    {
                        string errMsg = string.Empty;
                        errMsg = ExcelReader.LoadExcelFileIntoCourseEvaluations(filePath);
                        if (errMsg != string.Empty)
                        {
                            sbErr.AppendLine(errMsg);
                            return;
                        }
                        // 4. Delete old evaluations from evalEssayAnswers and evalNumericAnswers sql tables
                        string strString6 = string.Format("DELETE FROM [dbo].[evalEssayAnswers] WHERE CourseTermID = {0}", ctID);
                        MsSql.ExecuteNonQuery(strString6);
                        string strString7 = string.Format("DELETE FROM [dbo].[evalNumericAnswers] WHERE CourseTermID = {0}", ctID);
                        MsSql.ExecuteNonQuery(strString7);

                        // 5. Write the courseEvaluations file to evalEssayAnswers and evalNumericAnswers sql tables
                        //    Course evaluation DT - first row is headers, and so we can skip it
                        //    Each row has 19 columns - one for every answer.
                        //    First two rows are "GraduateStudent" and "Expected Grade", and I will put these two in every answer.
                        //    So, each row will become 17 answers (except feedback ignored if not given).

                        // Get Faculty ID from CourseTerm ID (ctID - from above)
                        String sqlString = string.Format("Select Top 1 facultyID from CourseTerms where CourseTermID = {0}", ctID.ToString());
                        int facultyID = Int32.Parse(MsSql.ExecuteScalar(sqlString));

                        foreach (DataRow cdDataRow in ExcelReader.courseEvaluations.Rows)
                        {
                            // Get StudentDegree (Converted to bool)
                            string studentDegreeColumn = cdDataRow["StudentDegree"].ToString();
                            bool boolGradDegree =
                                studentDegreeColumn.Contains("研究", StringComparison.OrdinalIgnoreCase) ||
                                studentDegreeColumn.Contains("碩", StringComparison.OrdinalIgnoreCase) ||
                                string.IsNullOrEmpty(studentDegreeColumn);

                            // Get Expected Grade (Converted to Int - 5 (A) to 1 (F))
                            string expectedGrade = cdDataRow["ExpectedGrade"].ToString();
                            int intExpectedGrade = -1;
                            if (expectedGrade.Contains("A", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 5; }
                            else if (expectedGrade.Contains("B", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 4; }
                            else if (expectedGrade.Contains("C", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 3; }
                            else if (expectedGrade.Contains("D", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 2; }
                            else if (expectedGrade.Contains("E", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 1; }
                            else if (expectedGrade.Contains("F", StringComparison.OrdinalIgnoreCase))
                            { intExpectedGrade = 1; }

                            // Process all rows - divide numeric and essay answers
                            // I could and should do this based on this only.
                            // Could also use Type type = col.DataType; to get the type of question
                            // The "switch" is clearer, but needs updated for new evaluation forms
                            foreach (DataColumn col in cdDataRow.Table.Columns)
                            {
                                int HighestScore = -1; // lower than highest score
                                int LowestScore = 101;  //higher than lowest score
                                string strHighestScore = col.ExtendedProperties["HighestScore"].ToString();
                                string strLowestScore = col.ExtendedProperties["LowestScore"].ToString();
                                Int32.TryParse(strHighestScore, out HighestScore);
                                Int32.TryParse(strLowestScore, out LowestScore);
                                string columnName = col.ColumnName;
                                // Get questionID
                                String sqlString3 = string.Format("Select Top 1 evalQuestionsID from evalQuestions where QuestionName = '{0}'", columnName);
                                int questionID = Int32.Parse(MsSql.ExecuteScalar(sqlString3));
                                // Insert answer into answer tables
                                switch (columnName)
                                {
                                    case "StudentDegree":
                                        break;
                                    case "ExpectedGrade":                                   
                                    case "Difficulty":
                                    case "Fairness":
                                    case "HowMuchLearned":
                                    case "HowValuable":
                                    case "TeacherPrepared":
                                    case "WellDesigned":
                                    case "ReadingMaterial":
                                    case "GiftedTeacher":
                                    case "AnotherCourse":
                                    case "TeacherReformed":
                                    case "TeacherPunctuality":
                                    case "TakeAgain":
                                    case "ConnectionLost":
                                    case "VideoChoppy":
                                    case "RecordedLectures":
                                        int score = -1;
                                        string strScore = cdDataRow[columnName].ToString();
                                        strScore = strScore.TrimStart();
                                        if (strScore.Length > 0)
                                        {
                                            strScore = strScore.Substring(0, 1);
                                            bool OK = Int32.TryParse(strScore, out score);
                                        }
                                        // Check for error
                                        if (score > HighestScore || score < LowestScore)
                                        {
                                            score = -1;   // Nothing inserted into Database on next statement.
                                        }
                                        // Insert into database evalNumericAnswers Table
                                        if (score > -1)
                                        {
                                            insertNumericAnswerIntoDB(ctID, facultyID, boolGradDegree,
                                            intExpectedGrade, questionID, score, "");
                                        }
                                        break;
                                    case "FeedBackForTeacher":
                                    case "FeedbackOnlineLearning":
                                        string strAnswer = cdDataRow[columnName].ToString();
                                        if (strAnswer.Length > 1)
                                        {
                                            List<string> stringList = new List<string>();
                                            if (strAnswer.Length < 980)
                                            {
                                                stringList.Add(strAnswer);
                                            }
                                            else if (strAnswer.Length < 1960)
                                            {
                                                stringList.Add(strAnswer.Substring(0, 980));
                                                stringList.Add(". . ." + strAnswer.Substring(980));
                                            }
                                            else
                                            {
                                                stringList.Add(strAnswer.Substring(0, 980));
                                                stringList.Add(". . ." + strAnswer.Substring(980, 980));
                                                if (strAnswer.Length < 2940)
                                                {
                                                    stringList.Add(". . . " + strAnswer.Substring(1960));
                                                }
                                                else
                                                {
                                                    stringList.Add(". . . " + strAnswer.Substring(1960,970) + " End deleted");
                                                }
                                            }
                                            foreach (string str in stringList) 
                                            {
                                                insertEssayAnswerIntoDB(ctID, facultyID, boolGradDegree,
                                                    intExpectedGrade, questionID, str, str.Length, "");
                                            }
                                        }
                                        break;
                                    default:
                                        // Unknown error, but ignore for now
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        sbErr.AppendLine("Missing file: " + filePath);
                    }
                }
                progress.Report(0);  // Value not used, so set to 0
            }
            catch (Exception ex)
            {
                sbErr.AppendLine(ex.Message);
            }
        }

        private void insertNumericAnswerIntoDB( int courseTermID, int facultyID, bool gradStudent, 
                     int expectedGrade, int questionID, int score, string adminNote)
        {
            // Escape any single quotes
            adminNote = adminNote.Replace("'", "''");

            string sqlString = @"
            INSERT INTO[dbo].[evalNumericAnswers]
            ([GradStudent], [ExpectedGrade], [CourseTermID], [FacultyID], [QuestionID], [Score], [AdminNote])
            VALUES
            ( '{0}', {1}, {2}, {3}, {4}, {5}, N'{6}')";

            sqlString = string.Format(sqlString, gradStudent, expectedGrade,courseTermID,facultyID, questionID, score, adminNote);
            MsSql.ExecuteNonQuery(sqlString);
        }

        private void insertEssayAnswerIntoDB(int courseTermID, int facultyID, bool gradStudent,
             int expectedGrade, int questionID, string essay, int characters, string adminNote)
        {
            // Escape any single quotes
            adminNote = adminNote.Replace("'", "''");
            essay = essay.Replace("'", "''");

            string sqlString = @"
            INSERT INTO[dbo].[evalEssayAnswers]
            ([GradStudent],[ExpectedGrade],[CourseTermID],[FacultyID],[QuestionID],[Essay],[Characters],[AdminNote])
            VALUES
            ( '{0}', {1}, {2}, {3}, {4}, N'{5}', {6}, N'{7}')";

            sqlString = string.Format(sqlString, gradStudent, expectedGrade, courseTermID, facultyID, questionID, essay, characters, adminNote);
            MsSql.ExecuteNonQuery(sqlString);
        }



        private bool AlreadyInFilesTable(int CourseTermID)
        {
            string sqlString = @"
                SELECT [CourseTermID]
                FROM[dbo].[evalExcelFiles]
                Where CourseTermID = " + CourseTermID.ToString();
            DataTable dt = new DataTable();
            MsSql.FillDataTable(dt, sqlString);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        private void InsertInFilesTable(DataGridViewRow dgvRow)
        {
            string sqlString = @"
            INSERT INTO[dbo].[evalExcelFiles]
            (
                [RelativePath], [CourseTermID], [FacultyID], [intResponses], [intStudentsInCourse], 
                [FileProcessed], [Note]
            )
            VALUES
            (
               '{0}',{1},{2},0,0,
               0,'{3}'
            )";
            ExecuteUpdateOrInsertSql(dgvRow, sqlString);
        }

        private void UpdateFilesTable(DataGridViewRow dgvRow)
        {
            string sqlString = @"
            UPDATE [dbo].[evalExcelFiles]
                SET [RelativePath] = '{0}'
                , [FacultyID] = {2}
                , [intResponses] = 0
                , [intStudentsInCourse] = 0
                , [FileProcessed] = 0
                , [Note] = '{3}'
                WHERE [CourseTermID] = {1}";
            ExecuteUpdateOrInsertSql(dgvRow, sqlString);
        }

        private void ExecuteUpdateOrInsertSql(DataGridViewRow dgvRow, String sqlString)
        {
            // Get required values to insert
            string relativePath = dgvRow.Cells["FileName"].Value.ToString();
            int courseTermID = (int)dgvRow.Cells["CourseTerm"].Value;
            // Getting facultyID from CourseTermID
            int facultyID = 0;
            DataTable dt = new DataTable();
            String sqlString2 = string.Format("Select TOP 1 facultyID FROM CourseTerms ct WHERE ct.courseTermID = {0}", courseTermID.ToString());
            string strFacultyID = MsSql.ExecuteScalar(sqlString2);
            if (strFacultyID != string.Empty) { facultyID = Convert.ToInt32(strFacultyID); }
            // Execute update
            sqlString = string.Format(sqlString, relativePath, courseTermID.ToString(), facultyID.ToString(), "Update: " + DateTime.Now.ToString("yyyy-MM-dd"));
            string errMsg = MsSql.ExecuteNonQuery(sqlString);
            if (errMsg != string.Empty) { MessageBox.Show(errMsg); }
        }
    }
}