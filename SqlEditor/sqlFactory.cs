// using Microsoft.Office.Interop.Word;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Text;
using Windows.Media.AppBroadcasting;

namespace SqlEditor
{
    //internal partial class DataGridViewForm : Form
    public class SqlFactory
    {
        #region Variables
        public string errorMsg = String.Empty;
        // If strManualWhereClause is empty, it will replace the where list in sql
        public string strManualWhereClause = String.Empty;  
        public string myTable = "";
        public int myPage = 0;  // Asks for all records, 1 is first page
        public int myPageSize;  // Set by constructor
        public bool includeAllColumnsInAllTables { get; set; }
        public int TotalPages { get; set; } // Set when you set record count
        private int recordCount = 0;
        public int RecordCount
        {
            get { return recordCount; }
            set
            {
                recordCount = value;
                if (myPageSize > 0)
                {
                    TotalPages = (int)Math.Ceiling((decimal)recordCount / myPageSize);
                }
            }
        }

        // SQL built from 3 lists: myFields, myOrderBys, myWheres
        // SQL for a Combo uses the combo field and myComboWheres
        // myFields never changes (once it is constructed by the constructor)		
        // A field contains a .tableAlias, a .table, and a .fieldName
        public List<field> myFields = new List<field>();
        public List<orderBy> myOrderBys = new List<orderBy>();
        public List<where> myWheres = new List<where>();
        // Used to get the sql for a combo box dropdown items 
        public List<where> myComboWheres = new List<where>();

        // PKs_OstensiveDictionary - the fields that the table PK will show in the main grid or the combos.
        // "OstensiveDictionary" means the human understandable fields that identify the PK.
        // For example the OstensiveDefinition of "StudentDegreeID" = 32  might be
        // the Student name field and the degree name field.
        // There should be unique index on each table giving the Display keys / OstensiveDefinition fields.
        // The Tuple<string,string,string> is just the PK field in another format - although missing type.
        // The three values are the field.tableAlias, field.table, field.fieldName

        public Dictionary<Tuple<string, string, string>, List<field>> 
            PKs_OstensiveDictionary = new Dictionary<Tuple<string, string, string>, List<field>>();

        // Pks_InnerjoinMap - map from a PK for a table to all innerJoins in the table.
        // Each inner join is a FK in the table and the reference table it refers to.
        // Used to get sql table string and whenever we need to find the inner joins of a table.
        public Dictionary<Tuple<string, string, string>, List<innerJoin>> 
            PKs_InnerjoinMap = new Dictionary<Tuple<string, string, string>, List<innerJoin>>();

        // Following two used to get SQL for an aggregate function(s)
        public List<field> myGroupByFields = new List<field>();
        public List<field> myAggFieldList = new List<field>();

        public List<field> myFields2
        {
            get {
                if (myAggFieldList.Count > 0){
                    return myGroupByFields.Concat(myAggFieldList).ToList();
                }
                else { 
                    return myFields; 
                }
            }
        }

        public bool isAggregateTable
        {
            get
            {
                if (myAggFieldList.Count > 0) { return true; }
                else { return false; }
            }
        }

        #endregion

        //Constructors
        public SqlFactory(string table, int page, int pageSize) : this(table, page, pageSize, false) { }
        public SqlFactory(string table, int page, int pageSize, bool includeAllColumnsInAllTables)
        {
            // If includeAllColumnsInAllTables is true, this is very slow in datagridview
            // For example, the transcripts table will have 89 columns;
            // Database call is fast, only the display is slow;
            this.includeAllColumnsInAllTables = includeAllColumnsInAllTables;
            myTable = table;
            myPage = page;
            myPageSize = pageSize;

            // The main work of this constructor
            errorMsg = ConstructMyFieldsInnerJoinsAndOstensiveDefinitions();

            // If there is no ostensive definition for any PK or FK reference table,
            // add the primary key of the table (either myTable or the reference table for a FK)
            foreach (Tuple<string, string, string> pk in PKs_OstensiveDictionary.Keys)
            {
                if (PKs_OstensiveDictionary[pk].Count == 0)
                {
                    field PK_myTable = dataHelper.getTablePrimaryKeyField(myTable);
                    if (pk.Equals(PK_myTable.key))  // Comparing two Tuples - ? worked with ==
                    {
                        PKs_OstensiveDictionary[pk].Add(PK_myTable);
                    }
                    else // primary key of ref table - only PK's added to PK_OstensiveDictionary
                    {                           
                        field PK_RefTable = dataHelper.getFieldFromFieldsDT(pk.Item2, pk.Item3);
                        PK_RefTable.tableAlias = pk.Item1;
                        PKs_OstensiveDictionary[pk].Add(PK_RefTable);
                    }
                }
            }

        }

