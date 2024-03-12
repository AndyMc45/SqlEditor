using Microsoft.VisualBasic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SqlEditor
{
    public class connectionString
    {
        public string comboString { get; set; } // Connection string with all variable for use in comboBox.
        public string server { get; set; }
        public string user { get; set; }
        public string databaseName { get; set; } // MsSql, MySql 
        public string databaseType { get; set; } // MsSql, MySql 

        public connectionString(string comboString, string server, string user, string databaseName, string databaseType)
        {
            this.server = server;
            this.user = user;
            this.comboString = comboString;
            this.databaseName = databaseName;
            this.databaseType = databaseType;
        }
    }

    public static class AppData
    {
        private static string appName = "SqlEditor";

        public static void StoreFormOptions(FormOptions formOpts)
        {
            string jsonString = JsonSerializer.Serialize<FormOptions>(formOpts);
            SaveKeyValue("FormOptions", jsonString);
        }

        public static FormOptions GetFormOptions()
        {
            string jsonString = GetKeyValue("FormOptions");
            try
            {
                FormOptions formOpts = JsonSerializer.Deserialize<FormOptions>(jsonString);
                return formOpts;
            }
            catch
            {
                FormOptions formOptsNew = new FormOptions();
                return formOptsNew;
            }
        }

        public static void storeConnectionStringList(List<connectionString> csList)
        {
            // Create List
            List<string> strCsList = new List<string>();
            foreach (connectionString cs in csList)
            {
                strCsList.Add(JsonSerializer.Serialize<connectionString>(cs));
            }

            //Delete old list
            int i = 0;
            string currentValue = string.Empty;
            while (currentValue != "_end")
            {
                currentValue = Interaction.GetSetting(appName, "ConnectionList", i.ToString(), "_end");
                if (currentValue != "_end")
                {
                    Interaction.DeleteSetting(appName, "ConnectionList", i.ToString());
                }
                i++;
            }

            //Store the new list
            i = 0;
            foreach (string str in strCsList)
            {
                Interaction.SaveSetting(appName, "ConnectionList", i.ToString(), str);
                i++;
            }

        }

        public static connectionString? GetFirstConnectionStringOrNull()
        {
            string jsonString = Interaction.GetSetting(appName, "ConnectionList", 0.ToString(), "_end");
            if (jsonString == "_end")
            {
                return default;  // This is the way to return null
            }
            connectionString cs = JsonSerializer.Deserialize<connectionString>(jsonString);
            return cs;
        }

        public static List<connectionString> GetConnectionStringList()
        {
            List<string> jsonStringList = regitGetList("ConnectionList");
            List<connectionString> csList = new List<connectionString>();
            foreach (string str in jsonStringList)
            {
                connectionString cs = JsonSerializer.Deserialize<connectionString>(str);
                csList.Add(cs);
            }
            return csList;
        }

        public static bool sameConnectionString(connectionString value1, connectionString value2)
        {
            string v1 = JsonSerializer.Serialize<connectionString>(value1);
            string v2 = JsonSerializer.Serialize<connectionString>(value2);
            // Connection strings are the same if they differ only by spaces before or after = or ;
            v1 = Regex.Replace(v1, @"\s*=\\s*", "=");
            v1 = Regex.Replace(v1, @"\s*;\\s*", ";");
            v2 = Regex.Replace(v2, @"\s*=\\s*", "=");
            v2 = Regex.Replace(v2, @"\s*;\\s*", ";");
            if (v1 == v2) { return true; }
            return false;
        }

        public static void SaveKeyValue(string key, string keyValue)
        {
            Interaction.SaveSetting(appName, "SingleValue", key, keyValue);
        }

        public static string GetKeyValue(string key)
        {
            // string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return Interaction.GetSetting(appName, "SingleValue", key, string.Empty);
        }

        private static List<string> regitGetList(string section)
        {
            List<string> strList = new List<string>();
            int i = 0;
            string currentValue = Interaction.GetSetting(appName, section, 0.ToString(), "_end");
            while (currentValue != "_end")
            {
                strList.Add(currentValue);
                i++;
                currentValue = Interaction.GetSetting(appName, section, i.ToString(), "_end");
            }
            return strList;
        }

    }
}
