using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlTypes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Collections.Generic;


namespace SqlEditor
{
    public static class ExcelReader
    {
        public static DataTable headers { get; set; }  // Not really needed in this class
        public static DataTable courseEvaluations { get; set; }  //Very heart of class is to fill this

        // Helper method to read data from a single Excel file using the Open XML SDK
        // Enters this data into courseEvaluations datatable
        public static void LoadExcelFileIntoCourseEvaluations(string filePath)
        {
            // Open the spreadsheet document for read-only access
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                if (workbookPart == null) return;

                // Get the first worksheet part
                Sheet firstSheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (firstSheet?.Id == null) return;

                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id);
                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.Elements<SheetData>().First();

                bool headerRow = true;

                // Iterate through each row in the worksheet
                foreach (Row excelRow in sheetData.Elements<Row>())
                {
                    if (headerRow) {
                        headerRow = false; // prepare for next row
                        continue;
                    }
                    // Add row to courseEvaluation
                    DataRow ceNewRow = courseEvaluations.NewRow();
                    // Iterate through columns of courseEvaluation - finding matching column of courseEvaluation
                    foreach (DataColumn courseEvaluationDC in courseEvaluations.Columns)
                    {
                        string strCol = courseEvaluationDC.ExtendedProperties["ExcelColumn"].ToString();
                        int col = int.Parse(strCol);
                        try
                        {
                            Cell cell = (Cell)excelRow.ElementAt(col);  // might not exist, Note: Col 0 is COl A 
                            string cellValue = GetCellValue(cell, workbookPart);
                            ceNewRow[courseEvaluationDC.ColumnName] = cellValue;  //Add excel value to ce datatable
                        }
                        catch (Exception ex)
                        {

                            if (courseEvaluationDC.ExtendedProperties["NumericQuestion"].ToString() == "True")
                            {
                                ceNewRow[courseEvaluationDC.ColumnName] = "-1";  // -1 is no answer. Don't allow students to give a "-1"
                            } else {
                                ceNewRow[courseEvaluationDC.ColumnName] = string.Empty;
                            }
                        }
                    }
                    courseEvaluations.Rows.Add(ceNewRow);

                    // Iterate through each cell in the row
                    //foreach (Cell cell in row.Elements<Cell>()){
                    //    string cellValue = GetCellValue(cell, workbookPart); . . .; }
                }
            }
        }

        // A helper method to get the correct cell value (handling shared strings)
        private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
        {
            string value = cell.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                // If the cell value is a shared string, retrieve the actual string from the SharedStringTable
                SharedStringTablePart stringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTablePart != null)
                {
                    value = stringTablePart.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                }
            }
            return value;
        }
    }
}
