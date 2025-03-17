using System.Data;
using System.Globalization;
using System.Text;

namespace SqlEditor
{

    public static class dataHelper
    {
        // Variables
        public static DataTable currentDT { get; set; }   //Data Table that is bound to the datagrid
        public static DataTable tablesDT { get; set; }
        public static DataTable fieldsDT { get; set; }
        public static DataTable foreignKeysDT { get; set; }
        public static DataTable indexesDT { get; set; }
        public static DataTable indexColumnsDT { get; set; }
        public static DataTable comboDT { get; set; }
        public static DataTable lastFilterValuesDT { get; set; }
        public static DataTable testingDT { get; set; }

        public static void initializeDataTables()
        {
            currentDT = new DataTable("currentDT");  // Never use the names, but helpful in debugging
            tablesDT = new DataTable("tablesDT");
            fieldsDT = new DataTable("fieldsDT");
            foreignKeysDT = new DataTable("foreighKeysDT");
            indexesDT = new DataTable("indexesDT");
            indexColumnsDT = new DataTable("indexColumnsDT");
            comboDT = new DataTable("comboDT");
            testingDT = new DataTable("testingDT");
            lastFilterValuesDT = new DataTable("lastFilterValuesDT");
            // Add columns to lastFilterValuesDt
            LastFilterValuesDT_AddColumns();
        }

        private static void LastFilterValuesDT_AddColumns()
        {
            DataColumn dataColumn = new DataColumn("Table");
            lastFilterValuesDT.Columns.Add(dataColumn);
            dataColumn = new DataColumn("Mode");
            lastFilterValuesDT.Columns.Add(dataColumn);
            // Commment out 2025.02
            // dataColumn = new DataColumn("cMF");
            // lastFilterValuesDT.Columns.Add(dataColumn);
            for (int i = 0; i <= 8; i++)
            {
                DataColumn col1 = new DataColumn("cGFF" + i.ToString());
                DataColumn col2 = new DataColumn("cGFV" + i.ToString());
                lastFilterValuesDT.Columns.Add(col1);
                lastFilterValuesDT.Columns.Add(col2);
            }
            DataColumn dataColumn3 = new DataColumn("cCTL");
            lastFilterValuesDT.Columns.Add(dataColumn3);
            for (int i = 0; i <= 5; i++)
            {
                DataColumn col1 = new DataColumn("lCFF" + i.ToString());
                DataColumn col2 = new DataColumn("cCFV" + i.ToString());
                lastFilterValuesDT.Columns.Add(col1);
                lastFilterValuesDT.Columns.Add(col2);
            }
            DataColumn dataColumn4 = new DataColumn("manualFilter");
            lastFilterValuesDT.Columns.Add(dataColumn4);
        }

