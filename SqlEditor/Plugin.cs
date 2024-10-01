using SqlEditor.PluginsInterface;
using System.Reflection;
using System.Data;
using Unity;


namespace SqlEditor
{

    // public delegate void MainFormDelegate(Form dgvForm);

    internal static class Plugins
    {
        static IUnityContainer? container = null;
        static internal String pluginFilePath = String.Empty;
        // Get set of all plugins
        static internal IEnumerable<SqlEditor.PluginsInterface.IPlugin>? loadedPlugins;
        // When invoked, this will add the mainForm into all plugins
        public static void ExportForm(Form dgvForm)
        {
            if (loadedPlugins != null)
            {
                foreach (SqlEditor.PluginsInterface.IPlugin plugIn in loadedPlugins)
                {
                    plugIn.MainForm = dgvForm;
                }
            }
        }

        static internal MenuStrip Load_Plugins(
                    ref Dictionary<string, string> colHeaderTranslations, 
                    ref string translationCultureName, 
                    ref List<(string, string)> readOnlyFields, 
                    ref List<Func<string, string, DataRow, bool>> updateConstraints, 
                    ref List<Func<string, List<Tuple<String, String>>, bool>> insertConstraints,
                    ref List<Func<string, int, bool>> deleteConstraints) 
        {
            // First delete plugin marked for deletion
            string deletePluginPath = AppData.GetKeyValue("deletePluginPath");
            if (!String.IsNullOrEmpty(deletePluginPath))
            {
                if (Directory.Exists(deletePluginPath))
                {
                    DeleteDirectory(deletePluginPath);
                }
                AppData.SaveKeyValue("deletePluginPath", string.Empty);
            }

            MenuStrip plugInMenus = new MenuStrip();
            // Get plug in path
            string appDataPath = Application.CommonAppDataPath;
            string pluginFilePath = String.Format("{0}\\{1}", Application.CommonAppDataPath, "PluginsToConsume"); ;
            if (!Directory.Exists(pluginFilePath))
            {
                try { Directory.CreateDirectory(pluginFilePath); }
                catch { };
            }
            else
            {
                container = new UnityContainer();
                // Get recursive list of dll files

                List<string> dllFiles = GetListDllFiles(pluginFilePath);
                // string[] files = Directory.GetFiles(pluginFilePath, "*.dll");

                Int32 pluginCount = 1;

                foreach (String file in dllFiles)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(file);

                        IEnumerable<Type> types = GetLoadableTypes(assembly); // Double catch f
                        foreach (Type T in types)
                        {
                            foreach (Type iface in T.GetInterfaces())
                            {
                                if (iface == typeof(IPlugin))
                                {
                                    // pluginInstance.name = "transcripts"
                                    IPlugin pluginInstance = (IPlugin)Activator.CreateInstance(T, new[] { "Live Plugin " + pluginCount++ });
                                    container.RegisterInstance<IPlugin>(pluginInstance.Name(), pluginInstance);
                                }
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {

                    }
                }
                // At this point the unity container has all the plugin data loaded onto it. 
                // For each plugin, add its menustrip to plugInMenus
                // For each plugin, add its translations to the ColumnHeader
                if (container != null)
                {
                    loadedPlugins = container.ResolveAll<IPlugin>();
                    if (loadedPlugins.Count() > 0)
                    {
                        // Should check that a plugin is not loaded twice 
                        foreach (var loadedPlugin in loadedPlugins)
                        {
                            // Add menu strip
                            plugInMenus.Items.Add(loadedPlugin.CntTemplate().menuStrip);

                            // Translation - First in is approved if there is a conflict
                            foreach (string key in loadedPlugin.CntTemplate().ColumnHeaderTranslations.Keys)
                            {
                                if (!colHeaderTranslations.ContainsKey(key))
                                {
                                    colHeaderTranslations.Add(key, loadedPlugin.CntTemplate().ColumnHeaderTranslations[key]);
                                }
                            }
                            // The language of the translation - if two plugins have this, the last one wins!
                            translationCultureName = loadedPlugin.CntTemplate().TranslationCultureName;

                            // ReadOnly Fields - added twice if in two plugins but unlikely and o.k.
                            foreach ((string, string) readOnly in loadedPlugin.CntTemplate().ReadOnlyFields)
                            {
                                readOnlyFields.Add(readOnly);
                            }

                            // Add Constraints
                            foreach (Func<string, string, DataRow, bool> constraint in loadedPlugin.UpdateConstraints())
                            { 
                                updateConstraints.Add(constraint);
                            }
                            foreach (Func<string, List<Tuple<String, String>>, bool> constraint in loadedPlugin.InsertConstraints())
                            {
                                insertConstraints.Add(constraint);
                            }
                            foreach (Func<string, int, bool> constraint in loadedPlugin.DeleteConstraints())
                            {
                                deleteConstraints.Add(constraint);
                            }
                        }
                    }
                }
            }
            return plugInMenus;
        }

        static List<string> GetListDllFiles(string sourceDir)
        {
            List<string> dllList = new List<string>();
            // Get information about the source directory

            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists - just in case
            if (!dir.Exists) { return dllList; }

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the source directory and add if they end in .dll
            foreach (FileInfo file in dir.GetFiles("*.dll"))
            {
                dllList.Add(file.FullName);
            }
            foreach (DirectoryInfo subDir in dirs)
            {
                List<string> subDllList = GetListDllFiles(subDir.FullName);
                foreach (string dll in subDllList) { dllList.Add(dll); }  // Long way but clear
            }
            return dllList;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        // Used to delete readonly files 
        private static void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

    }


    // Future Plugin - Address book
    // Print mailing labels, phone book,address book, email list, get email list
}
