using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;

public static class ConvertidorCSVExcel
{
    public static void ConvertirCSVaExcel(string csvPath, string excelPath)
    {
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(excelPath, SpreadsheetDocumentType.Workbook))
        {
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            Sheets sheets = spreadsheetDocument.WorkbookPart!.Workbook.AppendChild<Sheets>(new Sheets());
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
            sheets.Append(sheet);

            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            using (StreamReader reader = new StreamReader(csvPath))
            {
                string? line;
                uint rowIndex = 1;

                while ((line = reader.ReadLine()) != null)
                {
                    Row row = new Row() { RowIndex = rowIndex };
                    sheetData.Append(row);

                    // Dividir per punt i coma (delimitador CSV)
                    string[] cells = line.Split(';');

                    for (int colIndex = 0; colIndex < cells.Length; colIndex++)
                    {
                        Cell cell = new Cell()
                        {
                            CellReference = GetCellReference((uint)colIndex + 1, rowIndex),
                            DataType = CellValues.String,
                            CellValue = new CellValue(cells[colIndex])
                        };
                        row.Append(cell);
                    }

                    rowIndex++;
                }
            }

            workbookpart.Workbook.Save();
        }
    }

    static string GetCellReference(uint columnIndex, uint rowIndex)
    {
        string columnName = GetColumnName(columnIndex);
        return columnName + rowIndex.ToString();
    }

    static string GetColumnName(uint columnIndex)
    {
        string columnName = "";

        while (columnIndex > 0)
        {
            uint modulo = (columnIndex - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            columnIndex = (columnIndex - modulo) / 26;
        }

        return columnName;
    }
}