namespace SqlEditor
{
    partial class frmLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            cmdOK = new Button();
            cmdCancel = new Button();
            txtPassword = new TextBox();
            lblPassword = new Label();
            cmbStoredPasswords = new ComboBox();
            btnAdd = new Button();
            btnDelete = new Button();
            lblStored = new Label();
            SuspendLayout();
            // 
            // cmdOK
            // 
            resources.ApplyResources(cmdOK, "cmdOK");
            cmdOK.Name = "cmdOK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            resources.ApplyResources(cmdCancel, "cmdCancel");
            cmdCancel.Name = "cmdCancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click_1;
            // 
            // txtPassword
            // 
            resources.ApplyResources(txtPassword, "txtPassword");
            txtPassword.Name = "txtPassword";
            txtPassword.KeyDown += txtPassword_KeyDown;
            // 
            // lblPassword
            // 
            resources.ApplyResources(lblPassword, "lblPassword");
            lblPassword.Name = "lblPassword";
            // 
            // cmbStoredPasswords
            // 
            cmbStoredPasswords.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStoredPasswords.FormattingEnabled = true;
            resources.ApplyResources(cmbStoredPasswords, "cmbStoredPasswords");
            cmbStoredPasswords.Name = "cmbStoredPasswords";
            cmbStoredPasswords.SelectedIndexChanged += cmbStoredPasswords_SelectedIndexChanged;
            // 
            // btnAdd
            // 
            resources.ApplyResources(btnAdd, "btnAdd");
            btnAdd.Name = "btnAdd";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            resources.ApplyResources(btnDelete, "btnDelete");
            btnDelete.Name = "btnDelete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // lblStored
            // 
            resources.ApplyResources(lblStored, "lblStored");
            lblStored.Name = "lblStored";
            lblStored.Click += lblStored_Click;
            // 
            // frmLogin
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblStored);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(cmbStoredPasswords);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Name = "frmLogin";
            Load += frmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button cmdOK;
        private Button cmdCancel;
        private TextBox txtPassword;
        private Label lblPassword;
        private ComboBox cmbStoredPasswords;
        private Button btnAdd;
        private Button btnDelete;
        private Label lblStored;
    }
}