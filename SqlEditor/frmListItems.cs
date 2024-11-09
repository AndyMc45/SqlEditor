//using System.Data;
// using System.Data.SqlClient;

using SqlEditor.Properties;

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

        // For a list, we will feed in the list into myList
        public List<string> myList = new List<string>();
        // For display keys we will also feed in list of initially selected items
        // We will also return this list if user says "o.k."
        public List<string> mySelectedList = new List<string>();

        public string returnString = string.Empty;
        public int returnIndex = -1;

        private void frmListItems_Load(object sender, EventArgs e)
        {
            // this.Text = formCaption; //"List of Databases on the Server";
            listBox1.Items.AddRange(myList.ToArray());
            // Set width of form
            int width = listBox1.Width;
            int initialWidth = width; // Used below to center the form
            using (Graphics g = listBox1.CreateGraphics())
            {
                System.Drawing.Font font = listBox1.Font;
                int vertScrollBarWidth = 15;
                List<string> itemsList = new List<string>(); // Only used to find widest text
                foreach (string str in listBox1.Items) { itemsList.Add(str); };
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

            if (myJob == job.SelectString)
            {
                this.cmdOK.Text = "OK";
            }
            else if (myJob == job.SelectMultipleStrings)
            {
                listBox1.SelectionMode = SelectionMode.MultiSimple;
                // Select items in mySelectedList
                foreach (string dk in mySelectedList)
                {
                    int i = listBox1.Items.IndexOf(dk);
                    if (i > -1)
                    {
                        listBox1.SetSelected(i, true);
                    }
                }
                this.cmdOK.Text = "OK";
            }
            else if (myJob == job.DeleteConnections)   // deleting databases fromlist
            {
                this.cmdOK.Text = "Delete"; // MyResources.delete;
            }
        }

        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {   // Return selected item
            if (myJob == job.SelectString || myJob == job.DeleteConnections)
            {
                if (listBox1.SelectedIndex > -1)
                {
                    returnIndex = listBox1.SelectedIndex;
                    returnString = listBox1.GetItemText(listBox1.SelectedItem);
                }
            }
            else if (myJob == job.SelectMultipleStrings)
            {
                mySelectedList = new List<string>();
                foreach (string str in listBox1.SelectedItems)
                {
                    mySelectedList.Add(str);
                }
                // returnIndex > -1 indicates O.K. selected as well as Count.
                returnIndex = mySelectedList.Count;
            }
            this.Close();
        }

        private void frmListItems_Resize(object sender, EventArgs e)
        {
            
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

