//using System.Data;
// using System.Data.SqlClient;

namespace SqlEditor
{
    internal partial class frmListItems
        : System.Windows.Forms.Form
    {
        public job myJob { get; set; }

        public frmListItems() : base()
        {
            //This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        // For deleting connection, we will build a list of connection strings - see below
        List<connectionString> csList = new List<connectionString>();
        // For a string list, we will feed in the list into myList
        public List<string> myList = new List<string>();
        // For display keys we will ALSO feed in list of initially selected items
        // We will also return this list if user says "o.k."
        // For
        public List<string> mySelectedValues = new List<string>();
        public List<int> mySelectedIndexes = new List<int>();
        // Indicates user has selected OK.
        public bool OK = false;
        public bool AllowNewValue = false;
        private void frmListItems_Load(object sender, EventArgs e)
        {
            if (AllowNewValue) 
            { 
                cmdNewValue.Enabled = true;
                cmdNewValue.Visible = true;
            }
            // this.Text = formCaption; //"List of Databases on the Server";
            listBox1.Items.AddRange(myList.ToArray());
            //Set width
            setListBoxWidth();
            if (listBox1.Items.Count == 0)
            {
                this.Height = this.Height - listBox1.Height;
                listBox1.Visible = false;
                cmdNewValue.Visible = false;
                txtNewInput.Visible = true;
            }
            //Set OK button text and preselect mySelectedValues items (if any)
            if (myJob == job.SelectString)
            {
                this.cmdOK.Text = "OK";
            }
            else if (myJob == job.SelectMultipleStrings)
            {
                this.cmdOK.Text = "OK";
                listBox1.SelectionMode = SelectionMode.MultiSimple;
                // Select items in mySelectedValues
                foreach (string dk in mySelectedValues)
                {
                    int i = listBox1.Items.IndexOf(dk);
                    if (i > -1)
                    {
                        listBox1.SetSelected(i, true);
                    }
                }
            }
            else if (myJob == job.DeleteConnections)   // deleting databases fromlist
            {
                this.cmdOK.Text = "Delete";
            }
        }

        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            OK = true;
            // Return selected indices and values 
            if (myJob == job.SelectString || myJob == job.DeleteConnections)
            {
                if (txtNewInput.Text != String.Empty)
                {
                    mySelectedValues.Add(txtNewInput.Text);
                }
                else if (listBox1.SelectedIndex > -1)
                {
                    mySelectedIndexes.Add(listBox1.SelectedIndex);
                    mySelectedValues.Add(listBox1.GetItemText(listBox1.SelectedItem));
                }
            }
            else if (myJob == job.SelectMultipleStrings)
            {
                mySelectedIndexes =
                    listBox1.SelectedIndices.Cast<int>().ToList();
                // The following cast should clear the preselected values, but just in case . . .
                mySelectedValues = new List<string>();
                mySelectedValues =
                    listBox1.SelectedItems.Cast<string>().ToList();
            }
            this.Close();
        }

        private void setListBoxWidth()
        {
            // Set width of form
            int width = listBox1.Width;
            int initialWidth = width; // Used below to center the form
            using (Graphics g = listBox1.CreateGraphics())
            {
                System.Drawing.Font font = listBox1.Font;
                int vertScrollBarWidth = 15;
                List<string> itemsList = new List<string>(); // Only used to find widest text
                foreach (string str in listBox1.Items) { itemsList.Add(str); }
                ;
                itemsList.Add(this.Text); // Makes box as wide as the text, but doesn't include close button
                lblText.Text = this.Text; // Use because the caption may be too small
                foreach (string s in itemsList)
                {
                    int newWidth = (int)g.MeasureString(s, font).Width + vertScrollBarWidth;
                    if (width < newWidth)
                    {
                        width = newWidth;
                    }
                }
            }
            listBox1.Width = width;
            this.Width = width + listBox1.Left + listBox1.Left + 15;
            // listing strings to select - maybe databases or tables or anything
            this.StartPosition = FormStartPosition.Manual;
            //Method 2. The manual way
            this.Top = (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2;
            this.Left = (Screen.PrimaryScreen.Bounds.Width - this.Width) / 2;
        }

        private void txtNewInput_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmdNewValue_Click(object sender, EventArgs e)
        {
            txtNewInput.Visible = true;
        }

        //This form used for (1) deleting connections AND listing / selecting a from a list
        public enum job
        {
            DeleteConnections,
            SelectString,
            SelectMultipleStrings
        }


    }
}

