using System.Data;
namespace SqlEditor.PluginsInterface
{
    public interface IPlugin
    {
        // Interface requires 7 things - a string("Name()") and a ControlTemplate("PlugInControls()")
        // a MainForm and three functions (UpdateConstraints(), InsertConstraints(), and DeleteConstraints())
        // and a NewTableAction()
        String Name();
        ControlTemplate CntTemplate();

        // MainForm is exported back to every plugin
        Form MainForm { set; }

        List<Func<String, String, DataRow, bool>> UpdateConstraints();
        List<Func<string, List<Tuple<String, String>>, bool>> InsertConstraints();
        List<Func<String, int, bool>> DeleteConstraints();
        Action<String> NewTableAction();
    }
}
