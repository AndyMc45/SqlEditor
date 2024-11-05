using System.DirectoryServices.ActiveDirectory;

namespace SqlEditor
{
    public class FormOptions
    {
        public FormOptions()   // After the program runs once, these options are stored in registry - no use changing here
        {
            debugging = false;
            debugInt = 0;
            runTimer = false;
            narrowColumns = false;
            pageSize = 0;
            logFileName = string.Empty;
            orderComboListsByPK = false;
            excelFilesFolder = String.Empty;
        }
        public bool debugging { get; set; }
        public int debugInt { get; set; }
        public bool runTimer { get; set; }
        public int pageSize { get; set; }
        public string logFileName { get; set; }
        public bool narrowColumns { get; set; }
        public bool orderComboListsByPK { get; set; }
        public string excelFilesFolder { get; set; }


        public FileStream? ts;
        public Color[] nonDkColorArray = new Color[] { Color.LightCyan, Color.LavenderBlush, Color.Plum, Color.Pink, Color.LightGray, Color.LightSalmon, Color.Azure, Color.OrangeRed };
        public Color[] DkColorArray = new Color[] { Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow, Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow };
        public Color DefaultColumnColor = Color.Yellow;
        public Color PrimaryKeyColor = Color.Pink;

    }

    internal class ConnectionOptions
    {
        public ConnectionOptions()
        {
            readOnly = false;
            mySql = false;
            msSql = false;
        }
        internal bool readOnly { get; set; }
        internal bool mySql { get; set; }
        internal bool msSql { get; set; }
    }

    internal class TableOptions
    {
        internal TableOptions()
        {
            writingNewTable = false;
            writingNewFilter = false;
            writingNewPage = false;
            clearingAllFilters = false;
            delayRebindGridFV = false;
            delayWriteGrid = false;
            doNotWriteGrid = false;
            FkFieldInEditingControl = null;
            tableHasForeignKeys = false;
            mergingDuplicateKeys = false;
        }
        // internal bool fixingDatabase { get; set; }
        // internal string strStaticWhereClause { get; set; }
        internal bool writingNewTable { get; set; }
        internal bool writingNewFilter { get; set; }
        internal bool writingNewPage { get; set; }
        internal bool clearingAllFilters { get; set; }
        internal bool delayRebindGridFV { get; set; }
        internal bool delayWriteGrid { get; set; }
        internal bool doNotWriteGrid { get; set; }
        internal field? FkFieldInEditingControl { get; set; }
        internal bool tableHasForeignKeys { get; set; }
        internal bool mergingDuplicateKeys { get; set; }

    }

}
