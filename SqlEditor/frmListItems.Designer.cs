
namespace SqlEditor
{
    partial class frmListItems
    {

        #region "Windows Form Designer generated code "
        //public static frmDeleteDatabase CreateInstance()
        //{
        //	frmDeleteDatabase theInstance = new frmDeleteDatabase();
        //	theInstance.Form_Load();
        //	return theInstance;
        //}
        private string[] visualControls = new string[] { "components", "ToolTipMain", "cmdDelete", "cmdExit", "lstDatabaseList", "listBoxHelper1", "commandButtonHelper1" };
        //Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;
        public ToolTip ToolTipMain;
        public Button cmdOK;
        public ListBox listBox1;
        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ToolTipMain = new ToolTip(components);
            cmdOK = new Button();
            listBox1 = new ListBox();
            lblText = new Label();
            SuspendLayout();
            // 
            // cmdOK
            // 
            cmdOK.AllowDrop = true;
            cmdOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdOK.BackColor = SystemColors.Control;
            cmdOK.Font = new Font("Microsoft Sans Serif", 8.25F);
            cmdOK.ForeColor = SystemColors.ControlText;
            cmdOK.Location = new Point(280, 299);
            cmdOK.Name = "cmdOK";
            cmdOK.RightToLeft = RightToLeft.No;
            cmdOK.Size = new Size(89, 40);
            cmdOK.TabIndex = 1;
            cmdOK.Text = "resOK";
            cmdOK.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdOK.UseVisualStyleBackColor = false;
            cmdOK.Click += cmdOK_Click;
            // 
            // listBox1
            // 
            listBox1.AllowDrop = true;
            listBox1.BackColor = SystemColors.Window;
            listBox1.Font = new Font("Microsoft Sans Serif", 8.25F);
            listBox1.ForeColor = SystemColors.WindowText;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(24, 43);
            listBox1.Name = "listBox1";
            listBox1.RightToLeft = RightToLeft.No;
            listBox1.Size = new Size(345, 140);
            listBox1.TabIndex = 0;
            // 
            // lblText
            // 
            lblText.AutoSize = true;
            lblText.Location = new Point(21, 13);
            lblText.Name = "lblText";
            lblText.Size = new Size(161, 17);
            lblText.TabIndex = 2;
            lblText.Text = "Replaced by the caption";
            // 
            // frmListItems
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(397, 351);
            Controls.Add(lblText);
            Controls.Add(cmdOK);
            Controls.Add(listBox1);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            Location = new Point(3, 22);
            Name = "frmListItems";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Caption";
            Load += frmListItems_Load;
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private Label lblText;
    }
}