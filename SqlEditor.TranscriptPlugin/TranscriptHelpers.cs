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

        public static void fillGradRequirementsDT(int studentDegreeID, ref StringBuilder sbErrors)
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
                dataHelper.AddRowToFieldsDT("StudentReq", 11, "eReqType", "eReqType", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                //RequirementArea table - 
                dataHelper.AddRowToFieldsDT("StudentReq", 3, "ReqArea", "ReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 13, "eReqArea", "eReqArea", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                // DeliveryMethodTable
                dataHelper.AddRowToFieldsDT("StudentReq", 4, "DelMethName", "DelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 15, "eDelMethName", "eDelMethName", "nvarchar", false, false, false, false, true, 200, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 16, "rDeliveryLevel", "rDeliveryLevel", "int", false, false, false, false, true, 4, String.Empty, String.Empty, 0);
                // GradRequirement Table itself
                dataHelper.AddRowToFieldsDT("StudentReq", 5, "Required", "Required", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 17, "Limit", "Limit", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                // Need to calucate the following from transcript
                dataHelper.AddRowToFieldsDT("StudentReq", 6, "Courses", "Courses", "int", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 7, "Earned", "Earned", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 8, "Needed", "Needed", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                dataHelper.AddRowToFieldsDT("StudentReq", 9, "InProgress", "InProgress", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
                //                dataHelper.AddRowToFieldsDT("StudentReq", 18, "Icredits", "Icredits", "real", false, false, false, false, false, 4, String.Empty, String.Empty, 0);
            }

            // 2.  Get the Student Requirements table via SQL
            StringBuilder sb = MsSql.getFillStudentRequirementTableSql(studentDegreeID);
            DataTable requirementsDT = new DataTable();
            String sqlString = sb.ToString();
            MsSql.FillDataTable(requirementsDT, sqlString);

            // 3. Create studentReqDT and add a column for each field in StudentReq pseudo table.
            PrintToWord.studentReqDT = new System.Data.DataTable();
            //3a. Create sqlFactory for StudentReq
            SqlFactory sqlStudentReq = new SqlFactory("StudentReq", 0, 0);
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

            // 3b. Transfer requirementsDT into PrintToWord.studentReqDT
            foreach (DataRow requirementsDR in requirementsDT.Rows)
            {
                Decimal required = Decimal.Parse(requirementsDR["Required"].ToString());
                Decimal earned = Decimal.Parse(requirementsDR["Earned"].ToString());
                Decimal inprogress = Decimal.Parse(requirementsDR["InProgress"].ToString());

                // Adjust values for qpa - Earned is credits and InProgress is the quality points earned
                if (requirementsDR["ReqType"].ToString().ToLower() == "qpa")
                {
                    if (inprogress > 0)
                    {
                        Decimal qpa = earned / inprogress;
                        qpa = Math.Round(qpa, 2);
                        earned = qpa;
                        inprogress = 0;
                    }
                    else
                    {
                        earned = 0;
                        inprogress = 0;
                    }
                }
                Decimal needed = Math.Max(0, required - earned);

                DataRow dr = PrintToWord.studentReqDT.NewRow();
                // "StudentReqID" is the Primary key and column set to autoincrement.
                dataHelper.setColumnValueInDR(dr, "Required", required);
                dataHelper.setColumnValueInDR(dr, "Limit", requirementsDR["Limit"].ToString());
                dataHelper.setColumnValueInDR(dr, "ReqArea", requirementsDR["ReqArea"].ToString());
                dataHelper.setColumnValueInDR(dr, "eReqArea", requirementsDR["eReqArea"].ToString());

                dataHelper.setColumnValueInDR(dr, "DelMethName", requirementsDR["DelMethName"].ToString());
                dataHelper.setColumnValueInDR(dr, "eDelMethName", requirementsDR["eDelMethName"].ToString());
                dataHelper.setColumnValueInDR(dr, "rDeliveryLevel", requirementsDR["rDeliveryLevel"].ToString());
                dataHelper.setColumnValueInDR(dr, "ReqType", requirementsDR["ReqType"].ToString());

                dataHelper.setColumnValueInDR(dr, "Courses", requirementsDR["Courses"].ToString());
                dataHelper.setColumnValueInDR(dr, "Earned", earned);
                dataHelper.setColumnValueInDR(dr, "InProgress", inprogress);
                dataHelper.setColumnValueInDR(dr, "Needed", needed);

                //Add this row to the table
                PrintToWord.studentReqDT.Rows.Add(dr);

                //if (requirementsDR["ReqArea"].ToString() == "QPA")
                //{
                //    decimal QPA = 0;
                //    if (qpaCredits > 0)
                //    {
                //        QPA = Math.Round(qpaPoints / qpaCredits, 2);
                //    }
                //    dataHelper.setColumnValueInDR(dr, "Earned", QPA);
                //    dataHelper.setColumnValueInDR(dr, "InProgress", 0);
                //    if (QPA < rReqCredits)
                //    {
                //        dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - QPA);
                //    }
                //}
                //else
                //{
                //    // Fill in columns 
                //    dataHelper.setColumnValueInDR(dr, "Earned", earned);
                //    dataHelper.setColumnValueInDR(dr, "InProgress", inProgress);
                //    if (rReqCredits > earned)
                //    {
                //        dataHelper.setColumnValueInDR(dr, "Needed", rReqCredits - earned);
                //    }
                //    else
                //    {
                //        dataHelper.setColumnValueInDR(dr, "Needed", 0);
                //    }
                //    dataHelper.setColumnValueInDR(dr, "Icredits", iCredits);
                //}
                //// Add this row to studentReqDT
                ////PrintToWord.studentReqDT.Rows.Add(dr);
            }


            //    if (fakeRow) { break; }



            //// Update student QPA and total Credits - not yet committed to database
            //DataRow studentDegreeInfoRow = PrintToWord.studentDegreeInfoDT.Rows[0];
            //decimal totalQPA = 0;
            //if (totalQpaCredits > 0)
            //{
            //    totalQPA = Math.Round(totalQpaPoints / totalQpaCredits, 2);
            //}
            //dataHelper.setColumnValueInDR(studentDegreeInfoRow, "creditsEarned", totalCreditsEarned);
            //dataHelper.setColumnValueInDR(studentDegreeInfoRow, "QPA", totalQPA);
            //dataHelper.setColumnValueInDR(studentDegreeInfoRow, "lastUpdated", DateTime.Now.ToShortDateString());

            //// Save these three changes down to the database - start over from scratch.
            //field pkField = dataHelper.getTablePrimaryKeyField(TableName.studentDegrees);
            //string pk = dataHelper.getColumnValueinDR(studentDegreeInfoRow, pkField.fieldName);
            //sqlString = String.Format("Select * from {0} where {1} = '{2}'", TableName.studentDegrees, pkField.fieldName, pk);
            //MsSqlWithDaDt dadt = new MsSqlWithDaDt(sqlString);
            //List<field> fieldsToUpdate = new List<field>();
            //fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "creditsEarned"));
            //fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "QPA"));
            //fieldsToUpdate.Add(dataHelper.getFieldFromFieldsDT(TableName.studentDegrees, "lastUpdated"));
            //MsSql.SetUpdateCommand(fieldsToUpdate, dadt.da);
            //dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "creditsEarned", totalCreditsEarned);
            //dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "QPA", totalQPA);
            //dataHelper.setColumnValueInDR(dadt.dt.Rows[0], "lastUpdated", DateTime.Now.ToShortDateString());
            //try { dadt.da.Update(dadt.dt); } catch { }

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
            columnHeaderTranslations.Add("earned", "以得到");
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
            columnHeaderTranslations.Add("limit", "限制");
            columnHeaderTranslations.Add("needed", "還需要得到");
            columnHeaderTranslations.Add("note", "說明");
            columnHeaderTranslations.Add("othercredits", "別的學分");
            columnHeaderTranslations.Add("province", "省市");
            columnHeaderTranslations.Add("provincename", "省市名字");
            columnHeaderTranslations.Add("rdeliverymethod", "r上課方法");
            columnHeaderTranslations.Add("repeatspermitted", "可重複");
            columnHeaderTranslations.Add("reqarea", "規則範圍");
            columnHeaderTranslations.Add("reqtype", "規則類別");
            columnHeaderTranslations.Add("required", "畢業要求");
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
            columnHeaderTranslations.Add("check for transcript errors", "檢查成績單是否有錯誤");

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
