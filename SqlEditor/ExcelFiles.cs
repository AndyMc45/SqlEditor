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
        public static string LoadExcelFileIntoCourseEvaluations(string filePath)
        {
            StringBuilder errSB = new StringBuilder(); 
            // Open the spreadsheet document for read-only access
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                if (workbookPart == null) return errSB.ToString();

                // Get the first worksheet part
                Sheet firstSheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
                if (firstSheet?.Id == null) return errSB.ToString();

                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id);
                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.Elements<SheetData>().First();

                bool headerRow = true;

                // Iterate through each row in the worksheet
                foreach (Row excelRow in sheetData.Elements<Row>())
                {
                    // Skip the first row - headerRow true on first call only
                    if (headerRow) {
                        headerRow = false;
                        // Check that the excel columns are in the right places
                        // Excel Header row must have the expected string for column in it
                        foreach (DataColumn courseEvaluationDC in courseEvaluations.Columns)
                        {
                            string strCol = courseEvaluationDC.ExtendedProperties["ExcelColumnLetter"].ToString();
                            string headerMustContain = courseEvaluationDC.ExtendedProperties["HeaderMustContain"].ToString();
                            string cellReference = strCol + excelRow.RowIndex.ToString();
                            // The following might return 'null' - but for header row I doubt it.
                            Cell cell = excelRow.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == cellReference);
                            string cellValue = string.Empty;
                            if (cell is not null)
                            {
                                cellValue = GetCellValue(cell, workbookPart);
                            }
                            if (!cellValue.Contains(headerMustContain, StringComparison.OrdinalIgnoreCase))
                            {
                                string errMsg = string.Format("Error in {3}: Header in col {0} (i.e. {1}) does not contain '{2}", strCol, cellValue, headerMustContain, filePath);
                                errSB.AppendLine(errMsg);
                            }
                        }
                        if (errSB.Length > 0) 
                        { 
                            return errSB.ToString(); //Skip the rest of the file
                        }
                        else 
                        { 
                            continue;  // Skip the below code
                        }
                    }
                    // Add row to courseEvaluation
                    DataRow ceNewRow = courseEvaluations.NewRow();
                    // Iterate through columns of courseEvaluation - finding matching column of courseEvaluation
                    foreach (DataColumn courseEvaluationDC in courseEvaluations.Columns)
                    {
                        string strCol = courseEvaluationDC.ExtendedProperties["ExcelColumnLetter"].ToString();
                        string cellReference = strCol + excelRow.RowIndex.ToString();
                        // The following might return 'null'
                        Cell cell = excelRow.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == cellReference);
                        // Get default value for empty cell (either string.empty or "-1")
                        string cellValue = string.Empty;   
                        if (courseEvaluationDC.ExtendedProperties["NumericQuestion"].ToString() == "True")
                        {
                            cellValue = "-1";
                        }
                        // Get value that is in the cell
                        if (cell is not null)
                        { 
                            cellValue = GetCellValue(cell, workbookPart);
                        }
                        //Add excel value to ce datatable
                        ceNewRow[courseEvaluationDC.ColumnName] = cellValue;  
                    }
                    courseEvaluations.Rows.Add(ceNewRow);
                }
            }
            return errSB.ToString();
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
                    // Not sure why value is sometimes string.empty, but it causes an error, so I add
                    int outValue = 0;
                    if (int.TryParse(value, out outValue))
                    {
                        value = stringTablePart.SharedStringTable.ElementAt(outValue).InnerText;
                    }
                }
            }
            return value;
        }
    }
}
