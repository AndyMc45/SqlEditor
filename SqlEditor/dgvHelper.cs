using System.Data;


namespace SqlEditor
{
    public static class dgvHelper
    {
        public static Dictionary<string, string> translations = new Dictionary<string, string>();

        public static bool translate;

        public static List<(string, string)> readOnlyField = new List<(string, string)>();


        // Results of this coloring use in color combo boxes above
        public static void SetHeaderColorsOnWritePage(DataGridView dgv, string myTable, List<field> myFields)
        {
            FormOptions formOptions = AppData.GetFormOptions();
            dgv.EnableHeadersVisualStyles = false;
            int nonDkNumber = -1;
            int dkNumber = -1;
            bool currentArrayIsDkColors = false;
            string lastTable = myTable;
            for (int i = 0; i < myFields.Count; i++)
            {
                // Display keys and foreignkeys
                field fieldi = myFields[i];
                bool myDisplayKey = dataHelper.isDisplayKey(fieldi) && fieldi.table == myTable;
                bool myForeignKey = dataHelper.isForeignKeyField(fieldi) && fieldi.table == myTable;
                bool myPrimaryKey = dataHelper.isTablePrimaryKeyField(fieldi); // Only myPrimaryKey in fields
                // Primary Key - easy
                if (myPrimaryKey)
                {
                    dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.PrimaryKeyColor;
                    dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.PrimaryKeyColor;
                }
                // Display Key - might be a typical display key or a foreign key - not yet handling a displaykey of foreign key
                else if (myDisplayKey)
                {
                    dkNumber++;  // Increase dkNumber
                    if (dkNumber == formOptions.DkColorArray.Count()) { dkNumber = 0; }
                    dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.DkColorArray[dkNumber];
                    dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DkColorArray[dkNumber];
                    // Next two used below to handle a displaykey of foreign key
                    currentArrayIsDkColors = true;  // Tells me which array to use
                    if (myForeignKey)
                    {
                        lastTable = dataHelper.getForeignKeyRefField(fieldi).table;  // tells me we are handling a foreign key
                    }
                    else
                    {
                        lastTable = myTable;
                    }
                }
                else if (myForeignKey)  // A typical (non display-key) foreign key
                {
                    nonDkNumber++;
                    if (nonDkNumber == formOptions.nonDkColorArray.Count()) { nonDkNumber = 0; }
                    dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.nonDkColorArray[nonDkNumber];
                    dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.nonDkColorArray[nonDkNumber];
                    currentArrayIsDkColors = false;
                    lastTable = dataHelper.getForeignKeyRefField(fieldi).table;
                }
                // We are handling a display key of a foreign key - this assumes these occur after the foreign key
                else if (lastTable != myTable & fieldi.table != myTable)
                {
                    if (currentArrayIsDkColors)  // the foreign key is a disiplay key
                    {
                        dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.DkColorArray[dkNumber];
                        dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DkColorArray[dkNumber];
                    }
                    else  // The foreign key is not a display key
                    {
                        dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.nonDkColorArray[nonDkNumber];
                        dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.nonDkColorArray[nonDkNumber];
                    }
                }
                else  // All other columns are yellow
                {
                    dgv.Columns[i].HeaderCell.Style.BackColor = formOptions.DefaultColumnColor;
                    dgv.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DefaultColumnColor;
                    lastTable = myTable;
                }
            }
        }

