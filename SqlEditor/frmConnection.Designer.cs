
namespace SqlEditor
{
    partial class frmConnection
    {
        #region "Windows Form Designer generated code "

        private string[] visualControls = new string[] { "components", "ToolTipMain", "cmdDatabaseList", "optAccess", "optMsSql", "Frame1", "_optSpecialDatabase_2", "_optSpecialDatabase_1", "_optSpecialDatabase_0", "txtUserId", "CommonDialog", "chkReadOnly", "txtServer", "txtShortName", "txtPath", "cmdFile", "txtHelp", "cmbStrings", "cmdTest", "cmdCancel", "cmdOK", "Label4", "Label7", "Label6", "Label5", "Label3", "Label2", "Label1", "lblConnection", "optSpecialDatabase", "commandButtonHelper1" };

        //Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;
        public ToolTip ToolTipMain;
        public Button cmdDatabaseList;
        public TextBox txtUserId;
        public OpenFileDialog CommonDialogOpen;
        public SaveFileDialog CommonDialogSave;
        public FontDialog CommonDialogFont;
        public ColorDialog CommonDialogColor;
        public PrintDialog CommonDialogPrint;
        public TextBox txtServer;
        public TextBox txtDatabase;
        public TextBox txtHelp;
        public ComboBox cmbStrings;
        public Button cmdTest;
        public Button cmdCancel;
        public Button cmdOK;
        public Label lblUserID;
        public Label lblServer;
        public Label lblDatabase;
        public Label lblConnection;
        public RadioButton[] optSpecialDatabase = new RadioButton[3];
        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ToolTipMain = new ToolTip(components);
            cmdDatabaseList = new Button();
            txtUserId = new TextBox();
            CommonDialogOpen = new OpenFileDialog();
            CommonDialogSave = new SaveFileDialog();
            CommonDialogFont = new FontDialog();
            CommonDialogColor = new ColorDialog();
            CommonDialogPrint = new PrintDialog();
            txtServer = new TextBox();
            txtDatabase = new TextBox();
            txtHelp = new TextBox();
            cmbStrings = new ComboBox();
            cmdTest = new Button();
            cmdCancel = new Button();
            cmdOK = new Button();
            lblUserID = new Label();
            lblServer = new Label();
            lblDatabase = new Label();
            lblConnection = new Label();
            SuspendLayout();
            // 
            // cmdDatabaseList
            // 
            cmdDatabaseList.AllowDrop = true;
            cmdDatabaseList.BackColor = SystemColors.Control;
            cmdDatabaseList.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmdDatabaseList.ForeColor = SystemColors.ControlText;
            cmdDatabaseList.Location = new Point(664, 35);
            cmdDatabaseList.Name = "cmdDatabaseList";
            cmdDatabaseList.RightToLeft = RightToLeft.No;
            cmdDatabaseList.Size = new Size(161, 26);
            cmdDatabaseList.TabIndex = 25;
            cmdDatabaseList.Text = "Sql database list";
            cmdDatabaseList.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdDatabaseList.UseVisualStyleBackColor = false;
            cmdDatabaseList.Click += cmdDatabaseList_Click;
            // 
            // txtUserId
            // 
            txtUserId.AcceptsReturn = true;
            txtUserId.AllowDrop = true;
            txtUserId.BackColor = SystemColors.Window;
            txtUserId.Cursor = Cursors.IBeam;
            txtUserId.Font = new Font("Microsoft Sans Serif", 8.25F);
            txtUserId.ForeColor = SystemColors.WindowText;
            txtUserId.Location = new Point(488, 35);
            txtUserId.MaxLength = 0;
            txtUserId.Name = "txtUserId";
            txtUserId.RightToLeft = RightToLeft.No;
            txtUserId.Size = new Size(161, 23);
            txtUserId.TabIndex = 15;
            // 
            // txtServer
            // 
            txtServer.AcceptsReturn = true;
            txtServer.AllowDrop = true;
            txtServer.BackColor = SystemColors.Window;
            txtServer.Cursor = Cursors.IBeam;
            txtServer.Font = new Font("Microsoft Sans Serif", 8.25F);
            txtServer.ForeColor = SystemColors.WindowText;
            txtServer.Location = new Point(8, 35);
            txtServer.MaxLength = 0;
            txtServer.Name = "txtServer";
            txtServer.RightToLeft = RightToLeft.No;
            txtServer.Size = new Size(209, 23);
            txtServer.TabIndex = 12;
            // 
            // txtDatabase
            // 
            txtDatabase.AcceptsReturn = true;
            txtDatabase.AllowDrop = true;
            txtDatabase.BackColor = SystemColors.Window;
            txtDatabase.Cursor = Cursors.IBeam;
            txtDatabase.Font = new Font("Microsoft Sans Serif", 8.25F);
            txtDatabase.ForeColor = SystemColors.WindowText;
            txtDatabase.Location = new Point(264, 35);
            txtDatabase.MaxLength = 0;
            txtDatabase.Name = "txtDatabase";
            txtDatabase.RightToLeft = RightToLeft.No;
            txtDatabase.Size = new Size(169, 23);
            txtDatabase.TabIndex = 8;
            // 
            // txtHelp
            // 
            txtHelp.AcceptsReturn = true;
            txtHelp.AllowDrop = true;
            txtHelp.Anchor = AnchorStyles.None;
            txtHelp.BackColor = SystemColors.Control;
            txtHelp.BorderStyle = BorderStyle.None;
            txtHelp.Cursor = Cursors.IBeam;
            txtHelp.Font = new Font("Microsoft Sans Serif", 8.25F);
            txtHelp.ForeColor = SystemColors.WindowText;
            txtHelp.Location = new Point(0, 204);
            txtHelp.MaxLength = 0;
            txtHelp.Multiline = true;
            txtHelp.Name = "txtHelp";
            txtHelp.RightToLeft = RightToLeft.No;
            txtHelp.Size = new Size(829, 46);
            txtHelp.TabIndex = 5;
            txtHelp.Text = "Text1";
            // 
            // cmbStrings
            // 
            cmbStrings.AllowDrop = true;
            cmbStrings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbStrings.BackColor = SystemColors.Window;
            cmbStrings.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmbStrings.ForeColor = SystemColors.WindowText;
            cmbStrings.Location = new Point(8, 96);
            cmbStrings.Name = "cmbStrings";
            cmbStrings.RightToLeft = RightToLeft.No;
            cmbStrings.Size = new Size(809, 25);
            cmbStrings.TabIndex = 3;
            cmbStrings.SelectedIndexChanged += cmbStrings_SelectedIndexChanged;
            // 
            // cmdTest
            // 
            cmdTest.AllowDrop = true;
            cmdTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdTest.BackColor = SystemColors.Control;
            cmdTest.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmdTest.ForeColor = SystemColors.ControlText;
            cmdTest.Location = new Point(560, 168);
            cmdTest.Name = "cmdTest";
            cmdTest.RightToLeft = RightToLeft.No;
            cmdTest.Size = new Size(81, 31);
            cmdTest.TabIndex = 2;
            cmdTest.Text = "Test";
            cmdTest.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdTest.UseVisualStyleBackColor = false;
            cmdTest.Click += cmdTest_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.AllowDrop = true;
            cmdCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdCancel.BackColor = SystemColors.Control;
            cmdCancel.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmdCancel.ForeColor = SystemColors.ControlText;
            cmdCancel.Location = new Point(748, 167);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.RightToLeft = RightToLeft.No;
            cmdCancel.Size = new Size(73, 32);
            cmdCancel.TabIndex = 1;
            cmdCancel.Text = "Cancel";
            cmdCancel.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdCancel.UseVisualStyleBackColor = false;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // cmdOK
            // 
            cmdOK.AllowDrop = true;
            cmdOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdOK.BackColor = SystemColors.Control;
            cmdOK.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmdOK.ForeColor = SystemColors.ControlText;
            cmdOK.Location = new Point(647, 168);
            cmdOK.Name = "cmdOK";
            cmdOK.RightToLeft = RightToLeft.No;
            cmdOK.Size = new Size(81, 31);
            cmdOK.TabIndex = 0;
            cmdOK.Text = "O.K.";
            cmdOK.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdOK.UseVisualStyleBackColor = false;
            cmdOK.Click += cmdOK_Click;
            // 
            // lblUserID
            // 
            lblUserID.AllowDrop = true;
            lblUserID.BackColor = SystemColors.Control;
            lblUserID.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblUserID.ForeColor = SystemColors.ControlText;
            lblUserID.Location = new Point(488, 15);
            lblUserID.Name = "lblUserID";
            lblUserID.RightToLeft = RightToLeft.No;
            lblUserID.Size = new Size(153, 17);
            lblUserID.TabIndex = 14;
            lblUserID.Text = "{2} User ID";
            // 
            // lblServer
            // 
            lblServer.AllowDrop = true;
            lblServer.BackColor = SystemColors.Control;
            lblServer.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblServer.ForeColor = SystemColors.ControlText;
            lblServer.Location = new Point(8, 15);
            lblServer.Name = "lblServer";
            lblServer.RightToLeft = RightToLeft.No;
            lblServer.Size = new Size(209, 17);
            lblServer.TabIndex = 11;
            lblServer.Text = "{0} Server: ";
            // 
            // lblDatabase
            // 
            lblDatabase.AllowDrop = true;
            lblDatabase.BackColor = SystemColors.Control;
            lblDatabase.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblDatabase.ForeColor = SystemColors.ControlText;
            lblDatabase.Location = new Point(264, 15);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.RightToLeft = RightToLeft.No;
            lblDatabase.Size = new Size(169, 17);
            lblDatabase.TabIndex = 9;
            lblDatabase.Text = "{1} Database:";
            // 
            // lblConnection
            // 
            lblConnection.AllowDrop = true;
            lblConnection.BackColor = SystemColors.Control;
            lblConnection.Font = new Font("Microsoft Sans Serif", 8.25F);
            lblConnection.ForeColor = SystemColors.ControlText;
            lblConnection.Location = new Point(8, 67);
            lblConnection.Name = "lblConnection";
            lblConnection.RightToLeft = RightToLeft.No;
            lblConnection.Size = new Size(809, 17);
            lblConnection.TabIndex = 4;
            // 
            // frmConnection
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(833, 266);
            Controls.Add(cmdDatabaseList);
            Controls.Add(txtUserId);
            Controls.Add(txtServer);
            Controls.Add(txtDatabase);
            Controls.Add(txtHelp);
            Controls.Add(cmbStrings);
            Controls.Add(cmdTest);
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Controls.Add(lblUserID);
            Controls.Add(lblServer);
            Controls.Add(lblDatabase);
            Controls.Add(lblConnection);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            Location = new Point(4, 30);
            Name = "frmConnection";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Connection String";
            Load += frmConnection_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}