namespace SqlEditor.PluginsInterface
{
    public interface IPlugin
    {
        // Interface requires two things - a string("Name()") and a ControlTemplate("PlugInControls()")
        String Name();
        ControlTemplate CntTemplate();

        // MainForm is exported back to every plugin
        Form MainForm { set; }

    }
}
