using Microsoft.Data.SqlClient;
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

        // Update every StudentDegree status (signified by -1)
        public static int UpdateStudentDegreeStatus(ref int rowsAffected)
        {
            return UpdateStudentDegreeStatus(-1, ref rowsAffected);
        }
        // Update a specific StudentDegree status
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

        public static string FillPluginTranscript(DataTable plugTransDT, int studentDegreeID)
        {
            string query = "GetStudentTranscript";
            CommandType commandType = CommandType.StoredProcedure;
            List<(string, string)> parameters = new List<(string, string)>();
            SqlParameter sqlPar1 = new SqlParameter("@StudentDegreeID", studentDegreeID.ToString());
            parameters.Add(("@StudentDegreeID", studentDegreeID.ToString()));
            int rowsAffected = 0;
            string error = MsSql.FillDataTable(plugTransDT, query, parameters, commandType);
            return error;
        }

        public static string FillStudentReq(DataTable StuReqDT, int studentDegreeID)
        {
            string query = "GetStudentRequirementTable";
            CommandType commandType = CommandType.StoredProcedure;
            List<(string, string)> parameters = new List<(string, string)>();
            SqlParameter sqlPar1 = new SqlParameter("@StudentDegreeID", studentDegreeID.ToString());
            parameters.Add(("@StudentDegreeID", studentDegreeID.ToString()));
            int rowsAffected = 0;
            string error = MsSql.FillDataTable(StuReqDT, query, parameters, commandType);
            return error;
        }

        public static StringBuilder getCTETranscriptSQL(int StudentDegreeID, string boolForCreditRows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("   DECLARE @sd_ID int;");
            sb.AppendLine(String.Format("   SET @sd_ID = {0}", StudentDegreeID.ToString()));
            sb.AppendLine("");
            sb.AppendLine("   DECLARE @sDegree_ID int;");
            sb.AppendLine("   SET @sDegree_ID = (Select sd.degreeID from StudentDegrees as sd where sd.studentDegreeID = @sd_ID) ");
            sb.AppendLine("");
            sb.AppendLine("   DECLARE @sHandbook_ID int;");
            sb.AppendLine("   SET @sHandbook_ID = (Select sd.handbookID from StudentDegrees as sd where sd.studentDegreeID = @sd_ID) ");
            sb.AppendLine("");
            sb.AppendLine("   DECLARE @BoolValue nchar(6);");
            sb.AppendLine(String.Format("   SET @BoolValue = '{0}'", boolForCreditRows));
            sb.AppendLine("");
            sb.AppendLine("; with stTrans AS");
            sb.AppendLine("(");
            sb.AppendLine("   Select s.credits as sCredits, g.earnedCredits as eCredits, cDegreeLevel = cdl.degreeLevel, ");
            sb.AppendLine("         cra.reqArea as cReqArea, cra.Ancestors as cReqAncestors, g.grade as cGrade, ");
            sb.AppendLine("         g.creditsInQPA as cCreditsInQPA, g.QP as cGradeQP");
            sb.AppendLine("      From Transcript as t  ");
            sb.AppendLine("      inner join StudentDegrees as sd on t.studentDegreeID = sd.studentDegreeID AND sd.studentDegreeID = @sd_ID  -- Ian hua - Dao shuo");
            sb.AppendLine("      inner join HandBooks as hb on sd.handbookID = hb.handbookID");
            sb.AppendLine("      inner join Grades as g on t.gradeID = g.gradesID");
            sb.AppendLine("      inner join GradeStatus as gs on t.gradeStatusID = gs.gradeStatusID");
            sb.AppendLine("      inner join CourseTermSection as cts on t.courseTermSectionID = cts.courseTermSectionID");
            sb.AppendLine("      inner join Section as s on s.sectionID = cts.sectionID");
            sb.AppendLine("      Inner Join DegreeLevel as cdl on s.degreeLevelID = cdl.degreeLevelID ");
            sb.AppendLine("      inner join CourseTerms as ct on ct.courseTermID = cts.courseTermID");
            sb.AppendLine("      inner join Courses as c on c.courseID = ct.courseID");
            sb.AppendLine("      inner join RequirementArea as cra on c.requirementAreaID = cra.requirementAreaID");
            sb.AppendLine("   where gs.forCredit = @BoolValue");
            sb.AppendLine(")");
            return sb;
        }

        public static StringBuilder getFillStudentRequirementTableSql(int StudentDegreeID)
        {
            /// I added "courses" - but deleted the "Needed"-- calculate this in printout or datagrid ?
            StringBuilder sb = getCTETranscriptSQL(StudentDegreeID, "True");
            sb.AppendLine(" Select grt.cReqType as ReqType, grt.reqTypeDK as eReqTYpe, ra.reqArea as ReqArea, ra.eReqArea as eReqArea, dm.delMethName as DelMethName,");
            sb.AppendLine("  dm.eDelMethName as eDelMethName, dm.deliveryLevel as rDeliveryLevel, gr.reqUnits as Required, gr.creditLimit as Limit");
            sb.AppendLine("  , (Select Count(stTrans.sCredits) From stTrans ");
            sb.AppendLine("   Where stTrans.eCredits = 'True'  ");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  ) ");
            sb.AppendLine("  as Courses    ");
            sb.AppendLine("  ,");
            sb.AppendLine("  CASE");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'credits' or LOWER(grt.reqTypeDK) = 'hours' THEN");
            sb.AppendLine("  ISNULL((Select Sum(stTrans.sCredits) From stTrans ");
            sb.AppendLine("   Where stTrans.eCredits = 'True'  ");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  ), 0) ");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'qpa'THEN");
            sb.AppendLine("  ISNULL((Select Sum(stTrans.sCredits) From stTrans ");
            sb.AppendLine("   Where stTrans.eCredits = 'True'  ");
            sb.AppendLine("    AND stTrans.cCreditsInQPA = 'True'");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  ), 0) ");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'times' THEN 0");
            sb.AppendLine("  Else 0 ");
            sb.AppendLine("  END as Earned    ");
            sb.AppendLine("  , ");
            sb.AppendLine("  CASE");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'credits' or LOWER(grt.reqTypeDK) = 'hours' THEN");
            sb.AppendLine("  ISNULL((Select Sum(stTrans.sCredits) From stTrans ");
            sb.AppendLine("   Where stTrans.cGrade = 'NG'  ");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  ), 0) ");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'times' THEN");
            sb.AppendLine("  (Select Count(stTrans.sCredits) From stTrans ");
            sb.AppendLine("   Where stTrans.cGrade = 'NG'  ");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  )");
            sb.AppendLine("  WHEN LOWER(grt.reqTypeDK) = 'qpa' THEN");
            sb.AppendLine("  ISNULL((Select Sum(stTrans.sCredits * stTrans.cGradeQP) From stTrans ");
            sb.AppendLine("   Where stTrans.eCredits = 'True'");
            sb.AppendLine("    AND stTrans.cCreditsInQPA = 'True'");
            sb.AppendLine("    AND stTrans.cDegreeLevel >= rLevel.degreeLevel");
            sb.AppendLine("    AND(ra.Ancestors = 'ALL' or stTrans.cReqArea = ra.reqArea");
            sb.AppendLine("     or Exists(Select value From string_split(stTrans.cReqAncestors, ',') Where value = ra.ReqArea))");
            sb.AppendLine("  ), 0) ");
            sb.AppendLine("  ELSE 0");
            sb.AppendLine("  END as InProgress  ");
            sb.AppendLine("  ,");
            sb.AppendLine("  ra.zOrder");
            sb.AppendLine(" ");
            sb.AppendLine("  From GradRequirements as gr ");
            sb.AppendLine("   Inner Join RequirementArea ra on gr.reqAreaID = ra.requirementAreaID");
            sb.AppendLine("   Inner Join GradRequirementType as grt on gr.gradReqTypeID = grt.gradReqTypeID");
            sb.AppendLine("   Inner Join DeliveryMethod as dm on gr.rDeliveryMethodID = dm.deliveryMethodID");
            sb.AppendLine("   Inner Join Degrees as rDegree on gr.degreeID = rDegree.degreeID");
            sb.AppendLine("   Inner Join DegreeLevel as rLevel on rDegree.degreeLevelID = rLevel.degreeLevelID");
            sb.AppendLine("  where gr.degreeID = @sDegree_ID AND gr.handbookID = @sHandbook_ID");
            sb.AppendLine("  ORDER BY ra.zOrder ");
            return sb;
        }


    }
}

