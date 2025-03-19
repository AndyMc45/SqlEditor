using System.Data;
using System.Text;

namespace SqlEditor.TranscriptPlugin
{
    internal static class TranscriptMsSql
    {
        public static void transcriptProblems(DataGridViewForm dgvForm)
        {
            if (dgvForm.currentSql != null && dgvForm.currentSql.myTable.ToLower() == "transcript")
            {
                // 1.  Get first transcript problems into a data_table
                StringBuilder sb = new StringBuilder();
                sb.Append("Select t.transcriptID ");
                sb.Append("From Transcript as t ");
                sb.Append("inner join StudentDegrees as sd on t.studentDegreeID = sd.studentDegreeID ");
                sb.Append("inner join Degrees as d on d.degreeID = sd.degreeID ");
                sb.Append("inner join DegreeLevel as dl on dl.degreeLevelID = d.degreeLevelID ");
                sb.Append("inner join Grades as g on t.gradeID = g.gradesID ");
                sb.Append("inner join GradeStatus as gs on t.gradeStatusID = gs.gradeStatusID ");
                sb.Append("inner join CourseTermSection as cts on t.courseTermSectionID = cts.courseTermSectionID ");
                sb.Append("inner join Section as s on s.sectionID = cts.sectionID ");
                sb.Append("inner join DegreeLevel as cdl on s.degreeLevelID = cdl.degreeLevelID ");
                sb.Append("Where(g.earnedCredits = 'true' or g.creditsInQPA = 'true') and gs.forCredit = 'false' ");
                String strSQL = sb.ToString();
                DataTable dt = new DataTable();
                MsSql.FillDataTable(dt, strSQL);

                // 1b.  Show error rows or no error message 
                StringBuilder sbMessage = new StringBuilder();
                sbMessage.AppendLine("Summary: ");
                string errMessage = string.Empty;
                string successMessage = string.Empty;
                if (dt.Rows.Count > 0)
                {
                    List<String> orConditions = new List<String>();
                    foreach (DataRow row in dt.Rows)
                    {
                        String atomicStatement = String.Format("({0} = '{1}' OR {0} IS NULL)", "transcriptID", row["transcriptID"].ToString());
                        orConditions.Add(atomicStatement);
                    }
                    string whereCondition = String.Join(" OR ", orConditions);
                    dgvForm.setTxtManualFilterText(whereCondition);
                    dgvForm.writeGrid_NewFilter(false);
                    // Message box
                    errMessage = "Error: Students have grades in courses that are not for credit!!!";
                    sbMessage.AppendLine(errMessage);
                    MessageBox.Show(sbMessage.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    successMessage = "Test Passed: No auditors have a grade.";
                    sbMessage.AppendLine(successMessage);
                    MessageBox.Show(sbMessage.ToString(), "Test Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                // 2. Check for 2nd error
                DialogResult reply = MessageBox.Show("Check for other errors?", "Check for errors", MessageBoxButtons.YesNo);
                if (reply == DialogResult.Yes)
                {
                    sb = new StringBuilder();
                    sb.Append("Select t.transcriptID,cn.courseName, term.term, ");
                    sb.Append("cdl.degreeLevelName, dl.degreeLevelName,  ");
                    sb.Append("s.credits, gs.forCredit, ");
                    sb.Append("g.grade, g.creditsInQPA ");
                    sb.Append("From Transcript as t ");
                    sb.Append("inner join StudentDegrees as sd on t.studentDegreeID = sd.studentDegreeID ");
                    sb.Append("inner join HandBooks as hb on sd.handbookID = hb.handbookID ");
                    sb.Append("inner join Degrees as d on d.degreeID = sd.degreeID ");
                    sb.Append("inner join DegreeLevel as dl on dl.degreeLevelID = d.degreeLevelID ");
                    sb.Append("inner join Grades as g on t.gradeID = g.gradesID ");
                    sb.Append("inner join GradeStatus as gs on t.gradeStatusID = gs.gradeStatusID ");
                    sb.Append("inner join CourseTermSection as cts on t.courseTermSectionID = cts.courseTermSectionID ");
                    sb.Append("inner join Section as s on s.sectionID = cts.sectionID ");
                    sb.Append("Inner Join DegreeLevel as cdl on s.degreeLevelID = cdl.degreeLevelID ");
                    sb.Append("inner join CourseTerms as ct on ct.courseTermID = cts.courseTermID ");
                    sb.Append("inner join Terms as term on term.termID = ct.termID ");
                    sb.Append("inner join Courses as c on c.courseID = ct.courseID ");
                    sb.Append("inner join CourseNames as cn on c.courseNameID = cn.courseNameID ");
                    sb.Append("inner join RequirementArea as cra on c.requirementAreaID = cra.requirementAreaID ");
                    sb.Append("Where dl.degreeLevel > cdl.degreeLevel and gs.forCredit = 'true' ");
                    strSQL = sb.ToString();
                    dt = new DataTable();
                    MsSql.FillDataTable(dt, strSQL);
                    //
                    if (dt.Rows.Count > 0)
                    {
                        List<String> orConditions = new List<String>();
                        foreach (DataRow row in dt.Rows)
                        {
                            String atomicStatement = String.Format("({0} = '{1}' OR {0} IS NULL)", "transcriptID", row["transcriptID"].ToString());
                            orConditions.Add(atomicStatement);
                        }
                        string whereCondition = String.Join(" OR ", orConditions);
                        dgvForm.setTxtManualFilterText(whereCondition);
                        dgvForm.writeGrid_NewFilter(false);
                        // Message box
                        errMessage = "Error: Graduate students can't take undergrad courses !!!";
                        sbMessage.AppendLine(errMessage);
                        MessageBox.Show(errMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        successMessage = "Test Passed: Students all in courses at their level or above.";
                        sbMessage.AppendLine(successMessage);
                        MessageBox.Show(successMessage, "Test Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    // Summary message
                    MessageBox.Show(sbMessage.ToString(), "Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please load transcript table first.");
            }
        }

        public static int UpdateStudentDegreeStatus(ref int rowsAffected)
        {
            return UpdateStudentDegreeStatus(-1, ref rowsAffected);
        }
        public static int UpdateStudentDegreeStatus(int studentDegreeID, ref int rowsAffected)
        {
            int returnInt = -1;
            List<(string, string)> parameters = new List<(string, string)>();
            parameters.Add(("@studentDegreeID", studentDegreeID.ToString()));
            MsSql.ExecuteNonQuery("UpdateStudentDegreesStatus", parameters, CommandType.StoredProcedure, ref rowsAffected);
            // Return sdsPK if given one student
            if (studentDegreeID > -1)
            {
                string sql = String.Format("SELECT TOP (1) [studentDegreeStatusID] FROM [dbo].[StudentDegreesStatus] as sds " +
                    "Where sds.[studentDegreeID] = '{0}'", studentDegreeID.ToString());
                string sdsPK = MsSql.ExecuteScalar(sql);
                if (sdsPK != string.Empty) { returnInt = Int32.Parse(sdsPK); }
            }
            return returnInt;
        }
    }
}

