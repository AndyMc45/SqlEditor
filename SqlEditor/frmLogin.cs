
namespace SqlEditor
{
    internal partial class frmLogin : Form
    {
        internal List<string> pwList = new List<string>();
        internal string? password;
        internal frmLogin()
        {
            InitializeComponent();
        }

        internal void checkLists()
        {
            if (pwList.Count != cmbStoredPasswords.Items.Count)
            {
                string msg = "pwList: " + pwList.Count.ToString() + "  cmbCount: " + cmbStoredPasswords.Items.Count.ToString() + Environment.NewLine;
                for (int i = 0; i < Math.Min(pwList.Count, cmbStoredPasswords.Items.Count); i++)
                {
                    msg = msg + pwList[i] +  "   :   " + cmbStoredPasswords.Items[i].ToString() + Environment.NewLine;
                }
                MessageBox.Show(msg);
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '#';
            // Load stored passwords
            pwList.Clear();  // Not needed but for clarity 
            string pw = String.Empty;
            pwList.Add(pw);
            for (int i = 0; i < 10; i++)
            {
                pw = AppData.GetPasswordKeyValue(i);
                if (!String.IsNullOrEmpty(pw) && pw != "_end")
                {
                    pwList.Add(pw);
                }
            }
            //Load combobox
            foreach (string pwItem in pwList)
            {
                string pwShort = GetPasswordClue(pwItem);
                cmbStoredPasswords.Items.Add(pwShort);
            }
            checkLists();
        }

        private void cmdCancel_Click(Object eventSender, EventArgs eventArgs)
        {
            password = "";
            this.Close();
        }
        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            password = txtPassword.Text;
            this.Close();

        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { SendKeys.Send("{TAB}"); }

        }

        private void cmdCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            checkLists();

            if (txtPassword.Text.Length > 0)
            {
                pwList.Insert(1, txtPassword.Text);
                cmbStoredPasswords.Items.Insert(1, GetPasswordClue(txtPassword.Text));
                AppData.StorePasswordKeyValues(pwList);
            }
            checkLists();

        }

        private string GetPasswordClue(string pw)
        {
            string shortPw = string.Empty;
            if (pw.Length == 0)
            {
                // shortPw already empty 
            }
            else if (pw.Length < 4)
            {
                shortPw = "Very short";
            }
            else
            {
                shortPw = pw.Substring(0, 2);
                int dots = pw.Length;
                while (dots > 0)
                {
                    shortPw = shortPw + " .";
                    dots = dots - 3;
                }
                shortPw = shortPw + " " + pw.Substring(pw.Length - 1, 1);
            }
            return shortPw;
        }

        private void lblStored_Click(object sender, EventArgs e)
        {

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            checkLists();

            int i = cmbStoredPasswords.SelectedIndex;
            if (i > 0)
            {
                txtPassword.Text = String.Empty;
                if (i < pwList.Count)  // Just checking
                {
                    pwList.RemoveAt(i);
                }
                //Clear and reload combobox
                cmbStoredPasswords.Items.Clear();
                foreach (string pwItem in pwList)
                {
                    string pwShort = GetPasswordClue(pwItem);
                    cmbStoredPasswords.Items.Add(pwShort);
                }
                // Store Password
                AppData.StorePasswordKeyValues(pwList);
            }
            checkLists();

        } 

        private void cmbStoredPasswords_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkLists();

            txtPassword.Text = pwList[cmbStoredPasswords.SelectedIndex];
            checkLists();

        }
    }
}