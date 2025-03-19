using SqlEditor.PluginsInterface;
using SqlEditor.TranscriptPlugin.Properties;
using System.Data;
using System.Text;


namespace SqlEditor.TranscriptPlugin
{
    // This plugin class has been discovered by reflection in the main program form.
    // It has been instantiated - and so the constructor has run.
    // Some of its public values have been changed by the main program
    // A click in the main program calls its callback function
    // The callback function opens frmTranscriptOptions - which allows user to do various things
    public class TransPlugin : IPlugin
    {
        #region Variables - all required by interface
        public String Name() { return this.name; }
        private String name = String.Empty;

        public ControlTemplate CntTemplate() { return cntTemplate; }
        private ControlTemplate cntTemplate;

        public Form MainForm { set => mainForm = value; }
        private static Form mainForm;   // This will be set to the main form by a delegate

        public List<Func<String, String, DataRow, bool>> UpdateConstraints() { return updateConstraints; }
        private List<Func<String, String, DataRow, bool>> updateConstraints;

        public List<Func<string, List<Tuple<String, String>>, bool>> InsertConstraints() { return insertConstraints; }
        private List<Func<string, List<Tuple<String, String>>, bool>> insertConstraints;

        public List<Func<string, int, bool>> DeleteConstraints() { return deleteConstraints; }
        private List<Func<string, int, bool>> deleteConstraints;
        public Action<string> NewTableAction() { return newTableAction; }
        private Action<string> newTableAction;


        #endregion

        // Constructor - Called in main program when plugin loaded.  So ignore the "0 references".
        public TransPlugin(String name)
        {
            this.name = "Transcripts";
            var menuList = new List<(String, String)>
            {
                ("Print Transcript", "printTranscript"),
                ("Print Class List", "printClassList"),
                ("Update StudentDegrees Table", "updateStudentDegreesTable"),
                ("Check for transcript errors", "checkForTranscriptErrors"),
                ("Options", "options")
            };
            // Set appData to desired culture, and then translate if this is same as translationCultureName.
            // See DataGridViewForm.cs constructor.    
            Dictionary<string, string> columnHeaderTranslations = TranscriptHelper.FillColumnHeaderTranslationDictionary();

            List<(String, String)> readOnlyFields = new List<(string, string)>
            {
                (TableName.studentDegrees, "creditsEarned"), (TableName.studentDegrees, "lastTerm") ,
                (TableName.studentDegrees, "QPA"), (TableName.studentDegrees, "academicStatusID")
            };

            String translationCultureName = "zh-Hant";  // Hard coded to the language of the translation

            frmTranscriptOptions fOptions = new frmTranscriptOptions();

            cntTemplate = new ControlTemplate(("Transcripts", "transcriptMenu"),
                                                menuList,
                                                Transcript_CallBack,
                                                fOptions,
                                                translationCultureName,
                                                columnHeaderTranslations,
                                                readOnlyFields);

            updateConstraints = new List<Func<String, String, DataRow, bool>>
            {
                transcriptOnGradeConstraintPassed
            };

            insertConstraints = new List<Func<string, List<Tuple<String, String>>, bool>>
            {
                transcriptCourseLevelCheckPassed
            };

            deleteConstraints = new List<Func<string, int, bool>>();

            newTableAction = newTableTranscriptPlugin;
        }