        // ConstructMyFieldsInnerJoinsAndOstensiveDefinitions is complex
        // It fills variables with everything about the table that the program needs to know 
        // (1) myInnerJoins and myFields lists -
        //      Goes through table and adds to myInnerJoins and myFields lists
        // (2) PKs_InnerjoinMap[PK] --> list of FK inner joins in that PK table
        //      A map from every primary key to all ForeignKeys.  
        //      The list of FKs does NOT contain grandchildren.
        //      But the PK of the reference table (the son) will be a key in PKs_InnerJoinMap,
        //      and this will have its FKs.
        // (3) PKs_OstensiveDictionary[PK] --> list of display fields for PK
        //      A map from every primary key to all its displayFields
        //      The list of Display keys has grandchildren.  And every field has an FKAncestors list
        //      that maps the Display back to a column in myTable.
        //      Consider "StudentDegreesID", the table has 3 FK display keys (studentID, degreeID, handbookID).
        //      The student table and degree table both have one display key (the names)
        //      The degreeID PK has 2 display keys: The name and DelMethID.
        //      The DelMethID display key has one display key (the name).  
        //      So StudentDegreesID will map to 4 display keys.
        //      The degreeName/Handbook/StudentName fields all have one FK ancester-the FK in the StudentDegree table
        //      The DelMethod Name filed has two ancestors- Delivery Method table PK and FK in StudentDegree Table
        // (4) Finally, keep a list of errors. After returning from the constructor, datagridview1 reports these to the user.
        //
        // NOTE: Once all these things are set by this routine, they never change in this sqlFactory object.
        private string ConstructMyFieldsInnerJoinsAndOstensiveDefinitions()
        {
            List<string> errMsgs = new List<string>(); // By reference, because errors may be in lower tables
            List<field> displayKeys = new List<field>();  // By reference, because these are in lower tables (i.e. recursive calls add some) 
            List<string> allTables = new List<string>();  // By reference, use to set tableAlias for each table 
            List<field> ancestorFields = new List<field>();  // By value, because these are sent down to lower tables
            // Begin the program
            field pkMyTable = dataHelper.getTablePrimaryKeyField(myTable);
            myFields.Add(pkMyTable);  // Always want this first
            // This does all the work
            ConstructMyFieldsInnerJoinsAndOstensiveDefinitions(
                pkMyTable, ancestorFields, ref displayKeys, ref allTables, ref errMsgs); 
            return String.Join(", ", errMsgs); 
        }

