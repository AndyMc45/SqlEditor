using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;


namespace SqlEditor
{
    public static class Print_to_Excel
    {
        public static void datagridview_to_Exel(string fileName, DataGridView dgv)
        {
            int columncount = dgv.Columns.Count;
            int rowcount = dgv.Rows.Count;

            //// 1. Create workbook, add a sheet and save it
            SpreadsheetDocument ssd = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
            //Add work part to the Document  
            WorkbookPart wbp = ssd.AddWorkbookPart();
            wbp.Workbook = new Workbook();

            //add work sheet to the work part  
            WorksheetPart wsp = wbp.AddNewPart<WorksheetPart>();
            wsp.Worksheet = new Worksheet(new SheetData());
            // add sheets   
            Sheets shts = ssd.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            // Append a new worksheet and associate it with the workbook.  
            Sheet sheet = new Sheet()
            {
                Id = ssd.WorkbookPart.
                // create an new sheet  
                GetIdOfPart(wsp),
                SheetId = 1,
                Name = "mySheet"
            };
            shts.Append(sheet);

            //// 2. Add header
            //Create a new row, cell   
            Row row = new Row();
            Cell[] cell = new Cell[columncount];
            // the below used to create temple of the existing gridview  
            for (int i = 0; i < columncount; i++)
            {
                string[] columnhead = new string[columncount];
                columnhead[i] = dgv.Columns[i].HeaderText.ToString();
                cell[i] = new Cell();
                cell[i].CellReference = GetExcelColumnName(i);
                cell[i].DataType = CellValues.String;
                cell[i].CellValue = new CellValue(columnhead[i]);
                row.Append(cell[i]);
            }
            // wsp has a WorkSheet which has child elements.  First element is SheetData
            SheetData sheetData = wsp.Worksheet.GetFirstChild<SheetData>();
            sheetData.Append(row);

            //// 3. Add rows

            for (int i = 0; i < rowcount; i++)
            {
                //create a row  
                row = new Row();
                //create the cell dynamically using array  
                cell = new Cell[columncount];
                for (int j = 0; j < columncount; j++)
                {
                    // get the value in the grid view  
                    string data1 = dgv.Rows[i].Cells[j].Value.ToString();
                    cell[j] = new Cell();
                    cell[j].CellReference = GetExcelColumnName(j);
                    cell[j].DataType = CellValues.String;
                    cell[j].CellValue = new CellValue(data1);
                    row.Append(cell[j]);
                }
                sheetData.Append(row);
            }
            wbp.Workbook.Save();
            ssd.Dispose();

        }
        private static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }
    }
}
