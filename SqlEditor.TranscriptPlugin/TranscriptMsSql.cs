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

        public static string UpdateEveryChangedStudentDegreeStatus(ref int rowsAffected)
        {
            return UpdateEveryChangedStudentDegreeStatus(-1, ref rowsAffected);
        }
        public static string UpdateEveryChangedStudentDegreeStatus(int studentDegreeID, ref int rowsAffected)
        {
            string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(";with cte AS (Select *, ROW_NUMBER() OVER(PARTITION BY[studentDegreeID] ORDER BY[dateEstablished] DESC) as rn ");
            sb.AppendLine("From[StudentStatusHistory])UPDATE[dbo].[StudentDegrees] SET [studentStatusID] = cte.studentStatusID ");
            sb.AppendLine("FROM cte Inner Join StudentDegrees on StudentDegrees.studentDegreeID = cte.studentDegreeID");
            sb.AppendLine("AND rn = 1 AND ");
            if(studentDegreeID < 0) 
            {
                // Update all the changed statuses
                sb.AppendLine("cte.dateCreated >= StudentDegrees.lastUpdated");
            }
            else
            {
                // Update the given student status
                sb.AppendLine("cte.studentDegreeID = " + studentDegreeID.ToString());
            }
            string query = sb.ToString();
            result = MsSql.ExecuteNonQuery(query, ref rowsAffected);
            return result;
        }

        public static string updateEveryChangedStudentCreditsLastTermQPA(ref int rowsAffected)
        {
            return updateEveryChangedStudentCreditsLastTermQPA(-1, ref rowsAffected);
        }
        public static string updateEveryChangedStudentCreditsLastTermQPA(int studentDegreeID, ref int rowsAffected)
        {
                string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; With cte AS( ");
            sb.AppendLine("SELECT [Transcript].[studentDegreeID], ");
            sb.AppendLine("Max(Transcript.createDate) as maxCreateDate,");
            sb.AppendLine("Max(Terms.term) as maxTerm, ");
            sb.AppendLine("Sum(CASE ");
            sb.AppendLine("when Grades.earnedCredits = 'True' then Section.credits");
            sb.AppendLine("else 0 end) as totalCredits, ");
            sb.AppendLine("Sum(CASE ");
            sb.AppendLine("When Grades.earnedCredits = 'True' AND Grades.creditsInQPA = 'True' then Section.credits");
            sb.AppendLine("Else 0 END) as totalCreditsinQPA,");
            sb.AppendLine("Sum(CASE");
            sb.AppendLine("When Grades.earnedCredits = 'True' AND Grades.creditsInQPA = 'True' then QP * Section.credits");
            sb.AppendLine("Else 0 End) as totalQPs");
            sb.AppendLine("FROM [Transcript] ");
            sb.AppendLine("inner join CourseTermSection on  CourseTermSection.courseTermSectionID = Transcript.courseTermSectionID");
            sb.AppendLine("inner join Section on CourseTermSection.sectionID = Section.sectionID");
            sb.AppendLine("inner join CourseTerms on CourseTerms.courseTermID = CourseTermSection.courseTermID");
            sb.AppendLine("inner join Courses on Courses.courseID = CourseTerms.courseID");
            sb.AppendLine("inner join DegreeLevel as courseDegreeLevel on Section.degreeLevelID = courseDegreeLevel.degreeLevelID ");
            sb.AppendLine("inner join Terms on Terms.termID = CourseTerms.termID");
            sb.AppendLine("inner join Grades on Grades.gradesID = Transcript.gradeID");
            sb.AppendLine("inner join StudentDegrees on[Transcript].[studentDegreeID] = StudentDegrees.studentDegreeID");
            sb.AppendLine("inner join Degrees on Degrees.degreeID = StudentDegrees.degreeID");
            sb.AppendLine("inner join DegreeLevel as degreeDegreeLevel on degreeDegreeLevel.degreeLevelID = Degrees.degreeLevelID");
            sb.AppendLine("where courseDegreeLevel.degreeLevel >= degreeDegreeLevel.degreeLevel");
            sb.AppendLine("Group By[Transcript].[studentDegreeID] ");
            sb.AppendLine(")");
            sb.AppendLine("UPDATE StudentDegrees");
            sb.AppendLine("SET[creditsEarned] = CTE.totalCredits, ");
            sb.AppendLine("[QPA] = CASE ");
            sb.AppendLine("WHEN totalCreditsinQPA = 0 then 0");
            sb.AppendLine("ELSE ROUND(totalQPs / totalCreditsinQPA, 2) END,");
            sb.AppendLine("[lastUpdated] = GETDATE(), ");
            sb.AppendLine("[lastTermID] = Terms.termID");
            sb.AppendLine("FROM StudentDegrees Inner Join CTE");
            sb.AppendLine("on StudentDegrees.studentDegreeID = CTE.studentDegreeID");
            sb.AppendLine("Inner Join Terms on Terms.term = CTE.maxTerm");
            if (studentDegreeID < 0)
            {
                sb.AppendLine("Where StudentDegrees.lastUpdated <= cte.maxCreateDate"); // Remove this 'where' line to update all
            }
            else
            {
                sb.AppendLine("Where cte.studentDegreeID = " + studentDegreeID.ToString()); // Remove this 'where' line to update all
            }
            string query = sb.ToString();
            result = MsSql.ExecuteNonQuery(query, ref rowsAffected);
            return result;
        }

        public static string updateEveryChangedStudentFirstTermID(ref int rowsAffected)
        {
            string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; With cte AS(");
            sb.AppendLine("SELECT [Transcript].[studentDegreeID],");
            sb.AppendLine("Max(Transcript.createDate) as maxCreateDate,");
            sb.AppendLine("Min(Terms.term) as minTerm");
            sb.AppendLine("FROM [Transcript] ");
            sb.AppendLine("inner join CourseTermSection on  CourseTermSection.courseTermSectionID = Transcript.courseTermSectionID");
            sb.AppendLine("inner join CourseTerms on CourseTerms.courseTermID = CourseTermSection.courseTermID");
            sb.AppendLine("inner join Terms on Terms.termID = CourseTerms.termID");
            sb.AppendLine("inner join StudentDegrees on[Transcript].[studentDegreeID] = StudentDegrees.studentDegreeID");
            sb.AppendLine("Group By [Transcript].[studentDegreeID]");
            sb.AppendLine(")");
            sb.AppendLine("UPDATE StudentDegrees");
            sb.AppendLine("SET [firstTermID] = Terms.termID");
            sb.AppendLine("FROM StudentDegrees Inner Join CTE");
            sb.AppendLine("on StudentDegrees.studentDegreeID = CTE.studentDegreeID");
            sb.AppendLine("Inner Join Terms on Terms.term = CTE.minTerm");
            sb.AppendLine("Where StudentDegrees.lastUpdated <= cte.maxCreateDate");
            string query = sb.ToString();
            result = MsSql.ExecuteNonQuery(query, ref rowsAffected);
            return result;
        }


    }
}