        public static void SetNewColumnWidths(DataGridView dgv, List<field> myFields, bool narrowColumns)
        {
            // Time consuming in transcript table - so only call this routine when really needed
            // Also, this function takes 10 seconds for transcript table - so don't run it in loading page
            for (int i = 0; i < myFields.Count; i++)   // Using myFields.Count and not dgv.Columns becauser use may add a column to dgv
            {
                field fl = myFields[i];
                string headerText = myFields[i].fieldName;  // Default headerText = Original header text
                int headerWidth;  // No default
                System.Drawing.Font font = dgv.Font;
                using (Graphics g = dgv.CreateGraphics())
                {
                    headerWidth = Math.Max(62, (int)g.MeasureString(headerText, font).Width);  // 62 the shortest
                }
                int currentWidth = dgv.Columns[i].Width;
                // Defaults
                dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // default Alignment
                int nextWidth = dataHelper.getStoredWidth(fl.table, fl.fieldName, currentWidth);  // Default 
                // Set 'shortenHeaderText', and nextWidth with switch
                DbType dbType = fl.dbType;
                switch (dbType)
                {
                    case DbType.Int32:
                    case DbType.Int16:
                    //case DbType.Decimal:
                    //case DbType.Int64:
                    case DbType.Byte:
                    case DbType.SByte:   // -127 to 127 - signed byte
                                         // case DbType.Double:
                                         // case DbType.Single:
                        dgv.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        if (narrowColumns)
                        {
                            // ReadOnly = false means this is editable
                            if (dgv.Columns[i].ReadOnly == true && (dataHelper.isTablePrimaryKeyField(myFields[i]) || dataHelper.isForeignKeyField(myFields[i])))
                            {
                                nextWidth = 3;
                                dgv.Columns[i].DefaultCellStyle.ForeColor = Color.White;
                            }
                            else
                            {
                                nextWidth = Math.Max(62, headerWidth);
                                dgv.Columns[i].DefaultCellStyle.ForeColor = Color.Navy;
                            }
                        }
                        else
                        {
                            nextWidth = Math.Max(62, headerWidth);
                            dgv.Columns[i].DefaultCellStyle.ForeColor = Color.Navy;
                        }
                        break;
                    default:
                        // Get the longest of the first 40 items
                        int r = 0;
                        int longestWidth = 62; // default
                        using (Graphics g = dgv.CreateGraphics())
                        {
                            foreach (DataGridViewRow row in dgv.Rows)
                            {
                                r = r + 1;
                                if (r > 40) { break; }
                                if (row.Cells[i].Value != null)
                                {
                                    int thisItemWidth = (int)g.MeasureString(row.Cells[i].Value.ToString(), font).Width;
                                    longestWidth = Math.Max(thisItemWidth, longestWidth);
                                }
                            }
                        }
                        if (narrowColumns)
                        {
                            nextWidth = Math.Max(headerWidth, longestWidth);
                        }
                        else
                        {
                            nextWidth = Math.Max(headerWidth, longestWidth);
                        }
                        break;
                }  // End switch
                // dgv.Columns[i].HeaderText = headerText;
                dgv.Columns[i].Width = nextWidth;
                // Prepare for next load of table before program closed - every column must be at least 62
                dataHelper.storeColumnWidth(fl.table, fl.fieldName, nextWidth);
            }
        }

        public static void TranslateHeaders(DataGridView dgv)
        {
            if (translate)
            {
                // Rename the header rows (if a plugin has added some colHeaderTranslation.keys)
                // Microsoft might add a "1" at the end if two columns have the same header
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    string headerText = col.HeaderText;
                    string lastCharacter = headerText.Substring(headerText.Length - 1);
                    bool lastCharacterRemoved = false;
                    if (Int32.TryParse(lastCharacter, out int result))
                    {
                        headerText = headerText.Substring(0, headerText.Length - 1);
                        lastCharacterRemoved = true;
                    }
                    string newHeaderText = TranslateString(headerText);
                    // Change header text
                    if (headerText != newHeaderText)
                    {
                        if (lastCharacterRemoved) { newHeaderText = newHeaderText + lastCharacter; }
                        col.HeaderText = newHeaderText;
                    }
                }
            }
        }

        public static string TranslateString(string englishKey)
        {
            string returnValue = englishKey;
            if (translate)
            {
                if (translations.ContainsKey(englishKey.ToLower()))
                {
                    returnValue = translations[englishKey.ToLower()];
                    return returnValue;
                }
                else if (englishKey.Length > 2)
                {
                    if (englishKey.Substring(englishKey.Length - 2, 2) == "ID")
                    {
                        string shortKey = englishKey.Substring(0, englishKey.Length - 2);
                        if (translations.ContainsKey(shortKey.ToLower()))
                        {
                            returnValue = translations[shortKey.ToLower()] + "ID";
                            return returnValue;
                        }
                    }
                }
                return englishKey;
            }
            return englishKey;
        }


    }
}
