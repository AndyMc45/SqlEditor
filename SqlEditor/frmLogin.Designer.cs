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
            // frmLogin
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
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
    }
}