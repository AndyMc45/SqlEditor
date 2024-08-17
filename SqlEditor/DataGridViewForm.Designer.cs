namespace SqlEditor
{
    partial class DataGridViewForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataGridViewForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            splitContainer1 = new SplitContainer();
            tableLayoutPanel = new TableLayoutPanel();
            GridContextMenu = new ContextMenuStrip(components);
            GridContextMenu_SetAsMainFilter = new ToolStripMenuItem();
            GridContextMenu_SetFKasMainFIlter = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            GridContextMenu_OrderCombolByPK = new ToolStripMenuItem();
            GridContextMenu_TimesUsedAsFK = new ToolStripMenuItem();
            GridContextMenu_UnusedAsFK = new ToolStripMenuItem();
            toolSripSeparator2 = new ToolStripSeparator();
            GridContextMenu_RestoreFilters = new ToolStripMenuItem();
            GridContextMenu_ClearFilters = new ToolStripMenuItem();
            lblManualFilter = new Label();
            lblMainFilter = new Label();
            cmbMainFilter = new ComboBox();
            lblGridFilter = new Label();
            cmbGridFilterFields_0 = new ComboBox();
            cmbGridFilterFields_1 = new ComboBox();
            cmbGridFilterValue_3 = new ComboBox();
            cmbGridFilterFields_4 = new ComboBox();
            cmbGridFilterValue_4 = new ComboBox();
            cmbGridFilterFields_5 = new ComboBox();
            cmbGridFilterValue_1 = new ComboBox();
            cmbGridFilterValue_5 = new ComboBox();
            cmbGridFilterValue_2 = new ComboBox();
            cmbGridFilterFields_2 = new ComboBox();
            cmbGridFilterValue_0 = new ComboBox();
            cmbGridFilterFields_3 = new ComboBox();
            rbMerge = new RadioButton();
            btnDeleteAddMerge = new Button();
            rbAdd = new RadioButton();
            rbDelete = new RadioButton();
            rbEdit = new RadioButton();
            rbView = new RadioButton();
            lblComboFilter = new Label();
            cmbGridFilterFields_7 = new ComboBox();
            cmbGridFilterValue_6 = new ComboBox();
            cmbGridFilterFields_6 = new ComboBox();
            cmbGridFilterValue_7 = new ComboBox();
            cmbGridFilterFields_8 = new ComboBox();
            cmbGridFilterValue_8 = new ComboBox();
            lblCmbFilterField_3 = new Label();
            cmbComboFilterValue_3 = new ComboBox();
            lblCmbFilterField_4 = new Label();
            cmbComboFilterValue_4 = new ComboBox();
            lblCmbFilterField_5 = new Label();
            cmbComboFilterValue_5 = new ComboBox();
            lblCmbFilterField_0 = new Label();
            cmbComboFilterValue_0 = new ComboBox();
            lblCmbFilterField_1 = new Label();
            cmbComboFilterValue_1 = new ComboBox();
            lblCmbFilterField_2 = new Label();
            cmbComboFilterValue_2 = new ComboBox();
            cmbComboTableList = new ComboBox();
            btnBlackLine = new Button();
            btnReload = new Button();
            txtManualFilter = new TextBox();
            btnExtra = new Button();
            txtMessages = new TextBox();
            dataGridView1 = new DataGridView();
            MainMenu1 = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuConnections = new ToolStripMenuItem();
            mnuConnectionList = new ToolStripMenuItem();
            mnuBlankLine3 = new ToolStripMenuItem();
            mnuAddConnection = new ToolStripMenuItem();
            mnuDeleteConnection = new ToolStripMenuItem();
            mnuClose = new ToolStripMenuItem();
            mnuOpenTables = new ToolStripMenuItem();
            mnuTools = new ToolStripMenuItem();
            mnuPrintCurrentTable = new ToolStripMenuItem();
            mnuIT_Tools = new ToolStripMenuItem();
            mnuDatabaseInfo = new ToolStripMenuItem();
            mnuToolsBackupDatabase = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            mnuForeignKeyMissing = new ToolStripMenuItem();
            mnuDisplayKeysList = new ToolStripMenuItem();
            mnuDuplicateDisplayKeys = new ToolStripMenuItem();
            mnuRapidlyMergeDKs = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            mnuLoadPlugin = new ToolStripMenuItem();
            mnuRemovePlugin = new ToolStripMenuItem();
            mnuShowITTools = new ToolStripMenuItem();
            mnuHelp = new ToolStripMenuItem();
            mnuHelpFile = new ToolStripMenuItem();
            toolStripBottom = new ToolStrip();
            txtRecordsPerPage = new ToolStripTextBox();
            toolStripButton4 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButtonColumnWidth = new ToolStripButton();
            saveFileDialog1 = new SaveFileDialog();
            folderBrowserDialog1 = new FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            GridContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            MainMenu1.SuspendLayout();
            toolStripBottom.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(splitContainer1, "splitContainer1");
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel);
            splitContainer1.Panel1.Controls.Add(txtMessages);
            resources.ApplyResources(splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dataGridView1);
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(tableLayoutPanel, "tableLayoutPanel");
            tableLayoutPanel.ContextMenuStrip = GridContextMenu;
            tableLayoutPanel.Controls.Add(lblManualFilter, 1, 4);
            tableLayoutPanel.Controls.Add(lblMainFilter, 1, 0);
            tableLayoutPanel.Controls.Add(cmbMainFilter, 4, 0);
            tableLayoutPanel.Controls.Add(lblGridFilter, 1, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_0, 4, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_1, 11, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_3, 8, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_4, 11, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_4, 15, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_5, 18, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_1, 15, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_5, 22, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_2, 22, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_2, 18, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_0, 8, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_3, 4, 2);
            tableLayoutPanel.Controls.Add(rbMerge, 21, 0);
            tableLayoutPanel.Controls.Add(btnDeleteAddMerge, 19, 0);
            tableLayoutPanel.Controls.Add(rbAdd, 17, 0);
            tableLayoutPanel.Controls.Add(rbDelete, 15, 0);
            tableLayoutPanel.Controls.Add(rbEdit, 13, 0);
            tableLayoutPanel.Controls.Add(rbView, 11, 0);
            tableLayoutPanel.Controls.Add(lblComboFilter, 1, 6);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_7, 11, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_6, 8, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_6, 4, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_7, 15, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_8, 18, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_8, 22, 3);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_3, 8, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_3, 11, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_4, 14, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_4, 17, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_5, 20, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_5, 23, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_0, 8, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_0, 11, 6);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_1, 14, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_1, 17, 6);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_2, 20, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_2, 23, 6);
            tableLayoutPanel.Controls.Add(cmbComboTableList, 4, 6);
            tableLayoutPanel.Controls.Add(btnBlackLine, 8, 5);
            tableLayoutPanel.Controls.Add(btnReload, 23, 0);
            tableLayoutPanel.Controls.Add(txtManualFilter, 4, 4);
            tableLayoutPanel.Controls.Add(btnExtra, 25, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // GridContextMenu
            // 
            GridContextMenu.ImageScalingSize = new Size(20, 20);
            GridContextMenu.Items.AddRange(new ToolStripItem[] { GridContextMenu_SetAsMainFilter, GridContextMenu_SetFKasMainFIlter, toolStripSeparator1, GridContextMenu_OrderCombolByPK, GridContextMenu_TimesUsedAsFK, GridContextMenu_UnusedAsFK, toolSripSeparator2, GridContextMenu_RestoreFilters, GridContextMenu_ClearFilters });
            GridContextMenu.Name = "contextMenuStrip1";
            resources.ApplyResources(GridContextMenu, "GridContextMenu");
            GridContextMenu.Opening += GridContextMenu_Opening_1;
            // 
            // GridContextMenu_SetAsMainFilter
            // 
            GridContextMenu_SetAsMainFilter.Name = "GridContextMenu_SetAsMainFilter";
            resources.ApplyResources(GridContextMenu_SetAsMainFilter, "GridContextMenu_SetAsMainFilter");
            GridContextMenu_SetAsMainFilter.Click += GridContextMenu_SetAsMainFilter_Click;
            // 
            // GridContextMenu_SetFKasMainFIlter
            // 
            GridContextMenu_SetFKasMainFIlter.DoubleClickEnabled = true;
            GridContextMenu_SetFKasMainFIlter.Name = "GridContextMenu_SetFKasMainFIlter";
            resources.ApplyResources(GridContextMenu_SetFKasMainFIlter, "GridContextMenu_SetFKasMainFIlter");
            GridContextMenu_SetFKasMainFIlter.Click += GridContextMenu_SetFkAsMainFilter_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // GridContextMenu_OrderCombolByPK
            // 
            GridContextMenu_OrderCombolByPK.CheckOnClick = true;
            GridContextMenu_OrderCombolByPK.Name = "GridContextMenu_OrderCombolByPK";
            resources.ApplyResources(GridContextMenu_OrderCombolByPK, "GridContextMenu_OrderCombolByPK");
            GridContextMenu_OrderCombolByPK.Click += GridContextMenu_OrderComboByPK_Click;
            // 
            // GridContextMenu_TimesUsedAsFK
            // 
            GridContextMenu_TimesUsedAsFK.Name = "GridContextMenu_TimesUsedAsFK";
            resources.ApplyResources(GridContextMenu_TimesUsedAsFK, "GridContextMenu_TimesUsedAsFK");
            GridContextMenu_TimesUsedAsFK.Click += GridContextMenu_TimesUsedAsFK_Click;
            // 
            // GridContextMenu_UnusedAsFK
            // 
            GridContextMenu_UnusedAsFK.Name = "GridContextMenu_UnusedAsFK";
            resources.ApplyResources(GridContextMenu_UnusedAsFK, "GridContextMenu_UnusedAsFK");
            GridContextMenu_UnusedAsFK.Click += GridContextMenu_UnusedAsFK_Click;
            // 
            // toolSripSeparator2
            // 
            toolSripSeparator2.Name = "toolSripSeparator2";
            resources.ApplyResources(toolSripSeparator2, "toolSripSeparator2");
            // 
            // GridContextMenu_RestoreFilters
            // 
            GridContextMenu_RestoreFilters.Name = "GridContextMenu_RestoreFilters";
            resources.ApplyResources(GridContextMenu_RestoreFilters, "GridContextMenu_RestoreFilters");
            GridContextMenu_RestoreFilters.Click += GridContextMenu_RestoreFilters_Click;
            // 
            // GridContextMenu_ClearFilters
            // 
            GridContextMenu_ClearFilters.Name = "GridContextMenu_ClearFilters";
            resources.ApplyResources(GridContextMenu_ClearFilters, "GridContextMenu_ClearFilters");
            GridContextMenu_ClearFilters.Click += GridContextMenu_ClearFilters_Click;
            // 
            // lblManualFilter
            // 
            resources.ApplyResources(lblManualFilter, "lblManualFilter");
            tableLayoutPanel.SetColumnSpan(lblManualFilter, 3);
            lblManualFilter.Name = "lblManualFilter";
            // 
            // lblMainFilter
            // 
            resources.ApplyResources(lblMainFilter, "lblMainFilter");
            tableLayoutPanel.SetColumnSpan(lblMainFilter, 3);
            lblMainFilter.Name = "lblMainFilter";
            // 
            // cmbMainFilter
            // 
            resources.ApplyResources(cmbMainFilter, "cmbMainFilter");
            cmbMainFilter.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbMainFilter.AutoCompleteSource = AutoCompleteSource.ListItems;
            tableLayoutPanel.SetColumnSpan(cmbMainFilter, 7);
            cmbMainFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMainFilter.ForeColor = SystemColors.WindowText;
            cmbMainFilter.Name = "cmbMainFilter";
            cmbMainFilter.DropDown += AdjustWidthComboBox_DropDown;
            cmbMainFilter.SelectedIndexChanged += cmbMainFilter_SelectedIndexChanged;
            // 
            // lblGridFilter
            // 
            resources.ApplyResources(lblGridFilter, "lblGridFilter");
            tableLayoutPanel.SetColumnSpan(lblGridFilter, 3);
            lblGridFilter.Name = "lblGridFilter";
            // 
            // cmbGridFilterFields_0
            // 
            resources.ApplyResources(cmbGridFilterFields_0, "cmbGridFilterFields_0");
            cmbGridFilterFields_0.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_0.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_0, 4);
            cmbGridFilterFields_0.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_0.ForeColor = Color.Black;
            cmbGridFilterFields_0.FormattingEnabled = true;
            cmbGridFilterFields_0.Name = "cmbGridFilterFields_0";
            cmbGridFilterFields_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_0.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterFields_1
            // 
            resources.ApplyResources(cmbGridFilterFields_1, "cmbGridFilterFields_1");
            cmbGridFilterFields_1.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_1.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_1, 4);
            cmbGridFilterFields_1.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_1.ForeColor = Color.Black;
            cmbGridFilterFields_1.FormattingEnabled = true;
            cmbGridFilterFields_1.Name = "cmbGridFilterFields_1";
            cmbGridFilterFields_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_1.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_3
            // 
            resources.ApplyResources(cmbGridFilterValue_3, "cmbGridFilterValue_3");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_3, 3);
            cmbGridFilterValue_3.FormattingEnabled = true;
            cmbGridFilterValue_3.Name = "cmbGridFilterValue_3";
            cmbGridFilterValue_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_3.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_3.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_3.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_4
            // 
            resources.ApplyResources(cmbGridFilterFields_4, "cmbGridFilterFields_4");
            cmbGridFilterFields_4.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_4.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_4, 4);
            cmbGridFilterFields_4.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_4.ForeColor = Color.Black;
            cmbGridFilterFields_4.FormattingEnabled = true;
            cmbGridFilterFields_4.Name = "cmbGridFilterFields_4";
            cmbGridFilterFields_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_4.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_4
            // 
            resources.ApplyResources(cmbGridFilterValue_4, "cmbGridFilterValue_4");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_4, 3);
            cmbGridFilterValue_4.FormattingEnabled = true;
            cmbGridFilterValue_4.Name = "cmbGridFilterValue_4";
            cmbGridFilterValue_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_4.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_4.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_4.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_5
            // 
            resources.ApplyResources(cmbGridFilterFields_5, "cmbGridFilterFields_5");
            cmbGridFilterFields_5.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_5.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_5, 4);
            cmbGridFilterFields_5.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_5.ForeColor = Color.Black;
            cmbGridFilterFields_5.FormattingEnabled = true;
            cmbGridFilterFields_5.Name = "cmbGridFilterFields_5";
            cmbGridFilterFields_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_5.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_1
            // 
            resources.ApplyResources(cmbGridFilterValue_1, "cmbGridFilterValue_1");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_1, 3);
            cmbGridFilterValue_1.FormattingEnabled = true;
            cmbGridFilterValue_1.Name = "cmbGridFilterValue_1";
            cmbGridFilterValue_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_1.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_1.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_1.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterValue_5
            // 
            resources.ApplyResources(cmbGridFilterValue_5, "cmbGridFilterValue_5");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_5, 3);
            cmbGridFilterValue_5.FormattingEnabled = true;
            cmbGridFilterValue_5.Name = "cmbGridFilterValue_5";
            cmbGridFilterValue_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_5.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_5.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_5.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterValue_2
            // 
            resources.ApplyResources(cmbGridFilterValue_2, "cmbGridFilterValue_2");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_2, 3);
            cmbGridFilterValue_2.FormattingEnabled = true;
            cmbGridFilterValue_2.Name = "cmbGridFilterValue_2";
            cmbGridFilterValue_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_2.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_2.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_2.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_2
            // 
            resources.ApplyResources(cmbGridFilterFields_2, "cmbGridFilterFields_2");
            cmbGridFilterFields_2.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_2.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_2, 4);
            cmbGridFilterFields_2.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_2.ForeColor = Color.Black;
            cmbGridFilterFields_2.FormattingEnabled = true;
            cmbGridFilterFields_2.Name = "cmbGridFilterFields_2";
            cmbGridFilterFields_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_2.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_0
            // 
            resources.ApplyResources(cmbGridFilterValue_0, "cmbGridFilterValue_0");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_0, 3);
            cmbGridFilterValue_0.FormattingEnabled = true;
            cmbGridFilterValue_0.Name = "cmbGridFilterValue_0";
            cmbGridFilterValue_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_0.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_0.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_0.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_3
            // 
            resources.ApplyResources(cmbGridFilterFields_3, "cmbGridFilterFields_3");
            cmbGridFilterFields_3.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_3.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_3, 4);
            cmbGridFilterFields_3.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_3.ForeColor = Color.Black;
            cmbGridFilterFields_3.FormattingEnabled = true;
            cmbGridFilterFields_3.Name = "cmbGridFilterFields_3";
            cmbGridFilterFields_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_3.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // rbMerge
            // 
            resources.ApplyResources(rbMerge, "rbMerge");
            tableLayoutPanel.SetColumnSpan(rbMerge, 2);
            rbMerge.Name = "rbMerge";
            rbMerge.UseVisualStyleBackColor = true;
            rbMerge.CheckedChanged += rbMerge_CheckedChanged;
            // 
            // btnDeleteAddMerge
            // 
            resources.ApplyResources(btnDeleteAddMerge, "btnDeleteAddMerge");
            tableLayoutPanel.SetColumnSpan(btnDeleteAddMerge, 2);
            btnDeleteAddMerge.ContextMenuStrip = GridContextMenu;
            btnDeleteAddMerge.ForeColor = Color.Black;
            btnDeleteAddMerge.Name = "btnDeleteAddMerge";
            btnDeleteAddMerge.UseVisualStyleBackColor = true;
            btnDeleteAddMerge.Click += btnDeleteAddMerge_Click;
            // 
            // rbAdd
            // 
            resources.ApplyResources(rbAdd, "rbAdd");
            tableLayoutPanel.SetColumnSpan(rbAdd, 2);
            rbAdd.Name = "rbAdd";
            rbAdd.UseVisualStyleBackColor = true;
            rbAdd.CheckedChanged += rbAdd_CheckedChanged;
            // 
            // rbDelete
            // 
            resources.ApplyResources(rbDelete, "rbDelete");
            tableLayoutPanel.SetColumnSpan(rbDelete, 2);
            rbDelete.Name = "rbDelete";
            rbDelete.UseVisualStyleBackColor = true;
            rbDelete.CheckedChanged += rbDelete_CheckedChanged;
            // 
            // rbEdit
            // 
            resources.ApplyResources(rbEdit, "rbEdit");
            tableLayoutPanel.SetColumnSpan(rbEdit, 2);
            rbEdit.Name = "rbEdit";
            rbEdit.UseVisualStyleBackColor = true;
            rbEdit.CheckedChanged += rbEdit_CheckedChanged;
            // 
            // rbView
            // 
            resources.ApplyResources(rbView, "rbView");
            tableLayoutPanel.SetColumnSpan(rbView, 2);
            rbView.Name = "rbView";
            rbView.UseVisualStyleBackColor = true;
            rbView.CheckedChanged += rbView_CheckedChanged;
            // 
            // lblComboFilter
            // 
            resources.ApplyResources(lblComboFilter, "lblComboFilter");
            tableLayoutPanel.SetColumnSpan(lblComboFilter, 3);
            lblComboFilter.Name = "lblComboFilter";
            // 
            // cmbGridFilterFields_7
            // 
            resources.ApplyResources(cmbGridFilterFields_7, "cmbGridFilterFields_7");
            cmbGridFilterFields_7.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_7.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_7, 4);
            cmbGridFilterFields_7.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_7.ForeColor = Color.Black;
            cmbGridFilterFields_7.FormattingEnabled = true;
            cmbGridFilterFields_7.Name = "cmbGridFilterFields_7";
            cmbGridFilterFields_7.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_6
            // 
            resources.ApplyResources(cmbGridFilterValue_6, "cmbGridFilterValue_6");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_6, 3);
            cmbGridFilterValue_6.FormattingEnabled = true;
            cmbGridFilterValue_6.Name = "cmbGridFilterValue_6";
            cmbGridFilterValue_6.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_6.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_6.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_6.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_6
            // 
            resources.ApplyResources(cmbGridFilterFields_6, "cmbGridFilterFields_6");
            cmbGridFilterFields_6.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_6.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_6, 4);
            cmbGridFilterFields_6.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_6.ForeColor = Color.Black;
            cmbGridFilterFields_6.FormattingEnabled = true;
            cmbGridFilterFields_6.Name = "cmbGridFilterFields_6";
            cmbGridFilterFields_6.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_7
            // 
            resources.ApplyResources(cmbGridFilterValue_7, "cmbGridFilterValue_7");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_7, 3);
            cmbGridFilterValue_7.FormattingEnabled = true;
            cmbGridFilterValue_7.Name = "cmbGridFilterValue_7";
            cmbGridFilterValue_7.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_7.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_7.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_7.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_8
            // 
            resources.ApplyResources(cmbGridFilterFields_8, "cmbGridFilterFields_8");
            cmbGridFilterFields_8.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_8.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_8, 4);
            cmbGridFilterFields_8.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_8.ForeColor = Color.Black;
            cmbGridFilterFields_8.FormattingEnabled = true;
            cmbGridFilterFields_8.Name = "cmbGridFilterFields_8";
            cmbGridFilterFields_8.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_8
            // 
            resources.ApplyResources(cmbGridFilterValue_8, "cmbGridFilterValue_8");
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_8, 3);
            cmbGridFilterValue_8.FormattingEnabled = true;
            cmbGridFilterValue_8.Name = "cmbGridFilterValue_8";
            cmbGridFilterValue_8.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_8.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_8.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_8.Enter += cmbGridFilterValue_Enter;
            // 
            // lblCmbFilterField_3
            // 
            resources.ApplyResources(lblCmbFilterField_3, "lblCmbFilterField_3");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_3, 3);
            lblCmbFilterField_3.Name = "lblCmbFilterField_3";
            // 
            // cmbComboFilterValue_3
            // 
            resources.ApplyResources(cmbComboFilterValue_3, "cmbComboFilterValue_3");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_3, 3);
            cmbComboFilterValue_3.FormattingEnabled = true;
            cmbComboFilterValue_3.Name = "cmbComboFilterValue_3";
            cmbComboFilterValue_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_3.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_3.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_3.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_3.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_4
            // 
            resources.ApplyResources(lblCmbFilterField_4, "lblCmbFilterField_4");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_4, 3);
            lblCmbFilterField_4.Name = "lblCmbFilterField_4";
            // 
            // cmbComboFilterValue_4
            // 
            resources.ApplyResources(cmbComboFilterValue_4, "cmbComboFilterValue_4");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_4, 3);
            cmbComboFilterValue_4.FormattingEnabled = true;
            cmbComboFilterValue_4.Name = "cmbComboFilterValue_4";
            cmbComboFilterValue_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_4.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_4.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_4.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_4.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_5
            // 
            resources.ApplyResources(lblCmbFilterField_5, "lblCmbFilterField_5");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_5, 3);
            lblCmbFilterField_5.Name = "lblCmbFilterField_5";
            // 
            // cmbComboFilterValue_5
            // 
            resources.ApplyResources(cmbComboFilterValue_5, "cmbComboFilterValue_5");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_5, 3);
            cmbComboFilterValue_5.FormattingEnabled = true;
            cmbComboFilterValue_5.Name = "cmbComboFilterValue_5";
            cmbComboFilterValue_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_5.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbComboFilterValue_5.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_5.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_5.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_5.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_0
            // 
            resources.ApplyResources(lblCmbFilterField_0, "lblCmbFilterField_0");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_0, 3);
            lblCmbFilterField_0.Name = "lblCmbFilterField_0";
            // 
            // cmbComboFilterValue_0
            // 
            resources.ApplyResources(cmbComboFilterValue_0, "cmbComboFilterValue_0");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_0, 3);
            cmbComboFilterValue_0.FormattingEnabled = true;
            cmbComboFilterValue_0.Name = "cmbComboFilterValue_0";
            cmbComboFilterValue_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_0.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_0.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_0.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_0.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_1
            // 
            resources.ApplyResources(lblCmbFilterField_1, "lblCmbFilterField_1");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_1, 3);
            lblCmbFilterField_1.Name = "lblCmbFilterField_1";
            // 
            // cmbComboFilterValue_1
            // 
            resources.ApplyResources(cmbComboFilterValue_1, "cmbComboFilterValue_1");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_1, 3);
            cmbComboFilterValue_1.FormattingEnabled = true;
            cmbComboFilterValue_1.Name = "cmbComboFilterValue_1";
            cmbComboFilterValue_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_1.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_1.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_1.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_1.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_2
            // 
            resources.ApplyResources(lblCmbFilterField_2, "lblCmbFilterField_2");
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_2, 3);
            lblCmbFilterField_2.Name = "lblCmbFilterField_2";
            // 
            // cmbComboFilterValue_2
            // 
            resources.ApplyResources(cmbComboFilterValue_2, "cmbComboFilterValue_2");
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_2, 3);
            cmbComboFilterValue_2.FormattingEnabled = true;
            cmbComboFilterValue_2.Name = "cmbComboFilterValue_2";
            cmbComboFilterValue_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_2.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_2.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_2.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_2.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // cmbComboTableList
            // 
            resources.ApplyResources(cmbComboTableList, "cmbComboTableList");
            cmbComboTableList.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbComboTableList.AutoCompleteSource = AutoCompleteSource.ListItems;
            tableLayoutPanel.SetColumnSpan(cmbComboTableList, 4);
            cmbComboTableList.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbComboTableList.FormattingEnabled = true;
            cmbComboTableList.Name = "cmbComboTableList";
            cmbComboTableList.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboTableList.SelectedIndexChanged += cmbComboTableList_SelectedIndexChanged;
            // 
            // btnBlackLine
            // 
            btnBlackLine.BackColor = Color.Black;
            btnBlackLine.CausesValidation = false;
            tableLayoutPanel.SetColumnSpan(btnBlackLine, 16);
            btnBlackLine.ForeColor = Color.Black;
            resources.ApplyResources(btnBlackLine, "btnBlackLine");
            btnBlackLine.Name = "btnBlackLine";
            btnBlackLine.UseVisualStyleBackColor = false;
            // 
            // btnReload
            // 
            resources.ApplyResources(btnReload, "btnReload");
            btnReload.Image = Properties.MyResources.iconmonstr_arrow_32_OneRight;
            btnReload.Name = "btnReload";
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // txtManualFilter
            // 
            tableLayoutPanel.SetColumnSpan(txtManualFilter, 20);
            resources.ApplyResources(txtManualFilter, "txtManualFilter");
            txtManualFilter.Name = "txtManualFilter";
            txtManualFilter.TextChanged += txtManualFilter_TextChanged;
            // 
            // btnExtra
            // 
            tableLayoutPanel.SetColumnSpan(btnExtra, 3);
            resources.ApplyResources(btnExtra, "btnExtra");
            btnExtra.Name = "btnExtra";
            btnExtra.UseVisualStyleBackColor = true;
            btnExtra.Click += btnExtra_Click;
            // 
            // txtMessages
            // 
            txtMessages.BackColor = SystemColors.ControlLight;
            resources.ApplyResources(txtMessages, "txtMessages");
            txtMessages.ForeColor = Color.Red;
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.WhiteSmoke;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(dataGridView1, "dataGridView1");
            dataGridView1.ContextMenuStrip = GridContextMenu;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.WhiteSmoke;
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.RowTemplate.Height = 27;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.CellEnter += dataGridView1_CellEnter;
            dataGridView1.CellLeave += dataGridView1_CellLeave;
            dataGridView1.CellParsing += dataGridView1_CellParsing;
            dataGridView1.CellValidated += dataGridView1_CellValidated;
            dataGridView1.CellValidating += dataGridView1_CellValidating;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.ColumnWidthChanged += dataGridView1_ColumnWidthChanged;
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
            dataGridView1.DataError += dataGridView1_DataError;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.Enter += dataGridView1_Enter;
            dataGridView1.MouseLeave += dataGridView1_MouseLeave;
            dataGridView1.Validating += dataGridView1_Validating;
            dataGridView1.Validated += dataGridView1_Validated;
            // 
            // MainMenu1
            // 
            MainMenu1.BackColor = SystemColors.ControlLight;
            MainMenu1.ImageScalingSize = new Size(20, 20);
            MainMenu1.Items.AddRange(new ToolStripItem[] { mnuFile, mnuOpenTables, mnuTools, mnuIT_Tools, mnuHelp });
            resources.ApplyResources(MainMenu1, "MainMenu1");
            MainMenu1.Name = "MainMenu1";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuConnections, mnuClose });
            mnuFile.Name = "mnuFile";
            resources.ApplyResources(mnuFile, "mnuFile");
            // 
            // mnuConnections
            // 
            mnuConnections.DropDownItems.AddRange(new ToolStripItem[] { mnuConnectionList, mnuBlankLine3, mnuAddConnection, mnuDeleteConnection });
            mnuConnections.Name = "mnuConnections";
            resources.ApplyResources(mnuConnections, "mnuConnections");
            // 
            // mnuConnectionList
            // 
            mnuConnectionList.Name = "mnuConnectionList";
            resources.ApplyResources(mnuConnectionList, "mnuConnectionList");
            mnuConnectionList.DropDownItemClicked += mnuDatabaseList_DropDownItemClicked;
            // 
            // mnuBlankLine3
            // 
            mnuBlankLine3.Name = "mnuBlankLine3";
            resources.ApplyResources(mnuBlankLine3, "mnuBlankLine3");
            // 
            // mnuAddConnection
            // 
            mnuAddConnection.Name = "mnuAddConnection";
            resources.ApplyResources(mnuAddConnection, "mnuAddConnection");
            mnuAddConnection.Click += mnuAddDatabase_Click;
            // 
            // mnuDeleteConnection
            // 
            mnuDeleteConnection.Name = "mnuDeleteConnection";
            resources.ApplyResources(mnuDeleteConnection, "mnuDeleteConnection");
            mnuDeleteConnection.Click += mnuDeleteConnectionString_Click;
            // 
            // mnuClose
            // 
            mnuClose.Name = "mnuClose";
            resources.ApplyResources(mnuClose, "mnuClose");
            mnuClose.Click += mnuClose_Click;
            // 
            // mnuOpenTables
            // 
            mnuOpenTables.Name = "mnuOpenTables";
            resources.ApplyResources(mnuOpenTables, "mnuOpenTables");
            mnuOpenTables.DropDownItemClicked += mnuOpenTables_DropDownItemClicked;
            mnuOpenTables.Click += mnuOpenTables_Click;
            // 
            // mnuTools
            // 
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuPrintCurrentTable });
            mnuTools.Name = "mnuTools";
            resources.ApplyResources(mnuTools, "mnuTools");
            // 
            // mnuPrintCurrentTable
            // 
            mnuPrintCurrentTable.Name = "mnuPrintCurrentTable";
            resources.ApplyResources(mnuPrintCurrentTable, "mnuPrintCurrentTable");
            mnuPrintCurrentTable.Click += mnuPrintCurrentTable_Click;
            // 
            // mnuIT_Tools
            // 
            mnuIT_Tools.DropDownItems.AddRange(new ToolStripItem[] { mnuDatabaseInfo, mnuToolsBackupDatabase, toolStripSeparator2, mnuForeignKeyMissing, mnuDisplayKeysList, mnuDuplicateDisplayKeys, mnuRapidlyMergeDKs, toolStripSeparator3, mnuLoadPlugin, mnuRemovePlugin, mnuShowITTools });
            mnuIT_Tools.Name = "mnuIT_Tools";
            resources.ApplyResources(mnuIT_Tools, "mnuIT_Tools");
            // 
            // mnuDatabaseInfo
            // 
            mnuDatabaseInfo.Name = "mnuDatabaseInfo";
            resources.ApplyResources(mnuDatabaseInfo, "mnuDatabaseInfo");
            mnuDatabaseInfo.Click += mnuToolsDatabaseInformation_Click;
            // 
            // mnuToolsBackupDatabase
            // 
            mnuToolsBackupDatabase.Name = "mnuToolsBackupDatabase";
            resources.ApplyResources(mnuToolsBackupDatabase, "mnuToolsBackupDatabase");
            mnuToolsBackupDatabase.Click += mnuToolsBackupDatabase_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            // 
            // mnuForeignKeyMissing
            // 
            mnuForeignKeyMissing.Name = "mnuForeignKeyMissing";
            resources.ApplyResources(mnuForeignKeyMissing, "mnuForeignKeyMissing");
            mnuForeignKeyMissing.Click += mnuForeignKeyMissing_Click;
            // 
            // mnuDisplayKeysList
            // 
            mnuDisplayKeysList.Name = "mnuDisplayKeysList";
            resources.ApplyResources(mnuDisplayKeysList, "mnuDisplayKeysList");
            mnuDisplayKeysList.Click += mnuDisplayKeysList_Click;
            // 
            // mnuDuplicateDisplayKeys
            // 
            mnuDuplicateDisplayKeys.Name = "mnuDuplicateDisplayKeys";
            resources.ApplyResources(mnuDuplicateDisplayKeys, "mnuDuplicateDisplayKeys");
            mnuDuplicateDisplayKeys.Click += mnuToolDuplicateDisplayKeys_Click;
            // 
            // mnuRapidlyMergeDKs
            // 
            mnuRapidlyMergeDKs.CheckOnClick = true;
            mnuRapidlyMergeDKs.Name = "mnuRapidlyMergeDKs";
            resources.ApplyResources(mnuRapidlyMergeDKs, "mnuRapidlyMergeDKs");
            mnuRapidlyMergeDKs.Click += mnuRapidlyMergeDKs_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            // 
            // mnuLoadPlugin
            // 
            mnuLoadPlugin.Name = "mnuLoadPlugin";
            resources.ApplyResources(mnuLoadPlugin, "mnuLoadPlugin");
            mnuLoadPlugin.Click += mnuLoadPlugin_Click;
            // 
            // mnuRemovePlugin
            // 
            mnuRemovePlugin.Name = "mnuRemovePlugin";
            resources.ApplyResources(mnuRemovePlugin, "mnuRemovePlugin");
            mnuRemovePlugin.Click += mnuRemovePlugin_Click;
            // 
            // mnuShowITTools
            // 
            mnuShowITTools.Checked = true;
            mnuShowITTools.CheckOnClick = true;
            mnuShowITTools.CheckState = CheckState.Checked;
            mnuShowITTools.Name = "mnuShowITTools";
            resources.ApplyResources(mnuShowITTools, "mnuShowITTools");
            mnuShowITTools.CheckedChanged += mnuShowITTools_CheckedChanged;
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuHelpFile });
            mnuHelp.Name = "mnuHelp";
            resources.ApplyResources(mnuHelp, "mnuHelp");
            // 
            // mnuHelpFile
            // 
            mnuHelpFile.Name = "mnuHelpFile";
            resources.ApplyResources(mnuHelpFile, "mnuHelpFile");
            // 
            // toolStripBottom
            // 
            resources.ApplyResources(toolStripBottom, "toolStripBottom");
            toolStripBottom.ImageScalingSize = new Size(20, 20);
            toolStripBottom.Items.AddRange(new ToolStripItem[] { txtRecordsPerPage, toolStripButton4, toolStripButton3, toolStripButton2, toolStripButtonColumnWidth });
            toolStripBottom.Name = "toolStripBottom";
            toolStripBottom.ShowItemToolTips = false;
            toolStripBottom.Stretch = true;
            toolStripBottom.ItemClicked += toolStripBottom_ItemClicked;
            // 
            // txtRecordsPerPage
            // 
            txtRecordsPerPage.Alignment = ToolStripItemAlignment.Right;
            txtRecordsPerPage.Name = "txtRecordsPerPage";
            resources.ApplyResources(txtRecordsPerPage, "txtRecordsPerPage");
            txtRecordsPerPage.Leave += txtRecordsPerPage_Leave;
            // 
            // toolStripButton4
            // 
            toolStripButton4.Alignment = ToolStripItemAlignment.Right;
            toolStripButton4.AutoToolTip = false;
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = Properties.MyResources.iconmonstr_arrow_32_OneRight;
            resources.ApplyResources(toolStripButton4, "toolStripButton4");
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.Alignment = ToolStripItemAlignment.Right;
            toolStripButton3.AutoToolTip = false;
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(toolStripButton3, "toolStripButton3");
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.Alignment = ToolStripItemAlignment.Right;
            toolStripButton2.AutoToolTip = false;
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Properties.MyResources.iconmonstr_arrow_32_OneLEFT;
            resources.ApplyResources(toolStripButton2, "toolStripButton2");
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButtonColumnWidth
            // 
            toolStripButtonColumnWidth.Alignment = ToolStripItemAlignment.Right;
            resources.ApplyResources(toolStripButtonColumnWidth, "toolStripButtonColumnWidth");
            toolStripButtonColumnWidth.AutoToolTip = false;
            toolStripButtonColumnWidth.BackColor = Color.Silver;
            toolStripButtonColumnWidth.CheckOnClick = true;
            toolStripButtonColumnWidth.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonColumnWidth.DoubleClickEnabled = true;
            toolStripButtonColumnWidth.Margin = new Padding(10, 0, 10, 0);
            toolStripButtonColumnWidth.Name = "toolStripButtonColumnWidth";
            toolStripButtonColumnWidth.Click += toolStripColumnWidth_Click;
            // 
            // DataGridViewForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(toolStripBottom);
            Controls.Add(splitContainer1);
            Controls.Add(MainMenu1);
            KeyPreview = true;
            Name = "DataGridViewForm";
            FormClosed += DataGridViewForm_FormClosed;
            Load += DataGridViewForm_Load;
            Resize += DataGridViewForm_Resize;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            GridContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            MainMenu1.ResumeLayout(false);
            MainMenu1.PerformLayout();
            toolStripBottom.ResumeLayout(false);
            toolStripBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public MenuStrip MainMenu1;
        public DataGridView dataGridView1;

        public ToolStripMenuItem mnuFile;
        public ToolStripMenuItem mnuConnections;
        private ToolStripMenuItem mnuConnectionList;
        public ToolStripMenuItem mnuBlankLine3;
        public ToolStripMenuItem mnuAddConnection;
        public ToolStripMenuItem mnuDeleteConnection;
        public ToolStripMenuItem mnuClose;
        public ToolStripMenuItem mnuOpenTables;
        public ToolStripMenuItem mnuHelp;
        public ToolStripMenuItem mnuHelpFile;
        private ToolStrip toolStripBottom;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel;
        private Label lblMainFilter;
        private ContextMenuStrip GridContextMenu;
        private ToolStripMenuItem GridContextMenu_SetAsMainFilter;
        private ComboBox cmbMainFilter;
        private RadioButton rbAdd;
        private RadioButton rbEdit;
        private RadioButton rbView;
        private RadioButton rbDelete;
        private RadioButton rbMerge;
        private TextBox txtMessages;
        private ToolStripTextBox txtRecordsPerPage;
        private Button btnDeleteAddMerge;
        private ToolStripMenuItem mnuDatabaseInfo;
        private ComboBox cmbGridFilterFields_0;
        private ToolStripMenuItem contextMenu_ShowAllFilters;
        private ComboBox cmbGridFilterFields_1;
        private ComboBox cmbGridFilterFields_2;
        private Label lblGridFilter;
        private Label lblComboFilter;
        private ComboBox cmbGridFilterValue_3;
        private ComboBox cmbGridFilterFields_4;
        private ComboBox cmbGridFilterValue_4;
        private ComboBox cmbGridFilterFields_5;
        private ComboBox cmbGridFilterValue_1;
        private ComboBox cmbGridFilterValue_5;
        private ComboBox cmbGridFilterValue_2;
        private ComboBox cmbGridFilterValue_0;
        private ComboBox cmbGridFilterFields_3;
        private ComboBox cmbComboTableList;
        private Label lblCmbFilterField_0;
        private Label lblCmbFilterField_1;
        private Label lblCmbFilterField_2;
        private Label lblCmbFilterField_3;
        private Label lblCmbFilterField_4;
        private Label lblCmbFilterField_5;
        private ComboBox cmbComboFilterValue_0;
        private ComboBox cmbComboFilterValue_1;
        private ComboBox cmbComboFilterValue_2;
        private ComboBox cmbComboFilterValue_3;
        private ComboBox cmbComboFilterValue_4;
        private ComboBox cmbComboFilterValue_5;
        private ToolStripMenuItem GridContextMenu_SetFKasMainFIlter;
        private ToolStripMenuItem mnuDuplicateDisplayKeys;
        private ToolStripMenuItem mnuForeignKeyMissing;
        private ComboBox cmbGridFilterFields_7;
        private ComboBox cmbGridFilterValue_6;
        private ComboBox cmbGridFilterFields_6;
        private ComboBox cmbGridFilterValue_7;
        private ComboBox cmbGridFilterFields_8;
        private ComboBox cmbGridFilterValue_8;
        private Button btnBlackLine;
        private Button btnReload;
        private ToolStripMenuItem GridContextMenu_TimesUsedAsFK;
        private ToolStripButton toolStripButtonColumnWidth;
        private ToolStripSeparator toolSripSeparator2;
        private ToolStripMenuItem GridContextMenu_RestoreFilters;
        private ToolStripMenuItem GridContextMenu_ClearFilters;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem GridContextMenu_OrderCombolByPK;
        private ToolStripMenuItem backupDatabaseToolStripMenuItem;
        private ToolStripMenuItem mnuToolsBackupDatabase;
        private SaveFileDialog saveFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label lblManualFilter;
        private TextBox txtManualFilter;
        private ToolStripMenuItem mnuTools;
        internal ToolStripMenuItem mnuIT_Tools;
        public ToolStripMenuItem mnuPrintCurrentTable;
        private ToolStripMenuItem mnuShowITTools;
        private ToolStripMenuItem mnuLoadPlugin;
        private ToolStripMenuItem mnuRemovePlugin;
        private ToolStripMenuItem mnuDisplayKeysList;
        private Button btnHiddenExtra;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuRapidlyMergeDKs;
        private ToolStripSeparator toolStripSeparator3;
        private Button btnExtra;
        private ToolStripMenuItem GridContextMenu_UnusedAsFK;
        //private Button button2;
    }
}