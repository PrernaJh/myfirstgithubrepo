using DevExpress.Spreadsheet;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ParcelPrepGov.Reports.Utility
{
    public static class WorkbookExtensions
    {
        public static Workbook CreateWorkbook()
        {
            var workbook = new Workbook();
            workbook.Unit = DevExpress.Office.DocumentUnit.Point;
            return workbook;
        }

        public static void ImportDataToWorkSheets<T>(this Workbook workbook, ref int workSheetIndex, IEnumerable<T> dataSource)
        {
            if(dataSource.Count() == 0 && workSheetIndex > 0)
            {
                workbook.Worksheets.Add();
                workSheetIndex++;
            }
            else if (dataSource.Count() == 0 && workSheetIndex == 0)
            {
                workSheetIndex++;
            }

            int count = 1;
            var workSheetName = GetWorksheetName<T>();
            int maxRows = 1048576; // 1,048,576
            workbook.Unit = DevExpress.Office.DocumentUnit.Point;

            for (int offset = 0; offset < dataSource.Count(); offset += maxRows - 1)
            {           
                if (workSheetIndex >= workbook.Worksheets.Count())
                {
                    workbook.Worksheets.Add();
                }

                var workSheet = workbook.Worksheets[workSheetIndex++];
                workSheet.Name = workSheetName;
                workSheetName = GetWorksheetName<T>(count++);
                workSheet.ImportDataToWorksheet<T>(dataSource.Skip(offset).Take(maxRows - 1));          
            }
            workbook.Sheets.ActiveSheet = workbook.Sheets.FirstOrDefault(); // first sheet default 
        }

        /// <summary>
        /// Used to delete columns that have the excel ignore attribute on the view model in the current worksheet.
        /// </summary>                
        public static void DeleteIgnoredColumns<T>(this Worksheet workSheet)
        {
            var cols = workSheet.GetIgnoredColumns<T>(null);

            if(cols.Count > 0)
            {
                DeleteColumns<T>(workSheet, cols.ToArray());
            }
        }

        /// <summary>
        /// Used to return a list of column names that are ignored in the current T passed in
        /// </summary>        
        public static IList<string> GetIgnoredColumns<T>(this Worksheet workSheet, string userRole)
        {
            var cols = new List<string>();
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                var nameAttribute = property.GetAttribute<ExcelIgnoreAttribute>();
                if (nameAttribute != null)
                {
                    if (userRole == null)
                    {
                        cols.Add(property.Name);
                    }
                    else
                    {
                        if (nameAttribute?.Roles != null)
                        {
                            foreach (var role in nameAttribute?.Roles)
                            {
                                if (role == null || userRole == role)
                                {
                                    cols.Add(property.Name);
                                }
                            }
                        }
                        else
                        {
                            cols.Add(property.Name);
                        }
                    }
                }
            }

            return cols;
        }
        public static void FixupReportWorkSheet<T>(this Worksheet workSheet, string host, string userRole = null, bool addTotals = false)
        {
            var rowCount = workSheet.GetDataRange().RowCount; 
            var headers = workSheet.GetColumns<T>();
            var colCount = headers.Count;
            var col = 0;
            var hyperlinkCol = 0;
            var totals = new Dictionary<int, decimal>(); // col -> total

            var columns = workSheet.FindColumns<T>(new string[] {
                "PACKAGE_ID",
                "PACKAGE_ID_HYPERLINK",
                "TRACKING_NUMBER",
                "TRACKING_NUMBER_HYPERLINK",
                "CONTAINER_ID",
                "CONTAINER_ID_HYPERLINK",
                "CONTAINER_TRACKING_NUMBER",
                "CONTAINER_TRACKING_NUMBER_HYPERLINK",
                "INQUIRY_ID",
                "INQUIRY_ID_HYPERLINK"
            });

            AddHyperLink(workSheet, "PACKAGE_ID", rowCount, columns);
            AddHyperLink(workSheet, "TRACKING_NUMBER", rowCount, columns);
            AddHyperLink(workSheet, "CONTAINER_ID", rowCount, columns);
            AddHyperLink(workSheet, "CONTAINER_TRACKING_NUMBER", rowCount, columns);
            AddHyperLink(workSheet, "INQUIRY_ID", rowCount, columns);

            col = 0;
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                var displayAttribute = property.GetAttribute<DisplayFormatAttribute>();
                var columnName = property.Name;
                var total = 0m;
                if (displayAttribute?.FormatType == "COUNT")
                {
                    for (int i = 1; i < rowCount; i++)
                    {
                        var cell = workSheet.Cells[i, col];
                        cell.NumberFormat = IntegerFormatString();
                        total += DecimalValue(cell, 0);
                    }
                }
                else if (displayAttribute?.FormatType == "DECIMAL")
                {
                    for (int i = 1; i < rowCount; i++)
                    {
                        var cell = workSheet.Cells[i, col];
                        cell.NumberFormat = DecimalFormatString(displayAttribute.Precision);
                        total += DecimalValue(cell, displayAttribute.Precision);
                    }
                }                
                else if (displayAttribute?.FormatType == "PERCENT")
                {
                    for (int i = 1; i < rowCount; i++)
                    {
                        var cell = workSheet.Cells[i, col];
                        cell.NumberFormat = PercentFormatString(displayAttribute.Precision);
                    }
                }
                else if (displayAttribute?.FormatType == "AVERAGE")
                {
                    var refCols = workSheet.FindColumns<T>(displayAttribute.References);
                    // Is this a weighted average?
                    if (displayAttribute.References.Length >= 1 &&
                        refCols.TryGetValue(displayAttribute.References[0], out var count))
                    {
                        for (int i = 1; i < rowCount; i++)
                        {
                            var cell = workSheet.Cells[i, col];
                            cell.NumberFormat = DecimalFormatString(displayAttribute.Precision);
                            var countCell = workSheet.Cells[i, count];
                            total += DecimalValue(countCell, 0) * DecimalValue(cell, displayAttribute.Precision);
                        }
                    }
                    else
                    {
                        for (int i = 1; i < rowCount; i++)
                        {
                            var cell = workSheet.Cells[i, col];
                            cell.NumberFormat = DecimalFormatString(displayAttribute.Precision);
                            total += DecimalValue(cell, displayAttribute.Precision);
                        }
                    }
                }
                else if (displayAttribute?.FormatType == "DATE")
                {
                    var formatString = DateFormatString();
                    var cell = workSheet.Cells[0, col];
                    cell.ColumnWidthInCharacters = (double)formatString.Length;
                    for (int i = 1; i < rowCount; i++)
                    {
                        cell = workSheet.Cells[i, col];
                        cell.NumberFormat = formatString;
                    }
                }
                else if (displayAttribute?.FormatType == "DATE_TIME")
                {
                    var formatString = DateTimeFormatString();
                    var cell = workSheet.Cells[0, col];
                    cell.ColumnWidthInCharacters = (double)formatString.Length;
                    for (int i = 1; i < rowCount; i++)
                    {
                        cell = workSheet.Cells[i, col];
                        cell.NumberFormat = formatString;
                    }
                }
                else if (displayAttribute?.FormatType == "DATE_TIME_WS")
                {
                    var formatString = DateTimeFormatStringWithSeconds();
                    var cell = workSheet.Cells[0, col];
                    cell.ColumnWidthInCharacters = (double)formatString.Length;
                    for (int i = 1; i < rowCount; i++)
                    {
                        cell = workSheet.Cells[i, col];
                        cell.NumberFormat = formatString;
                    }
                }
                totals[col] = total;
                col++;
            }

            if (addTotals && rowCount > 1)
            {
                col = 0;
                foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
                {
                    var displayAttribute = property.GetAttribute<DisplayFormatAttribute>();
                    var columnName = property.Name;
                    var cell = workSheet.Cells[rowCount, col];
                    if (displayAttribute?.FormatType == "COUNT")
                    {
                        cell.Formula = $"SUM({CellName(1, col)}:{CellName(rowCount - 1, col)})";
                        cell.NumberFormat = IntegerFormatString();
                    }
                    else if (displayAttribute?.FormatType == "DECIMAL")
                    {
                        cell.Formula = $"SUM({CellName(1, col)}:{CellName(rowCount - 1, col)})";
                        cell.NumberFormat = DecimalFormatString(displayAttribute.Precision);
                    }
                    else if (displayAttribute?.FormatType == "PERCENT" && displayAttribute.References.Length == 2)
                    {
                        var refCols = workSheet.FindColumns<T>(displayAttribute.References);
                        if (displayAttribute.References.Length >= 2 &&
                            refCols.TryGetValue(displayAttribute.References[0], out var count) &&
                            refCols.TryGetValue(displayAttribute.References[1], out var total))
                        {
                            cell.Formula = $"IFERROR({CellName(rowCount, count)}/{CellName(rowCount, total)}, 0)";
                            cell.NumberFormat = PercentFormatString(displayAttribute.Precision);
                        }
                    }
                    else if (displayAttribute?.FormatType == "AVERAGE")
                    {
                        var refCols = workSheet.FindColumns<T>(displayAttribute.References);
                        // Is this a weighted average?
                        if (displayAttribute.References.Length >= 1 &&
                            refCols.TryGetValue(displayAttribute.References[0], out var count))
                        {
#if true
                            // example =SUM(N2:N4 * L2:L4) / L5
                            cell.ArrayFormula = $"IFERROR(SUM({CellName(1, col)}:{CellName(rowCount - 1, col)}*{CellName(1, count)}:{CellName(rowCount - 1, count)})/{CellName(rowCount, count)}, 0)";;
#else                       // Need to compute it by hand
                            cell.Value = totals[count] == 0m ? 0.0 : Math.Round((double) (totals[col] / totals[count]), displayAttribute.Precision);
#endif
                        }
                        else
                        {
                            // example =SUM(N2:N4) / (rowCount-1)
                            cell.Formula = $"SUM({CellName(1, col)}:{CellName(rowCount - 1, col)})/{rowCount-1}";
                        }
                        cell.NumberFormat = DecimalFormatString(displayAttribute.Precision);
                    }
                    else if (col == 0)
                    {
                        cell.Value = "Totals";
                    }
                    col++;
                }
            }

            var columnsToDelete = new List<string>();
            columnsToDelete.AddRange(new string[] {
                "ID",                
                "PACKAGE_DATA_SET_ID",
            });
            columnsToDelete.AddRange(workSheet.GetColumns<T>().Where(n => n.EndsWith("_HYPERLINK")));
            columnsToDelete.AddRange(workSheet.GetColumns<T>().Where(n => n.EndsWith("_STRING")));
            columnsToDelete.AddRange(workSheet.GetIgnoredColumns<T>(userRole));
            columnsToDelete = columnsToDelete.Distinct().ToList();
            workSheet.DeleteColumns<T>(columnsToDelete.ToArray());            
        }
        public static IList<string> GetColumns<T>(this Worksheet workSheet)
        {
            var headers = new List<string>();
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                headers.Add(property.Name);
            }
            return headers;
        }
        public static IDictionary<string, int> FindColumns<T>(this Worksheet workSheet, string[] columnNames)
        {
            var columns = new Dictionary<string, int>();
            var col = 0;
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                if (columnNames.Contains(property.Name))
                    columns[property.Name] = col;
                col++;
            }
            return columns;
        }
        public static T GetAttribute<T>(this PropertyInfo property)
        {
            return (T) property.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        /// <summary>
        /// Can only be called once per worksheet
        /// </summary>        
        /// <example>When called more than once columns could be removed unexpectedly, column names should be unique</example>        
        public static void DeleteColumns<T>(this Worksheet workSheet, string [] columnNames)
        {
            var columns = workSheet.FindColumns<T>(columnNames);
            foreach (var col in columns.Values.OrderByDescending(c => c))
            {
                workSheet.Columns.Remove(col, 1);
            }
        }

        static string[] ColumnNames = {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
            };
        public static string CellName(int row, int column)
        {
            return $"{ColumnNames[column]}{row+1}";
        }
        private static void AddHyperLink(this Worksheet workSheet, string columnName, int rowCount, IDictionary<string,int> columns)
        {
            int col, hyperlinkCol;
            if (columns.TryGetValue(columnName, out col)
                && columns.TryGetValue($"{columnName}_HYPERLINK", out hyperlinkCol))
            {
                for (int i = 1; i < rowCount; i++)
                {                    
                    var cell = workSheet.Cells[i, col];
                    var linkText = cell.Value.TextValue;                    
                    
                    if (StringHelper.Exists(linkText))
                    {
                        var url = workSheet.Cells[i, hyperlinkCol].Value.TextValue;
                        if (StringHelper.Exists(url) && url != linkText)
                        {
                            cell.Formula = $"HYPERLINK(\"{url}\",\"{linkText}\")";
                            cell.Font.Color = Color.Blue;
                            cell.Font.UnderlineType = UnderlineType.Single;
                        }
                        else
                        {
                            // leave the cell alone EA. cell.Value.TextValue = cell.Value.TextValue
                        }
                    }
                    else
                    {
                        cell.Value = string.Empty;
                    }
                }
            }
        }
        private static string GetWorksheetName<T>(int? count = null)
        {
            var workSheetName = typeof(T).Name;
            if (workSheetName.Length >= 25)
            {
                workSheetName = $"{ typeof(T).Name.Substring(0, 25) }";
            }

            if (count != null)
            {
                workSheetName += $".{count}";
            }

            return workSheetName;
        }
        private static void ImportDataToWorksheet<T>(this Worksheet workSheet, IEnumerable<T> dataSource)
        {
            var col = 0;
            // Create header row
            foreach (var property in typeof(T).GetProperties(~BindingFlags.Static))
            {
                var nameAttribute = property.GetAttribute<DisplayNameAttribute>();
                workSheet.Cells[0, col++].Value = nameAttribute?.Name ?? property.Name;
            }
            workSheet.Import(dataSource, 1, 0);
            workSheet.Columns.AutoFit(0, col - 1);            
        }
        static string PercentFormatString(int precision)
        {
            return $"0.{new string('0', precision)}%";
        }
        static string IntegerFormatString()
        {
            return "#,##0";
        }    
        static string DecimalFormatString(int precision)
        {
            return $"0.{new string('0', precision)}";
        }      
        static string DateFormatString()
        {
            return "mm/dd/yyyy";
        }
        static string DateTimeFormatString()
        {
            return "mm/dd/yyyy hh:mm AM/PM";
        }
        static string DateTimeFormatStringWithSeconds()
        {
            return "mm/dd/yyyy hh:mm:ss AM/PM"; // add .000 for milliseconds
        }
        static decimal DecimalValue(Cell cell, int precision)
        {
           return (decimal) Math.Round(cell.Value.NumericValue, precision);
        }
    }
}
