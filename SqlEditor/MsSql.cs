using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace SqlEditor

{
    public class MsSqlWithDaDt
    {
        public MsSqlWithDaDt(string sqlString)
        {
            errorMsg = MsSql.FillDataTable(da, dt, sqlString);
        }
        public SqlDataAdapter da = new SqlDataAdapter();
        public DataTable dt = new DataTable();
        public string errorMsg = string.Empty;
    }


    public static class MsSql
    {
        public static bool BackupCurrentDatabase(string completeFilePath)
        {
            if (cn == null) { return false; }  // Also check this in call.

            bool success = false;
            var query = String.Format("BACKUP DATABASE [{0}] TO DISK='{1}'", cn.Database, completeFilePath);
            string result = string.Empty;
            result = ExecuteNonQuery(query);
            success = (result == string.Empty);
            return success;
        }

        // Properties
        public static string databaseType = "MsSql";
        public static string trueString = "True";
        public static string falseString = "False";

        // Two connections
        public static SqlConnection cn { get; set; }
        public static SqlConnection noDatabaseConnection { get; set; }

        // Three sql dataAdapters
        public static SqlDataAdapter currentDA { get; set; }

        // Used in editing dropdown combo.
        public static SqlDataAdapter comboDA { get; set; }

        // Used for all Data Tables that don't have own DataAdaptor - see "GetDataAdaptor" below 
        public static SqlDataAdapter readOnlyDA { get; set; }  // No update of table and so no need to keep adaptar, etc.

        // Methods
        private static SqlDataAdapter GetDataAdaptor(DataTable dataTable)
        {
            if (dataTable == dataHelper.currentDT)
            {
                if (currentDA == null) { currentDA = new SqlDataAdapter(); }
                return currentDA;
            }
            else if (dataTable == dataHelper.comboDT)
            {
                if (comboDA == null) { comboDA = new SqlDataAdapter(); }
                return comboDA;
            }
            else
            {
                readOnlyDA = new SqlDataAdapter();  // Always return a new one - can't use within a use.  
                return readOnlyDA;
            }
        }

        // Set update command - only one set field and the where is for PK=@PK - i.e. only one row        
        public static void SetUpdateCommand(List<field> fieldsToSet, DataTable dataTable)
        {
            if (fieldsToSet.Count > 0)  // Should always be true
            {
                // Get data adapter
                SqlDataAdapter da = GetDataAdaptor(dataTable);
                SetUpdateCommand(fieldsToSet, da);
            }
        }
        public static void SetUpdateCommand(List<field> fieldsToSet, SqlDataAdapter da)
        {
            SqlCommand sqlCmd = new SqlCommand();
            // Get primary key field and add it as parameter
            field pkFld = dataHelper.getTablePrimaryKeyField(fieldsToSet[0].table);
            string PK = pkFld.fieldName;
            sqlCmd.Parameters.Add("@" + PK, SqlDbType.Int, 4, PK);
            // Get update query String
            List<string> setList = new List<string>();
            foreach (field fieldToSet in fieldsToSet)
            {
                SqlDbType sqlDbType = GetSqlDbType(fieldToSet.dbType);
                int size = fieldToSet.size;
                setList.Add(String.Format("{0} = @{1}", fieldToSet.fieldName, fieldToSet.fieldName));
                sqlCmd.Parameters.Add("@" + fieldToSet.fieldName, sqlDbType, size, fieldToSet.ColumnName);
            }
            string sqlUpdate = String.Format("UPDATE {0} SET {1} WHERE {2} = {3}", fieldsToSet[0].table, String.Join(",", setList), PK, "@" + PK);
            sqlCmd.CommandText = sqlUpdate;
            sqlCmd.Connection = MsSql.cn;
            da.UpdateCommand = sqlCmd;

        }

        // Set delete command - one parameter: the primary key of the row to delete 
        public static void SetDeleteCommand(string tableName, DataTable dataTable)
        {
            // Do this once in the program
            string msg = string.Empty;
            // Get data adapter
            SqlDataAdapter da = GetDataAdaptor(dataTable);
            SetDeleteCommand(tableName, da);
        }
        public static void SetDeleteCommand(string tableName, SqlDataAdapter da)
        {
            string PK = dataHelper.getTablePrimaryKeyField(tableName).fieldName;
            string sqlUpdate = String.Format("DELETE FROM {0} WHERE {1} = {2}", tableName, PK, "@" + PK);
            SqlCommand sqlCmd = new SqlCommand(sqlUpdate, MsSql.cn);
            sqlCmd.Parameters.Add("@" + PK, SqlDbType.Int, 4, PK);
            da.DeleteCommand = sqlCmd;
        }

        // Set insert command - one parameter for each enabled combo field
        public static void SetInsertCommand(string tableName, List<where> whereList, DataTable dataTable)
        {
            SqlDataAdapter da = GetDataAdaptor(dataTable);
            SetInsertCommand(tableName, whereList, da);
        }
        public static void SetInsertCommand(string tableName, List<where> whereList, SqlDataAdapter da)
        {

            if (whereList.Count == 0)
            {
                string sqlString = String.Format("INSERT INTO {0} DEFAULT VALUES", tableName);
                SqlCommand sqlCmd = new SqlCommand(sqlString, MsSql.cn);
                da.InsertCommand = sqlCmd;
            }
            else
            {
                // Get the insert command and attach to adapter
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO ");
                sb.Append(tableName + " (");
                List<string> strFieldList = new List<string>();
                foreach (where wh in whereList)
                {
                    strFieldList.Add(wh.fl.fieldName);
                }
                string strFieldNames = String.Join(", ", strFieldList);
                sb.Append(strFieldNames);
                sb.Append(") VALUES (");
                string strParamFieldNames = "@" + String.Join(", @", strFieldList);
                sb.Append(strParamFieldNames + ")");
                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), MsSql.cn);

                // Add parameters
                foreach (where wh in whereList)
                {
                    SqlDbType sqlDbType = GetSqlDbType(wh.fl.dbType);
                    sqlCmd.Parameters.Add("@" + wh.fl.fieldName, sqlDbType, wh.fl.size, wh.fl.fieldName).Value = wh.whereValue;
                }
                da.InsertCommand = sqlCmd;
            }
        }

        public static SqlDbType GetSqlDbType(DbType dbType)
        {
            string strDbType = dbType.ToString();
            string strSqlDbType = string.Empty;
            switch (strDbType.ToLower())
            {
                case "int64":
                    strSqlDbType = "BigInt";
                    break;
                case "boolean":
                    strSqlDbType = "Bit";
                    break;
                case "ansistringfixedlength":
                    strSqlDbType = "Char";
                    break;
                case "double":
                    strSqlDbType = "Float";
                    break;
                case "int32":
                    strSqlDbType = "Int";
                    break;
                case "stringfixedlength":
                    strSqlDbType = "NChar";
                    break;
                case "string":
                    strSqlDbType = "NVarChar";
                    break;
                case "single":
                    strSqlDbType = "Real";
                    break;
                case "int16":
                    strSqlDbType = "SmallInt";
                    break;
                case "object":
                    strSqlDbType = "Variant";
                    break;
                case "byte":
                    strSqlDbType = "TinyInt";
                    break;
                case "guid":
                    strSqlDbType = "UniqueIdentifier";
                    break;
                case "binary":
                    strSqlDbType = "VarBinary";
                    break;
                case "ansistring":
                    strSqlDbType = "VarChar";
                    break;
                default:
                    strSqlDbType = strDbType;
                    break;
            }
            SqlDbType sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), strSqlDbType, true);
            return sqlDbType;
        }

        public static List<string> defaultConnectionStrings()
        {
            List<string> defaultStrings = new List<string>();
            // Don't include any spaces before or afer "=" or ";".
            defaultStrings.Add("Sql-database - chose windows or sql-authentication: ");
            defaultStrings.Add("Server={0};Database={1};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False");
            defaultStrings.Add("Server={0};Database={1};User id={2};Password={3};MultipleActiveResultSets=true;TrustServerCertificate=True");
            defaultStrings.Add("Azure database with Sql-authentication: ");
            defaultStrings.Add("Server={0};Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3}; MultipleActiveResultSets=True; Encrypt=True; TrustServerCertificate=False; Connection Timeout=30");
            defaultStrings.Add("Recent connections: ");
            return defaultStrings;
        }

        public static string GetFetchString(int offset, int pageSize)
        {
            return " OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + pageSize.ToString() + " ROWS ONLY";
        }

        public static void openConnection(string connectionString)
        {
            if (cn == null)   // may be false if using frmConnection
            {
                cn = new SqlConnection(connectionString);
            }
            if (cn.State != ConnectionState.Open)
            {
                cn.Open();  // May cause an error.  Message will be in 
            }
        }

        public static void openNoDatabaseConnection(string connectionString)
        {
            if (noDatabaseConnection == null)   // may be false if using frmConnection
            {
                noDatabaseConnection = new SqlConnection(connectionString);
            }
            if (noDatabaseConnection.State != ConnectionState.Open)
            {
                noDatabaseConnection.Open();
            }
        }

        public static void CloseConnection()
        {
            if (cn != null)
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
            cn = null;
        }

        public static void CloseDataAdapters()
        {
            currentDA = null;
            comboDA = null;
            readOnlyDA = null;
        }

        public static void CloseNoDatabaseConnection()
        {
            if (noDatabaseConnection != null)
            {
                if (noDatabaseConnection.State == ConnectionState.Open)
                {
                    noDatabaseConnection.Close();
                }
            }
            noDatabaseConnection = null;
        }

        public static int GetRecordCount(string strSql)
        {
            int result = 0;
            using (SqlCommand cmd = new SqlCommand(strSql, cn))
            {
                result = (int)cmd.ExecuteScalar();
            }
            return result;
        }

        public static string DeleteRowsFromDT(DataTable dt, where wh)   // Doing 1 where only, usually the PK & value
        {
            try
            {
                DataRow[] drs = dt.Select(string.Format("{0} = {1}", wh.fl.fieldName, wh.whereValue));
                foreach (DataRow dr in drs)
                {
                    // Delete datarow from dataTable
                    dr.Delete();
                }
                DataRow[] drArray = new DataRow[drs.Count()];
                for (int i = 0; i < drArray.Length; i++)
                {
                    drArray[i] = drs[i];
                }
                // Delete Command (set above) uses PK field of each dr to delete - should be first column of dt
                // Again, the dt might have inner joins but these are ignored.  Delete acts on the PK of main table.
                // A pledge - the dataTable must have an adaptor and its deleteCommand must be set
                MsSql.GetDataAdaptor(dt).Update(drArray);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
                Console.Beep();
            }
        }

        public static void testing()
        {
            string query = "GetStudentRequirementTable";
            CommandType commandType = CommandType.StoredProcedure;
            List<(string, string)> parameters = new List<(string, string)>();
            SqlParameter sqlPar1 = new SqlParameter("@StudentDegreeID", "2634");
            parameters.Add(("@StudentDegreeID", "2634"));
            int rowsAffected = 0;
            string error = FillDataTable(dataHelper.testingDT, query, parameters, commandType);
            if (string.IsNullOrEmpty(error)) { error = "No error"; }
            MessageBox.Show(error + " Rows affected: " + rowsAffected.ToString());
        }

        public static string FillDataTable(DataTable dt, string sqlString)
        {
            SqlDataAdapter da = GetDataAdaptor(dt);  // Returns readOnlyDA if not current or combo
            return FillDataTable(da, dt, sqlString);
        }

        public static string FillDataTable(SqlDataAdapter da, DataTable dt, string sqlString)
        {
            CommandType commandType = CommandType.Text;
            List<(string, string)> parameters = new List<(string, string)>();
            return FillDataTable(da, dt, sqlString, parameters, commandType);
        }

        public static string FillDataTable(DataTable dt, string sqlString, List<(string, string)> parameters, CommandType cmdType)
        {
            SqlDataAdapter da = GetDataAdaptor(dt);  // Returns readOnlyDA if dt is not currentDT or comboDT
            return FillDataTable(da, dt, sqlString, parameters, cmdType);
        }

        public static string FillDataTable(SqlDataAdapter da, DataTable dt, string sqlString, List<(string, string)> parameters, CommandType cmdType)
        {

            SqlCommand sqlCmd = new SqlCommand(sqlString, cn);
            sqlCmd.CommandType = cmdType;
            foreach ((string, string) parameter in parameters)
            {
                sqlCmd.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
            }
            da.SelectCommand = sqlCmd;
            try
            {
                da.Fill(dt);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message + "  SqlString: " + sqlString;
            }
        }

        public static string ExecuteNonQuery(string query)
        {
            int rowsAffected = 0;
            return ExecuteNonQuery(query, ref rowsAffected);
        }

        public static string ExecuteNonQuery(string query, ref int rowsAffected)
        {
            List<(string, string)> parameters = new List<(string, string)>();
            return ExecuteNonQuery(query, parameters, CommandType.Text, ref rowsAffected);
        }

        public static string ExecuteNonQuery(string query, List<(string, string)> parameters, CommandType cmdType, ref int rowsAffected)
        {
            string result = String.Empty;
            try
            {
                // Create command object
                using (var command = new SqlCommand(query, cn))
                {
                    // Set command type
                    command.CommandType = cmdType;
                    // Add parameters
                    foreach ((string, string) parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                    }
                    // Execute command
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                MessageBox.Show(ex.Message, Properties.MyResources.errorBackingUpDatabase, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
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

        public static string CreateForeignKey(string table, string field, string refTable, string refField)
        {
            string result = String.Empty;
            string constraintName = String.Format("FK_{0}_{1}_{2}", table, field, refTable);
            var query = String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY({2}) REFERENCES {3}({4}) ON DELETE CASCADE ON UPDATE CASCADE",
                table, constraintName, field, refTable, refField);
            result = ExecuteNonQuery(query);
            return result;
        }

        public static string CreateUniqueIndex(string table, List<string> fields, string indexName)
        {
            string result = String.Empty;
            string withClause = "WITH(DROP_EXISTING = ON)";
            if (String.IsNullOrEmpty(indexName))
            {
                withClause = "WITH(DROP_EXISTING = OFF)";
                indexName = String.Format("DK_{0}", table);
            }
            string columns = String.Join(",", fields);
            // "CREATE UNIQUE NONCLUSTERED INDEX {0} ON {1}({2}){3}";
            var query = String.Format("CREATE UNIQUE NONCLUSTERED INDEX {0} ON {1}({2}){3}",
                indexName, table, columns, withClause);
            result = ExecuteNonQuery(query);
            return result;
        }

        public static string initializeDatabaseInformationTables()
        {
            // foreignKeysDT
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT sfk.name, OBJECT_NAME(sfk.parent_object_id) FkTable, ");
            sb.Append("COL_NAME(sfkc.parent_object_id, sfkc.parent_column_id) FkColumn, ");
            sb.Append("OBJECT_NAME(sfk.referenced_object_id) RefTable, ");
            sb.Append("COL_NAME(sfkc.referenced_object_id, sfkc.referenced_column_id) RefPkColumn ");
            sb.Append("FROM  sys.foreign_keys sfk INNER JOIN sys.foreign_key_columns sfkc ");
            sb.Append("ON sfk.OBJECT_ID = sfkc.constraint_object_id ");
            sb.Append("INNER JOIN sys.tables t ON t.OBJECT_ID = sfkc.referenced_object_id");
            string sqlForeignKeys = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlForeignKeys, cn);
            readOnlyDA.Fill(dataHelper.foreignKeysDT);

            // Indexes - note: si.index_id = 0 if index is a heap (no columns)
            sb.Clear();
            sb.Append("select OBJECT_NAME(so.object_id) as TableName, si.[name] as IndexName, ");
            sb.Append("si.is_primary_key as is_PK, si.is_unique as _unique, ");
            sb.Append("(SELECT COUNT(*) FROM sys.index_columns sic ");
            sb.Append("WHERE si.object_id = sic.object_id AND si.index_id = sic.index_id ) as ColCount ");
            sb.Append("from sys.objects so  ");
            sb.Append("inner join sys.indexes si on so.object_id = si.object_id ");
            sb.Append("where so.is_ms_shipped <> 1 AND so.type = 'U' AND si.index_id > 0 ");
            sb.Append("order by TableName ");
            string sqlIndexes = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlIndexes, cn);
            readOnlyDA.Fill(dataHelper.indexesDT);

            // IndexColumns
            sb.Clear();
            sb.Append("SELECT OBJECT_NAME(so.object_id) as TableName, si.name as IndexName, COL_NAME(si.object_id, sic.column_id) as ColumnName, ");
            sb.Append("si.is_primary_key as is_PK, si.is_unique as _unique ");
            sb.Append("FROM sys.objects so ");
            sb.Append("inner join sys.indexes si on so.object_id = si.object_id ");
            sb.Append("inner join sys.index_columns sic on si.object_id = sic.object_id AND si.index_id = sic.index_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 and so.type = 'U' AND si.index_id > 0 ");
            sb.Append("ORDER BY TableName, is_PK desc, IndexName ");
            string sqlIndexColumns = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlIndexColumns, cn);
            readOnlyDA.Fill(dataHelper.indexColumnsDT);

            // Tables
            sb.Clear();
            sb.Append("SELECT so.name as TableName , ");
            sb.Append("so.name as TableDisplayName , ");
            sb.Append("st.max_column_id_used as ColNum, ");
            sb.Append("st.create_date as Created, st.modify_date as Modified, '' as DK_Index, 0 as Hidden ");
            sb.Append("FROM sys.objects so inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            sb.Append("ORDER BY TableName ");
            string sqlTables = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlTables, cn);
            readOnlyDA.Fill(dataHelper.tablesDT);

            // Fields - do this last
            sb.Clear();
            sb.Append("SELECT so.name as TableName, ");
            sb.Append("sc.column_id as ColNum,  ");
            sb.Append("Col_Name(so.object_id, sc.column_id) as ColumnName, ");
            sb.Append("Col_Name(so.object_id, sc.column_id) as ColumnDisplayName, ");
            sb.Append("TYPE_NAME(sc.system_type_id) as DataType, ");
            sb.Append("sc.is_nullable as Nullable, ");
            sb.Append("sc.is_identity as _identity, ");
            sb.Append("CAST('0' as bit) as is_PK, CAST('0' as bit) as is_FK, CAST('0' as bit) as is_DK, ");
            sb.Append("sc.max_length as MaxLength, ");
            sb.Append("'' as RefTable, '' as RefPkColumn, 0 as Width ");
            sb.Append("FROM sys.objects so inner join sys.columns sc on so.object_id = sc.object_id ");
            sb.Append("inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            sb.Append("ORDER BY ColNum ");
            string sqlFields = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlFields, cn);
            readOnlyDA.Fill(dataHelper.fieldsDT);
            string errorNotice = String.Empty;
            errorNotice = dataHelper.updateTablesDTtableOnProgramLoad(); // fills in "DK_Index"
            dataHelper.updateFieldsDTtableOnProgramLoad(); // fills in many columns that are used in the rest of the program
            return errorNotice;
        }

    }
}