        private void ConstructMyFieldsInnerJoinsAndOstensiveDefinitions(field pkCurrentTable, List<field> ancestorFields, ref List<field> displayKeys, ref List<string> allTables, ref List<string> errMsgs)
        {
            // Circular table inner join check
            bool circular = false;
            // Circular if the table is in its own ancestor fields
            foreach (field f in ancestorFields)
            {
                if (f.table == pkCurrentTable.table) { circular = true; }

            }
            // Store the error and skip this table.
            if (circular)
            {
                errMsgs.Append("Skipping circular table foreign key: (" + pkCurrentTable.table + ")");
            }
            else
            {
                // New list of innerJoins for this table - starts with empty list
                List<innerJoin> currentTableInnerJoins = new List<innerJoin>();

                // New Alias for current table if needed
                int i = allTables.Count(e => e.Equals(pkCurrentTable.table));
                if (i > 0)
                {
                    pkCurrentTable.tableAlias = pkCurrentTable.table + dataHelper.twoDigitNumber(i);
                }
                // Prepare for tableAlias of next use of refTable if any
                allTables.Add(pkCurrentTable.table);  

                // Loop through rows of current table
                DataRow[] drs = dataHelper.fieldsDT.Select("TableName = '" + pkCurrentTable.table + "'", "ColNum ASC");
                foreach (DataRow dr in drs)
                {
                    // Get the field for this row -- This field may or may not be added to myFields
                    field drField = dataHelper.getFieldFromFieldsDT(dr);
                    // Same tableAlias and ancestorFields used for all fields in current Table
                    drField.tableAlias = pkCurrentTable.tableAlias;
                    drField.FKancestors = ancestorFields;

                    // Divide cases - a foreign key - recursive call
                    if (dataHelper.isForeignKeyField(drField))
                    {
                        // Decide whether this inner join is included/shown -if not do nothing
                        if (includeAllColumnsInAllTables || pkCurrentTable.table == myTable || dataHelper.isDisplayKey(drField))
                        {
                            myFields.Add(drField);
                            // drField is a foreign key, so we can get the Primary Key of the Reference (Child) table
                            field pkRefTable = dataHelper.getForeignKeyRefField(drField);
                            // Add inner join to this tables inner joins
                            innerJoin new_ij = new innerJoin(drField, pkRefTable);
                            currentTableInnerJoins.Add(new_ij);
                            // Prepare variables for recursive call
                            // Start with empty newDisplayKeys and add them after returning from recursive call.
                            List<field> newDisplayKeys = new List<field>();
                            // AncestorFields for lower tables are drField + ancestorFields from higher tables.
                            List<field> myAncestorFields = new List<field>();
                            myAncestorFields.Add(drField);
                            myAncestorFields.AddRange(ancestorFields);
                            // Recursive call
                            ConstructMyFieldsInnerJoinsAndOstensiveDefinitions(
                                pkRefTable, myAncestorFields, ref newDisplayKeys, ref allTables, ref errMsgs);
                            // FK - display keys include lower display keys
                            if (dataHelper.isDisplayKey(drField))
                            {
                                displayKeys.AddRange(newDisplayKeys);
                            }
                        }
                    }
                    else if (dataHelper.isTablePrimaryKeyField(drField))
                    {
                        // Primary Key of main table added first in myFields
                        // Foreign keys are added as field in myFields
                        //    - so no need to add primary key of ref tables
                        // So there is no need to add any primary keys to myFields here
                    }
                    else // drField is neither a PK nor an FK
                    {
                        // Add drField to myFields if we are including all tables, this is myTable, or drField is a displaykey of this table
                        // If it is a displayKey, add it to this tables displaykeys.
                        if (includeAllColumnsInAllTables || pkCurrentTable.table == myTable || dataHelper.isDisplayKey(drField))
                        {
                            myFields.Add(drField);
                            if (dataHelper.isDisplayKey(drField))
                            {
                                displayKeys.Add(drField);
                            }
                        }
                    }
                }
                // Once all rows are processed,
                // map the pk of the CurrentTable to its inner joins and its display keys
                PKs_InnerjoinMap.Add(pkCurrentTable.key, currentTableInnerJoins);
                PKs_OstensiveDictionary.Add(pkCurrentTable.key, displayKeys);
                // Note:pk.key is the Tuple<pk.tableAlias, pk.table, pk.fieldName)
                // We use this because c# knows when two Tuple's are equal,
                // but I have not defined when two fields are equal.
            }

        }

        // The primary function of this class - 1 overload
        public string returnSql(command cmd)
        {
            return returnSql(cmd, false);
        }

        public string returnSql(command cmd, bool strict)
        {
            // The main function of this class - used for tables and combos.
            int offset = (myPage - 1) * myPageSize;
            string sqlString = "";
            if (cmd == command.count)
            {
                sqlString = "SELECT COUNT(1) FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, strManualWhereClause, strict);
            }
            else if (cmd == command.selectAll) // All the filtered pages - include 'myWheres'
            {
                sqlString = "SELECT " + SqlStatic.sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, strManualWhereClause, strict) + SqlStatic.sqlOrderByStr(myOrderBys) + " ";
            }
            else if (cmd == command.select)
            {
                // Aggregate fields
                if (isAggregateTable) 
                {
                    sqlString = "Select " + SqlStatic.sqlFieldString(myFields2)
                    + " FROM " + sqlTableString() + " "
                    + SqlStatic.sqlWhereString(myWheres, strManualWhereClause, strict);
                    if (myGroupByFields.Count > 0)
                    {
                        sqlString += " GROUP BY " + SqlStatic.sqlFieldString(myGroupByFields) + " ";
                    }
                    sqlString += SqlStatic.sqlOrderByStr(myOrderBys) + MsSql.GetFetchString(offset, myPageSize);
                }
                else 
                { 
                    // Sql 2012 required for this "Fetch" clause paging - works even when (recordCount <= offset + myPageSize)
                    sqlString = "SELECT " + SqlStatic.sqlFieldString(myFields) + " FROM " + sqlTableString() + SqlStatic.sqlWhereString(myWheres, strManualWhereClause, strict) + SqlStatic.sqlOrderByStr(myOrderBys) + MsSql.GetFetchString(offset, myPageSize);
                }
            }
            return sqlString;
        }

