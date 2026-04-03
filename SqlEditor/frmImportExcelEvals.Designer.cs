namespace SqlEditor
{
    partial class frmImportExcelEvals
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
            lblSelectFolder = new Label();
            btnSelectFolder = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            lblTerm = new Label();
            cmbBoxTerm = new ComboBox();
            connectionStringBindingSource = new BindingSource(components);
            dataGridViewExcelFiles = new DataGridView();
            btnRun = new Button();
            ckbUpdateCourses = new CheckBox();
            lblYearString = new Label();
            cmbEvalFormName = new ComboBox();
            progressBar1 = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)connectionStringBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewExcelFiles).BeginInit();
            SuspendLayout();
            // 
            // lblSelectFolder
            // 
            lblSelectFolder.AutoSize = true;
            lblSelectFolder.Location = new Point(361, 12);
            lblSelectFolder.Margin = new Padding(4, 0, 4, 0);
            lblSelectFolder.Name = "lblSelectFolder";
            lblSelectFolder.Size = new Size(137, 25);
            lblSelectFolder.TabIndex = 0;
            lblSelectFolder.Text = "Selected Folder:";
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(506, 12);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(136, 33);
            btnSelectFolder.TabIndex = 1;
            btnSelectFolder.Text = "Select folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // lblTerm
            // 
            lblTerm.AutoSize = true;
            lblTerm.Location = new Point(54, 12);
            lblTerm.Name = "lblTerm";
            lblTerm.Size = new Size(54, 25);
            lblTerm.TabIndex = 2;
            lblTerm.Text = "Term:";
            // 
            // cmbBoxTerm
            // 
            cmbBoxTerm.FormattingEnabled = true;
            cmbBoxTerm.Location = new Point(114, 12);
            cmbBoxTerm.Name = "cmbBoxTerm";
            cmbBoxTerm.Size = new Size(240, 33);
            cmbBoxTerm.TabIndex = 3;
            cmbBoxTerm.SelectedIndexChanged += cmbBoxTerm_SelectedIndexChanged;
            // 
            // connectionStringBindingSource
            // 
            connectionStringBindingSource.DataSource = typeof(connectionString);
            // 
            // dataGridViewExcelFiles
            // 
            dataGridViewExcelFiles.AllowUserToAddRows = false;
            dataGridViewExcelFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewExcelFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewExcelFiles.Location = new Point(89, 112);
            dataGridViewExcelFiles.Name = "dataGridViewExcelFiles";
            dataGridViewExcelFiles.RowHeadersWidth = 62;
            dataGridViewExcelFiles.Size = new Size(1494, 742);
            dataGridViewExcelFiles.TabIndex = 4;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(1393, 12);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(149, 33);
            btnRun.TabIndex = 5;
            btnRun.Text = "Process Files";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // ckbUpdateCourses
            // 
            ckbUpdateCourses.AutoSize = true;
            ckbUpdateCourses.Location = new Point(1393, 55);
            ckbUpdateCourses.Name = "ckbUpdateCourses";
            ckbUpdateCourses.Size = new Size(164, 29);
            ckbUpdateCourses.TabIndex = 6;
            ckbUpdateCourses.Text = "Update Courses";
            ckbUpdateCourses.UseVisualStyleBackColor = true;
            // 
            // lblYearString
            // 
            lblYearString.AutoSize = true;
            lblYearString.Location = new Point(54, 67);
            lblYearString.Margin = new Padding(4, 0, 4, 0);
            lblYearString.Name = "lblYearString";
            lblYearString.Size = new Size(115, 25);
            lblYearString.TabIndex = 8;
            lblYearString.Text = "Form Name: ";
            // 
            // cmbEvalFormName
            // 
            cmbEvalFormName.FormattingEnabled = true;
            cmbEvalFormName.Location = new Point(183, 58);
            cmbEvalFormName.Margin = new Padding(4, 5, 4, 5);
            cmbEvalFormName.Name = "cmbEvalFormName";
            cmbEvalFormName.Size = new Size(171, 33);
            cmbEvalFormName.TabIndex = 9;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(506, 55);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(611, 34);
            progressBar1.TabIndex = 10;
            // 
            // frmImportExcelEvals
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1666, 924);
            Controls.Add(progressBar1);
            Controls.Add(cmbEvalFormName);
            Controls.Add(lblYearString);
            Controls.Add(ckbUpdateCourses);
            Controls.Add(btnRun);
            Controls.Add(dataGridViewExcelFiles);
            Controls.Add(cmbBoxTerm);
            Controls.Add(lblTerm);
            Controls.Add(btnSelectFolder);
            Controls.Add(lblSelectFolder);
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmImportExcelEvals";
            Text = "Import Student Evaluation Excel files";
            Load += frmImportExcelEvals_Load;
            ((System.ComponentModel.ISupportInitialize)connectionStringBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewExcelFiles).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblSelectFolder;
        private Button btnSelectFolder;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label lblTerm;
        private ComboBox cmbBoxTerm;
        private BindingSource connectionStringBindingSource;
        private DataGridView dataGridViewExcelFiles;
        private Button btnRun;
        private CheckBox ckbUpdateCourses;
        private Label lblYearString;
        private ComboBox cmbEvalFormName;
        private ProgressBar progressBar1;
    }
}