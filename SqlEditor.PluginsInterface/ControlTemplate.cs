// "Shared" means that all plugins have a ControlTemplate
// This has many things

namespace SqlEditor.PluginsInterface
{
    public class ControlTemplate
    {
        public Form frmTemplate;

        public ToolStripMenuItem menuStrip;  // Constructed from menuItems, menuName, CallBack

        public String TranslationCultureName;

        public Dictionary<string, string> ColumnHeaderTranslations;

        public List<(String, String)> ReadOnlyFields;

        public event EventHandler<EventArgs<String>> CallBack;

        // Constructor - called by plugin
        public ControlTemplate(
                (String, String) menuName,
                List<(String, String)> menuItems,
                EventHandler<EventArgs<String>> callBack,
                Form frmOptions,
                String translationCultureName,
                Dictionary<string, string> columnHeaderTranslations,
                List<(String, String)> readOnlyFields
            )
        {

            frmTemplate = frmOptions;
            TranslationCultureName = translationCultureName;
            ColumnHeaderTranslations = columnHeaderTranslations;
            ReadOnlyFields = readOnlyFields;

            // Define and fill the menuStrip
            CallBack = callBack;
            ToolStripMenuItem topLevelMenuStripItem = new ToolStripMenuItem(menuName.Item1);
            // topLevelMenuStripItem.Text = menuName.Item1;
            topLevelMenuStripItem.Tag = menuName.Item2;

            foreach ((String, String) tuple in menuItems)
            {
                ToolStripMenuItem dropDownMenuStripItem = new ToolStripMenuItem(tuple.Item1);
                dropDownMenuStripItem.Tag = tuple.Item2;
                dropDownMenuStripItem.Click += new EventHandler(MenuItemClickHandler);
                topLevelMenuStripItem.DropDownItems.Add(dropDownMenuStripItem);
            }
            menuStrip = topLevelMenuStripItem;
            menuStrip.Text = menuName.Item1;
            menuStrip.Tag = menuName.Item2;
            menuStrip.Click += new EventHandler(MenuItemClickHandler);
        }

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem receivedMenuItem = (ToolStripMenuItem)sender;
            CallBack.SafeInvoke(this, new EventArgs<string>(receivedMenuItem.Tag.ToString()));
        }

    }
}
