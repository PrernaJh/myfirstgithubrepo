using OfficeOpenXml;
using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Utilities
{
    public class ExcelWorkSheet : IDisposable
    {
		private readonly ExcelPackage package;
		private readonly ExcelWorksheet workSheet;
		private readonly Dictionary<string, int> columnIndices = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
		private readonly int headerRow = 1;

		private readonly string hyperLinkStyleName = "HyperLink";
		private ExcelNamedStyleXml hyperLinkStyle = null;

		public ExcelWorkSheet(string workSheetName, string [] headers)
        {
			package = new ExcelPackage();
			package.Workbook.Worksheets.Add(workSheetName);
			workSheet = package.Workbook.Worksheets[0];
			InsertRow(headerRow, headers);
		}

		public void Dispose()
		{
			package.Dispose();
		}

		public ExcelWorkSheet(string path, int workSheetIndex = 0)
        {
			var fileName = Path.GetFileName(path);
			using var stream = new FileStream(path, FileMode.Open);
			package = new ExcelPackage(stream);
			if (package.Workbook.Worksheets.Count <= workSheetIndex)
				throw new Exception($"Worksheet index is invalid: {workSheetIndex} for file: {fileName}");
			workSheet = package.Workbook.Worksheets[workSheetIndex];
			if (workSheet.Dimension == null || workSheet.Dimension.Rows < 1)
				throw new Exception($"Worksheet is empty in file: {fileName}");
			for (int column = 1; column <= ColumnCount; column++)
			{
				var columnName = NullUtility.NullExists(workSheet.Cells[1, column].Value).Trim();
				if (columnName != string.Empty)
				{
					columnIndices[columnName] = column;
				}
			}
		}

		public ExcelWorkSheet(Stream stream, int workSheetIndex = 0)
		{
			package = new ExcelPackage(stream);
			if (package.Workbook.Worksheets.Count <= workSheetIndex)
				throw new Exception($"Worksheet index is invalid: {workSheetIndex} for stream");
			workSheet = package.Workbook.Worksheets[workSheetIndex];
			if (workSheet.Dimension == null || workSheet.Dimension.Rows < 1)
				throw new Exception($"Worksheet is empty in stream");
			for (int column = 1; column <= ColumnCount; column++)
			{
				var columnName = NullUtility.NullExists(workSheet.Cells[1, column].Value).Trim();
				if (columnName != string.Empty)
				{
					columnIndices[columnName] = column;
				}
			}
		}

		public ExcelWorkSheet(Stream stream, string[] columnHeaders, int workSheetIndex = 0)
        {
			package = new ExcelPackage(stream);
			if (package.Workbook.Worksheets.Count <= workSheetIndex)
				throw new Exception($"Worksheet index is invalid: {workSheetIndex} for stream");
			workSheet = package.Workbook.Worksheets[workSheetIndex];
			if (workSheet.Dimension == null || workSheet.Dimension.Rows < 1)
				throw new Exception($"Worksheet is empty in stream");
			for (headerRow = 1; headerRow <= RowCount; headerRow++)
			{
				var columnName = NullUtility.NullExists(workSheet.Cells[headerRow, 1].Value);
				if (columnName == columnHeaders[0])
					break;
			}
			if (headerRow <= RowCount)
			{
				for (int column = 1; column <= ColumnCount; column++)
				{
					var columnName = NullUtility.NullExists(workSheet.Cells[headerRow, column].Value).Trim();
					if (columnName != string.Empty)
					{
						columnIndices[columnName] = column;
					}
				}
			}
			else
			{
				headerRow = 0;
				for (int column = 1; column <= columnHeaders.Length; column++)
				{
					columnIndices[columnHeaders[column-1]] = column;
				}
			}            
		}

		public async Task WriteAsync(Stream stream)
        {
			package.Workbook.Calculate();
			workSheet.Cells.AutoFitColumns();
			await package.SaveAsAsync(stream);
        }

		public async Task<byte[]> GetContentsAsync()
        {
			using (var stream = new MemoryStream())
			{
				await WriteAsync(stream);
				stream.Close();
				return stream.ToArray();
			}
		}

		public int HeaderRow
        {
			get
			{
				return headerRow;
			}
        }

		public int RowCount
		{
			get
			{
				return workSheet?.Dimension?.End == null ? 0 : workSheet.Dimension.End.Row;
			}
		}

		public int ColumnCount
        {
			get
			{
				return workSheet?.Dimension?.End == null ? 0 : workSheet.Dimension.End.Column;
			}
        }

		public void InsertRow(int row, string [] values, eDataTypes [] dataTypes = null)
        {
			workSheet.Cells[row,1].Insert(eShiftTypeInsert.EntireRow);
			if (dataTypes == null)
            {
				dataTypes = new eDataTypes[values.Length];
				for (int i = 0; i < dataTypes.Length; i++)
					dataTypes[i] = eDataTypes.String;
            }

			workSheet.Cells[row,1].LoadFromText(string.Join("|", values), 
				new ExcelTextFormat() { Delimiter = '|', DataTypes = dataTypes });
		}

		public void InsertFormula(string cell, string formula)
        {
			workSheet.Cells[cell].Formula = formula;
        }

		public void InsertHyperlink(string cell, string displayName, string hyperlink)
        {
			if (hyperLinkStyle == null)
            {
				// Create named style for Hyperlinks.
				hyperLinkStyle = package.Workbook.Styles.CreateNamedStyle(hyperLinkStyleName);
				hyperLinkStyle.Style.Font.UnderLine = true;
				hyperLinkStyle.Style.Font.Color.SetColor(Color.Blue);

			}
			workSheet.Cells[cell].Formula = $"HYPERLINK(\"{hyperlink}\",\"{displayName}\")";
			workSheet.Cells[cell].StyleName = hyperLinkStyleName;
        }

        public string [] GetRow(int row)
        {
			var values = new string[ColumnCount];
			for (int column = 1; column < ColumnCount; column++)
				values[column - 1] = NullUtility.NullExists(workSheet.Cells[row, column].Value).Trim();
			return values;
		}

		public string GetStringValue(int row, string columnName, string defaultValue = "")
        {
			var value = defaultValue;
			if (columnIndices.TryGetValue(columnName, out var column) || columnIndices.TryGetValue(columnName.Replace(" ", ""), out column))
				value = NullUtility.NullExists(workSheet.Cells[row, column].Value).Trim();
			return value;
        }

		public int GetIntValue(int row, string columnName, int defaultValue = 0)
        {
			if (!Int32.TryParse(GetStringValue(row, columnName), out var value))
				value = defaultValue;
			return value;
        }
		public string GetFormattedIntValue(int row, string columnName, int width)
        {
			if (!Int32.TryParse(GetStringValue(row, columnName), out var value))
				return string.Empty;
			return value.ToString($"D{width}");
        }

		public DateTime GetDateValue(int row, string columnName, DateTime defaultValue = new DateTime())
        {
			if (!DateTime.TryParse(GetStringValue(row, columnName), out var value))
				value = defaultValue;
			return value;
        }
    }
}
