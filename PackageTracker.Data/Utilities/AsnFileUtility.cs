using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PackageTracker.Data.Utilities
{
    public static class AsnFileUtility
    {
        public static bool MatchFileName(string fileMatchString, string fileName)
        {
            // Match input file for subclient e.g. SubClient.AsnFileTrigger == "FILENAME(1,3)=762", fileName = "0762_CMOP_TEST_6-20"
            var match = Regex.IsMatch(fileMatchString, "FILENAME[(][0-9]+,[0-9]+[)]=[0-9]");
            if (match)
            {
                var parts = fileMatchString.Replace("(", ",").Replace(")=", ",").Split(",");
                var start = Int32.Parse(parts[1]);
                var len = Int32.Parse(parts[2]);
                return fileName.Substring(start, len) == parts[3];
            }
            else if (fileMatchString == "FOLDER")
            {
                return true;
            }
            return false;
        }

        // Format output file string, e.g. "EOD_RETURNASN_%SITE_NAME%_%SUBCLIENT_NAME%_%DATETIME:yyyyMMddHHmm%"
        public static string FormatExportFileName(SubClient subClient, string fileFormatString, DateTime localCreateDate)
        {
            var fileName = fileFormatString
                .Replace("%CLIENT_NAME%", subClient.ClientName)
                .Replace("%SUBCLIENT_NAME%", subClient.Name)
                .Replace("%SITE_NAME%", subClient.SiteName);
            var dateTime = "%DATETIME:";
            var n = fileName.IndexOf(dateTime);
            int m = fileName.IndexOf("%", n + dateTime.Length);
            if (n > 0 && m > n)
            {
                var dateString = fileName.Substring(n, m - n + 1);
                var parts = dateString.Replace("%", "").Split(":");
                if (parts.Length > 1)
                    fileName = fileName.Replace(dateString, localCreateDate.ToString(parts[1]));
                else
                    fileName = fileName.Replace(dateString, localCreateDate.ToString());
            }
            return fileName;
        }
    }
}
