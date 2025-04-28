// using DocumentFormat.OpenXml.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SqlEditor.Properties;

using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;


namespace SqlEditor
{
    // The basic design of the program (old version - needs updated):
    // sqlCurrent stores the table and fields, the Where clauses, and one OrderBy clause.
    // sqlCurrent.returnSql returns the sql string.  This is then bound to the Grid (via an sqlDataAdaptor)

    //User actions, and how the program reacts to each as follows
    //0.  Form Load - Loads user settings and other things which don't depend on the Connection
    //1.  Open New connection -- called near end of Form Load and by File-->connection menu
    //    First calls closeConnection (reinitiae everything to empty state; list tables in menu), 
    //    and then open the new connection
    //2.  Open New Table -- calls writeGrid_NewTable
    //    WriteGrid_NewTable - Sets new sqlCurrent - Calls sqlCurrent.SetInnerJoins which sets sql table strings and field strings.
    //    WriteGrid_NewTable also resets the top filters and then sets them up for the table (with no actual filtering)
    //    WriteGrid_NewTable calls Write New Filters that calls write Grid
    //    Write the New Filters sets the where clauses in sqlCurrent.  Sets orderBy on first call.
    //    Write the New Page - binds dataViewGrid1 and then does some formatting on datagridview.
    //3.  Five modes
    //    View -- Base
    //    Edit  -- User selects 1 column in table to edit - not table PK or DK but may be FK - and FK may have several DK columns.
    //             (PK - Primary Key, FK - Foreign key, DK - Display Key.  DK is a column that defines the object - user can't change a DK.)
    //          -- Selecting a column also sets currentDA.UpdateCommand (i.e. currentSql's dataadapter's UpdateCommand)
    //          -- User clicks on an edit column - textbox appears for non-keys, drop-down appears for FK.
    //          -- When user exits the cell, call currentDA.Update()

