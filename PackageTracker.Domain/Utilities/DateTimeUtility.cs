using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
    public static class DateTimeUtility
    {
        /// <summary>
        /// Use domain agreed upon date time format for nullable date
        /// </summary>        
        /// <returns>Empty when date isn't defined, and MM/DD/YYYY hh:mm tt otherwise</returns>
        public static string GetMonthDateYearWithAmPm(this DateTime? date)
        {
            string formattedDate;
            if (date.HasValue && date.Value == DateTime.MinValue)
            {
                formattedDate = string.Empty;
            }
            else
            {
                formattedDate = date.HasValue ? date.Value.ToString("MM/dd/yyyy hh:mm tt") : string.Empty;
            }

            return formattedDate;            
        }

        /// <summary>
        /// Use domain agreed upon date format for nullable date
        /// </summary>        
        /// <returns>Empty when date isn't defined, and MM/DD/YYYY otherwise</returns>
        public static string GetMonthDateYearOnly(this DateTime? date)
        {
            string formattedDate;
            if (date.HasValue && date.Value == DateTime.MinValue)
            {
                formattedDate = string.Empty;
            }
            else
            {
                formattedDate = date.HasValue ? date.Value.ToString("MM/dd/yyyy") : string.Empty;
            }

            return formattedDate;
        }
    }
}
