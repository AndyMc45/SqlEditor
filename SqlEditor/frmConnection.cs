// using System.Data.SqlClient;
using SqlEditor.Properties;
using System.Data;
using System.Text;

namespace SqlEditor
{
    internal partial class frmConnection
        : System.Windows.Forms.Form
    {

        public frmConnection() : base()
        {
            InitializeComponent();
            csList = new List<connectionString>();
        }

        public bool success = false;
        List<connectionString> csList { get; set; }
        private string password = string.Empty;

        private void frmConnection_Load(object sender, EventArgs e)
        {
            lblConnection.Text = MyResources.EnterConnectionStringDirections;
            //Load txtHelp
            txtHelp.Text = MyResources.MustPassTestBeforeOKButton;

            //Disable OK
            cmdOK.Enabled = false;

            // Load the dropdown combo list
            // Start with defaults - assume these are formatted correctly - no unnecessary spaces
            List<string> defaultComboItems = MsSql.defaultConnectionStrings();
            // Add past successful connections
            csList = AppData.GetConnectionStringList();
            foreach (connectionString conString in csList)
            {
                string strCb = conString.comboString;
                strCb = strCb.Replace(" ;", ";").Replace("; ", ";").Replace(" =", "=").Replace("= ", "=");
                if (!defaultComboItems.Contains(strCb))
                {
                    defaultComboItems.Add(strCb);
                }
            }
            cmbStrings.Items.Clear();
            cmbStrings.Items.AddRange(defaultComboItems.ToArray());

            // Select the first - this will set the 3 text boxes
            cmbStrings.SelectedIndex = 0;
        }

        private void cmdDatabaseList_Click(Object eventSender, EventArgs eventArgs)
        {
            string conStr = cmbStrings.Text;

            // Remove Database = {1} from conStr
            int one = conStr.IndexOf("{1}");
            if (one > -1)
            {
                int nextSemi = conStr.IndexOf(";", one);
                int lastSemi = conStr.Substring(0, one).LastIndexOf(";");
                conStr = conStr.Substring(0, lastSemi + 1) + conStr.Substring(nextSemi + 1);
            }

            if (CheckForMissingVariables(conStr))
            {
                // Replace {0} and {2} if string if present - {1} is not present because deleted above
                if (MakeSubstitutions(ref conStr))
                {
                    List<string> databaseList = getSqlDatabaseList(conStr);
                    if (databaseList.Count > 0)
                    {
                        //frmDeleteDatabase used to show databases
                        frmListItems databaseListForm = new frmListItems();
                        databaseListForm.myList = databaseList;
                        databaseListForm.myJob = frmListItems.job.SelectString;
                        databaseListForm.Text = "Select Database";
                        databaseListForm.ShowDialog();
                        bool OKselected = databaseListForm.OK;
                        string selectedValue = string.Empty;
                        if (databaseListForm.mySelectedValues.Count > 0)
                        { 
                            selectedValue = databaseListForm.mySelectedValues[0];
                        }
                        if (selectedValue != string.Empty)
                        {
                            this.txtDatabase.Text = selectedValue;
                        }
                        databaseListForm = null;
                    }
                }
            }
        }

        private List<string> getSqlDatabaseList(string cs)
        {
            List<string> strList = new List<string>();
            try
            {
                MsSql.openNoDatabaseConnection(cs);
                DataTable databases = MsSql.noDatabaseConnection.GetSchema("Databases");
                foreach (DataRow database in databases.Rows)
                {
                    strList.Add(database.Field<String>("database_name"));
                }
                MsSql.CloseNoDatabaseConnection();
            }
            catch (Exception exc)
            {
                MessageBox.Show(Properties.MyResources.errorOpeningConnection + exc.Message, Properties.MyResources.errorOpeningConnection, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return strList;
        }

        private void cmdTest_Click(object sender, EventArgs e)
        {
            string cs = cmbStrings.Text;
            if (CheckForMissingVariables(cs))
            {
                if (MakeSubstitutions(ref cs))   // May fail if user fails to give password
                                                 //Try to open connection
                    try
                    {
                        MsSql.CloseConnection();
                        MsSql.openConnection(cs);  // No error handling in openConnection(cs)
                        MessageBox.Show(MyResources.testPassed, "Success");
                        cmdOK.Enabled = true;
                        success = true;
                    }
                    catch (System.Exception excep)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(MyResources.errorOpeningConnection);
                        sb.AppendLine(excep.Message);
                        MessageBox.Show(sb.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cmdOK.Enabled = false;
                        success = false;
                    }
            }
        }

        private void cmdCancel_Click(Object eventSender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            string strCS = cmbStrings.Text;
            // Password, if any, already set
            strCS = strCS.Replace("{{3}", password);
            MakeSubstitutions(ref strCS);   // Always succeeds
            // Create connection String object and put it first in the list
            // The main form uses the first item in csList
            connectionString conString = new connectionString(cmbStrings.Text, this.txtServer.Text,
                this.txtUserId.Text, this.txtDatabase.Text, MsSql.databaseType);
            // Remove old occurrences from stored list of strings - csList is filled of form load.
            foreach (connectionString cs in csList)
            {
                if (AppData.sameConnectionString(cs, conString))
                {
                    csList.Remove(cs);
                    break;  // only remove once or you will get an error
                }
            }
            csList.Insert(0, conString);
            AppData.storeConnectionStringList(csList);  // Stores JSON for each connectionString
            this.Close();
        }

        private void cmbStrings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStrings.SelectedIndex > -1)
            {
                // Condition required because a default may have been selected
                if (csList.Count > cmbStrings.SelectedIndex)
                {
                    txtServer.Text = csList[cmbStrings.SelectedIndex].server;
                    txtUserId.Text = csList[cmbStrings.SelectedIndex].user;
                    txtDatabase.Text = csList[cmbStrings.SelectedIndex].databaseName;
                }
            }
        }

        private bool CheckForMissingVariables(string conStr)
        {
            StringBuilder sb = new StringBuilder();
            if (conStr.IndexOf("{0}") > -1 && this.txtServer.Text == "")
            {
                sb.AppendLine(MyResources.youMustEnterServerName);
            }
            if (conStr.IndexOf("{2}") > -1 && this.txtUserId.Text == string.Empty)
            {
                sb.AppendLine(MyResources.enterUserNameOrNoUserString2);
            }
            if (conStr.IndexOf("{1}") > -1 && txtDatabase.Text == string.Empty)
            {
                sb.AppendLine(MyResources.pleaseChooseDatabaseString);
            }

            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool MakeSubstitutions(ref string cs)
        {
            bool OK = true;
            cs = cs.Replace("{0}", this.txtServer.Text);
            cs = cs.Replace("{1}", this.txtDatabase.Text);
            cs = cs.Replace("{2}", this.txtUserId.Text);
            if (cs.IndexOf("{3}") > -1)
            {
                frmLogin passwordForm = new frmLogin();
                passwordForm.Text = txtServer.Text;
                passwordForm.ShowDialog();
                string password = passwordForm.password;
                passwordForm = null;
                if (!String.IsNullOrEmpty(password))
                {
                    cs = cs.Replace("{3}", password);
                }
                else { OK = false; }
            }
            return OK;
        }

    }
}