    public partial class DataGridViewForm : Form
    {
        #region Variables
        private FormOptions? formOptions;
        private ConnectionOptions? connectionOptions;
        private TableOptions? tableOptions;  // Many: writingTable, doNotWriteTable, tableHasForiegnKeys, FKFieldInEditingControl, etc. 
        internal ProgramMode programMode = ProgramMode.none;  //none, view, edit, add, delete, merge
        public SqlFactory? currentSql = null;  //builds the sql string via .myInnerJoins, .myFields, .myWheres, my.OrderBys
        internal BindingList<where>? MainFilterList;  // Use this to fill in past main filters in combo box
        // public MainFormDelegate dgvMainFormDelegate;
        private String uiCulture = string.Empty;
        private Dictionary<string, List<string>> aliasTableDictionary = new Dictionary<string, List<string>>();
        private ILogger myLogger;
        private List<string> dirtyGFCombos = new List<string>();

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.M))
            {
                GridContextMenu_SetAsMainFilter_Click(null, null);
                return true;
            }
            else if (keyData == (Keys.Control | Keys.F))
            {
                GridContextMenu_SetFkAsMainFilter_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Entire Form constructor, events, and Log file

        public DataGridViewForm(ILogger<DataGridViewForm> logger)
        {
            myLogger = logger;
            // pluginError = true if the last load of the program did not cause error.
            string pluginError = AppData.GetKeyValue("PluginError");
            if (String.IsNullOrEmpty(pluginError)) { pluginError = "noError"; }
            myLogger.LogInformation("Plugin Error status: {status}", pluginError);

            MenuStrip pluginMenus = new MenuStrip();
            if (pluginError == "noError")
            {
                AppData.SaveKeyValue("PluginError", "hasError");  // Prepare for next load - undone below if no error
                myLogger.LogInformation("Loading Plugins");
                try
                {
                    // This loads plugins into Plugins.loadedPlugins AND return pluginMenus, translations, etc.
                    pluginMenus = Plugins.Load_Plugins(ref dgvHelper.translations, ref dgvHelper.translationCultureName,
                        ref dgvHelper.readOnlyField, ref dgvHelper.updateConstraints,
                        ref dgvHelper.insertConstraints, ref dgvHelper.deleteConstraints,
                        ref dgvHelper.newTableActions);
                }
                catch (Exception ex)
                {
                    myLogger.LogInformation("Error loading plugin: {ex}", ex.Message);
                }
            }
            AppData.SaveKeyValue("PluginError", "noError");  // Only get here if there is no fatal error.

            uiCulture = AppData.GetKeyValue("UICulture");

            dgvHelper.translate = (dgvHelper.translationCultureName == uiCulture);

            myLogger.LogInformation("uiCulture: {culture}", uiCulture);
            myLogger.LogInformation("Translation Culture: {trans}", dgvHelper.translationCultureName);

            // Sets culture - this can be set in a plugin
            if (IsUICulture(uiCulture))
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(uiCulture);
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(uiCulture);
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            }
            //Required by windows forms
            myLogger.LogInformation("Initializing Form Component");
            InitializeComponent();
            // Some setting to speed up datagridview
            myLogger.LogInformation("Settings to speed up DataGridView");
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.None);
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.RowHeadersVisible = false;

            // Define delegate for main form
            // dgvMainFormDelegate = Plugins.ExportForm;

            // Add plugin menus if any
            myLogger.LogInformation("Plugin Menu Count: {n}", pluginMenus.Items.Count.ToString());
            if (pluginMenus.Items.Count > 0)
            {
                // Translate plugin menus
                ToolStripItemCollection toolStripItems = pluginMenus.Items;
                foreach (ToolStripItem toolStripItem in toolStripItems)
                {
                    toolStripItem.Text = dgvHelper.TranslateString(toolStripItem.Text);
                    if (toolStripItem is ToolStripMenuItem)
                    {
                        // Should do a recursive call, but I only use one deep 
                        ToolStripMenuItem innerMenu = (ToolStripMenuItem)toolStripItem;
                        foreach (ToolStripItem innerMenuItem in innerMenu.DropDownItems)
                        {
                            innerMenuItem.Text = dgvHelper.TranslateString(innerMenuItem.Text);
                        }
                    }
                }
                MainMenu1.Items.AddRange(pluginMenus.Items);
            }

            // Invoke delegate - this will load mainForm into all plugins
            //dgvMainFormDelegate(this);
            Plugins.ExportForm(this);

            // Clone context menu items and add them to Tools menu.
            myLogger.LogInformation("Cloning context menu");
            ToolStripItem[] items = new ToolStripItem[GridContextMenu.Items.Count];
            foreach (ToolStripItem tsi in GridContextMenu.Items)
            {
                tsi.Tag = tsi.Name;
                if (tsi is ToolStripSeparator)
                {
                    ToolStripSeparator tsiNew = new ToolStripSeparator();
                    mnuTools.DropDownItems.Add(tsiNew);
                }
                else if (tsi is ToolStripMenuItem)
                {
                    ToolStripMenuItem tsiNew = new ToolStripMenuItem();
                    tsiNew.Text = ((ToolStripMenuItem)tsi).Text;
                    tsiNew.Click += mnuToolsMenuStripItem_Click;
                    // Tag used to identify this tsi in the click event
                    tsiNew.Tag = tsi.Name;
                    mnuTools.DropDownItems.Add(tsiNew);
                }
            }
        }

        private void DataGridViewForm_Load(object sender, EventArgs e)
        {
            myLogger.LogInformation("Loading form");
            //Shrink size of elements if the screen is too small.
            int tlp_pixelWidth = tableLayoutPanel.Width;
            System.Drawing.Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            if (workingArea.Width < tlp_pixelWidth)
            {
                myLogger.LogInformation("Upadate TableLayOutPanel width: {width}", workingArea.Width.ToString());
                tableLayoutPanel.Width = workingArea.Width;
            }

            // Set things that don't change even if connection changes
            formOptions = AppData.GetFormOptions();  // Sets initial option values
            formOptions.narrowColumns = false;  // Always begin with wide columns
            formOptions.runTimer = false;  // Set to true when debugging to check what is slow

            // 0.  Main filter datasource always a list of wheres and 'last' element always the same (i.e. "No Reasearch object")
            field fi = new field("none", "none", DbType.Int32, 4, fieldType.pseudo);
            where wh = new where(fi, "0");
            wh.DisplayMember = MyResources.MainFilterNotSet;
            MainFilterList = new BindingList<where>();
            MainFilterList.Add(wh);
            cmbMainFilter.DisplayMember = "DisplayMember";
            cmbMainFilter.ValueMember = "ValueMemeber";

            formOptions.loadingMainFilter = true;
            cmbMainFilter.DataSource = MainFilterList;
            formOptions.loadingMainFilter = false;
            cmbMainFilter.Enabled = false;  // Enabled by EnableMainFilter() when more than one element

            // 1. Set pageSize
            formOptions.pageSize = 100;
            string strPageSize = AppData.GetKeyValue("RPP");  // If not set, this will return string.Empty
            int newPageSize = 0;
            if (int.TryParse(strPageSize, out newPageSize))
            {
                if (newPageSize > 9)  // Don't allow less than 10
                {
                    formOptions.pageSize = newPageSize;
                }
            }
            txtRecordsPerPage.Text = formOptions.pageSize.ToString();

            // 2. Set font size
            DesignControlsOnFormLoad();  // Set font size and other features of various controls

            // 3. Load Database List (in files menu)
            myLogger.LogInformation("Loading DatabaseList");
            Load_mnuDatabaseList();  // Add previously open databases to databases menu dropdown list

            // 4. Open Log file
            // openLogFile(); //errors ignored 

            // 5. Open last connection 
            myLogger.LogInformation("Opening Connection");
            string msg = OpenConnection();  // Returns any error msg
            if (msg != string.Empty)
            {
                myLogger.LogInformation("Error Opening connection: {msg}", msg);
                msgText(msg); txtMessages.ForeColor = System.Drawing.Color.Red;
            }

            // 6. Set Mode and filters to "none".
            programMode = ProgramMode.none;
            SetFiltersColumnsTablePanel(); // Will disable filters and call SetTableLayoutPanelHeight();

            // 7. Hide IT tools
            mnuShowITTools.Checked = false;
            myLogger.LogInformation("Form Loaded");
        }

        private void DataGridViewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //Cleanup
                MsSql.CloseDataAdapters();
                MsSql.CloseConnection();
            }
            catch { }
        }

        // Set height and width main items    
        private void DataGridViewForm_Resize(object sender, EventArgs e)
        {
            dataGridView1.Width = this.Width;
            SetTableLayoutPanelHeight();
        }

        private void DesignControlsOnFormLoad()
        {
            // Note: these things that never change           

            // Control arrays - can't make array in design mode in .net; so I make them here
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            RadioButton[] radioButtons = { rbView, rbEdit, rbDelete, rbAdd, rbMerge };

            //Get font size from registry and define "font"
            int size = 8;  // default
            try { size = Convert.ToInt32(Interaction.GetSetting(AppData.appName, "Options", "FontSize", "9")); } catch { }
            System.Drawing.Font font = new System.Drawing.Font("Arial", size, FontStyle.Regular);

            // Set font of all single controls - 
            dataGridView1.Font = font;
            lblMainFilter.Font = font;
            lblGridFilter.Font = font;
            lblComboFilter.Font = font;
            cmbMainFilter.Font = font;
            cmbComboTableList.Font = font;
            txtRecordsPerPage.Font = font;

            // Set font of arrays 
            for (int i = 0; i <= cmbGridFilterFields.Count() - 1; i++)
            {
                cmbGridFilterFields[i].Font = font;
                cmbGridFilterFields[i].DropDownStyle = ComboBoxStyle.DropDownList;
                cmbGridFilterFields[i].FlatStyle = FlatStyle.Flat;
                cmbGridFilterFields[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbGridFilterFields[i].AutoCompleteMode = AutoCompleteMode.None;
                cmbGridFilterValue[i].Font = font;
                cmbGridFilterValue[i].DropDownStyle = ComboBoxStyle.DropDownList;
                cmbGridFilterValue[i].FlatStyle = FlatStyle.Flat;
                cmbGridFilterValue[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbGridFilterValue[i].AutoCompleteMode = AutoCompleteMode.None;

            }
            for (int i = 0; i <= cmbComboFilterValue.Count() - 1; i++)
            {
                lblCmbFilterFields[i].Font = font;
                cmbComboFilterValue[i].Font = font;
                cmbGridFilterFields[i].DropDownStyle = ComboBoxStyle.DropDown;
                cmbGridFilterFields[i].FlatStyle = FlatStyle.Flat;
                cmbComboFilterValue[i].AutoCompleteSource = AutoCompleteSource.CustomSource;
                cmbComboFilterValue[i].AutoCompleteMode = AutoCompleteMode.None;
            }
            for (int i = 0; i <= radioButtons.Count() - 1; i++)
            {
                radioButtons[i].Font = font;
            }
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = formOptions.DefaultColumnColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = formOptions.DefaultColumnColor;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Opening and closing Connection 
        private string OpenConnection()
        {
            programMode = ProgramMode.none;  // Affects SetAllFilters and sets PanelHeight
            SetFiltersColumnsTablePanel();
            connectionOptions = new ConnectionOptions();  // resets options
            StringBuilder sb = new StringBuilder();
            bool foundError = false;
            try
            {
                // 1. Close old connection if any - also turns off various menu items.
                CloseConnection();

                AppData.databaseName = String.Empty;

                // 2. Get connection string - Opens 1st stored connection if any
                connectionOptions.msSql = true;  // Other options not yet implemented

                connectionString csObject = AppData.GetFirstConnectionStringOrNull();
                if (csObject == null)
                {
                    sb.AppendLine(MyResources.NoPreviousConnectionSet);
                    foundError = true;
                }
                if (csObject.comboString.IndexOf("{3}") >= 0)
                {
                    frmLogin passwordForm = new frmLogin();
                    passwordForm.Text = csObject.server;
                    passwordForm.ShowDialog();
                    string password = passwordForm.password;
                    passwordForm = null;
                    if (String.IsNullOrEmpty(password)) { sb.AppendLine("Passsword not entered."); foundError = true; }
                    csObject.comboString = csObject.comboString.Replace("{3}", password);
                }

                if (!foundError)
                {
                    // Get password from user

                    // {0} is server, {1} is Database, {2} is user, {3} is password (unknown)
                    string cs = String.Format(csObject.comboString, csObject.server, csObject.databaseName, csObject.user);

                    // 5. Open connection
                    MsSql.openConnection(cs);  // May cause an error.  Errors not handled in openConnection(cs)

                    AppData.databaseName = csObject.databaseName;
                    this.Text = AppData.databaseName;

                    // 6. Initialize datatables
                    dataHelper.initializeDataTables();  // Program uses 8 different dataTables - this sets 8 variables to these tables

                    // 7. Fill Information Datatables  // Files 6 of the above tables with info about the database
                    string NoDK = MsSql.initializeDatabaseInformationTables();
                    if (!string.IsNullOrEmpty(NoDK)) { msgTextAdd(NoDK); }

                    // 8.  Load Table mainmenu
                    foreach (DataRow row in dataHelper.tablesDT.Rows)
                    {
                        ToolStripItem tsi = new ToolStripMenuItem();
                        string tn = row["TableName"].ToString();
                        tsi.Name = tn;
                        tsi.Text = dgvHelper.TranslateString(tn);
                        if (tn != null)
                        {
                            mnuOpenTables.DropDownItems.Add(tsi);
                        }
                    }
                }
            }
            catch (System.Exception excep)
            {
                sb.AppendLine(Properties.MyResources.errorOpeningConnection);
                MessageBox.Show(Properties.MyResources.errorOpeningConnection + Environment.NewLine + excep.Message + Environment.NewLine + "Error Number: " + Information.Err().Number.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CloseConnection();
            }
            return sb.ToString();
        }

        private void CloseConnection()
        {
            MsSql.CloseDataAdapters();
            MsSql.CloseConnection();

            // Clear other old values
            currentSql = null;

            // Delete the old mnuOpentable members
            if (mnuOpenTables.DropDownItems != null)
            {
                mnuOpenTables.DropDownItems.Clear();
            }
            // Hide special menus
            mnuForeignKeyMissing.Enabled = false;

        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Write to Gird

        public void writeGrid_NewTable(string table, bool clearManualFilter)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            mnuOpenTables.HideDropDown();  // Close the menu to help debugging
            Stopwatch watch = new Stopwatch(); if (formOptions.runTimer) { watch.Start(); }

            // 0. New tableOptions and Clear Grid-Combo filters
            tableOptions = new TableOptions(); // Resets options - WritingNewTable and firsttimeWritingTable are true

            // 1 - Record lastFilter for last table and then clear filters
            if (currentSql != null)  // This is the currentSql from the last Table
            {
                RecordLastFilter();  // Old table
            }
            dirtyGFCombos.Clear();  // All GF combos will be rebound 
            tableOptions.writingTable = true;  // Only make it true when it is needed
            ClearFilters(clearManualFilter); // All events are cancelled via "if(Selected_index != -1) . . ."
            rbView.Checked = true;  // 
            tableOptions.writingTable = false;  // Only make it true when it is needed

            // Add row to last filter for NEW table if we have not added it yet (we keep 1 row in dataHelper.LastFilterValuesDT for each table)
            DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", table, dataHelper.lastFilterValuesDT);
            if (lastFilterDR is null)
            {
                lastFilterDR = dataHelper.lastFilterValuesDT.NewRow();
                // Add a row with table field = this table, should also add main filter somewhere
                dataHelper.setColumnValueInDR(lastFilterDR, "Table", table);
                dataHelper.lastFilterValuesDT.Rows.Add(lastFilterDR);
            }

            // 1.5 - Run any plugin newTableAction actions
            foreach (Action<string> a in dgvHelper.newTableActions)
            {
                a(table);
            }


            //2. Create currentSql - same currentSql used until new table is loaded - same myFields and myInnerJoins
            //   Creating currentSql does not load any data into it
            currentSql = new SqlFactory(table, 1, formOptions.pageSize);

            // Check if above produces error message - very rare
            if (!String.IsNullOrEmpty(currentSql.errorMsg))
            {
                msgTextError(currentSql.errorMsg);
            }
            else
            {
                msgText(dgvHelper.TranslateString("Table") + " : " + dgvHelper.TranslateString(currentSql.myTable));
            }

            //3. Get aliasTableList - For use in txtManualFilter
            aliasTableDictionary.Clear();
            foreach (field fl in currentSql.myFields)
            {
                if (!aliasTableDictionary.ContainsKey(fl.tableAlias))
                {
                    List<string> fieldNameList = new List<string>();
                    aliasTableDictionary.Add(fl.tableAlias, fieldNameList);
                }
                aliasTableDictionary[fl.tableAlias].Add(fl.fieldName);
            }

            //4. Check if table has foreign keys
            foreach (field fl in currentSql.myFields)
            {
                if (fl.table == currentSql.myTable && dataHelper.isForeignKeyField(fl))
                {
                    tableOptions.tableHasForeignKeys = true; break;
                }
            }

            //4. On first load, Bind the cmbComboTableList with Primary keys of Reference Tables

            // Set up the cmbComboField and cmbComboFieldValue - but can't bind later to data yet.
            // Bind cmbComboTableList to list of foreign keys.
            if (tableOptions.tableHasForeignKeys)
            {
                BindingList<field> foreignKeyList = new BindingList<field>();

                string filter = String.Format("TableName = '{0}' and is_FK = 'true'", currentSql.myTable);
                DataRow[] drs = dataHelper.fieldsDT.Select(filter);
                // Add all foreign key fields to foreignKeyList 
                foreach (DataRow dr in drs)
                {
                    field fi = dataHelper.getFieldFromFieldsDT(dr);
                    foreignKeyList.Add(fi);  // All keys are in myTable -
                }
                // Bind the cmbComboTableList Combo control
                cmbComboTableList.BindingContext = new BindingContext();  // Required to change 1-by-1.
                cmbComboTableList.DisplayMember = "DisplayMember";
                cmbComboTableList.ValueMember = "ValueMember";  //Entire field
                // Must not attempt to load values into cmbComboFieldValue, because currentDT is not loaded 
                // Use "firstTimeWritingTable = true" to shut this off.
                tableOptions.writingTable = true; // Shuts off the rebinding of cmbGridFilterValues
                cmbComboTableList.DataSource = foreignKeyList;
                tableOptions.writingTable = false;
            }
            else
            {
                cmbComboTableList.DataSource = null;
                cmbComboTableList.Visible = false;
                cmbComboTableList.Enabled = false;
            }

            //5. Bind 9 cmbGridFilterFields with fields for user to select
            //    Only setting up cmbGridFilterFields - cmbGridFilterValues set on cmbGridFilterField SelectionChanged event
            //    All cmbGridFilterFields values are fields in myTable
            int comboNumber = 0;  // Keeps track of which cmbGridFilterField we are loading
            //5a.  Bind first cmbGridFilterFields[0] to all "yellow" text fields in my table (non-DK, non-FK, non-PK)
            BindingList<field> filterFields = new BindingList<field>();
            foreach (field fl in currentSql.myFields)
            {
                // Yellow Keys                
                if (!(dataHelper.isTablePrimaryKeyField(fl) || dataHelper.isForeignKeyField(fl) || dataHelper.isDisplayKey(fl)))
                {
                    filterFields.Add(fl);
                }
            }
            // Add Primary key at end
            field pkField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
            filterFields.Add(pkField);
            //Then Bind combo to filterFields bindingList - will be comboNumber 0.
            cmbGridFilterFields[comboNumber].BindingContext = new BindingContext();  // Previously Required, but I still use it.
            cmbGridFilterFields[comboNumber].DisplayMember = "DisplayMember";
            cmbGridFilterFields[comboNumber].ValueMember = "ValueMember";  //Entire field
            cmbGridFilterFields[comboNumber].Enabled = true; //Required below (when binding cmbComboTableList)
            tableOptions.writingTable = true;  // Used to cancel events that write the Table
            cmbGridFilterFields[comboNumber].DataSource = filterFields; // Fires change_selectedindex
            tableOptions.writingTable = false;
            comboNumber++; // O goes to 1

            //5b.   Bind one cmbGridFilterFields for each DK and FK - 
            foreach (field fl in currentSql.myFields)
            {
                if (fl.table == currentSql.myTable && (dataHelper.isForeignKeyField(fl) || (dataHelper.isDisplayKey(fl))))
                {
                    if (comboNumber < cmbGridFilterFields.Count())
                    {
                        BindingList<field> dkField = new BindingList<field>();
                        dkField.Add(fl);  // One element only
                        cmbGridFilterFields[comboNumber].Enabled = true;  // Required below (when binding cmbComboTableList)
                        cmbGridFilterFields[comboNumber].DisplayMember = "DisplayMember";
                        cmbGridFilterFields[comboNumber].ValueMember = "ValueMember";  //Entire field
                        tableOptions.writingTable = true; // following calls cmbGridFilterValue_textchanged. "Writing table" cancels re-write.
                        cmbGridFilterFields[comboNumber].DataSource = dkField;
                        tableOptions.writingTable = false;
                        comboNumber++;
                    }
                }
            }

            //6. Restore last grid filters
            GridContextMenu_RestoreFilters_GridFilters();

            //8.  Enable or disable menu items
            GridContextMenu_SetFKasMainFIlter.Enabled = tableOptions.tableHasForeignKeys;
            DataRow[] drs2 = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));

            GridContextMenu_SetAsMainFilter.Enabled = (drs2.Count() > 0);
            mnuForeignKeyMissing.Enabled = true;

            msgTimer(" NewTable: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");
            if (formOptions.runTimer) { watch.Stop(); }

            //9. WriteGrid - next step
            writeGrid_NewFilter(false);
        }

        public void writeGrid_NewFilter(bool newLastFilter)
        {
            RecordLastFilter();
            // 1. Update lastFilter and currentSql.myWheres
            SetSqlWheresFromFilters();
            // Do after above, because on first load cmbCombo filters use Last Filter
            // since the combo filters are not yet added.
            //if (newLastFilter) { 
            //    UpdateLastFilter(); 
            //}

            // 2. Get record Count
            string strSql = currentSql.returnSql(command.count);
            // Error likely caused by manual filter
            try
            {
                currentSql.RecordCount = MsSql.GetRecordCount(strSql);
            }
            catch (Exception e)
            {
                msgTextError(strSql);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(e.Message);
                sb.AppendLine("Sql in the message area. Right click on the area twice to select all and then copy.");
                MessageBox.Show(sb.ToString(), "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 3. Fetch must have an order by clause - so I may add one on first column
            if (currentSql.myOrderBys.Count == 0)  //Should always be true at this point
            {
                orderBy ob = new orderBy(currentSql.myFields[0], System.Windows.Forms.SortOrder.Ascending);
                currentSql.myOrderBys.Add(ob);
            }

            writeGrid_NewPage();
        }

        public void writeGrid_NewPage()
        {
            Stopwatch watch = new Stopwatch(); if (formOptions.runTimer) { watch.Start(); }
            msgTimer("New Page. ");

            // 1. Get the Sql command for grid
            //    CENTRAL and Only USE OF sqlCurrent.returnSql IN PROGRAM
            string strSql = currentSql.returnSql(command.select);

            //2. Clear the grid 
            if (dataGridView1.DataSource != null)
            {
                tableOptions.writingTable = true;
                dataGridView1.DataSource = null;  // Will resize columns
                tableOptions.writingTable = false;
            }
            dataGridView1.Columns.Clear();  // Deleting this results in FK fields not being colored

            msgTimer(" NewPage0: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");

            // 3. Fill currentDT and Bind the Grid to it.
            dataHelper.currentDT = new DataTable();
            string errorMsg = MsSql.FillDataTable(dataHelper.currentDT, strSql);
            if (errorMsg != string.Empty)
            {
                msgTextError(strSql);
                MessageBox.Show(errorMsg, "ERROR in WriteGrid_NewPage", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Bind Grid
            tableOptions.writingTable = true;
            // Add all the columns - doing this manually because it is faster
            for (int i = 0; i < dataHelper.currentDT.Columns.Count; i++)  // currentDT already bound
            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                DataColumn dc = dataHelper.currentDT.Columns[i];
                field fl = currentSql.myFields[i];
                if (dataHelper.isTablePrimaryKeyField(fl) || dataHelper.isForeignKeyField(fl))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    // These have a width of 3 in narrow columns; 
                    if (formOptions.narrowColumns)
                    {
                        column.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        column.DefaultCellStyle.ForeColor = System.Drawing.Color.Navy;
                    }
                }
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                //Change to default width if set in afdFieldData
                int savedWidth = dataHelper.getStoredWidth(fl.table, fl.fieldName, 0);
                if (savedWidth == 0)
                {
                    column.Width = 90;
                }
                else
                {
                    column.Width = savedWidth;
                }
                column.DataPropertyName = dc.ColumnName;
                column.Name = dc.ColumnName;
                column.HeaderText = dc.ColumnName;
                dataGridView1.Columns.Add(column);
            }

            // 4. Replace foreign key colums with FkComboColumn**
            //    This takes about .2 seconds a column - so only do this if editing
            if (programMode == ProgramMode.edit)
            {
                // dataGridView1.AutoGenerateColumns = false;
                int intCols = dataGridView1.ColumnCount;
                for (int i = 0; i < intCols; i++)
                {
                    if (dataHelper.isForeignKeyField(currentSql.myFields[i]))
                    {
                        DataGridViewColumn dgvCol = dataGridView1.Columns[i];
                        int colWidth = dgvCol.Width;
                        string dpn = dgvCol.DataPropertyName;
                        int index = dgvCol.Index;
                        msgTimer("Np" + i.ToString() + " :" + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");
                        dataGridView1.Columns.Remove(dgvCol);
                        FkComboColumn col = new FkComboColumn();
                        col.ValueType = typeof(Int32);
                        col.DataPropertyName = dpn;
                        col.Name = dpn;
                        col.HeaderText = dpn;
                        col.Width = colWidth;
                        tableOptions.writingTable = true;
                        dataGridView1.Columns.Insert(index, col);
                        tableOptions.writingTable = false;
                    }
                }
            }
            msgTimer(" NewPage1: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");

            dataGridView1.DataSource = dataHelper.currentDT;
            tableOptions.writingTable = false;

            // Add columnNames
            for (int i = 0; i < currentSql.myFields.Count; i++)
            {
                currentSql.myFields[i].ColumnName = dataGridView1.Columns[i].DataPropertyName;
            }

            msgTimer(" NewPage2: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");

            //6. Format controls
            // a. Set toolStripButton3 caption
            toolStripButton3.Text = currentSql.myPage.ToString() + "/" + currentSql.TotalPages.ToString();
            // b. Set form caption
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(AppData.databaseName))
            {
                sb.Append(AppData.databaseName + " -  ");
            }

            // StringBuilder sb = new StringBuilder(dbPath.Substring(dbPath.LastIndexOf("\\") + 1));
            sb.Append(dgvHelper.TranslateString(currentSql.myTable));
            string strRowsAndPage = String.Format(MyResources.XrowsPageY, currentSql.RecordCount.ToString(), currentSql.myPage.ToString());
            sb.Append("    -     " + strRowsAndPage);
            this.Text = sb.ToString();
            // c. Show correct orderby glyph
            if (currentSql.myOrderBys.Count > 0)
            {
                field fldob = currentSql.myOrderBys[0].fld;
                int gridColumn = currentSql.myFields.FindIndex(x => x == fldob);  // gridColumn index = myFields index
                System.Windows.Forms.SortOrder sortOrder = currentSql.myOrderBys[0].sortOrder;
                dataGridView1.Columns[gridColumn].SortMode = DataGridViewColumnSortMode.Programmatic;
                dataGridView1.Columns[gridColumn].HeaderCell.SortGlyphDirection = sortOrder;
            }

            // e. Format the rest of the grid and form
            dgvHelper.SetHeaderColorsOnWritePage(dataGridView1, currentSql.myTable, currentSql.myFields);// Must do everytime we re-write grid
            SetColumnsReadOnlyProperty(); // Might change editable column
            SetFiltersColumnsTablePanel();   // Because Write_NewPage may require changes
            // A plugin might add translations and a translationCultureName
            // Use the translation if it equals to uiCulture (might change to first 2 letters the same)
            tableOptions.writingTable = true;
            dgvHelper.TranslateHeaders(dataGridView1);
            tableOptions.writingTable = false;
            if (tableOptions.firstTimeWritingTable)
            {
                ColorComboBoxes();   // Must do after the above for some reason
            }

            msgTimer(" NewPage5: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");


            //7. Restore any cmbComboValues if the first time loading table
            tableOptions.writingTable = true;
            if (tableOptions.firstTimeWritingTable && tableOptions.tableHasForeignKeys)
            {
                tableOptions.firstTimeWritingTable = false;
                // Bind all cmbComboFieldValue combos - this does not get done in Write_NewTable because grid not yet loaded
                // I suspect I could all the combo filter things here and then show rows, but this works 
                tableOptions.writingTable = false;  // the switch that makes the following do the binding
                BindAllComboFilterValueCombos();
                // This will color the cmbComboFieldFields and Values.
                // I have already added any lastFilter cmbComboFieldValues to the currentSql.myWheres
                // This will put them in the combobox values
                // GridContextMenu_RestoreFilters_ComboFilters();
            }
            tableOptions.writingTable = false;
            if (formOptions.runTimer) { watch.Stop(); }
            ; msgTimer(" NewPage6: " + Math.Round(watch.Elapsed.TotalMilliseconds, 2).ToString() + ". ");
        }

        private bool RecordLastFilter()
        {
            bool filterRowChanged = false;
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            bool newLastFilter = true;
            string whValue = string.Empty;  // Default

            // 2.  Get last filter
            //     Created lastFilterDR on table load, and so this should never be null.
            DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", currentSql.myTable, dataHelper.lastFilterValuesDT);

            // 3. cmbGridFilterFields - Currently 9.
            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                if (cmbGridFilterFields[i].Enabled)
                {
                    //cmbGridFilterFields - something is selected (SelectedIndex > -1) and all values are fields 
                    field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                    // For ComboStyle.DropDown the text is the value 
                    if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDown)
                    {
                        // User can add value -
                        // Add second clause because selected value might be String.Empty (?)
                        if (cmbGridFilterValue[i].Text != string.Empty || cmbGridFilterValue[i].SelectedIndex > 0)
                        {
                            whValue = cmbGridFilterValue[i].Text;
                        }
                        else
                        {
                            whValue = string.Empty;
                        }
                    }
                    // ComboBoxStyle.DropDownList have integer values;
                    else if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDownList)  // Only other option
                    {
                        if (cmbGridFilterValue[i].SelectedIndex > 0) // 0 is the pseudo item
                        {
                            whValue = cmbGridFilterValue[i].SelectedValue.ToString();
                        }
                        else
                        {
                            whValue = "0";
                        }
                    }
                    if (dataHelper.getColumnValueinDR(lastFilterDR, "cGFF" + i.ToString()) != selectedField.fieldName
                        || dataHelper.getColumnValueinDR(lastFilterDR, "cGFV" + i.ToString()) != whValue)
                    {
                        dataHelper.setColumnValueInDR(lastFilterDR, "cGFF" + i.ToString(), selectedField.fieldName);
                        dataHelper.setColumnValueInDR(lastFilterDR, "cGFV" + i.ToString(), whValue);
                        filterRowChanged = true;
                    }
                }
            }

            // 4. cmbComboFilterFields - Currently 6.  These are all string values; no FK
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (cmbComboFilterValue[i].Enabled)  // True off visible
                {
                    if (cmbComboFilterValue[i].DataSource != null)  // Probably not needed but just in case 
                    {
                        // ComboFV is a non-PK non-FK.  The selected object is the text
                        field comboFF = (field)cmbComboFilterValue[i].Tag;
                        whValue = cmbComboFilterValue[i].Text;
                        // Don't write a bad whValue
                        if (dataHelper.TryParseToDbType(whValue, comboFF.dbType))
                        {
                            if (dataHelper.getColumnValueinDR(lastFilterDR, "lCFF" + i.ToString()) != comboFF.fieldName
                                || dataHelper.getColumnValueinDR(lastFilterDR, "cCFV" + i.ToString()) != whValue)
                            {
                                dataHelper.setColumnValueInDR(lastFilterDR, "lCFF" + i.ToString(), comboFF.fieldName);
                                dataHelper.setColumnValueInDR(lastFilterDR, "cCFV" + i.ToString(), whValue);
                                filterRowChanged = true;
                            }
                        }
                    }
                }
            }
            // cmbComboTableList
            if (cmbComboTableList.SelectedIndex > -1)
            {
                field fl = (field)cmbComboTableList.SelectedValue;
                whValue = fl.fieldName;
            }
            else
            {
                whValue = String.Empty;
            }
            if (dataHelper.getColumnValueinDR(lastFilterDR, "cCTL") != whValue)
            {
                dataHelper.setColumnValueInDR(lastFilterDR, "cCTL", whValue);
                filterRowChanged = true;
            }
            //Manual filter
            whValue = txtManualFilter.Text;

            if (dataHelper.getColumnValueinDR(lastFilterDR, "manualFilter") != whValue)
            {
                dataHelper.setColumnValueInDR(lastFilterDR, "manualFilter", whValue);
                filterRowChanged = true;
            }
            return filterRowChanged;
        }

        private void SetSqlWheresFromFilters()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            //1. Clear any old filters from currentSql
            currentSql.myWheres.Clear();
            currentSql.strManualWhereClause = string.Empty;

            //2. Main filter - add this where to the currentSql)
            if (programMode != ProgramMode.add && cmbMainFilter.Enabled && cmbMainFilter.SelectedIndex != cmbMainFilter.Items.Count - 1)
            {
                where mfWhere = (where)cmbMainFilter.SelectedValue;
                if (Convert.ToInt32(mfWhere.whereValue) > 0)
                {
                    // Check that the table and field is in the myFields
                    field PkField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                    if (currentSql.MainFilterTableIsInMyTable(mfWhere.fl.table, out string tableAlias))
                    {
                        mfWhere.fl.tableAlias = tableAlias;
                        currentSql.myWheres.Add(mfWhere);
                    }
                }
            }

            //3. cmbGridFilterFields - Currently 9.
            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                if (cmbGridFilterFields[i].Enabled)
                {
                    //cmbGridFilterFields - something is selected (selectedIndex>-1) and all values are fields
                    field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                    string whValue = string.Empty;
                    bool addWhere = false;
                    // On ProgramMode.add, we only filter on displayKeys.
                    // In other modes, we filter on all of these
                    if (programMode != ProgramMode.add || dataHelper.isDisplayKey(selectedField))
                    {
                        // Step 1.  Determine if filter x has a value, and if not, clear old value from lastFilter row
                        // ComboBoxStyle.DropDownList have integer values;
                        // For ComboStyle.DropDown the text is the value 
                        if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDown)
                        {
                            // User can add value - add second clause because selected value index 1 might be String.Empty
                            if (cmbGridFilterValue[i].Text != string.Empty || cmbGridFilterValue[i].SelectedIndex > 0)
                            {
                                whValue = cmbGridFilterValue[i].Text;
                                addWhere = true;
                            }
                        }
                        else
                        {
                            if (cmbGridFilterValue[i].SelectedIndex > 0) // 0 is the pseudo item
                            {
                                whValue = cmbGridFilterValue[i].SelectedValue.ToString();
                                addWhere = true;
                            }
                        }

                        // Step 2.  If filter x has a value, add where and write value to lastFilter table
                        if (addWhere)
                        {
                            where wh = new where(selectedField, whValue);
                            if (dataHelper.TryParseToDbType(wh.whereValue, selectedField.dbType))
                            {
                                currentSql.myWheres.Add(wh);
                            }
                            else
                            {
                                string erroMsg = String.Format(dataHelper.errMsg, dataHelper.errMsgParameter1, dataHelper.errMsgParameter2);
                                msgTextError(erroMsg);
                            }
                        }
                    }
                }
            }
            //4. cmbComboFilterFields - Currently 6. -  Better to update last filter and just use it.
            // First time writing the table, combos not filled - so use the lastFilterDR
            if (tableOptions.firstTimeWritingTable)
            {
                //Get last filter
                DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", currentSql.myTable, dataHelper.lastFilterValuesDT);
                if (lastFilterDR != null)  // Never true, but just in case
                {
                    // ComboFilterValue - set to empty string
                    for (int i = 0; i < cmbComboFilterValue.Length; i++)
                    {
                        if (cmbComboFilterValue[i].Enabled)
                        {
                            string cCFVi = dataHelper.getColumnValueinDR(lastFilterDR, "cCFV" + i.ToString());
                            if (cCFVi != String.Empty)
                            {
                                field comboFF = (field)cmbComboFilterValue[i].Tag;
                                where wh = new where(comboFF, cCFVi);
                                if (dataHelper.TryParseToDbType(wh.whereValue, comboFF.dbType))
                                {
                                    currentSql.myWheres.Add(wh);
                                }
                                else
                                {
                                    string erroMsg = String.Format(dataHelper.errMsg, dataHelper.errMsgParameter1, dataHelper.errMsgParameter2);
                                    msgTextError(erroMsg);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < cmbComboFilterValue.Length; i++)
                {
                    if (cmbComboFilterValue[i].Enabled)  // True off visible
                    {
                        if (cmbComboFilterValue[i].DataSource != null)  // Probably not needed but just in case 
                        {
                            // ComboFV is a non-PK non-FK.  The selected object is the text
                            if (cmbComboFilterValue[i].Text != String.Empty)
                            {
                                field comboFF = (field)cmbComboFilterValue[i].Tag;
                                where wh = new where(comboFF, cmbComboFilterValue[i].Text);
                                if (dataHelper.TryParseToDbType(wh.whereValue, comboFF.dbType))
                                {
                                    currentSql.myWheres.Add(wh);
                                }
                                else
                                {
                                    string erroMsg = String.Format(dataHelper.errMsg, dataHelper.errMsgParameter1, dataHelper.errMsgParameter2);
                                    msgTextError(erroMsg);
                                }
                            }
                        }
                    }
                }
            }
            currentSql.strManualWhereClause = txtManualFilter.Text;
        }

        internal void SetToStoredColumnWidths()
        {
            // dataGridView1.RowHeadersWidth = 27; //default
            for (int i = 0; i <= dataGridView1.ColumnCount - 1; i++)
            {
                string fld = currentSql.myFields[i].fieldName;
                string baseTable = currentSql.myFields[i].table;
                //Change to default width if set in afdFieldData
                int currentWidth = dataGridView1.Columns[i].Width;
                int savedWidth = dataHelper.getStoredWidth(baseTable, fld, currentWidth);
                // Time consuming for transcript - so seek to avoid
                if (savedWidth != currentWidth)
                {
                    dataGridView1.Columns[i].Width = savedWidth;
                }
            }

        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Setting up Filters, Colors, TablePanel
        private void SetTableLayoutPanelHeight()
        {
            // tableLayoutPanel.RowStyles[4] = new RowStyle(SizeType.Absolute, 19);
            int height = 2;
            if (programMode != ProgramMode.none) //2nd Row
            {
                height = cmbGridFilterFields_0.Top + cmbGridFilterFields_0.Height + 10;
            }
            if (cmbGridFilterFields_0.Enabled)  // 3rd row
            {
                height = cmbGridFilterFields_0.Top + cmbGridFilterFields_0.Height + 10;
            }
            if (cmbGridFilterFields_3.Enabled)  // 4th row
            {
                height = cmbGridFilterFields_3.Top + cmbGridFilterFields_3.Height + 10;
            }
            if (cmbGridFilterFields_6.Enabled)  // 5th row
            {
                height = cmbGridFilterFields_6.Top + cmbGridFilterFields_6.Height + 10;
            }

            if (txtManualFilter.Enabled && programMode != ProgramMode.none)  // First always true as of now
            {
                height = txtManualFilter.Top + txtManualFilter.Height + 10;
            }

            if (cmbComboTableList.Enabled)  // 7th row
            {
                height = cmbComboTableList.Top + cmbComboTableList.Height + 10;
                if (cmbComboFilterValue_3.Enabled)  // 8th row
                {
                    height = cmbComboFilterValue_3.Top + cmbComboFilterValue_3.Height + 10;
                }
            }

            tableLayoutPanel.Height = height;
            if (splitContainer1.Width > 0)
            {
                // Catching error: "Splitterdistance must be between panel1minsize and width and pan2minsize.
                try
                {
                    this.splitContainer1.SplitterDistance = txtMessages.Height + tableLayoutPanel.Height;
                }
                catch { }
            }
            // New
            dataGridView1.Height = splitContainer1.Panel2.Height;
        }

        private void SetColumnsReadOnlyProperty()
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.ReadOnly = true;  // default    
                if (programMode == ProgramMode.edit)  // 0 is the "Select column" choice
                {
                    int columnIndex = dataGridView1.Columns.IndexOf(col);
                    field colField = currentSql.myFields[columnIndex];
                    if (colField.table == currentSql.myTable &&
                        !dataHelper.isTablePrimaryKeyField(colField) &&
                        (!dataHelper.isDisplayKey(colField)) || tableOptions.allowDisplayKeyEdit)
                    {
                        (string, string) field = (colField.table, colField.fieldName);
                        if (!dgvHelper.readOnlyField.Contains(field))
                        {
                            col.ReadOnly = false;
                            // Make sure it is visible
                            if (col.Width < 62)
                            {
                                col.Width = 62;
                            }
                        }
                    }
                }
            }
        }

        private void ColorComboBoxes()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                // Color all 12 "Filter Grid" combos
                if (cmbGridFilterFields[i].Enabled == true)
                {
                    field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                    for (int j = 0; j < currentSql.myFields.Count; j++)
                    {
                        field colField = currentSql.myFields[j];
                        if (selectedField.key.Equals(colField.key))
                        {
                            if (dataGridView1.Columns.Count > 0)
                            {
                                cmbGridFilterValue[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                                cmbGridFilterFields[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                            }
                        }
                    }
                }
            }

            //Color the combo filter labels and combos - only used on NewTable first load and perhaps could be eliminated
            for (int i = 0; i < lblCmbFilterFields.Length; i++)
            {
                // Color combobox the same as corresponding header
                if (lblCmbFilterFields[i].Enabled == true)
                {
                    field selectedField = (field)cmbComboFilterValue[i].Tag; // Set in indexChange event
                    if (selectedField != null)
                    {
                        for (int j = 0; j < currentSql.myFields.Count; j++)
                        {
                            field colField = currentSql.myFields[j];
                            if (selectedField.key.Equals(colField.key))
                            {
                                if (dataGridView1.Columns.Count > 0)
                                {
                                    lblCmbFilterFields[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                                    cmbComboFilterValue[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Results of this coloring use in color combo boxes above
        // Called when Write_NewPage (near end),
        // Also called by Open connection, Load Form, Change of radio button (View, Edit, etc)
        // This calls SetColumnsReadOnlyProperty(); SetSelectedCellsColor(); SetTableLayoutPanelHeight();
        private void SetFiltersColumnsTablePanel()
        {
            if (currentSql != null && currentSql.strManualWhereClause != String.Empty)
            {
                EnableMainFilter(false);
                EnableGridFiltersAndSetStyle(false);
                EnableComboFilters(false);
            }
            else
            {
                switch (programMode)
                {
                    case ProgramMode.none:
                        EnableMainFilter(false);
                        EnableGridFiltersAndSetStyle(false);
                        EnableComboFilters(false);
                        break;
                    case ProgramMode.view:
                        EnableMainFilter(true);
                        EnableGridFiltersAndSetStyle(true);
                        EnableComboFilters(true);
                        break;
                    case ProgramMode.edit:
                        EnableMainFilter(true);
                        EnableGridFiltersAndSetStyle(true);
                        EnableComboFilters(true);
                        break;
                    case ProgramMode.delete:
                        EnableMainFilter(true);
                        EnableGridFiltersAndSetStyle(true);
                        EnableComboFilters(true);
                        break;
                    case ProgramMode.add:
                        EnableMainFilter(true);
                        EnableGridFiltersAndSetStyle(true);
                        EnableComboFilters(true);
                        break;
                    case ProgramMode.merge:
                        EnableMainFilter(true);
                        EnableGridFiltersAndSetStyle(true);
                        EnableComboFilters(true);
                        break;
                }
            }
            // All modes
            SetColumnsReadOnlyProperty();
            SetSelectedCellsColor();
            SetTableLayoutPanelHeight();
        }

        private void EnableMainFilter(bool enable)
        {
            // Remove Past filters for "programMode.none"
            if (!enable)
            {
                if (programMode == ProgramMode.none)
                {
                    while (MainFilterList.Count > 1) { MainFilterList.RemoveAt(MainFilterList.Count - 2); }
                }
                cmbMainFilter.Enabled = false;
            }
            else
            {
                if (MainFilterList.Count > 1)
                {
                    cmbMainFilter.Enabled = true;  // The Last item is the dummy field
                }
                if (MainFilterList.Count > 10) { MainFilterList.RemoveAt(9); }  // #10 is the dummy
            }
        }

        private void EnableGridFiltersAndSetStyle(bool enable)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            for (int i = 0; i < cmbGridFilterFields.Count(); i++)
            {
                if (cmbGridFilterFields[i].Items.Count == 0 || enable == false)
                {
                    cmbGridFilterFields[i].Visible = false;
                    cmbGridFilterFields[i].Enabled = false;
                    cmbGridFilterValue[i].Visible = false;
                    cmbGridFilterValue[i].Enabled = false;
                }
                else
                {
                    cmbGridFilterFields[i].Visible = true;
                    cmbGridFilterFields[i].Enabled = true;
                    cmbGridFilterValue[i].Visible = true;
                    cmbGridFilterValue[i].Enabled = true;
                    // Change Style - PK/FK --> DropDownlist, others-->DropDown
                    field gridFF = (field)cmbGridFilterFields[i].SelectedValue;  // Fill cmbGridFilterFieldList before this
                    if (gridFF != null)
                    {
                        if (dataHelper.isForeignKeyField(gridFF) || dataHelper.isTablePrimaryKeyField(gridFF))
                        {
                            if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDown)
                            {
                                cmbGridFilterValue[i].DropDownStyle = ComboBoxStyle.DropDownList;
                                cmbGridFilterValue[i].FlatStyle = FlatStyle.Popup;
                            }
                        }
                        else
                        {
                            if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDownList)
                            {
                                cmbGridFilterValue[i].DropDownStyle = ComboBoxStyle.DropDown;
                                cmbGridFilterValue[i].FlatStyle = FlatStyle.Standard;
                            }
                        }
                    }
                }
            }
        }

        private void EnableComboFilters(bool enable)
        {
            // Combo filter selection combo - cmbComboTableList_SelectionChange enables/disables GridFFcombos
            if (cmbComboTableList.Items.Count == 0 || enable == false)
            {
                cmbComboTableList.Enabled = false;
                cmbComboTableList.Visible = false;
            }
            else
            {
                cmbComboTableList.Enabled = true;
                cmbComboTableList.Visible = true;
            }

            // Six Combo filters - enable/disable GridFVcombos
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            // Keep in use labels visible, but disable to txtBox - and don't filter on them when disabled
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (lblCmbFilterFields[i].Visible && enable == true)
                {
                    cmbComboFilterValue[i].Enabled = true;
                }
                else
                {
                    cmbComboFilterValue[i].Enabled = false;
                }
            }
        }

        public void ClearFilters(bool clearManualFilter)
        {
            // Main filter - only clear for context menu request to clear all
            if (cmbMainFilter.Items.Count > 0)
            {
                if (!tableOptions.writingTable)
                {
                    // formOptions.loadingMainFilter = true; // Prevent event from firing
                    cmbMainFilter.SelectedIndex = cmbMainFilter.Items.Count - 1;
                    // formOptions.loadingMainFilter = false;
                }
            }
            // Manual filter
            if (clearManualFilter)
            {
                txtManualFilter.Text = String.Empty;
            }

            // Grid filters
            ClearAllGridFilters();

            // Combo filters -
            tableOptions.doNotRebindGridFV = true;
            ClearAllComboFilters(false);
            tableOptions.doNotRebindGridFV = false;
        }

        private void ClearAllGridFilters()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

            // If loading the table, include the datasources and hide filter
            if (tableOptions.writingTable)
            {
                // Clear label fields
                for (int i = 0; i < cmbGridFilterFields.Length; i++)
                {
                    cmbGridFilterFields[i].DataSource = null;
                    cmbGridFilterFields[i].Items.Clear();
                    cmbGridFilterFields[i].Visible = false;
                    cmbGridFilterFields[i].Enabled = false;
                    cmbGridFilterFields[i].Text = string.Empty;
                }

                // Clear the value fields 
                for (int i = 0; i < cmbGridFilterValue.Length; i++)
                {
                    cmbGridFilterValue[i].DataSource = null;
                    cmbGridFilterValue[i].Items.Clear();
                    if (tableOptions.writingTable)
                    {
                        cmbGridFilterValue[i].Visible = false;
                        cmbGridFilterValue[i].Enabled = false;
                        cmbGridFilterValue[i].Text = string.Empty;
                    }
                }
            }
            else
            {
                // Set value to the first item
                for (int i = 0; i < cmbGridFilterValue.Length; i++)
                {
                    if (cmbGridFilterValue[i].Visible)
                    {
                        if (cmbGridFilterValue[i].Items.Count > 0)  // Should always be true here
                        {
                            cmbGridFilterValue[i].SelectedIndex = 0;
                        }
                    }
                }

            }

        }
        private void ClearAllComboFilters(bool changingComboFilterTable)
        {
            // When calling this, set tableOptions.doNotRebindGridFV = true;

            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            if (tableOptions.writingTable || changingComboFilterTable)
            {
                for (int i = 0; i < cmbComboFilterValue.Length; i++)
                {
                    // Selection_change event of cmbComboTableList will make some visible
                    // Visible labels will continue to visible for the table, but cmbBoxes may be 
                    cmbComboFilterValue[i].DataSource = null;
                    lblCmbFilterFields[i].Text = String.Empty;
                    cmbComboFilterValue[i].Visible = false;
                    cmbComboFilterValue[i].Enabled = false;
                    lblCmbFilterFields[i].Visible = false;
                    lblCmbFilterFields[i].Enabled = false;
                    cmbComboFilterValue[i].Text = String.Empty;   // No major event when this changed
                }
            }
            else
            {
                for (int i = 0; i < cmbComboFilterValue.Length; i++)
                {
                    if (cmbComboFilterValue[i].Visible)
                    {
                        if (cmbComboFilterValue[i].Items.Count > 0)  // Should always be true here
                        {
                            cmbComboFilterValue[i].SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region EVENTS - Main Menu events

        private void mnuPrintCurrentTable_Click(object sender, EventArgs e)
        {

            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Excel Files (*.xlsx)|*.xlsx";
            saveFileDialog1.DefaultExt = "xlsx";
            saveFileDialog1.AddExtension = true;
            if (currentSql != null && MsSql.cn != null)
            {
                saveFileDialog1.FileName = String.Format("{0}--{1}--Page_{2}", MsSql.cn.Database, dgvHelper.TranslateString(currentSql.myTable), currentSql.myPage.ToString());
            }
            if (!String.IsNullOrEmpty(formOptions.excelFilesFolder))
            {
                saveFileDialog1.InitialDirectory = formOptions.excelFilesFolder;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                formOptions.excelFilesFolder = Path.GetDirectoryName(fileName);
                Print_to_Excel.datagridview_to_Exel(fileName, dataGridView1);

            }
        }

        private void mnuToolsMenuStripItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripItem gtsi in GridContextMenu.Items)
            {
                if (gtsi is ToolStripMenuItem)
                {
                    ToolStripMenuItem tsi = (ToolStripMenuItem)sender;
                    if (tsi.Tag == gtsi.Tag)
                    {
                        gtsi.PerformClick();
                    }
                }
            }
        }

        private void mnuClose_Click(object sender, EventArgs e)
        {
            AppData.StoreFormOptions(formOptions);
            this.Close();
        }

        private void mnuForeignKeyMissing_Click(object sender, EventArgs e)
        {
            // Get list of Non-foriegn keys - with perhaps one intended as foreign key
            DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}' AND is_PK = 'False' AND is_FK = 'False' AND DataType = 'int'", currentSql.myTable));
            if (drs.Count() > 0)
            {
                // 1. Get user choice for proposed FK
                List<string> columnList = new List<string>();
                foreach (DataRow dr in drs)
                {
                    columnList.Add(dr["ColumnName"].ToString());
                }
                frmListItems nonFK_ListForm = new frmListItems();
                nonFK_ListForm.myList = columnList;
                nonFK_ListForm.myJob = frmListItems.job.SelectString;
                nonFK_ListForm.Text = "Select column you want to make a foreign key";
                nonFK_ListForm.ShowDialog();
                string selectedNonFK = nonFK_ListForm.returnString;
                int selectedNonFKIndex = nonFK_ListForm.returnIndex;
                nonFK_ListForm = null;
                if (selectedNonFKIndex > -1 && !String.IsNullOrEmpty(selectedNonFK))
                {
                    // 2. Get user choice for proposed Ref Table
                    DataRow[] drs2 = dataHelper.fieldsDT.Select("is_PK = 'True'");
                    List<string> refTableList = new List<string>();
                    foreach (DataRow dr2 in drs2)
                    {
                        refTableList.Add(dr2["TableName"].ToString());
                    }
                    frmListItems RefTable_ListForm = new frmListItems();
                    RefTable_ListForm.myList = refTableList;
                    RefTable_ListForm.myJob = frmListItems.job.SelectString;
                    RefTable_ListForm.Text = "Select Reference Table";
                    RefTable_ListForm.ShowDialog();
                    string selectedRefTable = RefTable_ListForm.returnString;
                    int selectedRefTableIndex = RefTable_ListForm.returnIndex;
                    RefTable_ListForm = null;
                    if (selectedRefTableIndex > -1)
                    {
                        DataRow dr = drs[selectedNonFKIndex];  // Index in form same as index in drs.
                        // Get two inner join fields
                        field refField = dataHelper.getTablePrimaryKeyField(selectedRefTable);
                        field nonFkField = dataHelper.getFieldFromFieldsDT(currentSql.myTable, selectedNonFK);
                        StringBuilder sbWhere = new StringBuilder();
                        sbWhere.Append(" (NOT EXISTS (SELECT ");
                        sbWhere.Append(refField.fieldName + " FROM " + refField.table + " AS " + refField.tableAlias);
                        sbWhere.Append(" WHERE ");
                        sbWhere.Append(dataHelper.QualifiedAliasFieldName(refField) + " = " + dataHelper.QualifiedAliasFieldName(nonFkField));
                        sbWhere.Append("))");
                        txtManualFilter.Text = sbWhere.ToString(); // Will change currentSql.returnSql();
                        msgText(Properties.MyResources.eachOfDisplayedRowsHasEmptyFK + "  " + Properties.MyResources.ifNoDisplayedRowsNoEmptyFK);
                        writeGrid_NewFilter(false);
                    }
                }
            }
        }

        private void mnuToolDuplicateDisplayKeys_Click(object sender, EventArgs e)
        {
            // Get display key list of strings from fields table
            if (currentSql == null) { return; }
            showDuplicateDispayKeys();
        }

        private void showDuplicateDispayKeys()
        {
            if (currentSql == null) { return; }

            String filter = String.Format("TableName = '{0}' and is_DK = 'true'", currentSql.myTable);
            DataRow[] drsDKFieldsDT = dataHelper.fieldsDT.Select(filter);

            if (drsDKFieldsDT.Count() == 0)
            {
                msgText("No display keys!", true, false);
                mnuMergeDuplicateDKs.Checked = false;
                return;
            }

            List<String> dkFields = new List<String>();
            foreach (DataRow row in drsDKFieldsDT)
            {
                dkFields.Add(row["ColumnName"].ToString());
            }
            string fields = String.Join(",", dkFields);
            string fields2 = fields + ", Count(*)";
            String sqlSelectDuplicates = String.Format("Select {0} From {1} Group By {2} Having Count(*) > 1", fields2, currentSql.myTable, fields);
            MsSqlWithDaDt DaDt = new MsSqlWithDaDt(sqlSelectDuplicates);
            if (DaDt.errorMsg != string.Empty)
            {
                MessageBox.Show(DaDt.errorMsg, "ERROR in mnuToolDuplicateDisplayKeys_Click");
                return;
            }
            else if (DaDt.dt.Rows.Count == 0)
            {
                msgText(Properties.MyResources.EverythingOkNoDuplicates, true, false);
                mnuMergeDuplicateDKs.Checked = false; // Reset 
                return;
            }
            msgText("Count: " + DaDt.dt.Rows.Count.ToString());

            List<String> andConditions = new List<String>();
            foreach (DataRow row in DaDt.dt.Rows)
            {
                List<String> atomicStatements = new List<String>();
                foreach (string dkField in dkFields)
                {
                    field fl = new field(currentSql.myTable, dkField, DbType.String, 0);
                    String atomicStatement = String.Format("({0} = '{1}' OR {0} IS NULL)", dataHelper.QualifiedAliasFieldName(fl), row[dkField].ToString());
                    atomicStatements.Add(atomicStatement);
                }
                andConditions.Add("(" + String.Join(" AND ", atomicStatements) + ")");
                if (mnuMergeDuplicateDKs.Checked)
                {
                    break;  // Break after first row - i.e. fix one by one
                }
            }
            string whereCondition = String.Join(" OR ", andConditions);
            txtManualFilter.Text = whereCondition;
            writeGrid_NewFilter(false);
        }

        private void mnuToolsDatabaseInformation_Click(object sender, EventArgs e)
        {
            frmDatabaseInfo formDBI = new frmDatabaseInfo();
            formDBI.ShowDialog();
        }

        private void mnuDeleteConnectionString_Click(object sender, EventArgs e)
        {
            List<connectionString> csList = AppData.GetConnectionStringList();
            List<string> strCsList = new List<string>();
            foreach (connectionString cs in csList)
            {
                string strCS = String.Format(cs.comboString, cs.server, cs.databaseName, cs.user, "*******");
                strCsList.Add(strCS);
            }
            frmListItems databaseListForm = new frmListItems();
            databaseListForm.myList = strCsList;
            databaseListForm.myJob = frmListItems.job.DeleteConnections;
            databaseListForm.ShowDialog();
            string returnString = databaseListForm.returnString;
            databaseListForm = null;
            if (returnString != null)
            {
                foreach (connectionString cs in csList)
                {
                    string strCS = String.Format(cs.comboString, cs.server, cs.databaseName, cs.user, "*******");
                    if (strCS == returnString)
                    {
                        csList.Remove(cs);
                        break;   // Only remove the first one or you will get an error
                    }
                }
                AppData.storeConnectionStringList(csList);
                Load_mnuDatabaseList();
            }
        }

        private void mnuOpenTables_Click(object sender, EventArgs e) { }

        private void mnuOpenTables_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string tableName = e.ClickedItem.Name;  // the text can be translated.  Set Name = Text when adding ToolStripMenuItem
            //Open a new table
            writeGrid_NewTable(tableName, true);
        }

        private void mnuDatabaseList_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Get index clicked
            ToolStripItem clickedItem = e.ClickedItem;
            int index = 0;
            ToolStripMenuItem? fatherItem = sender as ToolStripMenuItem;
            if (fatherItem != null)
            {
                for (int i = 0; i < fatherItem.DropDownItems.Count; i++)
                {
                    if (fatherItem.DropDownItems[i] == clickedItem)
                    {
                        index = i; break;
                    }
                }
            }

            string readOnly = "", connectionString = "", str = "", tye = "", sd = "";
            //1. Change clicked link to index 0 - index 0 used to open connection
            if (index > 0)  //No need to change if already 0
            {
                List<connectionString> csList = AppData.GetConnectionStringList();
                connectionString cs = csList[index];   // Assumes list in dropdown matches csList
                csList.Remove(cs);
                csList.Insert(0, cs);
                AppData.storeConnectionStringList(csList);
            }
            //2. Open connection - this reads the index 0 settings.  Main use of openConnection
            //string msg = OpenConnection();
            //if (msg != string.Empty) { msgTextError(msg); }
            Application.Restart();
        }

        // Add Database - frmConnection
        internal void mnuAddDatabase_Click(Object eventSender, EventArgs eventArgs)
        {
            frmConnection connectionForm = new frmConnection();
            //Get connection string
            connectionForm.ShowDialog();
            bool connectionAdded = connectionForm.success;
            connectionForm.Close();
            //Store values, reload menu, open connection
            if (connectionAdded)
            {
                //load_mnuDatabaseList();
                //string msg = OpenConnection();
                //if (msg != string.Empty) { msgTextError(msg); }
                Application.Restart();
            }
        }
        internal void Load_mnuDatabaseList()
        {
            //Get list from App.Data
            List<connectionString> csList = AppData.GetConnectionStringList();
            mnuConnectionList.DropDownItems.Clear();
            foreach (connectionString cs in csList)
            {
                // {0} for server, {1} for Database, {2} for user, {3} for password (unknown)
                string csString = String.Format(cs.comboString, cs.server, cs.databaseName, cs.user, "******");
                mnuConnectionList.DropDownItems.Add(csString);
            }
        }

        private void mnuToolsBackupDatabase_Click(object sender, EventArgs e)
        {
            if (MsSql.cn != null)
            {
                // Get folder
                string folder = AppData.GetKeyValue("DatabaseBackupFolder");
                if (string.IsNullOrEmpty(folder) || !Path.Exists(folder))
                {
                    folder = SelectFolder(folder, true);  // Returns string.Empty if dialog canceled
                    if (folder == string.Empty)
                    {
                        return;
                    }
                }

                //Set complete path
                string filename = string.Format("{0}-{1}.bak", MsSql.cn.Database, DateTime.Now.ToString("yyyy-MM-dd-hh-mm"));
                string completeFilePath = Path.Combine(folder, filename);
                DialogResult result = MessageBox.Show(String.Format("Backup database to {0}?", completeFilePath), "Backup Database", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    result = MessageBox.Show("Choose a different folder?", "Change folder", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        folder = SelectFolder(folder, true);
                        if (folder == String.Empty)
                        {
                            return;
                        }
                        completeFilePath = Path.Combine(folder, filename);
                    }
                }
                if (MsSql.BackupCurrentDatabase(completeFilePath))
                {
                    MessageBox.Show(String.Format("Saved Database to {0}.", completeFilePath), "Backup Database");
                }
            }
        }
        private string SelectFolder(string defaultFolder, bool allowNewFolder)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowNewFolderButton = allowNewFolder;
            if (Path.Exists(defaultFolder))
            {
                folderBrowserDialog1.SelectedPath = defaultFolder;
            }
            DialogResult result = folderBrowserDialog1.ShowDialog();
            string fbdPath = folderBrowserDialog1.SelectedPath;
            if (result == DialogResult.Cancel) { return string.Empty; }
            AppData.SaveKeyValue("DatabaseBackupFolder", fbdPath);
            return fbdPath;
        }
        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region EVENTS - Context Menu events
        private void GridContextMenu_OrderComboByPK_Click(object sender, EventArgs e)
        {
            formOptions.orderComboListsByPK = GridContextMenu_OrderCombolByPK.Checked;
            writeGrid_NewTable(currentSql.myTable, false);
        }

        private void GridContextMenu_SetAsMainFilter_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                string value = dataGridView1.SelectedRows[0].Cells[0].Value.ToString(); //Integer
                where newMainFilter = dataHelper.GetMainFilterFromPrimaryKeyValue(currentSql.myTable, value);
                foreach (where wh in MainFilterList)
                {
                    if (wh.isSameWhereAs(newMainFilter)) { MainFilterList.Remove(wh); break; }
                }
                MainFilterList.Insert(0, newMainFilter);
                lblMainFilter.Text = MyResources.mainFilterShort + " " + dgvHelper.TranslateString(currentSql.myTable) + " : ";
                cmbMainFilter.Refresh();
                cmbMainFilter.Enabled = true;
                cmbMainFilter.SelectedIndex = 0;
                // writeGrid_NewFilter(); // Change event will do this
            }
            else
            {
                MessageBox.Show(Properties.MyResources.selectExactlyOneRow, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GridContextMenu_SetFkAsMainFilter_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                List<field> foreignKeys = new List<field>();
                // Get list of Foriegn keys
                for (int i = 0; i < currentSql.myFields.Count; i++)
                {
                    if (currentSql.myFields[i].table == currentSql.myTable && dataHelper.isForeignKeyField(currentSql.myFields[i]))
                    {
                        foreignKeys.Add(currentSql.myFields[i]);
                    }
                }
                if (foreignKeys.Count() > 0)  // always true because we only enable the event if true;
                {
                    List<string> tableList = new List<string>();
                    foreach (field fld in foreignKeys)
                    {
                        tableList.Add(fld.fieldName);
                    }
                    // Get user choice
                    frmListItems ParentTablesForm = new frmListItems();
                    ParentTablesForm.myList = tableList;
                    ParentTablesForm.myJob = frmListItems.job.SelectString;
                    ParentTablesForm.Text = "Select Foreign Key";
                    ParentTablesForm.ShowDialog();
                    string selectedFK = ParentTablesForm.returnString;
                    int selectedFKindex = ParentTablesForm.returnIndex;
                    ParentTablesForm = null;
                    if (selectedFKindex > -1 && !String.IsNullOrEmpty(selectedFK))
                    {
                        // 1.  Get Where for Parent table 
                        field fieldInCurrentTable = foreignKeys[selectedFKindex];
                        string refTableOfSelectedFK = dataHelper.getForeignKeyRefField(fieldInCurrentTable).table;
                        int dgColumnIndex = currentSql.getColumn(fieldInCurrentTable);
                        if (dgColumnIndex > -1)  // A needless check
                        {
                            string whereValue = dataGridView1.SelectedRows[0].Cells[dgColumnIndex].Value.ToString();
                            where newMainFilter = dataHelper.GetMainFilterFromPrimaryKeyValue(refTableOfSelectedFK, whereValue);
                            foreach (where wh in MainFilterList)
                            {
                                if (wh.isSameWhereAs(newMainFilter)) { MainFilterList.Remove(wh); break; }
                            }
                            MainFilterList.Insert(0, newMainFilter);
                            lblMainFilter.Text = MyResources.mainFilterShort + " " + dgvHelper.TranslateString(currentSql.myTable) + " : ";
                            cmbMainFilter.Refresh();
                            cmbMainFilter.Enabled = true;
                            cmbMainFilter.SelectedIndex = 0;
                            // Change event will write new filter
                        }
                        else
                        {
                            MessageBox.Show("Unusual error in Find in Parent table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.MyResources.selectExactlyOneRow, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GridContextMenu_FindUnusedFK_Click(object sender, EventArgs e)
        {
            // Find row which is not used as the Ref row of a foreign key
            if (currentSql != null)
            {
                string sqlString = currentSql.returnSql(command.selectAll);
                MsSqlWithDaDt dadt = new MsSqlWithDaDt(sqlString);
                string errorMsg = dadt.errorMsg;
                if (errorMsg != string.Empty) { MessageBox.Show(errorMsg, "ERROR in GridContextMenu_FindUsedFK_Click"); }
                foreach (DataRow dr in dadt.dt.Rows)
                {
                    // Get PK values
                    int drPKValue = Convert.ToInt32(dr.Field<int>(0));
                    //  Get rows in the fieldsDT that have myTable as RefTable 
                    DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                    // Count how many rows the firstPK occurs as FK's in other tables
                    int firstPKCount = 0;
                    foreach (DataRow drInner in drs)    // Only 1 dr with "Courses" main table, and that is the CourseTerm table.
                    {
                        string FKColumnName = dataHelper.getColumnValueinDR(drInner, "ColumnName");
                        string TableWithFK = dataHelper.getColumnValueinDR(drInner, "TableName");
                        string strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, drPKValue);
                        firstPKCount = firstPKCount + MsSql.GetRecordCount(strSql);
                    }
                    if (firstPKCount == 0)
                    {
                        where newMainFilter = dataHelper.GetMainFilterFromPrimaryKeyValue(currentSql.myTable, drPKValue.ToString());
                        foreach (where wh in MainFilterList)
                        {
                            if (wh.isSameWhereAs(newMainFilter)) { MainFilterList.Remove(wh); break; }
                        }
                        MainFilterList.Insert(0, newMainFilter);
                        cmbMainFilter.Refresh();
                        formOptions.loadingMainFilter = true;  // following will not write grid
                        cmbMainFilter.SelectedIndex = 0;
                        cmbMainFilter.Enabled = true;
                        formOptions.loadingMainFilter = false;
                        writeGrid_NewTable(currentSql.myTable, false);  // Shows one row, which is selected
                        txtMessages.Text = String.Format(Properties.MyResources.selectedRowIsNotValueOfForeignKey);
                        return;
                    }
                }
                msgTextError(Properties.MyResources.allKeysInThisPageAreInUse);
            }
        }

        private void GridContextMenu_TimesUsedAsFK_Click(object sender, EventArgs e)
        {
            // Find row which is not used as the Ref row of a foreign key
            if (currentSql != null)
            {
                if (dataGridView1.SelectedRows.Count != 1)
                {
                    msgTextError(Properties.MyResources.selectExactlyOneRow);
                    return;
                }

                // 1a. This assumes the first column in row is the Primary Key and it is an integer
                string value = dataGridView1.SelectedRows[0].Cells[0].Value.ToString(); //Integer
                int intValue = Convert.ToInt32(value);
                //  Get rows in the fieldsDT that have myTable as RefTable 
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                // Count how many rows the firstPK occurs as FK's in other tables
                int firstPKCount = 0;
                StringBuilder sb = new StringBuilder();
                foreach (DataRow drInner in drs)    // Only 1 dr with "Courses" main table, and that is the CourseTerm table.
                {
                    string FKColumnName = dataHelper.getColumnValueinDR(drInner, "ColumnName");
                    string TableWithFK = dataHelper.getColumnValueinDR(drInner, "TableName");
                    string strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, intValue);
                    int thisTableCount = MsSql.GetRecordCount(strSql);
                    sb.AppendLine(String.Format("{0} : {1} times", dgvHelper.TranslateString(TableWithFK), thisTableCount.ToString()));
                    firstPKCount = firstPKCount + thisTableCount;
                }
                sb.Append(String.Format("Total : {0} times", firstPKCount.ToString()));
                MessageBox.Show(sb.ToString());
            }
        }

        private void GridContextMenu_UnusedAsFK_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                //1.  Get table of unused rows
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                StringBuilder sb = new StringBuilder();
                field pk = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                sb.AppendLine(String.Format("Select [{0}].[{1}] from {0} Where ", pk.table, pk.fieldName));
                List<String> andConditions = new List<String>();
                if (drs.Length > 0)
                {
                    foreach (DataRow row in drs)
                    {
                        String atomicStatement = String.Format("NOT EXISTS (Select * FROM {2} WHERE [{0}].[{1}] = [{2}].[{3}])",
                            pk.table, pk.fieldName, row["TableName"], row["ColumnName"]);
                        andConditions.Add(atomicStatement);
                    }
                    string whereCondition = String.Join(" AND ", andConditions);
                    sb.AppendLine(whereCondition);

                    //1. Get Datatable with ID numbers of unused rows
                    string sqlString = sb.ToString();
                    DataTable dt = new DataTable();
                    MsSql.FillDataTable(dt, sqlString);
                    if (dt.Rows.Count > 0)
                    {
                        List<String> orConditions = new List<String>();
                        foreach (DataRow row in dt.Rows)
                        {
                            String atomicStatement = String.Format("([{0}] = '{1}' OR [{0}] IS NULL)", pk.fieldName, row[pk.fieldName].ToString());
                            orConditions.Add(atomicStatement);
                        }
                        whereCondition = String.Join(" OR ", orConditions);
                        setTxtManualFilterText(whereCondition);
                        writeGrid_NewFilter(false);
                        // Message box
                        String strMessage = "These rows are not being used as a foreign key.";
                        MessageBox.Show(strMessage);
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Every row in {0} is being used as Foreign Key.", currentSql.myTable));
                    }
                }
                else
                {
                    MessageBox.Show(String.Format("{0} not used as Foreign Key.", currentSql.myTable));
                }
            }
        }

        private void GridContextMenu_ClearFilters_Click(object sender, EventArgs e)
        {
            if (tableOptions != null)  // Make sure there is a table selected
            {
                ClearFilters(true);
                writeGrid_NewFilter(false);
            }
        }

        private void GridContextMenu_RestoreFilters_GridFilters()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

            if (tableOptions != null)  // Make sure there is a table selected
            {
                //Get last filter
                DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", currentSql.myTable, dataHelper.lastFilterValuesDT);

                if (lastFilterDR != null)
                {

                    // 1. Clear old mainFilter and Grid filters - comment out 2025.02
                    //formOptions.loadingMainFilter = true;
                    //cmbMainFilter.SelectedIndex = cmbMainFilter.Items.Count - 1;
                    //formOptions.loadingMainFilter = false;

                    // Grid Filter - Set to 1st element - empty string
                    tableOptions.doNotRebindGridFV = true;
                    for (int i = 0; i < cmbGridFilterValue.Length; i++)
                    {
                        if (cmbGridFilterValue[i].Enabled && cmbGridFilterValue[i].Items.Count > 0)
                        {
                            cmbGridFilterValue[i].SelectedItem = cmbGridFilterValue[i].Items[0];
                        }
                    }
                    tableOptions.doNotRebindGridFV = false;

                    // 2. Set the main filter - I have stored the primary key value
                    //    This could produce wrong result if two mainFilters have same whereValue
                    //    I should store both the table and field name.
                    // 2025.02 comment out
                    //string mainFilter = dataHelper.getColumnValueinDR(lastFilterDR, "cMF");
                    //foreach (where item in cmbMainFilter.Items)
                    //{
                    //    // if (item.fl.table == currentSql.myTable && item.whereValue == mainFilter)
                    //    if (item.whereValue == mainFilter)
                    //    {
                    //        formOptions.loadingMainFilter = true;
                    //        cmbMainFilter.SelectedItem = item;
                    //        formOptions.loadingMainFilter = false;
                    //        break;
                    //    }
                    //}

                    //5. Set Grid filter values - most complex
                    for (int i = 0; i < cmbGridFilterFields.Length; i++)
                    {
                        if (cmbGridFilterFields[i].Enabled)
                        {
                            string cGFFi = dataHelper.getColumnValueinDR(lastFilterDR, "cGFF" + i.ToString());
                            string cGFVi = dataHelper.getColumnValueinDR(lastFilterDR, "cGFV" + i.ToString());
                            field fld = null;
                            // cGFFi is a field with displayValue fieldName.  Always the first and only item.
                            // First get this field
                            if (cGFFi != string.Empty)
                            {
                                foreach (field item in cmbGridFilterFields[i].Items)
                                {
                                    if (item.fieldName == cGFFi)
                                    {
                                        cmbGridFilterFields[i].SelectedItem = item;
                                        fld = item;
                                        break;
                                    }
                                }
                            }
                            // Select value may be text or a primary key of the table in the dropdownlist
                            if (fld != null && cGFVi != string.Empty)
                            {
                                if (dataHelper.isDisplayKey(fld))  // May be text or FK
                                {
                                    // Text - note using the text not the PK of the text
                                    if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDown)
                                    {
                                        cmbGridFilterValue[i].Text = cGFVi;
                                    }
                                    else // DropDownList DK - value is always an integer
                                    {
                                        // DropDownList - value is always an integer
                                        foreach (DataRowView vItem in cmbGridFilterValue[i].Items)
                                        {
                                            if (vItem.Row.ItemArray[1] != null)  //? null for first blank item?
                                            {
                                                string value = vItem.Row.ItemArray[1].ToString();  // Should be integer
                                                if (value == cGFVi)
                                                {
                                                    tableOptions.doNotWriteGrid = true;
                                                    cmbGridFilterValue[i].SelectedItem = vItem;
                                                    tableOptions.doNotWriteGrid = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GridContextMenu_RestoreFilters_ComboTableListFilter()
        {
            if (tableOptions != null)  // Make sure there is a table selected
            {
                //Get last filter - just created and so this should not be null
                DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", currentSql.myTable, dataHelper.lastFilterValuesDT);

                if (lastFilterDR != null)
                {

                    // 3. Set the cCTF - stored as field name;  cmbCombotableList values are fields
                    string cCTL = dataHelper.getColumnValueinDR(lastFilterDR, "cCTL");
                    // Restore old value, unless it is the current (first) value
                    if (cmbComboTableList.SelectedItem != cCTL)
                    {
                        foreach (field item in cmbComboTableList.Items)
                        {
                            if (item.fieldName == cCTL)
                            {
                                cmbComboTableList.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool GridContextMenu_RestoreFilters_ComboFilters()
        {
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            bool comboBoxUpdate = false;
            if (tableOptions != null)  // Make sure there is a table selected
            {
                //Get last filter
                DataRow lastFilterDR = dataHelper.getDataRowFromDataTable("Table", currentSql.myTable, dataHelper.lastFilterValuesDT);

                if (lastFilterDR != null)
                {

                    // ComboFilterValue - set to empty string
                    for (int i = 0; i < cmbComboFilterValue.Length; i++)
                    {
                        if (cmbComboFilterValue[i].Enabled) { cmbComboFilterValue[i].Text = string.Empty; }
                    }

                    //// 3. Set the cCTF - stored as field name;  cmbCombotableList values are fields
                    //string cCTL = dataHelper.getColumnValueinDR(lastFilterDR, "cCTL");
                    //foreach (field item in cmbComboTableList.Items)
                    //{
                    //    if (item.fieldName == cCTL)
                    //    {
                    //        cmbComboTableList.SelectedItem = item;
                    //        break;
                    //    }
                    //}
                    // 4. Set combo filters - comboFilters are always the text.  
                    for (int i = 0; i < cmbComboFilterValue.Length; i++)
                    {
                        if (cmbComboFilterValue[i].Enabled)  // True off visible
                        {
                            if (cmbComboFilterValue[i].DataSource != null)  // Probably not needed but just in case 
                            {
                                string cCFVi = dataHelper.getColumnValueinDR(lastFilterDR, "cCFV" + i.ToString());
                                if (cCFVi != String.Empty)
                                {
                                    // ComboFV is a non - PK non - FK - the selected value is the text
                                    cmbComboFilterValue[i].Text = cCFVi;
                                    comboBoxUpdate = true;
                                    dataHelper.setColumnValueInDR(lastFilterDR, "cCFV" + i.ToString(), cCFVi);

                                }
                            }
                        }
                    }
                }
            }
            return comboBoxUpdate;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Events - DatagridView Events

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                MessageBox.Show("Current cell not yet saved. Click any cell to save.", "Change not saved");
            }
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (currentSql != null)
            {
                if (!tableOptions.writingTable)
                {
                    int i = e.Column.Index;
                    field fl = currentSql.myFields[i];
                    dataHelper.storeColumnWidth(fl.table, fl.fieldName, e.Column.Width);
                }
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(",CeDirty:" + dataGridView1.IsCurrentCellDirty.ToString());
            if (dataGridView1.IsCurrentCellDirty)
            {
                DataGridViewColumn col = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex];
                FkComboColumn fkCol = col as FkComboColumn;
                if (fkCol != null)
                {
                    SendKeys.Send("{ENTER}");
                }

            }
        }

        // Update datatable in FkComboEditingControl on entering cell.  
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit)
            {
                field colField = currentSql.myFields[e.ColumnIndex];
                msgText(currentSql.myTable);
                if (dataGridView1.Columns[e.ColumnIndex].ReadOnly)
                {
                    (string, string) field = (colField.table, colField.fieldName);
                    if (!dgvHelper.readOnlyField.Contains(field))
                    {
                        msgText(MyResources.aPluginHasMadeReadOnly);
                    }
                }
                else
                {
                    // Set update command
                    List<field> fieldsToUpdate = new List<field>();
                    fieldsToUpdate.Add(colField);
                    MsSql.SetUpdateCommand(fieldsToUpdate, dataHelper.currentDT);
                    // Handle foreign keys
                    DataGridViewColumn col = dataGridView1.Columns[e.ColumnIndex];
                    FkComboColumn Fkcol = col as FkComboColumn;
                    if (Fkcol != null)  // Will be null for non-foreign key row
                    {
                        field PK_refTable = dataHelper.getForeignKeyRefField(colField);
                        FillComboDT(PK_refTable, comboValueType.PK_refTable);

                        // Assign datatable to each cell in FK column
                        int index = dataHelper.currentDT.Columns.IndexOf(col.Name);
                        for (int j = 0; j < dataHelper.currentDT.Rows.Count; j++)
                        {
                            FkComboCell fkCell = (FkComboCell)dataGridView1.Rows[j].Cells[index];
                            fkCell.dataTable = dataHelper.comboDT;
                        }
                        tableOptions.FkFieldInEditingControl = colField;
                    }
                }
            }
        }

        // When foreign key selected in dropdown, this puts the selectedvalue into the cell.
        private void dataGridView1_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            // Note: CellParsing accepts user input and map it to a different cell value / format.
            if (programMode == ProgramMode.edit) msgDebug(", Cell_Parsing");
            // Using editing control - true for both foreign keys and strings, but false for checkbox changes
            if (dataGridView1.EditingControl != null)
            {
                DataGridViewColumn selectedColumn = dataGridView1.Columns[e.ColumnIndex];
                FkComboColumn Fkcol = selectedColumn as FkComboColumn;
                if (Fkcol != null)
                {
                    FkComboBoxEditingControl editingControl = dataGridView1.EditingControl as FkComboBoxEditingControl;
                    if (editingControl != null)  // Always true ?
                    {
                        int foreignKey = Convert.ToInt32(editingControl.SelectedValue);
                        e.Value = foreignKey;
                        e.ParsingApplied = true;
                        return;
                    }
                }
            }
            e.ParsingApplied = false;
        }

        // Push the change in the cell value down to the database
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CVC");
            if (!tableOptions.writingTable)
            {
                DataGridViewCell currentCell = dataGridView1.CurrentCell;
                field drColField = currentSql.myFields[currentCell.ColumnIndex];
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int pkIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[pkIndex].Value);
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName, PKvalue)).FirstOrDefault();
                if (dataGridView1.IsCurrentCellDirty) // always true
                {
                    dr.EndEdit();  // Changes rowstate to modified
                    DataRow[] drArray = new DataRow[1];
                    drArray[0] = dr;
                    bool constraintPassed = true;
                    foreach (Func<string, string, DataRow, bool> f in dgvHelper.updateConstraints)
                    {
                        constraintPassed = f(drColField.table, drColField.fieldName, dr);
                        if (!constraintPassed) { break; }
                    }
                    int i = 0;
                    if (constraintPassed)
                    {
                        i = MsSql.currentDA.Update(drArray);
                        msgText(Properties.MyResources.numberOfRowsModified + " : ");
                        msgTextAdd(i.ToString());
                        // Write the grid if this is a foreign key
                        if (i > 0)
                        {
                            DataGridViewColumn col = dataGridView1.Columns[currentCell.ColumnIndex];
                            FkComboColumn fkCol = col as FkComboColumn;
                            if (fkCol != null)
                            {
                                writeGrid_NewPage();
                            }
                        }
                    }
                    else
                    {
                        writeGrid_NewPage();
                    }
                }
            }
        }

        // Message to user about data error - cancel error
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("OOPS! Database error.");
            sb.AppendLine(e.Exception.Message);
            sb.AppendLine("Error happened " + e.Context.ToString());

            if (e.Context == DataGridViewDataErrorContexts.Commit)
            {
                sb.AppendLine("Commit error");
            }
            if (e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                sb.AppendLine("Cell change");
            }
            if (e.Context == DataGridViewDataErrorContexts.Parsing)
            {
                sb.AppendLine("Parsing error");
            }
            if (e.Context == DataGridViewDataErrorContexts.LeaveControl)
            {
                sb.AppendLine("Leave control error");
            }

            if ((e.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[e.RowIndex].ErrorText = "an error";
                view.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "an error";
                e.ThrowException = false;
            }

            MessageBox.Show(sb.ToString());

        }

        // Change background color of cell
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", BE");
            // Change colors
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = System.Drawing.Color.Aqua;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        // Restore default background color of cell
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", EE");
            // Change color back to default
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = DefaultBackColor;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        // Add new events to FkComboBoxEditingControl editing control
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            FkComboBoxEditingControl ctl = e.Control as FkComboBoxEditingControl;
            if (ctl != null)
            {
                if (programMode == ProgramMode.edit) msgDebug(", EditControlShowing");
                ctl.DropDown -= new EventHandler(AdjustWidthComboBox_DropDown);
                ctl.DropDown += new EventHandler(AdjustWidthComboBox_DropDown);
            }
        }

        // Only allow 2 rows to be selected - used when merging
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count != 0)
            {
                if (programMode == ProgramMode.merge)
                {
                    if (dataGridView1.SelectedRows.Count > 2)
                    {
                        for (int i = 2; i < dataGridView1.SelectedRows.Count; i++)
                        {
                            dataGridView1.SelectedRows[i].Selected = false;
                        }
                    }
                }
                else if (programMode == ProgramMode.edit)
                {
                    {
                        SetSelectedCellsColor();
                    }
                }
            }
        }

        // Sort grid on column
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            System.Windows.Forms.SortOrder newSortOrder = System.Windows.Forms.SortOrder.Ascending;   // default
            // Check for same column ascending and change to descending  
            if (currentSql.myOrderBys.Count > 0)
            {
                if (currentSql.myOrderBys[0].fld == currentSql.myFields[e.ColumnIndex])  // New column is the same as the old.
                {
                    if (currentSql.myOrderBys[0].sortOrder == System.Windows.Forms.SortOrder.Ascending)
                    {
                        newSortOrder = System.Windows.Forms.SortOrder.Descending;
                    }
                }
            }
            // Update myOrdersBy to sort by newColumn and dir - may be same column
            currentSql.myOrderBys.Clear();
            orderBy ob = new orderBy(currentSql.myFields[e.ColumnIndex], newSortOrder);
            currentSql.myOrderBys.Add(ob);
            currentSql.myPage = 1;
            // Write the grid with the new order - write Grid will format the header cell
            writeGrid_NewPage();
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Events - Grid and combo filter events & related functions

        // (A) Basic ideas
        //      1. Manually changing GridFFcombo will set up GridFVcombo, select the empty-string and rewrite the grid.
        //         Manually changing GridFVcombo will rewrite the grid.
        //         Programmatically changing either of these (by binding datasource) will not write the grid
        //         Set "writingGrid" or "doNotWriteGrid" to true before binding to know the change_selectedIndex event is programmatic.
        //      2. Changing one ComboFVcombo will rebind ALL relevant GridFVcombos (but not write grid).
        //         I set "doNotWriteGrid"=true before rebinding
        //      3. Changing ComboControl will rebind all ComboFVcombo.  This will rebind all GridFVcombos
        //         There is no reason to rebind GridFVcombos until all ComboFVcombos are bound. 
        //         I set "doNotRebindGridFV"=true, rebind all GridFVcombos, and then rebind the GridFVcombos.
        //      4. Point: Grid rewritten by Write_Page, Manually changing GridFFcombo or GridFVcombo, and nothing else
        // (B) GridFFcombos - combo (no dummy) -- The GridFilterFieldCombo.SelectedValue is the field to filter
        //      1.  Manual Selection change : Bind GridFVcombo -> GridFVcombo_SelectionChange (to empty-string) : Write_NewFilter
        //      2.  Write_NewTable : Setup and bind all GridFFcombos (often only 1 field), Setup GridFVcombos (Item 0 choosen)
        //      3.  Note: Some GridFFcombos have a Primary Key and some have a text-string.  These will be handled very differently
        // (C)  GridFVcombos – combo (first value dummy) -- The GridFilterValueCombo.Selected Value is the value of the filter
        //      1.  Manual Selection change : Write_NewFilter
        //      2.  Note: all the GridFVcombos are text fields. 
        // (D)  ComboController (no dummy) - The RefTable PK for each FK in myTable 
        //      1.  Selection change : Unbind GFV filters, setup & Bind GFV filters, Update All empty GridFVcombos
        //          (Do last two steps when mode changed as well.)
        //      2.  Write_NewTable: Bind this -> Will select Item 0.
        // (E)  ComboFVcombos (no dummy, user can type own value, selectedValue not used; only text (in box))
        //      1.  Enter:   combo_isDirty = false,
        //                   Bind this -> this Text_change : Set combo_isDirty = true  
        //                             -> this Selection_change : No event handler
        //      2.  Enter : Set isDirty = false
        //      3.  Text_change : Set combo isDirty = true (no need to do so if wt = true because all values empty or unbound)
        //      4.  Selection_Change:  No event handler
        //      5.  Leave : Rebinding = true  (to stop GFV change from rewriting Grid),  
        //                  If isDirty, Reload all relevant GRID_filter_VALUES -> GFFs selection_change: (binds GFV, won't write Grid) && 
        //                  Warn about non-empty Grid_filter_values that need rebound.
        // (Note)  Write_table : Writing_table = true, Setup all 4 types of combos, Bind GridFilterFields, Bind ComboController
        //                       On binding GridFFs -> GridFF Selection Change: Bind GridFVs -> GridFVs Selection change (stop)
        //                       On binding ComboController -> Bind ComboFVs (for PK) -> ComboFV_TextChange: does nothing

        //----------------------------------------------------------------------------------------------------------------------
        //          Grid FILTERS COMBOS EVENTS
        //----------------------------------------------------------------------------------------------------------------------

        private void cmbMainFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load_Form adds dummy item and binds (so Count = 1).
            if (!formOptions.loadingMainFilter)
            {   // "No filter" is the last item in the dropdown box.
                if (cmbMainFilter.SelectedIndex == cmbMainFilter.Items.Count - 1)
                {
                    lblMainFilter.Text = MyResources.MainFilterNotSet;
                    // Enable all tables
                    foreach (ToolStripItem tsi in mnuOpenTables.DropDownItems)
                    {
                        tsi.Enabled = true;
                        tsi.Visible = true;
                    }
                }
                else
                {   // Disable mnoOpenTable elements that don't contain this filter
                    where mfWhere = (where)cmbMainFilter.SelectedValue;
                    lblMainFilter.Text = MyResources.mainFilterShort + " " + dgvHelper.TranslateString(mfWhere.fl.table) + " : ";
                    foreach (ToolStripItem tsi in mnuOpenTables.DropDownItems)
                    {
                        SqlFactory sqlFac = new SqlFactory(tsi.Name, 0, 0);
                        if (sqlFac.MainFilterTableIsInMyTable(mfWhere.fl.table, out string tableAlias))
                        {
                            tsi.Visible = true;
                            tsi.Enabled = true;
                        }
                        else
                        {
                            tsi.Visible = false;
                            tsi.Enabled = false;
                        }
                    }
                }
                //Write new filter
                writeGrid_NewFilter(true);
            }
        }

        //-----------------------------------------------------------------------------------------
        //                                      GRID FILTERS COMBOS EVENTS          
        //----------------------------------------------------------------------------------------------------------------------

        // Fill cmbGridFilterValue with values for the selected field 
        public void cmbGridFilterFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            if (!tableOptions.doNotRebindGridFV)  // Not used because this only called programmatically when writing table
            {
                ComboBox cmb = (ComboBox)sender;
                if (cmb.SelectedIndex > -1)  // Do nothing when setting datasource to null
                {
                    for (int i = 0; i < cmbGridFilterFields.Count(); i++)
                    {
                        // Rebind corresponding GFV. This will write grid if writingTable and doNotRebindGFV are both false 
                        if (cmbGridFilterFields[i] == cmb)
                        {
                            tableOptions.doNotWriteGrid = true;
                            RebindOneGridFilterValueCombo(i, false);
                            tableOptions.doNotWriteGrid = false;

                            // Then Change color (first combo only) - either pink or yellow
                            if (i == 0)
                            {
                                field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                                if (dataHelper.isTablePrimaryKeyField(selectedField))
                                {
                                    cmbGridFilterFields[i].BackColor = formOptions.PrimaryKeyColor;
                                    cmbGridFilterValue[i].BackColor = formOptions.PrimaryKeyColor;
                                }
                                else  // Because everything except the primary key in this GridFFcombo is yellow
                                {
                                    cmbGridFilterFields[i].BackColor = formOptions.DefaultColumnColor;
                                    cmbGridFilterValue[i].BackColor = formOptions.DefaultColumnColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Calls Write_NewFilter - unless no-write flag set to true or programMode is "add".
        private void cmbGridFilterValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                if (cmb.SelectedIndex > -1)  // If data source null
                {
                    if (!(tableOptions.writingTable || tableOptions.doNotWriteGrid))
                    {
                        writeGrid_NewFilter(true);
                    }
                }
            }
        }

        private void cmbGridFilterValue_TextChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.DropDownStyle == ComboBoxStyle.DropDown)
            {
                if (!(tableOptions.writingTable || tableOptions.doNotWriteGrid))
                {
                    // writeGrid_NewFilter();  // Using Reload button
                }
            }


        }

        private void cmbGridFilterValue_Enter(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            ComboBox cmb = (ComboBox)sender;
            int i2 = 0;
            for (int i = 0; i < cmbGridFilterValue.Length; i++)
            {
                if (cmb == cmbGridFilterValue[i])
                {
                    cmb.BackColor = cmbGridFilterFields[i].BackColor;
                    i2 = i;
                }
            }
            if (dirtyGFCombos.Contains(cmb.Name))
            {
                RebindOneGridFilterValueCombo(i2, true);
                dirtyGFCombos.Remove(cmb.Name);
            }
        }


        // Only rebind empty filters - no change in Grid because rebinding GridFV selects the same first value = string.empty
        private void RebindAllGridFilterValueCombos()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

            // Better to also check for inconsistencies or warn user by turning the value red. 
            for (int i = 0; i < cmbGridFilterFields.Count(); i++)
            {
                if (cmbGridFilterFields[i].Enabled)
                {
                    tableOptions.doNotWriteGrid = true;
                    RebindOneGridFilterValueCombo(i, true);
                    tableOptions.doNotWriteGrid = false;
                }
            }
        }

        private void RebindOneGridFilterValueCombo(int i, bool selectOriginalValueAfterBinding)
        {
            if (!tableOptions.doNotRebindGridFV)
            {
                ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
                ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
                // 1. Get two combos and old values
                ComboBox cmbGridFF = cmbGridFilterFields[i];
                ComboBox cmbGridFV = cmbGridFilterValue[i];
                // Get old value
                string oldTextValue = String.Empty;
                int oldKeyIntValue = 0;
                if (selectOriginalValueAfterBinding)
                {
                    if (cmbGridFV.DropDownStyle == ComboBoxStyle.DropDown)  // combo has a text value
                    {
                        oldTextValue = cmbGridFV.Text;
                    }
                    else  // Combo has an integer value
                    {
                        if (cmbGridFV.SelectedIndex > 0)  // Item 0 has null value
                        {
                            oldKeyIntValue = (int)cmbGridFV.SelectedValue;
                        }
                    }
                }
                // 2. Set Datasource to null
                // This will fire the change event but both TextChange event and SelectionChange will have selectedIndex = -1 (which stops grid write)
                cmbGridFilterValue[i].DataSource = null;
                cmbGridFilterValue[i].Items.Clear();
                cmbGridFilterValue[i].Enabled = true;

                // 3. Fill datahelper.comboDT  - all selectedValues are in myTable - either an FK or a text field
                field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                if (dataHelper.isForeignKeyField(selectedField))
                {
                    field PK_refTable = dataHelper.getForeignKeyRefField(selectedField);
                    FillComboDT(PK_refTable, comboValueType.PK_refTable);
                }
                else
                {
                    FillComboDT(selectedField, comboValueType.textField_myTable);
                }
                DataRow firstRow = dataHelper.comboDT.NewRow();
                // The Datasource in GridFieldsCombos must have "DisplayMember" and "ValueMember" columns
                // Leave these values null, because they might need correct type
                dataHelper.comboDT.Rows.InsertAt(firstRow, 0); // Even if no rows
                cmbGridFilterValue[i].DisplayMember = "DisplayMember";
                cmbGridFilterValue[i].ValueMember = "ValueMember";

                // 4. Bind the combo dataHelper.comboDt
                // Will not rewrite Grid because I set doNotRewriteGrid = true whenever I call this method
                cmbGridFilterValue[i].DataSource = dataHelper.comboDT;
                cmbGridFilterValue[i].BackColor = cmbGridFilterFields[i].BackColor;   //A guess, may have become red
                // 5.  Select original value
                if (selectOriginalValueAfterBinding)
                {
                    // Restore old Value
                    if (cmbGridFV.DropDownStyle == ComboBoxStyle.DropDown)
                    {

                        tableOptions.doNotRebindGridFV = true;   // doNotWriteGrid already true when this method called.
                        cmbGridFV.Text = oldTextValue;
                        tableOptions.doNotRebindGridFV = false;  // Same value as entering this method - see above
                    }
                    else
                    {
                        if (oldKeyIntValue > 0)
                        {
                            int indexOfOldKey = -1;
                            for (int index = 1; index < cmbGridFV.Items.Count; index++)
                            {
                                DataRowView drv = (DataRowView)cmbGridFV.Items[index];
                                if ((int)drv.Row.ItemArray[1] == oldKeyIntValue)
                                {
                                    tableOptions.doNotRebindGridFV = true;   //doNotWriteGrid already true when this method called.
                                    cmbGridFV.SelectedIndex = index;
                                    tableOptions.doNotRebindGridFV = false;   // Same value as entering this method - see above
                                    return;
                                }
                            }
                            cmbGridFV.BackColor = System.Drawing.Color.Red;
                            tableOptions.doNotRebindGridFV = true;   //doNotWriteGrid already true when this method called.
                            cmbGridFV.SelectedIndex = 0;
                            tableOptions.doNotRebindGridFV = false;   // Same value as entering this method - see above
                        }
                    }
                }
            }
        }

        //  COMBO FILTERS COMBOS EVENTS-----------------------------------------------------------------------------------------

        // Unbind all CFV, set rebinding=true, set up and bind new CFV, rebind empty GFV and warn about others
        private void cmbComboTableList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            tableOptions.doNotRebindGridFV = true;
            ClearAllComboFilters(true);  //Combo filters only; not Grid Filters
            tableOptions.doNotRebindGridFV = false;

            ComboBox cmb = (ComboBox)sender;
            // Fill combo filters with each value in ostensive definition
            if (cmb.SelectedIndex > -1)  // Used when datasource cleared
            {
                field selectedFK = (field)cmb.SelectedValue;
                int comboNumber = 0;
                // Main work of this method - rebind all ComboFVcombos
                field PkRefTable = dataHelper.getForeignKeyRefField(selectedFK);
                field PkMyTable = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                // Get correct table allias
                foreach (innerJoin ij in currentSql.PKs_InnerjoinMap[PkMyTable.key])
                {
                    if (ij.fkFld.baseKey.Equals(selectedFK.baseKey))
                    {
                        PkRefTable.tableAlias = ij.pkRefFld.tableAlias;
                        break;
                    }
                }
                Tuple<string, string, string> key = Tuple.Create(PkRefTable.tableAlias, PkRefTable.table, PkRefTable.fieldName);
                foreach (field fi in currentSql.PKs_OstensiveDictionary[key])
                {
                    if (comboNumber < cmbComboFilterValue.Count()) // Skip if too many
                    {
                        // Set label and Load combo
                        lblCmbFilterFields[comboNumber].Text = dgvHelper.TranslateString(fi.fieldName) + " :"; // Shorter than DisplayName
                        lblCmbFilterFields[comboNumber].Visible = true;
                        lblCmbFilterFields[comboNumber].Enabled = true;
                        cmbComboFilterValue[comboNumber].Visible = true;
                        cmbComboFilterValue[comboNumber].Enabled = true;
                        cmbComboFilterValue[comboNumber].Tag = fi;  // Used in program - a foreign key
                        // Bind ComboFilterValue[comboNumber] - but wait to end to bind grid filter values (see below)
                        tableOptions.doNotRebindGridFV = true;
                        RebindOneComboFilterValueCombo(comboNumber);
                        tableOptions.doNotRebindGridFV = false;
                        if (!tableOptions.firstTimeWritingTable) // Grid not yet loaded and so can't get header color
                        {
                            // Set color of combo
                            int colIndex = currentSql.getColumn(fi);
                            // I added primary key if there is no display-key -  O.K. if this is the PK of this table.  But . . .
                            // If this is the primary key of a FK of this table, It will not be in table and colIndex will be -1
                            // I arbitrarily set this to .BackColor = formOptions.DkColorArray[0]
                            if (colIndex > -1)
                            {
                                lblCmbFilterFields[comboNumber].BackColor = dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor;
                                cmbComboFilterValue[comboNumber].BackColor = dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor;
                            }
                            else
                            {
                                lblCmbFilterFields[comboNumber].BackColor = formOptions.DkColorArray[0];
                                cmbComboFilterValue[comboNumber].BackColor = formOptions.DkColorArray[0];
                            }
                        }
                        comboNumber++;
                        //}
                    }
                }
            }
        }

        // Manual set "is dirty"=true-->Update GridFV on leave cell. Programatic: sets "is dirty", but does nothing because no leave 
        private void cmbComboFilterValue_TextChanged(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterValues = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            // Mark all Grid Filter combos as dirty - use to not rebind this gridFilter twice in a row.
            dirtyGFCombos.Clear();
            foreach (ComboBox comboBox in cmbGridFilterValues)
            {
                dirtyGFCombos.Add(comboBox.Name);
            }

            // When leaving cell, the drop down content of empty grid filter values will be updated
            tableOptions.currentComboFilterValue_isDirty = true;

            // Note: set ccfv_isDirty to false on entering cell and true when text changed
            // If this event (TextChanged) called programmatically, user never leaves cell; so ccfv_isDirty=true does nothing
        }

        private void cmbComboFilterValue_Enter(object sender, EventArgs e)
        {
            // Text_change will make this true, and then leave event will update GRID filter value dropdowns
            tableOptions.currentComboFilterValue_isDirty = false;
        }

        private void cmbComboFilterValue_Leave(object sender, EventArgs e)
        {
            // Planning on eliminating all this
            if (tableOptions != null)
            {
                if (tableOptions.currentComboFilterValue_isDirty)
                {
                    tableOptions.doNotWriteGrid = true;
                    // RebindAllGridFilterValueCombos();
                    tableOptions.doNotWriteGrid = false;
                    tableOptions.currentComboFilterValue_isDirty = false;
                }
            }
        }


        private void BindAllComboFilterValueCombos()
        {
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (cmbComboFilterValue[i].Enabled)
                {
                    RebindOneComboFilterValueCombo(i);
                }
            }
        }

        // Load the combo with all distinct values; Called by cmbComboTableList_SelectedIndexChanged
        // Will rebind all GridFV; If selectedIndex change is called programmatically use "doNotRebindGridFV = true".
        private void RebindOneComboFilterValueCombo(int i)
        {
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            ComboBox cmb = cmbComboFilterValue[i];
            List<string> strList = new List<string>();
            // Can't bind cmbFilterValueCombo on first load because currentDT not yet loaded
            // So I return to this at the end of writing the page
            if (!tableOptions.firstTimeWritingTable)
            {
                field fi = (field)cmb.Tag;  // Non-FK myTable
                FillComboDT(fi, comboValueType.textField_refTable);
                strList = dataHelper.comboDT.AsEnumerable().Select(x => x["DisplayMember"].ToString()).ToList();
            }
            // Insert Dummy element
            strList.Insert(0, String.Empty);
            BindingList<string> strBindingList = new BindingList<string>(strList);
            // Text will change to string.empty, but since it is unbound this is no change
            cmb.DataSource = strBindingList;
        }

        // Used in almost all combo boxs as the boxs dropdown event
        // Adjusts the width of dropdown item box to its longest showing item
        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            // All elements must have a "DisplayMember" field or property.

            // A. Get list of display strings in ComboBox
            var senderComboBox = (System.Windows.Forms.ComboBox)sender;
            if (senderComboBox.Items.Count > 0)
            {
                List<string> displayValueList = new List<string>();
                // 1. FkComboBoxEditingControl
                if (senderComboBox is FkComboBoxEditingControl)
                {
                    var itemsList = senderComboBox.Items.Cast<DataRowView>();
                    foreach (DataRowView drv in itemsList)
                    {
                        int index = drv.Row.Table.Columns.IndexOf("DisplayMember");
                        displayValueList.Add(drv.Row.ItemArray[index].ToString());
                    }
                }
                // 2. Combo bound to fields[]
                else if (senderComboBox.Items[0] is field)
                {
                    var itemsList = senderComboBox.Items.Cast<field>();
                    foreach (field fl in itemsList) { displayValueList.Add(fl.displayMember); }
                }
                // 3. Combo bound to DataRowView[]
                else if (senderComboBox.Items[0] is DataRowView)
                {
                    var itemsList = senderComboBox.Items.Cast<DataRowView>();
                    foreach (DataRowView drv in itemsList)
                    {
                        int index = drv.Row.Table.Columns.IndexOf("DisplayMember");
                        displayValueList.Add(drv.Row.ItemArray[index].ToString());
                    }
                }
                // 4. Combo bound to where[]
                else if (senderComboBox.Items[0] is where)
                {
                    var itemsList = senderComboBox.Items.Cast<where>();
                    foreach (where wh in itemsList) { displayValueList.Add(wh.DisplayMember); }
                }
                else if (senderComboBox.Items[0] is string)
                {
                    var itemsList = senderComboBox.Items.Cast<string>();
                    foreach (string str in itemsList) { displayValueList.Add(str); }
                }

                // B. Get and set width
                int width = senderComboBox.Width;
                using (Graphics g = senderComboBox.CreateGraphics())
                {
                    System.Drawing.Font font = senderComboBox.Font;
                    int vertScrollBarWidth = (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                        ? SystemInformation.VerticalScrollBarWidth : 0;
                    // var itemsList = senderComboBox.Items.Cast<object>().Select(item => item.ToString());
                    foreach (string s in displayValueList)
                    {
                        int newWidth = (int)g.MeasureString(s, font).Width + vertScrollBarWidth;
                        if (width < newWidth)
                        {
                            width = newWidth;
                        }
                    }
                }
                // Don't allow width to go past the end of the form
                if (senderComboBox.Left + width > ClientSize.Width)
                {
                    width = ClientSize.Width - senderComboBox.Left;
                }

                senderComboBox.DropDownWidth = width;
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Add-Delete-Merge Button
        private void btnDeleteAddMerge_Click(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

            txtMessages.Text = string.Empty;
            if (programMode == ProgramMode.merge)
            {
                // Select two if there are only 2 rows and they are not selected
                if (dataGridView1.Rows.Count == 2)
                {
                    if (dataGridView1.SelectedRows.Count != 2)
                    {
                        dataGridView1.SelectAll();
                    }
                }
                // Check if 2 rows selected
                if (dataGridView1.SelectedRows.Count != 2)
                {
                    msgTextError(Properties.MyResources.selectExactlyTwoRows);
                    return;
                }

                // Get two PK values - always use firstPK
                int firstPK = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                int secondPK = Convert.ToInt32(dataGridView1.SelectedRows[1].Cells[0].Value);
                MsSqlWithDaDt dadt = new MsSqlWithDaDt(currentSql.returnSql(command.selectAll, true));
                MergeTwoRows(currentSql.myTable, firstPK, secondPK, dadt);
            }
            else if (programMode == ProgramMode.delete)
            {
                if (dataGridView1.SelectedRows.Count != 1)
                {
                    msgTextError(Properties.MyResources.selectRowToDelete);
                    return;
                }
                // int index = dataGridView1.Rows.IndexOf(dataGridView1.SelectedRows[0]);
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int colIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                if (colIndex != 0)
                {
                    msgTextError(Properties.MyResources.firstColumnMustBePrimaryKey);
                    return;
                }
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[colIndex].Value);
                field PKField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                where wh = new where(PKField, PKvalue.ToString());
                Boolean constraintPassed = true;
                // delete - <table, record ID, bool>
                foreach (Func<string, int, bool> f in dgvHelper.deleteConstraints)
                {
                    constraintPassed = f(currentSql.myTable, PKvalue);
                    if (!constraintPassed) { break; }
                }

                string errorMsg = MsSql.DeleteRowsFromDT(dataHelper.currentDT, wh);

                if (errorMsg != string.Empty)
                {
                    msgTextError(errorMsg);
                    MessageBox.Show(String.Format("Error deleting row: {0}", errorMsg), "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    currentSql.strManualWhereClause = string.Empty;
                    rbView.Checked = true;
                    writeGrid_NewFilter(true);
                }
            }

            else if (programMode == ProgramMode.add)
            {
                // 1. Check that all display keys and foreign keys are loaded - and build where list
                List<where> whereList = new List<where>(); // Used when adding row
                List<where> dkWhere = new List<where>();  // Used to check for repeat displaykeys

                for (int i = 0; i < cmbGridFilterFields.Length; i++)  // Skip the first gridFilter - all others are display or FK's
                {
                    if (cmbGridFilterFields[i].Enabled)  // Sign it is being used
                    {
                        field cmbField = (field)cmbGridFilterFields[i].SelectedValue;  // Will be primary key
                        string cmbLabel = cmbGridFilterFields[i].Text;
                        // A string value  
                        if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDown)
                        {
                            if (cmbGridFilterValue[i].Text == string.Empty)
                            {
                                if (i > 0)  // ignore the first yellow dropdown if empty
                                {
                                    MessageBox.Show(String.Format(Properties.MyResources.pleaseSelectAvalueFor0, cmbLabel), Properties.MyResources.mustSelectAvalue, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                where wh = new where(cmbField, cmbGridFilterValue[i].Text);
                                whereList.Add(wh);
                                if (dataHelper.isDisplayKey(cmbField))
                                {
                                    dkWhere.Add(wh);
                                }

                            }
                        }
                        else  // cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDownList
                        {
                            if (cmbGridFilterValue[i].SelectedIndex == 0)
                            {
                                if (i > 0)  // ignore the first yellow dropdown if empty
                                {
                                    MessageBox.Show(String.Format("Please select a value for {0}", cmbLabel), "Please select a value.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                            else
                            {
                                //// Don't add first Grid filter if it is the primary key of this table
                                if (!dataHelper.isTablePrimaryKeyField(cmbField)) // Vacuous - but in future myTable primary key might be in cmbGridFFcombo[0]
                                {
                                    where wh = new where(cmbField, cmbGridFilterValue[i].SelectedValue.ToString());
                                    whereList.Add(wh);
                                    if (dataHelper.isDisplayKey(cmbField))  // Might be a foreign key
                                    {
                                        dkWhere.Add(wh);
                                    }
                                }
                            }
                        }
                    }
                }

                // 2. Check for errors - Defective table has no display keys, but can add an item
                if (dkWhere.Count > 0)
                {
                    // string newWhereClause = SqlFactory.SqlStatic.sqlWhereString(dkWhere, string.Empty, true);  // Only display keys enabled so filtered
                    // currentSql.strManualWhereClause = newWhereClause; // Next line Causes the following to search on this where only
                    currentSql.myWheres.Clear();
                    currentSql.myWheres = dkWhere;
                    string strSql = currentSql.returnSql(command.selectAll, true);
                    MsSqlWithDaDt dadt = new MsSqlWithDaDt(strSql);
                    string errorMsg = dadt.errorMsg;
                    if (errorMsg != string.Empty)
                    {
                        msgTextError(errorMsg);
                        MessageBox.Show(errorMsg, "ERROR in btnDeleteAddMerge_Click (Add)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // currentSql.strManualWhereClause = string.Empty;
                        return;
                    }

                    if (dadt.dt.Rows.Count > 0)
                    {
                        MessageBox.Show(Properties.MyResources.youAlreadyHaveThisObjectInDatabase, Properties.MyResources.displayKeyValueArrayMustBeUnique, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        currentSql.strManualWhereClause = string.Empty;
                        rbView.Checked = true;
                        writeGrid_NewFilter(true);
                        return;
                    }
                }

                // 3. Check plugin Added Constraints
                bool constraintPassed = true;
                List<Tuple<string, string>> tupleList = new List<Tuple<string, string>>();
                foreach (where wh in whereList)
                {
                    tupleList.Add(Tuple.Create(wh.fl.fieldName, wh.whereValue));
                }

                foreach (Func<string, List<Tuple<string, string>>, bool> f in dgvHelper.insertConstraints)
                {
                    constraintPassed = f(currentSql.myTable, tupleList);
                    if (!constraintPassed) { break; }
                }
                //  4. If all constraints passed, Everything O.K. Add the row
                if (constraintPassed)
                {
                    try
                    {
                        MsSql.SetInsertCommand(currentSql.myTable, whereList, dataHelper.currentDT);  // knows to use currentDA
                        MsSql.currentDA.InsertCommand.ExecuteNonQuery();
                        currentSql.strManualWhereClause = string.Empty;
                        rbView.Checked = true;
                        writeGrid_NewFilter(true);
                    }
                    catch (Exception ex)
                    {
                        msgTextError(ex.Message);
                        MessageBox.Show(ex.Message, "DATABASE ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    writeGrid_NewFilter(true);
                }
            }
        }

        private string MergeDecisionMessage()
        {
            //StringBuilder msgSB = new StringBuilder();
            //// Count how many rows the firstPK and secondPK as FK's in other tables
            //int firstPKCount = 0;
            //int secondPKCount = 0;
            //foreach (DataRow dr in fkRowsInFieldsDT)
            //{
            //    string FKColumnName = dataHelper.getColumnValueinDR(dr, "ColumnName");
            //    string TableWithFK = dataHelper.getColumnValueinDR(dr, "TableName");

            //    string strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, pk1);
            //    firstPKCount = firstPKCount + MsSql.GetRecordCount(strSql);
            //    strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, pk2);
            //    secondPKCount = secondPKCount + MsSql.GetRecordCount(strSql);
            //    txtMessages.Text = String.Format(" Counts: {0} and {1}", firstPKCount, secondPKCount);
            //}
            //msgSB.AppendLine(String.Format("To merge these two rows, we will first replace {0} occurrences of ID {1} with ID {2} in these tables.", firstPKCount, pk1, pk2));
            //msgSB.AppendLine(String.Format("Then we will delete the row {0} from this table.  Do you want to continue?", pk1));
            return "Message + Do you want to merge?";
        }

        private int GetDuplicateRow(string table, int pkDR, DataRow dr)
        {
            //1. Get list of display keys in this table
            String filter = String.Format("TableName = '{0}' and is_DK = 'true'", table);
            DataRow[] drsDKFieldsDT = dataHelper.fieldsDT.Select(filter);
            List<where> whList = new List<where>();
            foreach(DataRow row in drsDKFieldsDT)
            {
                field fld = dataHelper.getFieldFromFieldsDT(row);
                fld.tableAlias = fld.table;  // Alias will have "00" added to it.
                string columnValue = dr[fld.fieldName].ToString();
                where wh = new where(fld, columnValue);
                whList.Add(wh);
            }           
            string sqlWhere = SqlFactory.SqlStatic.sqlWhereString(whList, string.Empty, true);
            MsSqlWithDaDt dadt = new MsSqlWithDaDt(String.Format("Select * from {0} {1}", table, sqlWhere)); ;
            if(dadt.errorMsg != string.Empty)
            {
                msgText(dadt.errorMsg,true,true);
                return 0;
            }

            // 2. Find a different primary key value in the same table
            foreach (DataRow drMatch in dadt.dt.Rows)
            {
                int pkMatch = Int32.Parse(drMatch["pkFld.fieldName"].ToString());
                if (pkMatch != pkDR) { return pkMatch; }
            }
            return 0;
        }

        private bool MergeTwoRows(string table, int pk1, int pk2, MsSqlWithDaDt dadtParent)
        {
            StringBuilder msgSB = new StringBuilder();
            DialogResult reply = DialogResult.Yes;  // Used to skip question
            reply = MessageBox.Show("Are you sure you want to merge rows?", "Merge Row", MessageBoxButtons.YesNo);
            if (reply == DialogResult.Yes)
            {
                // 1. Get all the tables that have an FK for this parent table 
                DataRow[] fkRowsInFieldsDT = dataHelper.fieldsDT.Select(
                String.Format("RefTable = '{0}'", table));

                foreach (DataRow dr in fkRowsInFieldsDT)
                {
                    // 2. Merge these childTable with pk2 as FK into pk1 as FK)
                    string TableWithFK = dataHelper.getColumnValueinDR(dr, "TableName");
                    string FKColumnName = dataHelper.getColumnValueinDR(dr, "ColumnName");
                    field fkField = dataHelper.getFieldFromFieldsDT(TableWithFK, FKColumnName);
                    // 3. Select rows where FK value in pk2
                    string sqlString = String.Format("Select * from {0} where {1} = '{2}'"
                                        , TableWithFK, FKColumnName, pk2);
                    MsSqlWithDaDt dadtChild = new MsSqlWithDaDt(sqlString);
                    string errorMsg2 = dadtChild.errorMsg;
                    if (errorMsg2 != string.Empty)
                    {
                        msgText("Strange error using: " + sqlString, true, true);
                    }
                    // 3. Merge this row in this child table
                    foreach (DataRow childTableDR in dadtChild.dt.Rows)
                    {
                        // 4. Substitute pk1 for pk2 - if a duplicate then merge, else update row.
                        childTableDR[FKColumnName] = pk1;
                        // 5. Check for duplicate row
                        int pkDuplicateRow = 0;
                        field pkChildTable = dataHelper.getTablePrimaryKeyField(TableWithFK);
                        int pkChildDR = Int32.Parse(childTableDR[pkChildTable.fieldName].ToString());
                        pkDuplicateRow = GetDuplicateRow(TableWithFK, pkChildDR, childTableDR);
                        // 6. If no duplicate, then save childTableDR down to the database
                        if (pkDuplicateRow == 0)
                        {
                            // Save it down to the database
                            List<field> fieldsToUpdate = new List<field>();
                            fieldsToUpdate.Add(fkField);
                            MsSql.SetUpdateCommand(fieldsToUpdate, dadtChild.dt);
                            try
                            {
                                // dadtChild.da.Update(dadtChild.dt);
                            }
                            catch (Exception ex)
                            {
                                msgText(ex.Message, true, true);
                                return false;
                            }
                        }
                        // 7. If duplicate exists, we must merge it
                        else
                        {
                            //MsSqlWithDaDt 
                            //MergeTwoRows(TableWithFK,pkDuplicateRow, pkChildDR) 

                        }
                    }
                }
                // 4.  Delete merged row from currentDT
                //field PKField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                //where wh = new where(PKField, pk1.ToString());
                //// Set delete command, delete from currentDT and then call "update" to update the database
                //MsSql.SetDeleteCommand(table, dataHelper.currentDT);
                //string errorMsg = MsSql.DeleteRowsFromDT(dataHelper.currentDT, wh);
                return true;
            }
            return false;  // Dialog canceled
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region 5 Radio buttons and other Buttons (Reload, Wide columns)

        private void rbView_CheckedChanged(object sender, EventArgs e)
        {
            if (rbView.Checked)
            {
                programMode = ProgramMode.view;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = false;
                if (!tableOptions.writingTable)
                {
                    SetFiltersColumnsTablePanel();  // Calls SetColumnReadOnly() - which turns on and off col.ReadOnly
                }
            }
        }

        private void rbDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDelete.Checked)
            {
                programMode = ProgramMode.delete;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = "Delete row";
                // Add deleteCommand
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                SetFiltersColumnsTablePanel();
            }
        }

        private void rbEdit_CheckedChanged(object sender, EventArgs e)
        {
            if (rbEdit.Checked)
            {
                int selectedRow = 0;
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    selectedRow = dataGridView1.SelectedRows[0].Index;
                }
                programMode = ProgramMode.edit;
                dataGridView1.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
                dataGridView1.MultiSelect = false;
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }
                btnDeleteAddMerge.Enabled = false;
                SetFiltersColumnsTablePanel();  // Calls SetColReadOnlyProperty()
                writeGrid_NewPage();  // Needed to add FK_cols
                // Select the original row
                if (selectedRow > 0 && dataGridView1.Rows.Count > selectedRow)
                {
                    dataGridView1.Rows[selectedRow].Selected = true;
                }
            }
        }

        private void rbAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAdd.Checked)
            {
                programMode = ProgramMode.add;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = "Add row";
                SetFiltersColumnsTablePanel();
            }
        }

        private void rbMerge_CheckedChanged(object sender, EventArgs e)
        {
            if (rbMerge.Checked)
            {
                programMode = ProgramMode.merge;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = "Merge 2 rows";
                dataGridView1.MultiSelect = true;
                SetFiltersColumnsTablePanel();
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            writeGrid_NewFilter(true);
        }

        private void toolStripColumnWidth_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                toolStripButtonColumnWidth.Enabled = false;
                formOptions.narrowColumns = !formOptions.narrowColumns;
                if (formOptions.narrowColumns) { toolStripButtonColumnWidth.Text = MyResources.wide; }
                else { toolStripButtonColumnWidth.Text = MyResources.narrow; }
                this.SuspendLayout();
                dgvHelper.SetNewColumnWidths(dataGridView1, currentSql.myFields, formOptions.narrowColumns);
                this.ResumeLayout(false);
                toolStripButtonColumnWidth.Enabled = true;
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region 5 paging buttons & RecordsPerPage (RPP)
        // Paging - <<
        private void txtRecordsPerPage_Leave(object sender, EventArgs e)
        {
            int rpp = 0;
            if (int.TryParse(txtRecordsPerPage.Text, out rpp))
            {
                if (rpp > 9)
                {
                    formOptions.pageSize = rpp;
                    AppData.SaveKeyValue("RPP", rpp.ToString());
                }
            }
        }

        // Paging - <
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                if (currentSql.myPage > 1)
                {
                    currentSql.myPage--;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - pages / totalPages
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                frmCaptions captionsForm = new frmCaptions("Pages", "pages");
                //Get connection string
                captionsForm.totalPages = currentSql.TotalPages;
                captionsForm.gridFormWidth = this.Width;
                captionsForm.gridFormLeftLocation = this.Location.X;
                captionsForm.ShowDialog();
                int pageSelected;
                bool captionIsInt = Int32.TryParse(captionsForm.selectedCaption, out pageSelected);
                if (!captionIsInt)
                {
                    pageSelected = currentSql.myPage;
                }
                captionsForm.Close();
                if (pageSelected != currentSql.myPage)
                {
                    currentSql.myPage = pageSelected;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - >
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                if (currentSql.myPage < currentSql.TotalPages)
                {
                    currentSql.myPage++;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - >>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                int oneFifth = (int)Math.Ceiling((decimal)currentSql.TotalPages / 5);
                int pageLeap = Math.Min(currentSql.myPage + oneFifth, currentSql.TotalPages);
                if (pageLeap != currentSql.TotalPages)
                {
                    currentSql.myPage = pageLeap;
                    writeGrid_NewPage();
                }
            }
        }
        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Other functions and methods

        private bool IsUICulture(String UICulture)
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo culture in cultures)
            {
                if (culture.Equals(CultureInfo.InvariantCulture)) { continue; } //do not use "==", won't work
                else
                {
                    if (UICulture == culture.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int getDGVcolumn(field fld)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (fld.baseKey.Equals(currentSql.myFields[i].baseKey))
                {
                    return i;
                }
            }
            return 0;
        }


        private void callSqlWheresForCombo(field PkField)  // PK used to get inner joins 
        {   // Adds all the filters - FK, DK, non-Key and
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            // Clear any old filters from currentSql
            currentSql.myComboWheres.Clear();

            //Main filter - add this where to the currentSql)
            if (cmbMainFilter.SelectedIndex != cmbMainFilter.Items.Count - 1)
            {
                where mfWhere = (where)cmbMainFilter.SelectedValue;

                if (Convert.ToInt32(mfWhere.whereValue) > 0)
                {
                    if (currentSql.MainFilterTableIsInComboSql(mfWhere, PkField, out string tableAlias))
                    {
                        mfWhere.fl.tableAlias = tableAlias;
                        currentSql.myComboWheres.Add(mfWhere);
                    }
                }
            }
            // cmbComboFilterFields  (6 fields)
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (cmbComboFilterValue[i].Enabled)  // True iff visible
                {
                    if (cmbComboFilterValue[i].DataSource != null)  // Probably not needed but just in case 
                    {
                        if (cmbComboFilterValue[i].Text != String.Empty) // ComboFV is a non-PK non-FK
                        {
                            field comboFF = (field)cmbComboFilterValue[i].Tag;
                            field PKcomboFF = dataHelper.getTablePrimaryKeyField(comboFF.table);
                            PKcomboFF.tableAlias = comboFF.tableAlias;
                            if (currentSql.TableIsInMyInnerJoins(PkField, comboFF.tableAlias))  // Should always be true
                            {
                                where wh = new where(comboFF, cmbComboFilterValue[i].Text);
                                if (dataHelper.TryParseToDbType(wh.whereValue, comboFF.dbType))
                                {
                                    currentSql.myComboWheres.Add(wh);
                                }
                                else
                                {
                                    string erroMsg = String.Format(dataHelper.errMsg, dataHelper.errMsgParameter1, dataHelper.errMsgParameter2);
                                    msgTextError(erroMsg);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FillComboDT(field fl, comboValueType cmbValueType)
        {
            // 1. Set the SqlWheresForCombo in currentSql
            // If a foreign key
            if (cmbValueType == comboValueType.PK_myTable || cmbValueType == comboValueType.PK_refTable)
            {
                callSqlWheresForCombo(fl);  // Filter by main filter and visible Combo filter Combos
            }
            else  // a text field
            {
                field PkTable = dataHelper.getTablePrimaryKeyField(fl.table);
                PkTable.tableAlias = fl.tableAlias;
                callSqlWheresForCombo(PkTable);
            }
            // 2. Get SQL and Fill dataHelper.comboDT
            // combo.returnComboSql works very differently for Primary keys and non-Primary keys
            string strSql = currentSql.returnComboSql(fl, formOptions.orderComboListsByPK, cmbValueType);
            dataHelper.comboDT = new DataTable();
            string errorMsg = MsSql.FillDataTable(dataHelper.comboDT, strSql);
            if (errorMsg != string.Empty)
            {
                msgTextError(errorMsg);
                MessageBox.Show(errorMsg, "ERROR in FillComboDT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void SetSelectedCellsColor()
        {
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                DataGridViewCellStyle csReverse = new DataGridViewCellStyle();
                csReverse.SelectionBackColor = dataGridView1.DefaultCellStyle.SelectionForeColor;
                csReverse.SelectionForeColor = dataGridView1.DefaultCellStyle.SelectionBackColor;
                DataGridViewCellStyle csDefault = new DataGridViewCellStyle();
                csDefault.SelectionBackColor = dataGridView1.DefaultCellStyle.SelectionBackColor;
                csDefault.SelectionForeColor = dataGridView1.DefaultCellStyle.SelectionForeColor;
                tableOptions.writingTable = true;
                if (cell.ReadOnly == false)
                {
                    cell.Style = csReverse;  // Fires column width change
                }
                else
                {
                    cell.Style = csDefault;
                }
                tableOptions.writingTable = false;
            }
        }

        public void msgText(string text)
        {
            msgText(text, false, false);  // Show message in text box only
        }
        public void msgText(string text, bool showMsgBox, bool error)
        {
            if (error) { txtMessages.ForeColor = System.Drawing.Color.Red; }
            else { txtMessages.ForeColor = System.Drawing.Color.Navy; }
            string msg = text;
            txtMessages.Text = msg;
            txtMessages.ForeColor = System.Drawing.Color.Navy;
            if (showMsgBox)
            {
                if (error)
                {
                    MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(text, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void msgTextError(string text)
        {
            msgText(text, false, true);
        }

        private void msgTextAdd(string text)
        {
            string msg = text;
            txtMessages.Text += msg;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Debugging functions

        private void msgDebug(string text)
        {
            if (formOptions.debugging)
            {
                string msg = text;
                txtMessages.Text += msg;
            }
        }

        private void msgTimer(string text)
        {
            if (formOptions.runTimer)
            {
                msgTextAdd(text);
            }
        }

        // Used in Debugging only
        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeVed");
        }

        // Used in Debugging only
        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeVing[" + e.ColumnIndex.ToString() + "," + e.RowIndex.ToString() + "]");
        }

        // Used in Debugging only
        private void dataGridView1_Validated(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", GrVed");
        }

        // Used in Debugging only
        private void dataGridView1_Validating(object sender, CancelEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", GrVing");
        }

        // Used in Debugging only
        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeLeave");
        }
        // Used in Debugging only    
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", ClickCell");
        }

        // Used in Debugging only
        private void dataGridView1_Enter(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", EnterGrid");
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------


        private void txtManualFilter_TextChanged(object sender, EventArgs e)
        {
            if (!txtManualFilter.Enabled) { return; }

            string text = txtManualFilter.Text;
            if (text.Length == 0) { return; }

            // Show fields in 
            if (text.Length > 3)
            {
                if (text.Substring(text.Length - 2, 2) == "].")
                {
                    foreach (string key in aliasTableDictionary.Keys)
                    {
                        if (text.Length > key.Length + 2)
                        {
                            if (text.Substring(text.Length - (key.Length + 3)) == String.Format("[{0}].", key))
                            {
                                txtManualFilter.AutoCompleteMode = AutoCompleteMode.Suggest;
                                txtManualFilter.AutoCompleteCustomSource = null;
                                txtManualFilter.AutoCompleteSource = AutoCompleteSource.None;
                                AutoCompleteStringCollection lstStrings = new AutoCompleteStringCollection();
                                foreach (string fieldName in aliasTableDictionary[key])
                                {
                                    lstStrings.Add(text + "[" + fieldName + "]");

                                }
                                txtManualFilter.AutoCompleteCustomSource = lstStrings;
                                txtManualFilter.AutoCompleteSource = AutoCompleteSource.CustomSource;
                                return;
                            }
                        }
                    }
                    return;  // Found ]. but not with a table
                }

            }
            else
            {
                bool showTables = false;
                if (text == "[") { showTables = true; }
                else if (text.Length > 3)
                {
                    if (text.Substring(text.Length - 1) == "[" && text.Substring(text.Length - 2) != ".[")
                    { showTables = true; }
                }
                if (showTables)
                {
                    txtManualFilter.AutoCompleteMode = AutoCompleteMode.Suggest;
                    txtManualFilter.AutoCompleteCustomSource = null;
                    txtManualFilter.AutoCompleteSource = AutoCompleteSource.None;
                    AutoCompleteStringCollection lstStrings = new AutoCompleteStringCollection();
                    foreach (string key in aliasTableDictionary.Keys)
                    {
                        lstStrings.Add(text + key + "].");
                    }
                    txtManualFilter.AutoCompleteCustomSource = lstStrings;
                    txtManualFilter.AutoCompleteSource = AutoCompleteSource.CustomSource;
                }
            }
        }

        public void setTxtManualFilterText(string text)
        {
            // Show filter
            if (!mnuShowITTools.Checked) { mnuShowITTools.Checked = true; }
            txtManualFilter.Text = text;
        }

        private void mnuShowITTools_CheckedChanged(object sender, EventArgs e)
        {
            if (programMode != ProgramMode.none && mnuShowITTools.Checked == false)
            {
                Application_Restart();
            }
            else
            {
                foreach (ToolStripMenuItem mi in mnuIT_Tools.DropDownItems.OfType<ToolStripMenuItem>())
                {
                    // txtManualFilter shown if mnuShowITTools.Checked;
                    // So if you want them to show, check it if it is not checked
                    if (mi.Name != mnuShowITTools.Name)
                    {
                        mi.Visible = mnuShowITTools.Checked;
                        txtManualFilter.Visible = mnuShowITTools.Checked;
                        lblManualFilter.Visible = mnuShowITTools.Checked;
                        txtManualFilter.Enabled = mnuShowITTools.Checked;
                    }
                }
                if (mnuShowITTools.Checked)
                {
                    SetTableLayoutPanelHeight();
                    mnuIT_Tools.ShowDropDown();
                }
            }
        }

        private void mnuLoadPlugin_Click(object sender, EventArgs e)
        {
            string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string selectedFolder = SelectFolder(myDocumentsFolder, false);
            if (selectedFolder != string.Empty)
            {
                if (Directory.Exists(selectedFolder))
                {
                    string directoryName = new DirectoryInfo(selectedFolder).Name;
                    string appDataFolder = Application.CommonAppDataPath;
                    if (Directory.Exists(appDataFolder))
                    {
                        string plugInFolder = String.Format("{0}\\{1}\\{2}", appDataFolder, "PluginsToConsume", directoryName);
                        if (Directory.Exists(plugInFolder))
                        {
                            Directory.Delete(plugInFolder, true);
                        }
                        Directory.CreateDirectory(plugInFolder);
                        CopyDirectory(selectedFolder, plugInFolder, true);
                        Application_Restart();
                    }
                }
            }
        }
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists) { return; }

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private void mnuRemovePlugin_Click(object sender, EventArgs e)
        {
            // Get information about the source directory
            string plugInDirectory = String.Format("{0}\\{1}", Application.CommonAppDataPath, "PluginsToConsume");
            DirectoryInfo dirInfo = new DirectoryInfo(plugInDirectory);

            // Check if the source directory exists
            if (!dirInfo.Exists) { return; }
            // Cache directories before we start copying
            DirectoryInfo[] subDirsInfo = dirInfo.GetDirectories();
            List<string> pluginDirectories = new List<string>();
            foreach (DirectoryInfo subDir in subDirsInfo)
            {
                pluginDirectories.Add(subDir.FullName);
            }

            frmListItems directoryListForm = new frmListItems();
            directoryListForm.myList = pluginDirectories;
            directoryListForm.myJob = frmListItems.job.SelectString;
            directoryListForm.Text = "Select Plugin";
            directoryListForm.ShowDialog();
            string selectedDirectory = directoryListForm.returnString;
            int intSelectedDirectory = directoryListForm.returnIndex;
            directoryListForm = null;
            if (intSelectedDirectory > -1)
            {
                // Executed on next load
                AppData.SaveKeyValue("deletePluginPath", selectedDirectory);
                Application_Restart();
            }
        }

        private void Application_Restart()
        {
            try
            {
                Application.Restart();
                Environment.Exit(0);
            }
            catch { }
        }

        private void mnuDisplayKeysList_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                // Get list of display keys for current table
                frmListItems frmDisplayKeys = new frmListItems();
                frmDisplayKeys.myList = new List<string>();
                frmDisplayKeys.mySelectedList = new List<string>();
                DataRow[] drsList = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}'", currentSql.myTable));
                DataRow[] drsSelectedList = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}' AND is_DK = 'True'", currentSql.myTable));
                List<string> columnList = new List<string>();
                foreach (DataRow dr in drsList)
                {
                    columnList.Add(dr["ColumnName"].ToString());
                }
                List<string> dkColumnList = new List<string>();
                foreach (DataRow dr in drsSelectedList)
                {
                    dkColumnList.Add(dr["ColumnName"].ToString());
                }
                frmDisplayKeys.myJob = frmListItems.job.SelectMultipleStrings;
                frmDisplayKeys.Text = "Edit Display Keys (internal list)";
                frmDisplayKeys.myList = columnList;
                frmDisplayKeys.mySelectedList = dkColumnList;
                frmDisplayKeys.ShowDialog();
                List<string> selectedDKs = frmDisplayKeys.mySelectedList;
                int intSelectedDirectory = frmDisplayKeys.returnIndex;
                frmDisplayKeys = null;
                if (intSelectedDirectory > -1)
                {
                    //Select the DKs - NO - must turn unselected off
                    foreach (String columnName in columnList)
                    {
                        // Set Dk in fields
                        DataRow dataRow = dataHelper.getDataRowFromFieldsDT(currentSql.myTable, columnName);
                        if (selectedDKs.Contains(columnName))
                        {
                            dataHelper.setColumnValueInDR(dataRow, "is_DK", "true");
                        }
                        else
                        {
                            dataHelper.setColumnValueInDR(dataRow, "is_DK", "false");
                        }
                    }
                }
            }
        }

        private void btnExtra_Click(object sender, EventArgs e)
        {
            // MsSql.testing();
        }


        private void mnuRapidlyMergeDKs_Click(object sender, EventArgs e)
        {
            if (mnuMergeDuplicateDKs.Checked)
            {
                showDuplicateDispayKeys();
            }
        }


        #region Events that are unused or do nothing (accidently entered here or storing)
        private void GridContextMenu_FindInChild_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                // Get list of tables that have this table as foreign key
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                if (drs.Count() > 0)
                {
                    List<string> tableList = new List<string>();
                    foreach (DataRow dr in drs)
                    {
                        tableList.Add(dr["TableName"].ToString());
                    }

                    // Get user choice if more than one
                    frmListItems ChildTablesForm = new frmListItems();
                    ChildTablesForm.myList = tableList;
                    ChildTablesForm.myJob = frmListItems.job.SelectString;
                    ChildTablesForm.Text = "Select Table";
                    ChildTablesForm.ShowDialog();
                    string selectedTable = ChildTablesForm.returnString;
                    ChildTablesForm = null;
                    if (!String.IsNullOrEmpty(selectedTable))
                    {
                        writeGrid_NewTable(selectedTable, true);
                    }
                }
            }
        }

        private void GridContextMenu_Opening(object sender, CancelEventArgs e) { }

        private void toolStripBottom_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // SetToStoredColumnWidths();  // Takes too long for transcripts
        }

        private void GridContextMenu_Opening_1(object sender, CancelEventArgs e)
        {

        }

        #endregion

        private void mnuBatchInsert_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                writeGrid_NewFilter(true);  // O.K. but could do a 'needsReload' check before reloading.

                DialogResult reply = MessageBox.Show(String.Format("Do you want to batch insert {0} new records in table {1}?", currentSql.RecordCount.ToString(), currentSql.myTable), "Batch Insert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (reply == DialogResult.Yes)
                {
                    // 1. Select list of fields to change - must be one non-display key
                    List<field> selectedFields = new List<field>();
                    selectedFields = SelectListOfFieldsToChange(true);

                    if (selectedFields.Count() > 0)
                    {
                        // Check that at least one DK is selected
                        bool dkFiltered = false;
                        foreach (field fld in selectedFields)
                        {
                            // All fields are in myTable - see above conditional
                            if (dataHelper.isDisplayKey(fld))
                            {
                                dkFiltered = true;
                                break;
                            }
                        }
                        if (!dkFiltered)
                        {
                            MessageBox.Show("You must change at least one Display Key to batch insert.", "Batch Insert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        // 2. Get new value for the fields the user wants to change by list form for each field.

                        List<(field, string)> sqlInsertValues = SelectValuesForSelectedFields(selectedFields);

                        // Above returns empty list if user does not select values for all fields.
                        if (sqlInsertValues.Count() == 0) { return; }

                        // READY TO INSERT - loop through the currentDT.Rows
                        string lastErrorMsg = string.Empty;
                        int successCount = 0;
                        int failureCount = 0;
                        foreach (DataRow dr in dataHelper.currentDT.Rows)
                        {
                            // Need to get values for every column in table
                            List<(field, string)> sqlAllInsertValues = new List<(field, string)>();
                            // Begin by adding the user selected values
                            foreach ((field, string) sqlInsertValue in sqlInsertValues)
                            {
                                sqlAllInsertValues.Add(sqlInsertValue);
                            }

                            // Loop through all myFields, selecting ones that need inserted
                            // (Tech. note: Could loop through dr.Table.Columns, using dc.ItemArray(dc.Ordinal) to get value
                            for (int i = 0; i < currentSql.myFields.Count; i++)
                            {
                                field fl = currentSql.myFields[i];
                                if (fl.table == currentSql.myTable)  // In table
                                {
                                    if (!dataHelper.isTablePrimaryKeyField(fl))  // Not primary key
                                    {
                                        if (!sqlInsertValues.Any(x => x.Item1.fieldName == fl.fieldName)) // Not yet added
                                        {
                                            string fieldValue = dr.ItemArray[i].ToString();
                                            sqlAllInsertValues.Add((fl, fieldValue));
                                        }
                                    }
                                }
                            }
                            // Do insert for this dr
                            List<where> whList = new List<where>();
                            foreach ((field, string) iv in sqlAllInsertValues)
                            {
                                where wh = new where(iv.Item1, iv.Item2);
                                whList.Add(wh);
                            }
                            MsSql.SetInsertCommand(currentSql.myTable, whList, dataHelper.currentDT);  // knows to use currentDA
                            try
                            {
                                MsSql.currentDA.InsertCommand.ExecuteNonQuery();
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                lastErrorMsg = ex.Message;
                                failureCount++;
                            }
                            currentSql.strManualWhereClause = string.Empty;
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("Rows successfully inserted: " + successCount.ToString());
                            sb.AppendLine("Rows failed to inserted: " + failureCount.ToString());
                            if (lastErrorMsg != string.Empty)
                            {
                                sb.AppendLine("Last error message: ");
                                sb.AppendLine(lastErrorMsg);
                            }
                            DialogResult result = MessageBox.Show(sb.ToString(), "Batch Insert Result", MessageBoxButtons.OKCancel, MessageBoxIcon.None);
                            if (result == DialogResult.Cancel) { return; }
                        }
                    }
                }
            }
        }

        private void mnuBatchUpdate_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                writeGrid_NewFilter(true);  // Should do a 'needsReload' check.

                ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
                ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

                DialogResult reply = MessageBox.Show(String.Format("Do you want to batch update {0} records in table {1}?", currentSql.RecordCount.ToString(), currentSql.myTable), "Batch Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (reply == DialogResult.Yes)
                {
                    // 1. Select list of fields to change - must be one non-display key (not sure why)
                    List<field> selectedFields = new List<field>();
                    selectedFields = SelectListOfFieldsToChange(false);

                    if (selectedFields.Count() > 0)
                    {

                        // 2. Get new value for the fields the user wants to change by list form for each field.
                        List<(field, string)> sqlInsertValues = SelectValuesForSelectedFields(selectedFields);

                        // Above returns empty list if user does not select values for all fields.
                        if (sqlInsertValues.Count() == 0) { return; }

                        // Set update command - same for every row
                        List<field> fieldsToUpdate = new List<field>();
                        foreach ((field, string) fs in sqlInsertValues)
                        {
                            fieldsToUpdate.Add(fs.Item1);
                        }
                        MsSql.SetUpdateCommand(fieldsToUpdate, dataHelper.currentDT);


                        // READY TO Update - loop through the currentDT.Rows
                        string lastErrorMsg = string.Empty;
                        int successCount = 0;
                        int failureCount = 0;
                        foreach (DataRow dr in dataHelper.currentDT.Rows)
                        {
                            //3. Make the changes in each row and then push down to the database

                            // Loop through all myFields, selecting ones that need inserted
                            // (Tech. note: Could loop through dr.Table.Columns, using dc.ItemArray(dc.Ordinal) to get value
                            for (int i = 0; i < currentSql.myFields.Count; i++)
                            {
                                field fl = currentSql.myFields[i];
                                if (fl.table == currentSql.myTable)  // In table
                                {
                                    if (!dataHelper.isTablePrimaryKeyField(fl))  // Not primary key
                                    {

                                        if (sqlInsertValues.Any(x => x.Item1.fieldName == fl.fieldName)) // field in sqlInsertValue
                                        {
                                            string newFieldValue = sqlInsertValues.Find(x => x.Item1.fieldName == fl.fieldName).Item2;
                                            dr[i] = newFieldValue;
                                        }
                                    }
                                }
                            }
                            // Do update for this dr
                            try
                            {
                                MsSql.currentDA.UpdateCommand.ExecuteNonQuery();
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                lastErrorMsg = ex.Message;
                                failureCount++;
                            }
                            currentSql.strManualWhereClause = string.Empty;
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("Rows successfully inserted: " + successCount.ToString());
                            sb.AppendLine("Rows failed to inserted: " + failureCount.ToString());
                            if (lastErrorMsg != string.Empty)
                            {
                                sb.AppendLine("Last error message: ");
                                sb.AppendLine(lastErrorMsg);
                            }
                            DialogResult result = MessageBox.Show(sb.ToString(), "Batch Insert Result", MessageBoxButtons.OKCancel, MessageBoxIcon.None);
                            if (result == DialogResult.Cancel) { return; }
                        }
                    }
                }
            }
        }

        private List<field> SelectListOfFieldsToChange(bool needsFilteredDisplayKey)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };
            // Selected list of filtered fields to return
            List<field> selectedFields = new List<field>();

            // 1. Get list of filtered fields from GridFilterFields and their values
            bool oneDisplayFieldFiltered = false;
            // List of all filtered fields
            List<field> filteredFields = new List<field>();
            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                if (cmbGridFilterFields[i].Enabled)
                {
                    //cmbGridFilterFields - something is selected and all values are fields
                    field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                    // Batch insert and Batch update only handles fields in myTable  
                    if (selectedField.table == currentSql.myTable)
                    {
                        if (cmbGridFilterValue[i].SelectedIndex > 0) // 0 is the pseudo item
                        {
                            filteredFields.Add(selectedField);
                            if (dataHelper.isDisplayKey(selectedField))
                            {
                                oneDisplayFieldFiltered = true;   // At least on DK must be filtered on
                            }
                        }
                    }
                }
            }
            // Return if no key is filtered in the grid
            if (needsFilteredDisplayKey && !oneDisplayFieldFiltered)
            {
                MessageBox.Show("You must select at least one Display Key to batch insert.", "Batch Insert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return selectedFields;  // Will be empty
            }

            // Ask which keys the user wants to change
            frmListItems frmDisplayKeys = new frmListItems();
            frmDisplayKeys.myList = filteredFields.Select(fl => fl.fieldName).ToList();
            frmDisplayKeys.mySelectedList = new List<string>();
            frmDisplayKeys.myJob = frmListItems.job.SelectMultipleStrings;
            if (needsFilteredDisplayKey)
            {
                frmDisplayKeys.Text = "Select the fields to change in Inserted Rows.";
            }
            else
            {
                frmDisplayKeys.Text = "Select the fields to update.";
            }
            frmDisplayKeys.ShowDialog();
            int intSelectedRowsCount = frmDisplayKeys.returnIndex; // Returns -1 if exit or nothing selected
            // Returns list of fields (or empty list if canceled or nothing selected)
            if (intSelectedRowsCount > 0)
            {
                // Convert the selected strings back to fields
                foreach (string sf in frmDisplayKeys.mySelectedList)
                {
                    selectedFields.Add(filteredFields.FindLast(a => a.fieldName == sf)); // there will be exactly 1
                }
            }
            frmDisplayKeys = null;
            return selectedFields;
        }

        private List<(field, string)> SelectValuesForSelectedFields(List<field> selectedFields)
        {
            List<(field, string)> sqlInsertValues = new List<(field, string)>();
            // Get new values for the selected fields from dropdown options in that cmbGridFilterFields

            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5, cmbGridFilterFields_6, cmbGridFilterFields_7, cmbGridFilterFields_8 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5, cmbGridFilterValue_6, cmbGridFilterValue_7, cmbGridFilterValue_8 };

            List<(string, string)> SelectedFieldsWithNewValues = new List<(string, string)>();
            List<string> newValueDisplayMembers = new List<string>();
            List<string> newValueValueMembers = new List<string>();

            foreach (field sf in selectedFields)
            {
                for (int i = 0; i < cmbGridFilterFields.Length; i++)
                {
                    newValueDisplayMembers.Clear();
                    newValueValueMembers.Clear();
                    if (cmbGridFilterFields[i].Enabled)
                    {
                        // See if this is the correct cmbGridFilterField - there will be one
                        field cmbFilterField = (field)cmbGridFilterFields[i].SelectedValue; // DropDownList so SelectedIndex > -1
                        if (cmbFilterField.fieldName == sf.fieldName && cmbFilterField.table == currentSql.myTable)
                        {
                            string selectedDisplayMember = cmbGridFilterValue[i].Text;
                            // Fill up newValue lists
                            var itemsList = cmbGridFilterValue[i].Items.Cast<DataRowView>();
                            foreach (DataRowView drv in itemsList)
                            {
                                int index = drv.Row.Table.Columns.IndexOf("ValueMember");
                                string valueMember = string.Empty;
                                if (!Convert.IsDBNull(drv.Row.ItemArray[index]))
                                {
                                    int index2 = drv.Row.Table.Columns.IndexOf("DisplayMember");
                                    string displayMember = drv.Row.ItemArray[index2].ToString();
                                    if (displayMember != selectedDisplayMember)
                                    {
                                        if (cmbGridFilterValue[i].DropDownStyle == ComboBoxStyle.DropDownList)
                                        {
                                            int intValueMember = (int)drv.Row.ItemArray[index];
                                            valueMember = intValueMember.ToString();
                                        }
                                        else
                                        {
                                            valueMember = drv.Row.ItemArray[index].ToString();
                                        }
                                        newValueValueMembers.Add(valueMember);
                                        newValueDisplayMembers.Add(displayMember);
                                    }
                                }
                            }
                            // Get user choice
                            frmListItems frmNewValues = new frmListItems();
                            frmNewValues.myList = newValueDisplayMembers;
                            frmNewValues.myJob = frmListItems.job.SelectString;
                            frmNewValues.Text = "Select new value for " + sf.fieldName;
                            frmNewValues.ShowDialog();
                            string selectedItem = frmNewValues.returnString;
                            int intSelectedRowsCount = frmNewValues.returnIndex; // Returns -1 if exit or nothing selected
                            frmNewValues = null;
                            // Return if nothing is choosen
                            if (intSelectedRowsCount < 0)
                            {
                                sqlInsertValues.Clear();
                                return sqlInsertValues;
                            }
                            // The point of all this
                            for (int k = 0; k < newValueValueMembers.Count; k++)
                            {
                                // This will be true for some k. 
                                if (newValueDisplayMembers[k] == selectedItem)
                                {
                                    sqlInsertValues.Add((sf, newValueValueMembers[k]));
                                    break;  // From 3rd for loop
                                }
                            }
                        }
                    }
                } // Looking for the correct gridFilterField for this sf
                  // above two breaks will break to here, and then continue 1st for loop
            }  // End for loop - sqlInsertValues has the new values for the selected fields
            return sqlInsertValues;
        }

    }
}

