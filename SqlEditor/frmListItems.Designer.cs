
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
            SuspendLayout();
            // 
            // cmdOK
            // 
            cmdOK.AllowDrop = true;
            cmdOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdOK.BackColor = SystemColors.Control;
            cmdOK.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            cmdOK.ForeColor = SystemColors.ControlText;
            cmdOK.Location = new Point(224, 217);
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
            listBox1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            listBox1.ForeColor = SystemColors.WindowText;
            listBox1.ItemHeight = 20;
            listBox1.Location = new Point(24, 24);
            listBox1.Name = "listBox1";
            listBox1.RightToLeft = RightToLeft.No;
            listBox1.Size = new Size(345, 144);
            listBox1.TabIndex = 0;
            // 
            // frmListItems
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(10F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(397, 268);
            Controls.Add(cmdOK);
            Controls.Add(listBox1);
            Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            Location = new Point(3, 22);
            Name = "frmListItems";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Caption";
            Load += frmListItems_Load;
            ResumeLayout(false);
        }
        #endregion
    }
}