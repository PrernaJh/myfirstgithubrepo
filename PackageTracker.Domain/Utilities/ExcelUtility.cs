using OfficeOpenXml;
using System.Linq;

namespace PackageTracker.Domain.Utilities
{
	public static class ExcelUtility
	{
		public static ExcelWorkSheet GenerateExcel(string[] headers, string[] rows, eDataTypes[] dataTypes, string fileName)
		{
			var excel = new ExcelWorkSheet(fileName, headers);

			for (int i = 0; i < rows.Count(); i++)
			{
				var rowStart = i + 2;
				var value = rows.GetValue(i).ToString();
				var valueArray = new string[] { value };
				excel.InsertRow(rowStart, valueArray, dataTypes);
			}

			return excel;
		}
	}
}