        // This factory is still the currentSql factory
        public string returnComboSql(field cmbField, bool includePKinDisplayMember, comboValueType cmbValueType)  // Return all Display keys
        {
            string sqlString = string.Empty;

            // For primary keys, displayMember is the ostensive display keys and value member is the Pk value.
            // This displayMember is used in combos to identifies the row; and similarly in mainFilter dropdown items
            // If includePKinDisplayMember is true, the displayMember will begin with PK and be ordered by PK
            if (cmbValueType == comboValueType.PK_myTable || cmbValueType == comboValueType.PK_refTable)  // no distinction between these two
            {
                StringBuilder sqlFieldStringSB = new StringBuilder();
                List<field> fls = PKs_OstensiveDictionary[cmbField.key];  // Error if not present
                // If myTable has no display keys, make the primary field the display key
                if (fls.Count == 0)
                {
                    sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                }
                else if (fls.Count == 1 && !includePKinDisplayMember)  // 1 field only
                {
                    sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(fls[0]));
                }
                else  // 2 or more fields
                {
                    sqlFieldStringSB.Append("Concat_WS(',',");
                    if (includePKinDisplayMember)
                    {
                        sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                        sqlFieldStringSB.Append(",");
                    }
                    sqlFieldStringSB.Append(SqlStatic.sqlFieldString(fls));  // function converts fls to list of fields seperated by comma
                    sqlFieldStringSB.Append(")");
                }
                sqlFieldStringSB.Append(" as DisplayMember");
                sqlFieldStringSB.Append(", ");
                // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias) 
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as ValueMember");

                string orderBy = string.Empty;
                if (includePKinDisplayMember)
                {
                    orderBy = " Order by ValueMember";
                }
                else
                {
                    orderBy = " Order by DisplayMember";
                }
                //Not uses strStaticWhereClause in combo
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + sqlTableString(cmbField) + " " + SqlStatic.sqlWhereString(myComboWheres, string.Empty, false) + orderBy;
            }
            // For text fields return distinct values - used in combo boxes 
            else if (cmbValueType == comboValueType.textField_myTable || cmbValueType == comboValueType.textField_refTable)  // no distinction between these two
            {
                // For non-Keys return distinct values - used in combo boxes 
                StringBuilder sqlFieldStringSB = new StringBuilder();
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as DisplayMember");
                sqlFieldStringSB.Append(", ");
                // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias 
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as ValueMember");
                field PkField = dataHelper.getTablePrimaryKeyField(cmbField.table);
                PkField.tableAlias = cmbField.tableAlias;
                string tableString = sqlTableString(PkField);
                //if (dataHelper.isForeignKeyField(cmbField))
                //{
                //    ijList = PKs_InnerjoinMap[Keyfield.fieldName];
                //    field PkField = dataHelper.getForeignKeyRefField(Keyfield);
                //    tableString = sqlTableString(PkField, ijList);
                //}
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + tableString + " " + SqlStatic.sqlWhereString(myComboWheres, string.Empty, false) + " Order by DisplayMember";
            }
            return sqlString;
        }

        public string returnOneFieldSql(field fld)
        {
            string sqlString = "SELECT " + dataHelper.QualifiedAliasFieldName(fld) + " FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, strManualWhereClause, true) + SqlStatic.sqlOrderByStr(myOrderBys) + " ";
            return sqlString;
        }

        private string sqlTableString()
        {
            field myPK = dataHelper.getTablePrimaryKeyField(myTable);
            string ts = sqlTableString(myPK);
            return ts;
        }

        private string sqlTableString(field PkField)
        {
            string ts = PkField.table + " AS " + PkField.tableAlias;
            if (PKs_InnerjoinMap.ContainsKey(PkField.key))  // Should always be true, I think
            {
                List<innerJoin> ijList = PKs_InnerjoinMap[PkField.key];
                return sqlExtendTableStringByInnerJoins(ijList, ts);
            }
            else
            {
                return ts;
            }
        }

        private string sqlExtendTableStringByInnerJoins(List<innerJoin> ijList, string ts)
        {
            foreach (innerJoin ij in ijList)
            {
                string condition = string.Format(" {0} = {1} ", dataHelper.QualifiedAliasFieldName(ij.fkFld), dataHelper.QualifiedAliasFieldName(ij.pkRefFld));
                ts = "([" + ij.pkRefFld.table + "] AS " + ij.pkRefFld.tableAlias + " INNER JOIN " + ts + " ON " + condition + ")";
                // Recursive step - loop through innerjoins of the pkRefFld 
                if (PKs_InnerjoinMap.Keys.Contains(ij.pkRefFld.key))
                {
                    List<innerJoin> RefTable_ijs = PKs_InnerjoinMap[ij.pkRefFld.key];
                    ts = sqlExtendTableStringByInnerJoins(RefTable_ijs, ts);
                }
            }
            return ts;
        }

        public bool TableIsInMyInnerJoins(field PkField, string tableAliasName)
        {
            if (tableAliasName == PkField.tableAlias)  // Table can be filtered on itself 
            {
                return true;
            }
            Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
            if (PKs_InnerjoinMap.ContainsKey(key))
            {
                foreach (innerJoin ij in PKs_InnerjoinMap[key])
                {
                    // Recursive call
                    if (TableIsInMyInnerJoins(ij.pkRefFld, tableAliasName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Loop through all the inner joins.  The Keys are the PK's - check if one is my table
        // I could also use "MainFilterTableInCOmboSql" to do this by starting with myTable
        public bool MainFilterTableIsInMyTable(string mainFilterTable, out string tableAlias)
        {
            foreach (Tuple<string, string, string> key in PKs_InnerjoinMap.Keys)
            {
                if (key.Item2 == mainFilterTable)
                {
                    tableAlias = key.Item1;
                    return true;
                }
            }
            tableAlias = string.Empty;
            return false;
        }

        // Start with the table in the combo, and recursively trace all PK's that are inner-joins under it.
        public bool MainFilterTableIsInComboSql(where mainFilter, field PkField, out string tableAlias)
        {
            if (mainFilter.fl.baseKey.Equals(PkField.baseKey))  // Table itself is the first table in the Sql table string
            {
                tableAlias = PkField.tableAlias;
                return true;
            }
            // Recursive search
            Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
            if (PKs_InnerjoinMap.Keys.Contains(key))
            {
                foreach (innerJoin ij in PKs_InnerjoinMap[key])
                {
                    if (MainFilterTableIsInComboSql(mainFilter, ij.pkRefFld, out tableAlias))
                    {
                        return true;
                    }
                }
            }
            tableAlias = string.Empty;
            return false;
        }

        // Find index of field with unknown Alias - Use this method if you are certain there is only one
        // This function is also used in transcript plugin
        public int getColumn(field fld)
        {
            List<field> inAncestors = new List<field>();
            List<field> notInAncestors = new List<field>();
            return getColumn(fld, inAncestors, notInAncestors);
        }

        // Find index with unknown Alias - Use this method if there may be more than one field with same table and fieldName.
        public int getColumn(field fld, List<field> inAncestors, List<field> notInAncestors)
        {
            for (int i = 0; i < myFields.Count; i++)
            {
                // Convert inAncestor and notInAncestor to lists of tuple<string,string> - easier to compare
                List<Tuple<string, string>> inAncestorTuples = new List<Tuple<string, string>>();
                foreach (field f in inAncestors) { inAncestorTuples.Add(f.baseKey); }
                List<Tuple<string, string>> notInAncestorTuples = new List<Tuple<string, string>>();
                foreach (field f in notInAncestors) { notInAncestorTuples.Add(f.baseKey); }

                //If i is a candidate, then check carry out two check and return i
                if (myFields[i].baseKey.Equals(fld.baseKey))
                {
                    // I doubt this is ever true, but just in case 
                    if (myFields[i].FKancestors == null) { return i; }
                    // Assume true and do two checks
                    bool ancestorCheck = true; // default
                    // Create FKancestor tuple list from myFields[i].FKancestors
                    List<Tuple<string, string>> ancestorTuples = new List<Tuple<string, string>>();
                    foreach (field f in myFields[i].FKancestors) { ancestorTuples.Add(f.baseKey); }
                    // Do the 1st check
                    foreach (Tuple<string, string> t in inAncestorTuples)
                    {
                        if (!ancestorTuples.Contains(t)) { ancestorCheck = false; break; }
                    }
                    // Do 2nd check
                    if (ancestorCheck)
                    {
                        foreach (Tuple<string, string> t in notInAncestorTuples)
                        {
                            if (ancestorTuples.Contains(t)) { ancestorCheck = false; break; }
                        }
                        if (ancestorCheck) { return i; }
                    }
                }
            }
            return 0;  // An error
        }

        public int getColumn(field fld, field fkToCheck, bool inAncestorCheck)
        {
            List<field> emptyList = new List<field>();
            List<field> checkFields = new List<field>();
            checkFields.Add(fkToCheck);
            if (inAncestorCheck) { return getColumn(fld, checkFields, emptyList); }
            else { return getColumn(fld, emptyList, checkFields); }
        }


        // Class in class to note the static methods - but not consistently applied to all static methods
        public static class SqlStatic
        {
            // strict -- A string must be an exact match (non-strict uses "Like")
            public static string sqlWhereString(List<where> whereList, string strManualWhereClause, bool strict)
            {
                string whereConditions = sqlWhereStringConditions(whereList,strManualWhereClause,strict);
                if (whereConditions == string.Empty)
                {
                    return string.Empty;   // No where conditions
                }
                else
                {
                    // Return string constructed from list of conditions
                    return " WHERE " + whereConditions;
                }
            }
            public static string sqlWhereStringConditions(List<where> whereList, string strManualWhereClause, bool strict)
            {
                // Make a list of the conditions
                List<string> WhereStrList = new List<string>();
                foreach (where ws in whereList)
                {
                    string condition = "";
                    if (dataHelper.TryParseToDbType(ws.whereValue, ws.fl.dbType))
                    {
                        DbTypeType dbTypeType = dataHelper.GetDbTypeType(ws.fl.dbType);
                        // Get where condition
                        switch (dbTypeType)
                        {
                            case DbTypeType.isInteger:
                            case DbTypeType.isDecimal:
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = " + ws.whereValue;
                                break;
                            case DbTypeType.isDateTime:
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + "= '" + ws.whereValue + "'";
                                break;
                            case DbTypeType.isString:
                                if (strict)
                                {
                                    // Strict use of "Like"
                                    condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + ws.whereValue + "'";  //Exact string
                                }
                                else
                                {
                                    // Non-strict uses of "Like"
                                    condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " Like '" + ws.whereValue + "%'";  //Same starting string
                                }
                                break;
                            case DbTypeType.isBoolean:
                                if (ws.whereValue.ToLower() == "true")
                                {
                                    condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + MsSql.trueString + "'";
                                }
                                else
                                {
                                    condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + MsSql.falseString + "'";
                                }
                                break;
                        }
                    }
                    if (condition != "")
                    {
                        WhereStrList.Add(condition);
                    }
                }
                // Add manual filter
                if (!String.IsNullOrEmpty(strManualWhereClause))
                {
                    WhereStrList.Add(strManualWhereClause);
                }
                // Use list of conditions to get sql where clause.
                if (WhereStrList.Count > 0)
                {
                    // Return string constructed from list of conditions
                    return String.Join(" AND ", WhereStrList);
                }
                else
                {
                    return string.Empty;   // No where conditions
                }
            }

            public static string sqlFieldString(List<field> fieldList)
            {
                // Make a list of the qualified fields, i.e. [table].[field]
                List<string> fieldStrList = new List<string>();
                foreach (field fs in fieldList)
                {
                    fieldStrList.Add(dataHelper.QualifiedAliasFieldName(fs));
                }
                // Join with commas - .Join knows to skip a closing comma.
                return String.Join(",", fieldStrList);
            }

            public static string sqlOrderByStr(List<orderBy> orderByList)
            {
                if (orderByList.Count == 0) { return ""; }
                //Make a list of order by clauses
                List<string> orderByStrList = new List<string>();
                foreach (orderBy ob in orderByList)
                {
                    string qualFieldName = dataHelper.QualifiedAliasFieldName(ob.fld);
                    if (ob.sortOrder == System.Windows.Forms.SortOrder.Descending)
                    {
                        orderByStrList.Add(qualFieldName + " DESC");
                    }
                    else
                    {
                        orderByStrList.Add(qualFieldName + " ASC");
                    }
                }
                // Return string constructed by list of order by clauses
                return " ORDER BY " + String.Join(",", orderByStrList);
            }
        }
    }
}


//---------------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------------
//Some notes on sql-------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------------
//OpenSchema can have 2 variable: adSchemaTables and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,TABLE_TYPE)
//                                adSchemaColumns and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME)
//                                adSchemaIndexes and array(TABLE_CATALOG,TABLE_SCHEMA,INDEX_NAME,TYPE,TABLE_NAME)
//In access table_catalog and table_schema are Empty.  An empty table_name returns all tables.




