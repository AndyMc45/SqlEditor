﻿namespace SqlEditor.TranscriptPlugin
{
    partial class frmTranscriptOptions
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            tabControl1 = new TabControl();
            tabActions = new TabPage();
            chkIncludeAudits = new CheckBox();
            btnPrintCourseGrades = new Button();
            btnPrintEnglishTranscript = new Button();
            btnPrintCourseRole = new Button();
            btnPrintTranscript = new Button();
            tabStudent = new TabPage();
            dgvStudent = new DataGridView();
            tabTranscript = new TabPage();
            dgvTranscript = new DataGridView();
            tabRequirements = new TabPage();
            dgvRequirements = new DataGridView();
            tabOptions = new TabPage();
            lblPathDatabaseFolder = new Label();
            lblDatabaseFolder = new LinkLabel();
            lblPathCourseGradeTemplate = new Label();
            lblPathEnglishTranscriptTemplate = new Label();
            lblCourseGradesTemplate = new LinkLabel();
            lblEnglishTranscriptTemplate = new LinkLabel();
            lblPathDocumentFolder = new Label();
            lblDocumentFolder = new LinkLabel();
            lblRestartMsg = new Label();
            cmbInterfaceLanguage = new ComboBox();
            lblLanguage = new Label();
            lblPathCourseRoleTemplate = new Label();
            lblPathTemplateFolder = new Label();
            lblPathTransTemplate = new Label();
            lblTemplateFolder = new LinkLabel();
            lblCourseRoleTemplate = new LinkLabel();
            lblTranscriptTemplate = new LinkLabel();
            lblOptions = new Label();
            tabExit = new TabPage();
            toolStripBottom = new ToolStrip();
            toolStripBtnNarrow = new ToolStripButton();
            folderBrowserDialog1 = new FolderBrowserDialog();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            tabControl1.SuspendLayout();
            tabActions.SuspendLayout();
            tabStudent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStudent).BeginInit();
            tabTranscript.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTranscript).BeginInit();
            tabRequirements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRequirements).BeginInit();
            tabOptions.SuspendLayout();
            toolStripBottom.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabActions);
            tabControl1.Controls.Add(tabStudent);
            tabControl1.Controls.Add(tabTranscript);
            tabControl1.Controls.Add(tabRequirements);
            tabControl1.Controls.Add(tabOptions);
            tabControl1.Controls.Add(tabExit);
            tabControl1.Dock = DockStyle.Left;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1924, 941);
            tabControl1.TabIndex = 7;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            tabControl1.Resize += tabControl1_Resize;
            // 
            // tabActions
            // 
            tabActions.Controls.Add(chkIncludeAudits);
            tabActions.Controls.Add(btnPrintCourseGrades);
            tabActions.Controls.Add(btnPrintEnglishTranscript);
            tabActions.Controls.Add(btnPrintCourseRole);
            tabActions.Controls.Add(btnPrintTranscript);
            tabActions.Location = new Point(4, 34);
            tabActions.Margin = new Padding(4);
            tabActions.Name = "tabActions";
            tabActions.Size = new Size(1916, 903);
            tabActions.TabIndex = 4;
            tabActions.Text = "Actions";
            tabActions.UseVisualStyleBackColor = true;
            // 
            // chkIncludeAudits
            // 
            chkIncludeAudits.AutoSize = true;
            chkIncludeAudits.Location = new Point(26, 93);
            chkIncludeAudits.Name = "chkIncludeAudits";
            chkIncludeAudits.Size = new Size(231, 29);
            chkIncludeAudits.TabIndex = 18;
            chkIncludeAudits.Text = "Include Audited Courses";
            chkIncludeAudits.UseVisualStyleBackColor = true;
            // 
            // btnPrintCourseGrades
            // 
            btnPrintCourseGrades.Location = new Point(400, 162);
            btnPrintCourseGrades.Margin = new Padding(4, 5, 4, 5);
            btnPrintCourseGrades.Name = "btnPrintCourseGrades";
            btnPrintCourseGrades.Size = new Size(312, 39);
            btnPrintCourseGrades.TabIndex = 17;
            btnPrintCourseGrades.Text = "Print Course Grades";
            btnPrintCourseGrades.UseVisualStyleBackColor = true;
            btnPrintCourseGrades.Click += btnPrintCourseGrades_Click;
            // 
            // btnPrintEnglishTranscript
            // 
            btnPrintEnglishTranscript.Location = new Point(400, 50);
            btnPrintEnglishTranscript.Margin = new Padding(4);
            btnPrintEnglishTranscript.Name = "btnPrintEnglishTranscript";
            btnPrintEnglishTranscript.Size = new Size(312, 36);
            btnPrintEnglishTranscript.TabIndex = 16;
            btnPrintEnglishTranscript.Text = "列印英文成績單";
            btnPrintEnglishTranscript.UseVisualStyleBackColor = true;
            btnPrintEnglishTranscript.Click += btnPrintEnglishTranscript_Click;
            // 
            // btnPrintCourseRole
            // 
            btnPrintCourseRole.Location = new Point(26, 162);
            btnPrintCourseRole.Margin = new Padding(4);
            btnPrintCourseRole.Name = "btnPrintCourseRole";
            btnPrintCourseRole.Size = new Size(250, 36);
            btnPrintCourseRole.TabIndex = 15;
            btnPrintCourseRole.Text = "Print Course Role";
            btnPrintCourseRole.UseVisualStyleBackColor = true;
            btnPrintCourseRole.Click += btnPrintClassRole_Click;
            // 
            // btnPrintTranscript
            // 
            btnPrintTranscript.Location = new Point(26, 50);
            btnPrintTranscript.Margin = new Padding(4);
            btnPrintTranscript.Name = "btnPrintTranscript";
            btnPrintTranscript.Size = new Size(250, 36);
            btnPrintTranscript.TabIndex = 14;
            btnPrintTranscript.Text = "列印成績單";
            btnPrintTranscript.UseVisualStyleBackColor = true;
            btnPrintTranscript.Click += btnPrintTranscript_Click;
            // 
            // tabStudent
            // 
            tabStudent.AutoScroll = true;
            tabStudent.Controls.Add(dgvStudent);
            tabStudent.Location = new Point(4, 34);
            tabStudent.Margin = new Padding(4);
            tabStudent.Name = "tabStudent";
            tabStudent.Size = new Size(1916, 903);
            tabStudent.TabIndex = 3;
            tabStudent.Text = "Student";
            tabStudent.UseVisualStyleBackColor = true;
            // 
            // dgvStudent
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvStudent.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvStudent.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStudent.Dock = DockStyle.Left;
            dgvStudent.Location = new Point(0, 0);
            dgvStudent.Margin = new Padding(4);
            dgvStudent.Name = "dgvStudent";
            dgvStudent.RowHeadersWidth = 51;
            dgvStudent.RowTemplate.Height = 29;
            dgvStudent.Size = new Size(1914, 903);
            dgvStudent.TabIndex = 0;
            // 
            // tabTranscript
            // 
            tabTranscript.AutoScroll = true;
            tabTranscript.Controls.Add(dgvTranscript);
            tabTranscript.Location = new Point(4, 34);
            tabTranscript.Margin = new Padding(4);
            tabTranscript.Name = "tabTranscript";
            tabTranscript.Size = new Size(1916, 903);
            tabTranscript.TabIndex = 2;
            tabTranscript.Text = "Transcript";
            tabTranscript.UseVisualStyleBackColor = true;
            // 
            // dgvTranscript
            // 
            dgvTranscript.AllowUserToAddRows = false;
            dgvTranscript.AllowUserToDeleteRows = false;
            dgvTranscript.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvTranscript.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvTranscript.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTranscript.Dock = DockStyle.Left;
            dgvTranscript.GridColor = Color.WhiteSmoke;
            dgvTranscript.Location = new Point(0, 0);
            dgvTranscript.Margin = new Padding(4);
            dgvTranscript.Name = "dgvTranscript";
            dgvTranscript.RowHeadersWidth = 51;
            dgvTranscript.RowTemplate.Height = 29;
            dgvTranscript.Size = new Size(1968, 877);
            dgvTranscript.TabIndex = 0;
            // 
            // tabRequirements
            // 
            tabRequirements.AutoScroll = true;
            tabRequirements.Controls.Add(dgvRequirements);
            tabRequirements.Location = new Point(4, 34);
            tabRequirements.Margin = new Padding(4);
            tabRequirements.Name = "tabRequirements";
            tabRequirements.Padding = new Padding(4);
            tabRequirements.Size = new Size(1916, 903);
            tabRequirements.TabIndex = 1;
            tabRequirements.Text = "Requirements";
            tabRequirements.UseVisualStyleBackColor = true;
            // 
            // dgvRequirements
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvRequirements.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvRequirements.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRequirements.Dock = DockStyle.Left;
            dgvRequirements.Location = new Point(4, 4);
            dgvRequirements.Margin = new Padding(4);
            dgvRequirements.Name = "dgvRequirements";
            dgvRequirements.RowHeadersWidth = 51;
            dgvRequirements.RowTemplate.Height = 29;
            dgvRequirements.Size = new Size(1960, 869);
            dgvRequirements.TabIndex = 0;
            // 
            // tabOptions
            // 
            tabOptions.Controls.Add(lblPathDatabaseFolder);
            tabOptions.Controls.Add(lblDatabaseFolder);
            tabOptions.Controls.Add(lblPathCourseGradeTemplate);
            tabOptions.Controls.Add(lblPathEnglishTranscriptTemplate);
            tabOptions.Controls.Add(lblCourseGradesTemplate);
            tabOptions.Controls.Add(lblEnglishTranscriptTemplate);
            tabOptions.Controls.Add(lblPathDocumentFolder);
            tabOptions.Controls.Add(lblDocumentFolder);
            tabOptions.Controls.Add(lblRestartMsg);
            tabOptions.Controls.Add(cmbInterfaceLanguage);
            tabOptions.Controls.Add(lblLanguage);
            tabOptions.Controls.Add(lblPathCourseRoleTemplate);
            tabOptions.Controls.Add(lblPathTemplateFolder);
            tabOptions.Controls.Add(lblPathTransTemplate);
            tabOptions.Controls.Add(lblTemplateFolder);
            tabOptions.Controls.Add(lblCourseRoleTemplate);
            tabOptions.Controls.Add(lblTranscriptTemplate);
            tabOptions.Controls.Add(lblOptions);
            tabOptions.Location = new Point(4, 34);
            tabOptions.Margin = new Padding(4);
            tabOptions.Name = "tabOptions";
            tabOptions.Padding = new Padding(4);
            tabOptions.Size = new Size(1916, 903);
            tabOptions.TabIndex = 0;
            tabOptions.Text = "Options";
            tabOptions.UseVisualStyleBackColor = true;
            // 
            // lblPathDatabaseFolder
            // 
            lblPathDatabaseFolder.AutoSize = true;
            lblPathDatabaseFolder.BorderStyle = BorderStyle.FixedSingle;
            lblPathDatabaseFolder.Font = new Font("Times New Roman", 9F);
            lblPathDatabaseFolder.Location = new Point(322, 596);
            lblPathDatabaseFolder.Margin = new Padding(4, 0, 4, 0);
            lblPathDatabaseFolder.Name = "lblPathDatabaseFolder";
            lblPathDatabaseFolder.Size = new Size(185, 22);
            lblPathDatabaseFolder.TabIndex = 34;
            lblPathDatabaseFolder.Text = "Database backup Folder";
            // 
            // lblDatabaseFolder
            // 
            lblDatabaseFolder.AutoSize = true;
            lblDatabaseFolder.Location = new Point(42, 592);
            lblDatabaseFolder.Margin = new Padding(4, 0, 4, 0);
            lblDatabaseFolder.Name = "lblDatabaseFolder";
            lblDatabaseFolder.Size = new Size(213, 25);
            lblDatabaseFolder.TabIndex = 33;
            lblDatabaseFolder.TabStop = true;
            lblDatabaseFolder.Text = "Database backup Folder :";
            lblDatabaseFolder.LinkClicked += lblDatabaseFolder_LinkClicked;
            // 
            // lblPathCourseGradeTemplate
            // 
            lblPathCourseGradeTemplate.AutoSize = true;
            lblPathCourseGradeTemplate.BorderStyle = BorderStyle.FixedSingle;
            lblPathCourseGradeTemplate.Font = new Font("Times New Roman", 9F);
            lblPathCourseGradeTemplate.Location = new Point(322, 286);
            lblPathCourseGradeTemplate.Margin = new Padding(4, 0, 4, 0);
            lblPathCourseGradeTemplate.Name = "lblPathCourseGradeTemplate";
            lblPathCourseGradeTemplate.Size = new Size(228, 22);
            lblPathCourseGradeTemplate.TabIndex = 32;
            lblPathCourseGradeTemplate.Text = "Course Grades Template Path";
            // 
            // lblPathEnglishTranscriptTemplate
            // 
            lblPathEnglishTranscriptTemplate.AutoSize = true;
            lblPathEnglishTranscriptTemplate.BorderStyle = BorderStyle.FixedSingle;
            lblPathEnglishTranscriptTemplate.Font = new Font("Times New Roman", 9F);
            lblPathEnglishTranscriptTemplate.Location = new Point(322, 221);
            lblPathEnglishTranscriptTemplate.Margin = new Padding(4, 0, 4, 0);
            lblPathEnglishTranscriptTemplate.Name = "lblPathEnglishTranscriptTemplate";
            lblPathEnglishTranscriptTemplate.Size = new Size(251, 22);
            lblPathEnglishTranscriptTemplate.TabIndex = 30;
            lblPathEnglishTranscriptTemplate.Text = "English Transcript Template Path";
            // 
            // lblCourseGradesTemplate
            // 
            lblCourseGradesTemplate.Location = new Point(42, 281);
            lblCourseGradesTemplate.Margin = new Padding(4, 0, 4, 0);
            lblCourseGradesTemplate.Name = "lblCourseGradesTemplate";
            lblCourseGradesTemplate.Size = new Size(241, 29);
            lblCourseGradesTemplate.TabIndex = 29;
            lblCourseGradesTemplate.TabStop = true;
            lblCourseGradesTemplate.Text = "Course Grades Template:";
            lblCourseGradesTemplate.LinkClicked += lblCourseGradesTemplate_LinkClicked;
            // 
            // lblEnglishTranscriptTemplate
            // 
            lblEnglishTranscriptTemplate.AutoSize = true;
            lblEnglishTranscriptTemplate.Location = new Point(38, 216);
            lblEnglishTranscriptTemplate.Margin = new Padding(4, 0, 4, 0);
            lblEnglishTranscriptTemplate.Name = "lblEnglishTranscriptTemplate";
            lblEnglishTranscriptTemplate.Size = new Size(233, 25);
            lblEnglishTranscriptTemplate.TabIndex = 28;
            lblEnglishTranscriptTemplate.TabStop = true;
            lblEnglishTranscriptTemplate.Text = "English Transcript Template :";
            lblEnglishTranscriptTemplate.LinkClicked += lblEnglishTranscriptTemplate_LinkClicked;
            // 
            // lblPathDocumentFolder
            // 
            lblPathDocumentFolder.AutoSize = true;
            lblPathDocumentFolder.BorderStyle = BorderStyle.FixedSingle;
            lblPathDocumentFolder.Font = new Font("Times New Roman", 9F);
            lblPathDocumentFolder.Location = new Point(322, 406);
            lblPathDocumentFolder.Margin = new Padding(4, 0, 4, 0);
            lblPathDocumentFolder.Name = "lblPathDocumentFolder";
            lblPathDocumentFolder.Size = new Size(175, 22);
            lblPathDocumentFolder.TabIndex = 27;
            lblPathDocumentFolder.Text = "Document Folder Path";
            // 
            // lblDocumentFolder
            // 
            lblDocumentFolder.AutoSize = true;
            lblDocumentFolder.Location = new Point(38, 404);
            lblDocumentFolder.Margin = new Padding(4, 0, 4, 0);
            lblDocumentFolder.Name = "lblDocumentFolder";
            lblDocumentFolder.Size = new Size(159, 25);
            lblDocumentFolder.TabIndex = 26;
            lblDocumentFolder.TabStop = true;
            lblDocumentFolder.Text = "Document Folder :";
            lblDocumentFolder.LinkClicked += lblDocumentFolder_LinkClicked;
            // 
            // lblRestartMsg
            // 
            lblRestartMsg.AutoSize = true;
            lblRestartMsg.Location = new Point(322, 522);
            lblRestartMsg.Margin = new Padding(4, 0, 4, 0);
            lblRestartMsg.Name = "lblRestartMsg";
            lblRestartMsg.Size = new Size(585, 25);
            lblRestartMsg.TabIndex = 25;
            lblRestartMsg.Text = "After changing interface language, please close and restart this program.";
            // 
            // cmbInterfaceLanguage
            // 
            cmbInterfaceLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInterfaceLanguage.FormattingEnabled = true;
            cmbInterfaceLanguage.Location = new Point(322, 474);
            cmbInterfaceLanguage.Margin = new Padding(4);
            cmbInterfaceLanguage.Name = "cmbInterfaceLanguage";
            cmbInterfaceLanguage.Size = new Size(418, 33);
            cmbInterfaceLanguage.TabIndex = 24;
            cmbInterfaceLanguage.SelectedIndexChanged += cmbInterfaceLanguage_SelectedIndexChanged;
            // 
            // lblLanguage
            // 
            lblLanguage.AutoSize = true;
            lblLanguage.BackColor = Color.Transparent;
            lblLanguage.FlatStyle = FlatStyle.Flat;
            lblLanguage.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            lblLanguage.ImeMode = ImeMode.NoControl;
            lblLanguage.Location = new Point(38, 474);
            lblLanguage.Margin = new Padding(4);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(177, 25);
            lblLanguage.TabIndex = 23;
            lblLanguage.Text = "Interface Language";
            // 
            // lblPathCourseRoleTemplate
            // 
            lblPathCourseRoleTemplate.AutoSize = true;
            lblPathCourseRoleTemplate.BorderStyle = BorderStyle.FixedSingle;
            lblPathCourseRoleTemplate.Font = new Font("Times New Roman", 9F);
            lblPathCourseRoleTemplate.Location = new Point(322, 166);
            lblPathCourseRoleTemplate.Margin = new Padding(4, 0, 4, 0);
            lblPathCourseRoleTemplate.Name = "lblPathCourseRoleTemplate";
            lblPathCourseRoleTemplate.Size = new Size(210, 22);
            lblPathCourseRoleTemplate.TabIndex = 22;
            lblPathCourseRoleTemplate.Text = "Course Role Template Path";
            // 
            // lblPathTemplateFolder
            // 
            lblPathTemplateFolder.AutoSize = true;
            lblPathTemplateFolder.BorderStyle = BorderStyle.FixedSingle;
            lblPathTemplateFolder.Font = new Font("Times New Roman", 9F);
            lblPathTemplateFolder.Location = new Point(322, 344);
            lblPathTemplateFolder.Margin = new Padding(4, 0, 4, 0);
            lblPathTemplateFolder.Name = "lblPathTemplateFolder";
            lblPathTemplateFolder.Size = new Size(167, 22);
            lblPathTemplateFolder.TabIndex = 21;
            lblPathTemplateFolder.Text = "Template Folder Path";
            // 
            // lblPathTransTemplate
            // 
            lblPathTransTemplate.AutoSize = true;
            lblPathTransTemplate.BorderStyle = BorderStyle.FixedSingle;
            lblPathTransTemplate.Font = new Font("Times New Roman", 9F);
            lblPathTransTemplate.Location = new Point(322, 104);
            lblPathTransTemplate.Margin = new Padding(4, 0, 4, 0);
            lblPathTransTemplate.Name = "lblPathTransTemplate";
            lblPathTransTemplate.Size = new Size(194, 22);
            lblPathTransTemplate.TabIndex = 20;
            lblPathTransTemplate.Text = "Transcript Template Path";
            // 
            // lblTemplateFolder
            // 
            lblTemplateFolder.AutoSize = true;
            lblTemplateFolder.Location = new Point(38, 340);
            lblTemplateFolder.Margin = new Padding(4, 0, 4, 0);
            lblTemplateFolder.Name = "lblTemplateFolder";
            lblTemplateFolder.Size = new Size(147, 25);
            lblTemplateFolder.TabIndex = 19;
            lblTemplateFolder.TabStop = true;
            lblTemplateFolder.Text = "Template Folder :";
            lblTemplateFolder.LinkClicked += lblTemplateFolder_LinkClicked;
            // 
            // lblCourseRoleTemplate
            // 
            lblCourseRoleTemplate.AutoSize = true;
            lblCourseRoleTemplate.Location = new Point(38, 164);
            lblCourseRoleTemplate.Margin = new Padding(4, 0, 4, 0);
            lblCourseRoleTemplate.Name = "lblCourseRoleTemplate";
            lblCourseRoleTemplate.Size = new Size(191, 25);
            lblCourseRoleTemplate.TabIndex = 16;
            lblCourseRoleTemplate.TabStop = true;
            lblCourseRoleTemplate.Text = "Course Role Template :";
            lblCourseRoleTemplate.LinkClicked += lblCourseRoleTemplate_LinkClicked;
            // 
            // lblTranscriptTemplate
            // 
            lblTranscriptTemplate.AutoSize = true;
            lblTranscriptTemplate.Location = new Point(38, 100);
            lblTranscriptTemplate.Margin = new Padding(4, 0, 4, 0);
            lblTranscriptTemplate.Name = "lblTranscriptTemplate";
            lblTranscriptTemplate.Size = new Size(172, 25);
            lblTranscriptTemplate.TabIndex = 15;
            lblTranscriptTemplate.TabStop = true;
            lblTranscriptTemplate.Text = "Transcript Template :";
            lblTranscriptTemplate.LinkClicked += lblTranscriptTemplate_LinkClicked;
            // 
            // lblOptions
            // 
            lblOptions.AutoSize = true;
            lblOptions.BackColor = Color.Transparent;
            lblOptions.FlatStyle = FlatStyle.Flat;
            lblOptions.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            lblOptions.Location = new Point(38, 50);
            lblOptions.Margin = new Padding(4);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new Size(201, 25);
            lblOptions.TabIndex = 6;
            lblOptions.Text = "Folders and Templates";
            // 
            // tabExit
            // 
            tabExit.Location = new Point(4, 34);
            tabExit.Margin = new Padding(4);
            tabExit.Name = "tabExit";
            tabExit.Size = new Size(1916, 903);
            tabExit.TabIndex = 5;
            tabExit.Text = "Exit";
            tabExit.UseVisualStyleBackColor = true;
            // 
            // toolStripBottom
            // 
            toolStripBottom.Dock = DockStyle.Bottom;
            toolStripBottom.ImageScalingSize = new Size(20, 20);
            toolStripBottom.Items.AddRange(new ToolStripItem[] { toolStripBtnNarrow });
            toolStripBottom.Location = new Point(1924, 907);
            toolStripBottom.Name = "toolStripBottom";
            toolStripBottom.Size = new Size(0, 34);
            toolStripBottom.TabIndex = 15;
            toolStripBottom.Text = "toolStrip1";
            // 
            // toolStripBtnNarrow
            // 
            toolStripBtnNarrow.Alignment = ToolStripItemAlignment.Right;
            toolStripBtnNarrow.AutoToolTip = false;
            toolStripBtnNarrow.BackColor = Color.RoyalBlue;
            toolStripBtnNarrow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripBtnNarrow.ForeColor = Color.GhostWhite;
            toolStripBtnNarrow.ImageAlign = ContentAlignment.MiddleRight;
            toolStripBtnNarrow.ImageTransparentColor = Color.Magenta;
            toolStripBtnNarrow.Name = "toolStripBtnNarrow";
            toolStripBtnNarrow.Size = new Size(74, 29);
            toolStripBtnNarrow.Tag = "narrow";
            toolStripBtnNarrow.Text = "Narrow";
            toolStripBtnNarrow.Click += toolStripBtnNarrow_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // frmTranscriptOptions
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 941);
            Controls.Add(toolStripBottom);
            Controls.Add(tabControl1);
            Margin = new Padding(4);
            Name = "frmTranscriptOptions";
            WindowState = FormWindowState.Maximized;
            Load += frmTranscriptOptions_Load;
            Resize += frmTranscriptOptions_Resize;
            tabControl1.ResumeLayout(false);
            tabActions.ResumeLayout(false);
            tabActions.PerformLayout();
            tabStudent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStudent).EndInit();
            tabTranscript.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTranscript).EndInit();
            tabRequirements.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRequirements).EndInit();
            tabOptions.ResumeLayout(false);
            tabOptions.PerformLayout();
            toolStripBottom.ResumeLayout(false);
            toolStripBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public TabControl tabControl1;
        private Label lblOptions;
        public TabPage tabOptions;
        public TabPage tabRequirements;
        public DataGridView dgvRequirements;
        private TabPage tabTranscript;
        private DataGridView dgvTranscript;
        private TabPage tabStudent;
        private DataGridView dgvStudent;
        private ToolStrip toolStripBottom;
        private ToolStripButton toolStripBtnNarrow;
        private FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private Label lblSaveDocuments;
        private LinkLabel lblTranscriptTemplate;
        private TabPage tabActions;
        private Button btnPrintTranscript;
        private LinkLabel lblT;
        private LinkLabel lblCourseRoleTemplate;
        private Label lblPathTransTemplate;
        private LinkLabel lblTemplateFolder;
        private Label lblPathTemplateFolder;
        private Button btnPrintCourseRole;
        private Label lblPathCourseRoleTemplate;
        private Label lblLanguage;
        private Label lblRestartMsg;
        private ComboBox cmbInterfaceLanguage;
        private TabPage tabExit;
        private Label lblPathDocumentFolder;
        private LinkLabel lblDocumentFolder;
        private Label lblPathEnglishTranscriptTemplate;
        private LinkLabel lblCourseGradesTemplate;
        private LinkLabel lblEnglishTranscriptTemplate;
        private Button btnPrintCourseGrades;
        private Button btnPrintEnglishTranscript;
        private Label lblPathCourseGradeTemplate;
        private Label lblPathDatabaseFolder;
        private LinkLabel lblDatabaseFolder;
        private CheckBox chkIncludeAudits;
    }
}