        public static string QualifiedFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.table + "]");
            sb.Append(".");
            sb.Append("[" + fld.fieldName + "]");
            return sb.ToString();
        }

        public static string QualifiedAliasFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.tableAlias + "]");
            sb.Append(".");
            sb.Append("[" + fld.fieldName + "]");
            return sb.ToString();
        }

        public static DbType ConvertStringToDbType(string strVarType)
        {
            string strDbType = string.Empty;
            //  strVarType is sqlDbType but could also be mySql - just need to expand cases
            switch (strVarType.ToLower())
            {
                case "bigint":
                    strDbType = "Int64";
                    break;
                case "bit":
                    strDbType = "Boolean";
                    break;
                case "char":
                    strDbType = "AnsiStringFixedLength";
                    break;
                case "float":
                    strDbType = "Double";
                    break;
                case "int":
                    strDbType = "Int32";
                    break;
                case "nchar":
                    strDbType = "StringFixedLength";
                    break;
                case "nvarchar":
                    strDbType = "String";
                    break;
                case "real":
                    strDbType = "Single";
                    break;
                case "smallint":
                    strDbType = "Int16";
                    break;
                case "variant":
                    strDbType = "Object";
                    break;
                case "tinyint":
                    strDbType = "Byte";
                    break;
                case "uniqueidentifier":
                    strDbType = "Guid";
                    break;
                case "varbinary":
                    strDbType = "Binary";
                    break;
                case "varchar":
                    strDbType = "AnsiString";
                    break;
                default:
                    strDbType = strVarType;   // SqlDbType and DbType have same name
                    break;
            }
            DbType dbType = (DbType)Enum.Parse(typeof(DbType), strDbType, true);
            return dbType;
        }

        public static System.Type ConvertDbTypeToType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Boolean:
                    return typeof(bool);

                case DbType.Byte:
                    return typeof(byte);

                case DbType.StringFixedLength:
                    return typeof(char);

                case DbType.DateTime:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Double:
                    return typeof(double);
                case DbType.Int16:
                    return typeof(short);
                case DbType.Int32:
                    return typeof(int);
                case DbType.Int64:
                    return typeof(long);
                case DbType.Object:
                    return typeof(object);
                case DbType.SByte:
                    return typeof(sbyte);
                case DbType.Single:
                    return typeof(Single);
                case DbType.String:
                    return typeof(string);
                default:
                    return typeof(string);
            }
        }
        public static DbTypeType GetDbTypeType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Boolean:
                    return DbTypeType.isBoolean;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                case DbType.SByte:   // -127 to 127 - signed byte
                    return DbTypeType.isInteger;
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                    return DbTypeType.isDecimal;
                case DbType.Date:
                case DbType.DateTimeOffset:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Time:
                    return DbTypeType.isDateTime;
                case DbType.Binary:    // Not handled in program
                    return DbTypeType.isBinary;
                case DbType.String:
                case DbType.AnsiString:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return DbTypeType.isString;
            }
            return DbTypeType.isString;  // Not checking anything else; just a guess
        }

        public static string errMsg = String.Empty;
        public static string errMsgParameter1 = String.Empty;
        public static string errMsgParameter2 = String.Empty;
        private static Dictionary<ProgramMode, string> ModetoString = new Dictionary<ProgramMode, String>
        {
            [ProgramMode.add] = "add",
            [ProgramMode.delete] = "delete",
            [ProgramMode.edit] = "edit",
            [ProgramMode.none] = "none",
            [ProgramMode.view] = "view",
        };

        public static bool TryParseToDbType(string str, DbType dbType)
        {
            DbTypeType dbTypeType = GetDbTypeType(dbType);
            switch (dbTypeType)
            {
                case DbTypeType.isInteger:
                    int i;
                    if (int.TryParse(str, out i))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "integer";
                        return false;
                    }
                case DbTypeType.isDecimal:
                    Decimal dec;
                    if (Decimal.TryParse(str, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dec))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "decimal";
                        return false;
                    }
                case DbTypeType.isDateTime:
                    DateTime dt;
                    if (DateTime.TryParse(str, out dt))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "Date / Time";
                        return false;
                    }
                case DbTypeType.isBoolean:
                    Boolean tf;
                    if (Boolean.TryParse(str, out tf))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "Boolean";
                        return false;
                    }

            }
            return true;  // Not checking anything else - assuming it will be a string
        }

        public static where GetMainFilterFromPrimaryKeyValue(string table, string PKvalue)
        {
            field pkField = getTablePrimaryKeyField(table);
            where newWhere = new where(pkField, PKvalue);
            // Rest of this method is to set the "newWhere.DisplayMember" shown in mainFilter dropdown
            SqlFactory sf = new SqlFactory(table, 0, 0);
            sf.myComboWheres.Add(newWhere);
            // returnComboSql contains a "DisplayMember" column that is a comma seperated list of display fields.
            string strSql = sf.returnComboSql(pkField, false, comboValueType.PK_myTable);
            MsSqlWithDaDt dadt = new MsSqlWithDaDt(strSql);
            string displayMember = "Missing DK";  // Default - modified below
            if (dadt.dt.Rows.Count > 0)
            {
                int colIndex = dadt.dt.Columns["DisplayMember"].Ordinal;
                displayMember = dadt.dt.Rows[0][colIndex].ToString();
            }
            newWhere.DisplayMember = displayMember;
            return newWhere;
        }

        // Add a "0" to i if it is less than 10.
        public static string twoDigitNumber(int i)
        {
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
        }

        public static DataRow getDataRowFromDataTable(string column, string columnValue, DataTable dt)
        {
            DataRow dr = dt.Select(string.Format("{0} = '{1}'", column, columnValue)).FirstOrDefault();
            return dr;
        }

        #region  Getting information from FieldDT: public (internal) methods

        public static field getTablePrimaryKeyField(string table)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND is_PK", table)).FirstOrDefault();
            return getFieldFromFieldsDT(dr);
        }

        public static field getForeignKeyFromRefTableName(string myTable, string refTable)
        {   // Assumes we have checked that there is this foreignkey in FkTable - if two returns the first
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND RefTable = '{1}'", myTable, refTable)).FirstOrDefault();
            return getFieldFromFieldsDT(dr);
        }

        public static field getForeignKeyRefField(field foreignKey)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            DataRow dr = getDataRowFromFieldsDT(foreignKey.table, foreignKey.fieldName);
            string FkRefTable = getColumnValueinDR(dr, "RefTable");
            string FkRefCol = getColumnValueinDR(dr, "RefPkColumn");
            return getFieldFromFieldsDT(FkRefTable, FkRefCol);
        }

        public static DataRow getDataRowFromFieldsDT(string tableName, string columnName)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName, columnName)).FirstOrDefault();
            return dr;
        }


        public static bool isDisplayKey(field fi)
        {
            DataRow dr = getDataRowFromFieldsDT(fi.table, fi.fieldName);
            return bool.Parse(getColumnValueinDR(dr, "is_DK"));
        }

        public static bool isTablePrimaryKeyField(field fi)
        {
            DataRow dr = getDataRowFromFieldsDT(fi.table, fi.fieldName);
            return bool.Parse(getColumnValueinDR(dr, "is_PK"));
        }

        public static bool isForeignKeyField(field fi)
        {
            DataRow dr = getDataRowFromFieldsDT(fi.table, fi.fieldName);
            return bool.Parse(getColumnValueinDR(dr, "is_FK"));
        }

        public static void storeColumnWidth(string tableName, string columnName, int width)
        {
            setIntValueFieldsDT(tableName, columnName, "Width", width);
        }

        public static int getStoredWidth(string tableName, string columnName, int defaultWidth)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            int x = Int32.Parse(getColumnValueinDR(dr, "Width"));
            if (x == 0) { x = defaultWidth; }
            return x;
        }

        public static field getFieldFromFieldsDT(string tableName, string columnName)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            return getFieldFromFieldsDT(dr);
        }
        public static field getFieldFromFieldsDT(DataRow dr)
        {
            string tableName = getColumnValueinDR(dr, "TableName");
            string columnName = getColumnValueinDR(dr, "ColumnName");
            int size = Int32.Parse(getColumnValueinDR(dr, "MaxLength"));
            string strDbType = getColumnValueinDR(dr, "DataType");
            DbType dbType = dataHelper.ConvertStringToDbType(strDbType);
            return new field(tableName, columnName, dbType, size);
        }

        public static string getColumnValueinDR(DataRow dr, string columnName)
        {
            return Convert.ToString(dr[dr.Table.Columns.IndexOf(columnName)]);
        }

        public static void setColumnValueInDR(DataRow dr, string columnName, object newValue)
        {
            dr[dr.Table.Columns.IndexOf(columnName)] = newValue;
        }


        public static void AddRowToFieldsDT(string TableName, int ColNum, string ColumnName, string ColumnDisplayName
            , string DataType, bool Nullable, bool _identity, bool is_PK, bool is_FK, bool is_DK, short MaxLength
            , string RefTable, string RefPkColumn, int Width)
        {
            DataRow newRow = fieldsDT.NewRow();
            newRow[0] = TableName;
            newRow[1] = ColNum;
            newRow[2] = ColumnName;
            newRow[3] = ColumnDisplayName;
            newRow[4] = DataType;
            newRow[5] = Nullable;
            newRow[6] = _identity;
            newRow[7] = is_PK;
            newRow[8] = is_FK;
            newRow[9] = is_DK;
            newRow[10] = MaxLength;
            newRow[11] = RefTable;
            newRow[12] = RefPkColumn;
            newRow[13] = Width;
            fieldsDT.Rows.Add(newRow);
        }

        #endregion

        #region Getting information from FieldsDT and setting width: private methods

        private static void setIntValueFieldsDT(string tableName, string columnName, string columnToReturn, int value)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            dr[columnToReturn] = value;
        }

        #endregion

        #region  Loading tableDT (DK_Index) and fieldsDT (many columns) on connnection.open from other tables

        public static string updateTablesDTtableOnProgramLoad()
        {
            List<string> NoDKList = new List<string>();
            List<string> NoPKList = new List<string>();

            // Updates "DK_Index" in tablesDT
            foreach (DataRow dr in tablesDT.Rows)
            {
                // Get DK_Index for this table
                string tableName = dr["TableName"].ToString();

                // PK - used to check errors
                DataRow[] drs2 = dataHelper.indexesDT.Select(string.Format("TableName = '{0}' AND is_PK = 'True'", tableName));
                if (drs2.Count() == 0) { NoPKList.Add(tableName); }


                // DK - used in program as well as to check for errors
                DataRow[] drs = dataHelper.indexesDT.Select(string.Format("TableName = '{0}' AND _unique = 'True' AND is_PK = 'False'", tableName));
                if (drs.Count() == 0)
                {   // Table is a foreign key
                    DataRow[] drs3 = dataHelper.foreignKeysDT.Select(string.Format("RefTable = '{0}'", tableName));
                    if (drs3.Count() > 0)
                    {
                        NoDKList.Add(tableName);
                    }
                }
                else
                {
                    string indexName = string.Empty;
                    int dkRow = -1;
                    foreach (DataRow dr2 in drs)
                    {
                        indexName = dr2["IndexName"].ToString();
                        if (indexName.Length > 1)
                        {
                            if (indexName.Substring(0, 2).ToLower() == "dk")
                            {
                                dr["DK_Index"] = indexName;
                                break; // break out of inner for loop
                            }
                        }
                    }
                    // Found non-primary unique index not starting with "dk"
                    if (indexName != string.Empty)
                    {
                        dr["DK_Index"] = indexName;
                    }
                }
            }
            if (NoDKList.Count > 0 || NoPKList.Count > 0)
            {
                return "No DK: " + String.Join(", ", NoDKList) + " No PK: " + String.Join(", ", NoPKList);
            }
            else { return string.Empty; }
        }

        public static void updateFieldsDTtableOnProgramLoad()
        {
            // Updates FieldsDT from the other table.
            // Uses "Foundational" methods to put info into FieldsDT.
            // After calling this, we can get all information from FieldsDT
            // First update is_PK, is_DK, is_FK - because these used in innerjoin call below
            foreach (DataRow dr in fieldsDT.Rows)
            {
                field rowField = getFieldFromFieldsDT(dr);  // Each row in fieldsDT is a field

                //Set is_PK 
                field tablePkfield = getTablePrimaryKeyFoundational(rowField.table);
                if (rowField.fieldName == tablePkfield.fieldName)
                {
                    dr[dr.Table.Columns.IndexOf("is_PK")] = true;
                }

                // Set is_DK 
                if (isDisplayKeyFoundational(rowField.table, rowField.fieldName))
                {
                    dr[dr.Table.Columns.IndexOf("is_DK")] = true;  // Only displays itself
                }

                // Set is_FK + FkRefTable + FkRefCol + 
                if (fieldIsForeignKeyFoundational(rowField))
                {
                    dr[dr.Table.Columns.IndexOf("is_FK")] = true;  // Only displays itself
                    field refField = getForeignTablePrimaryKeyFoundational(rowField);
                    dr[dr.Table.Columns.IndexOf("RefTable")] = refField.table;  // Only displays itself
                    dr[dr.Table.Columns.IndexOf("RefPkColumn")] = refField.fieldName;  // Only displays itself
                }
            }
        }

        private static bool fieldIsForeignKeyFoundational(field fld)
        {
            DataRow[] dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", fld.table, fld.fieldName));
            if (dr.Count() > 0)  // Should be 0 or 1
            {
                return true;
            }
            return false;
        }

        private static field getTablePrimaryKeyFoundational(string table)
        {
            string strSelect = "TableName = '" + table + "' AND is_PK ='True'";
            DataRow dr = dataHelper.indexColumnsDT.Select(strSelect).FirstOrDefault();
            if (dr == null)
            {
                return new field(table, "MissingPrimaryKey", DbType.Int32, 4, fieldType.pseudo);
            }
            string columnName = dr[dr.Table.Columns.IndexOf("ColumnName")].ToString();
            return getFieldFromFieldsDT(table, columnName);
        }

        private static field getForeignTablePrimaryKeyFoundational(field rowfield)
        {
            DataRow dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", rowfield.table, rowfield.fieldName)).FirstOrDefault();
            if (dr == null)
            {
                return new field("MissingRefTable", "MissingRefColumn", DbType.Int32, 4, fieldType.pseudo);
            }
            string RefTable = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefTable")]);
            string RefPkColumn = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefPkColumn")]);
            return new field(RefTable, RefPkColumn, DbType.Int32, 4);
        }

        private static bool isDisplayKeyFoundational(string tableName, string fieldName)  // Note 'private
        {
            DataRow dr = dataHelper.tablesDT.Select(string.Format("TableName = '{0}'", tableName)).FirstOrDefault();
            if (dr != null) // always true 
            {
                string DK_Index = dr["DK_Index"].ToString();
                if (!String.IsNullOrEmpty(DK_Index))
                {
                    // May be two indexes on same column
                    DataRow[] drs = dataHelper.indexColumnsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName, fieldName));
                    foreach (DataRow dr2 in drs)
                    {
                        if (dr2["IndexName"].ToString() == DK_Index)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

    }


    #region Helper classes - field, where, innerJoin, orderBy, enums: command, ProgramMode, DbTypeType, fieldType, comboValueType

    public class field
    {
        public field(string table, string fieldName, DbType dbType, int size, fieldType fType)
        {

            // Only call this constructor with pseudoFields = true;
            this.table = table;
            this.tableAlias = table + "00";  //Default
            this.fieldName = fieldName;
            this.dbType = dbType;
            this.size = size;
            this.fType = fType;
            // this.displayMember = fieldName;  // Set below when needed
            this.ColumnName = fieldName;  // Default
        }
        public field(string table, string fieldName, DbType dbType, int size) :
            this(table, fieldName, dbType, size, fieldType.regular)
        { }

        public string fieldName { get; }
        public string table { get; set; }
        public string tableAlias { get; set; }
        public DbType dbType { get; set; }
        public int size { get; set; }
        public fieldType fType { get; set; }
        public field ValueMember { get { return this; } }  //Field itself-despite '0', used when binding Combos to fields.(?) 
        public string ColumnName { get; set; }  // Name Microsoft puts at the top of the column
        public List<field> FKancestors { get { return fkAncestors; } set { fkAncestors = value; } }
        private List<field> fkAncestors = new List<field>();
        public string displayMember
        {
            get
            {
                string lastTwoDigetOfTableAlias = tableAlias.Substring(tableAlias.Length - 2);
                if (fType == fieldType.pseudo)
                {
                    return "PsuedoField";  // Should never see this
                }
                //else if (fType == fieldType.longName)   // Never used
                //{
                //    return tableAlias + ":" + fieldName;  
                //}
                else
                {
                    if (dataHelper.isTablePrimaryKeyField(this))
                    {
                        return "PK: " + dgvHelper.TranslateString(fieldName);
                    }
                    else if (dataHelper.isForeignKeyField(this))
                    {
                        return "FK: " + dgvHelper.TranslateString(fieldName);
                    }
                    else
                    {
                        if (lastTwoDigetOfTableAlias! == "00")
                        {
                            return dgvHelper.TranslateString(fieldName); ;
                        }
                        else
                        {
                            return lastTwoDigetOfTableAlias + ": " + dgvHelper.TranslateString(fieldName); ;
                        }
                    }
                }
            }
        }

        // Key used to avoid needing to implement EqualityComparer<field> class - Used in dictionaries in sqlFactory
        public Tuple<string, string, string> key { get { return Tuple.Create(this.tableAlias, this.table, this.fieldName); } }
        public Tuple<string, string> baseKey { get { return Tuple.Create(this.table, this.fieldName); } }
    }


    public class where
    {
        public where(field fl, string whereValue)
        {
            this.fl = fl;
            this.whereValue = whereValue;
        }
        public where(string table, string field, string whereValue, DbType dbType, int size)
        {
            this.fl = new field(table, field, dbType, size);
            this.whereValue = whereValue;
            this.DisplayMember = whereValue;
        }

        public field fl { get; set; }
        public string whereValue { get; set; }

        public string DisplayMember { get; set; }  // Used to display where in combo

        public where ValueMember { get { return this; } }

        public bool isSameWhereAs(where wh)
        {
            if (wh == null) { return false; }
            if (this.fl.key.Equals(wh.fl.key) && this.whereValue == wh.whereValue)
            { return true; }
            else { return false; }
        }
    }

    public class innerJoin
    {
        public field fkFld { get; set; }
        public field pkRefFld { get; set; }
        public innerJoin(field fkFld, field pkRefField)
        {
            this.fkFld = fkFld;
            this.pkRefFld = pkRefField;
        }
    }

    public class orderBy
    {
        public field fld;
        public System.Windows.Forms.SortOrder sortOrder;

        public orderBy(field fld, System.Windows.Forms.SortOrder sortOrder)
        {
            this.fld = fld;
            this.sortOrder = sortOrder;
        }
    }

    public enum command
    {
        select,
        selectAll,
        selectOneField,
        count
    }

    public enum ProgramMode
    {
        none,
        view,
        edit,
        add,
        delete,
        merge
    }


    public enum DbTypeType
    {
        isString,
        isDecimal,
        isInteger,
        isDateTime,
        isBoolean,
        isBinary
    }

    public enum fieldType
    {
        regular,
        longName,
        pseudo
    }

    public enum comboValueType
    {
        // This enum is not actually needed, but using it to remember there are four returnComboSql cases
        PK_myTable,
        PK_refTable,
        textField_myTable,
        textField_refTable
    }

    #endregion



}