        // Define CallBack - If things are good, then open the form with 'job'
        void Transcript_CallBack(object? sender, EventArgs<string> e)
        {
            if (e.Value == "transcriptMenu")
            {
                // Disable some menuItems

            }
            else if (e.Value == "options")
            {
                frmTranscriptOptions fOptions = new frmTranscriptOptions();
                fOptions.myJob = frmTranscriptOptions.Job.options;
                fOptions.ShowDialog();
            }
            else if (e.Value == "printTranscript")
            {
                int studentDegreeID = SetStudentDegreeID();  // Shows error message if any
                if (studentDegreeID == 0)
                {
                    // Messages shown already by setStudentDegreeID and so do nothing here.
                }
                else
                {
                    // Update this studentDegreeID information - Error message already shown.
                    int rowsAffected = 0;
                    int sdsPK = TranscriptMsSql.UpdateStudentDegreeStatus(studentDegreeID, ref rowsAffected);

                    //Open form
                    frmTranscriptOptions fOptions = new frmTranscriptOptions();
                    fOptions.myJob = frmTranscriptOptions.Job.printTranscript;
                    fOptions.studentDegreeID = studentDegreeID;
                    fOptions.studentDegreeStatusID = sdsPK;
                    fOptions.headerTranslations = cntTemplate.ColumnHeaderTranslations;
                    fOptions.translationCultureName = cntTemplate.TranslationCultureName;

                    fOptions.ShowDialog();    // 
                }
            }
            // The only difference between class role and grade sheet is the template that is used
            else if (e.Value == "printClassList")
            {
                int courseTermID = SetCourseTermDegreeID();  // Shows error message if any
                if (courseTermID == 0)
                {
                    // Messages shown already by SetCourseTermDegreeID() so do nothing more.
                }
                else
                {
                    frmTranscriptOptions fOptions = new frmTranscriptOptions();
                    fOptions.myJob = frmTranscriptOptions.Job.printClassList;
                    fOptions.courseTermID = courseTermID;
                    fOptions.headerTranslations = cntTemplate.ColumnHeaderTranslations;
                    fOptions.translationCultureName = cntTemplate.TranslationCultureName;
                    fOptions.ShowDialog();    // 
                }
            }
            else if (e.Value == "updateStudentDegreesTable")
            {
                // MainForm variable in the plugin has been set to the mainForm of the program by a delegate.
                DataGridViewForm dgvForm = (DataGridViewForm)mainForm;
                // 1. Ask
                string msgStr = String.Format("{1}{0}{2} {3}",
                    Environment.NewLine,
                    PluginResources.doYouWantToUpdateStDeTable,
                    PluginResources.thiswillUpdateTheStudentDegreesTable,
                    PluginResources.andUpdateQPAbasedOnTranscriptTable);
                DialogResult result = MessageBox.Show(msgStr, PluginResources.updateStudentDegreesTable, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // 2. Upgrade StudentDegrees Table
                if (result == DialogResult.Yes)
                {
                    // 2a. Upgrade academicStatusID column
                    int rowsAffected = 0;
                    TranscriptMsSql.UpdateStudentDegreeStatus(ref rowsAffected);
                }
            }
            else if (e.Value == "checkForTranscriptErrors")
            {
                // MainForm variable in the plugin has been set to the mainForm of the program by a delegate.
                DataGridViewForm dgvForm = (DataGridViewForm)mainForm;
                TranscriptMsSql.transcriptProblems(dgvForm);
            }
            else
            {
                MessageBox.Show("Message From ToolStrip, Received in Inherited User Control : " + e.Value);
            }
        }

        private int SetStudentDegreeID()
        {
            StringBuilder sbError = new StringBuilder();
            int studentDegreeID = 0;
            // MainForm variable in the plugin has been set to the mainForm of the program by a delegate.  See mainForm constructor. 
            DataGridViewForm dgvForm = (DataGridViewForm)mainForm;
            // Get the studentDegreeID and then call "PrepareToPrint"
            SqlFactory sqlCur = dgvForm.currentSql;
            // Return 0 if no table selected in main form
            if (dgvForm.currentSql == null) { return 0; }
            // Try to get Student ID
            if (dgvForm.dataGridView1.SelectedRows.Count == 1)
            {
                if (dgvForm.currentSql.myTable == TableName.transcript)
                {
                    //Get studentDegreeID column
                    field fld = dataHelper.getForeignKeyFromRefTableName(dgvForm.currentSql.myTable, TableName.studentDegrees);
                    int colNum = dgvForm.getDGVcolumn(fld);
                    studentDegreeID = (Int32)dgvForm.dataGridView1.SelectedRows[0].Cells[colNum].Value;
                }
                else if (dgvForm.currentSql.myTable == TableName.studentDegrees)
                {
                    field fld = dataHelper.getTablePrimaryKeyField(TableName.studentDegrees);
                    int colNum = dgvForm.getDGVcolumn(fld);
                    studentDegreeID = (Int32)dgvForm.dataGridView1.SelectedRows[0].Cells[colNum].Value;
                }
                else if (dgvForm.currentSql.myTable == TableName.students)
                {
                    string err = String.Format(Properties.PluginResources.selectStudentInTable01, TableName.studentDegrees, TableName.students);
                    sbError.AppendLine(err);
                }
                else
                {
                    string err = String.Format(Properties.PluginResources.selectOneRowinTable0or1, TableName.transcript, TableName.studentDegrees);
                    sbError.AppendLine(err);
                }
            }
            else
            {
                sbError.AppendLine(Properties.PluginResources.selectExactlyOneRow);
            }

            if (sbError.Length > 0)
            {
                MessageBox.Show(sbError.ToString(), "Error message", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return 0;
            }
            return studentDegreeID;
        }

        private int SetCourseTermDegreeID()
        {
            StringBuilder sbError = new StringBuilder();
            int courseTermID = 0;
            // MainForm variable in the plugin has been set to the mainForm of the program by a delegate.  See mainForm constructor. 
            DataGridViewForm dgvForm = (DataGridViewForm)mainForm;
            // Try to get the courseTermID
            SqlFactory sqlCur = dgvForm.currentSql;
            // Return 0 if no table selected in main form
            if (dgvForm.currentSql == null) { return 0; }
            // Return value in selected row
            if (dgvForm.dataGridView1.SelectedRows.Count == 1)
            {
                if (dgvForm.currentSql.myTable == TableName.courseTermSection)
                {
                    //Get studentDegreeID column
                    field fld = dataHelper.getForeignKeyFromRefTableName(dgvForm.currentSql.myTable, TableName.courseTerms);
                    int colNum = dgvForm.getDGVcolumn(fld);
                    courseTermID = (Int32)dgvForm.dataGridView1.SelectedRows[0].Cells[colNum].Value;
                }
                else if (dgvForm.currentSql.myTable == TableName.courseTerms)
                {
                    field fld = dataHelper.getTablePrimaryKeyField(TableName.courseTerms);
                    int colNum = dgvForm.getDGVcolumn(fld);
                    courseTermID = (Int32)dgvForm.dataGridView1.SelectedRows[0].Cells[colNum].Value;
                }
                else if (dgvForm.currentSql.myTable == TableName.courses)
                {
                    string err = String.Format(Properties.PluginResources.selectCourseInTable01, TableName.courseTerms, TableName.courses);
                    sbError.AppendLine(err);
                }
                else
                {
                    string err = String.Format(Properties.PluginResources.selectOneRowinTable0or1, TableName.courseTermSection, TableName.courseTerms);
                    sbError.AppendLine(err);
                }
            }
            else
            {
                sbError.AppendLine(Properties.PluginResources.selectExactlyOneRow);
            }

            if (sbError.Length > 0)
            {
                MessageBox.Show(sbError.ToString(), "Error message", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return 0;
            }
            return courseTermID;
        }

        private static Func<String, String, DataRow, bool> transcriptOnGradeConstraintPassed =
            new Func<String, String, DataRow, bool>((table, fieldName, dr) =>
            {
                if (table != "Transcript") { return true; }
                if (fieldName != "gradeID" && fieldName != "gradeStatusID") { return true; }
                int gradeStatus = Int32.Parse(dataHelper.getColumnValueinDR(dr, "gradeStatusID"));
                int gradeID = Int32.Parse(dataHelper.getColumnValueinDR(dr, "gradeID"));
                DataTable dt = new DataTable();
                String sqlStr = String.Format("SELECT [forCredit] FROM [GradeStatus] Where [gradeStatusID] = {0}", gradeStatus.ToString());
                MsSql.FillDataTable(dt, sqlStr);
                if (dt.Rows.Count > 0)  // Always = 1
                {
                    DataRow gsDR = dt.Rows[0];
                    Boolean forCredit = Boolean.Parse(dataHelper.getColumnValueinDR(gsDR, "forCredit"));
                    dt = new DataTable();
                    sqlStr = String.Format("SELECT [grade], [creditsInQPA], [earnedCredits] FROM [Grades] Where [gradesID] = {0}", gradeID.ToString());
                    MsSql.FillDataTable(dt, sqlStr);
                    if (dt.Rows.Count > 0) // Always 1
                    {
                        DataRow gradesDR = dt.Rows[0];
                        Boolean creditsInQPA = Boolean.Parse(dataHelper.getColumnValueinDR(gradesDR, "creditsInQPA"));
                        Boolean earnedCredits = Boolean.Parse(dataHelper.getColumnValueinDR(gradesDR, "earnedCredits"));
                        if (!forCredit && (creditsInQPA || earnedCredits))
                        {
                            string grade = dataHelper.getColumnValueinDR(gradesDR, "grade");
                            string errorMessage = String.Format("Student can't get grade {0} in a course that is not for credit", grade);
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
                return true;
            });

        private static Func<string, List<Tuple<String, String>>, bool> transcriptCourseLevelCheckPassed =
            new Func<string, List<Tuple<String, String>>, bool>((table, fieldAndValueTuples) =>
            {
                if (table != "Transcript") { return true; }
                int studentDegreeID = 0;
                int courseTermSectionID = 0;
                foreach (Tuple<String, String> fieldAndValue in fieldAndValueTuples)
                {
                    if (fieldAndValue.Item1 == "studentDegreeID")
                    {
                        studentDegreeID = Int32.Parse(fieldAndValue.Item2);
                    }
                    if (fieldAndValue.Item1 == "courseTermSectionID")
                    {
                        courseTermSectionID = Int32.Parse(fieldAndValue.Item2);
                    }
                }
                if (studentDegreeID > 0 && courseTermSectionID > 0)  //Always true
                {
                    DataTable dt = new DataTable();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Select degreeLevel, degreeLevelName ");
                    sb.Append("From StudentDegrees inner join Degrees as d on d.degreeID = StudentDegrees.degreeID ");
                    sb.Append("inner join DegreeLevel on d.degreeLevelID = DegreeLevel.degreeLevelID ");
                    sb.Append("where StudentDegrees.studentDegreeID = {0} ");
                    String sqlStr = String.Format(sb.ToString(), studentDegreeID.ToString());
                    int studentDegreeLevel = 0;
                    string studentDegreeLevelName = string.Empty;
                    MsSql.FillDataTable(dt, sqlStr);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow sdDR = dt.Rows[0];
                        studentDegreeLevel = Int32.Parse(dataHelper.getColumnValueinDR(sdDR, "degreeLevel"));
                        studentDegreeLevelName = dataHelper.getColumnValueinDR(sdDR, "degreeLevelName");
                    }
                    // CourseLevel
                    int courseLevel = 0;
                    string courseDegreeLevelName = string.Empty;
                    dt = new DataTable();
                    sb = new StringBuilder();
                    sb.Append("Select degreeLevel, degreeLevelName ");
                    sb.Append("From CourseTermSection inner join Section on Section.sectionID = CourseTermSection.sectionID ");
                    sb.Append("inner join DegreeLevel on DegreeLevel.degreeLevelID = Section.degreeLevelID ");
                    sb.Append("Where courseTermSectionID = {0} ");
                    sqlStr = String.Format(sb.ToString(), courseTermSectionID.ToString());
                    MsSql.FillDataTable(dt, sqlStr);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow sdDR = dt.Rows[0];
                        courseLevel = Int32.Parse(dataHelper.getColumnValueinDR(sdDR, "degreeLevel"));
                        courseDegreeLevelName = dataHelper.getColumnValueinDR(sdDR, "degreeLevelName");
                    }
                    if (courseLevel < studentDegreeLevel)
                    {
                        string errorMessage = String.Format("{0} student can't take a {1} course", studentDegreeLevelName, courseDegreeLevelName);
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                // 2nd check
                int gradeID = 0;
                int gradeStatusID = 0;
                foreach (Tuple<String, String> fieldAndValue in fieldAndValueTuples)
                {
                    if (fieldAndValue.Item1 == "gradeID")
                    {
                        gradeID = Int32.Parse(fieldAndValue.Item2);
                    }
                    if (fieldAndValue.Item1 == "gradeStatusID")
                    {
                        gradeStatusID = Int32.Parse(fieldAndValue.Item2);
                    }
                }
                if (gradeID > 0 && gradeStatusID > 0)
                {
                    DataTable dt = new DataTable();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Select grade, earnedCredits, creditsInQPA ");
                    sb.Append("From Grades where Grades.gradesID = {0} ");
                    String sqlStr = String.Format(sb.ToString(), gradeID.ToString());
                    bool earnedCredits = true;
                    bool creditsInQPA = true;
                    string grade = String.Empty;
                    MsSql.FillDataTable(dt, sqlStr);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow gDR = dt.Rows[0];
                        earnedCredits = Boolean.Parse(dataHelper.getColumnValueinDR(gDR, "earnedCredits"));
                        creditsInQPA = Boolean.Parse(dataHelper.getColumnValueinDR(gDR, "creditsInQPA"));
                        grade = dataHelper.getColumnValueinDR(gDR, "grade");
                    }
                    // for Credit
                    bool forCredit = false;
                    dt = new DataTable();
                    sb = new StringBuilder();
                    sb.Append("Select forCredit from GradeStatus where gradeStatusID = {0} ");
                    sqlStr = String.Format(sb.ToString(), gradeStatusID.ToString());
                    MsSql.FillDataTable(dt, sqlStr);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow gsDR = dt.Rows[0];
                        forCredit = Boolean.Parse(dataHelper.getColumnValueinDR(gsDR, "forCredit"));
                    }
                    if (!forCredit && (creditsInQPA || earnedCredits))
                    {
                        string errorMessage = String.Format("Student can't get grade {0} in a course that is not for credit", grade);
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                return true;
            });

        private static Action<string> newTableTranscriptPlugin =
            new Action<string>((table) =>
        {
            if (table == "StudentDegreesStatus")
            {
                int rowsAffected = 0;
                TranscriptMsSql.UpdateStudentDegreeStatus(ref rowsAffected);
                // MainForm variable in the plugin has been set to the mainForm of the program by a delegate.
                DataGridViewForm dgvForm = (DataGridViewForm)mainForm;
                dgvForm.msgText("StudentDegreesStatus table updated by plugin");
            }
            // Do nothing

        });

    }
}
