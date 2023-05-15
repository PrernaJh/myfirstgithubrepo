using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Utilities
{
	public class StringUtility
	{
		public static bool HasValue(string input)
		{
			return !string.IsNullOrWhiteSpace(input);
		}

		public static bool HasNoValue(string input)
		{
			return string.IsNullOrWhiteSpace(input);
		}

		public static IEnumerable<ReadOnlyMemory<char>> SplitInParts(string stringInput, int partLength)
		{
			if (stringInput == null)
			{
				throw new ArgumentNullException(nameof(stringInput));
			}
			if (partLength <= 0)
			{
				throw new ArgumentException("Part length has to be positive.", nameof(partLength));
			}
			for (var i = 0; i < stringInput.Length; i += partLength)
			{
				yield return stringInput.AsMemory().Slice(i, Math.Min(partLength, stringInput.Length - i));
			}
		}

		public static string ConvertToBase64(string input)
		{
			if (input != null)
			{
				var plainTextBytes = Encoding.UTF8.GetBytes(input);
				return Convert.ToBase64String(plainTextBytes);
			}
			else
			{
				return string.Empty;
			}
		}

		public static decimal ParseIntoDecimalOrReturnZero(string stringInput)
		{
			decimal.TryParse(stringInput, out var parsedDecimal);
			return parsedDecimal;
		}

		public static string ConvertFromFromBase64(string input)
		{
			if (input != null)
			{
				var base64EncodedBytes = Convert.FromBase64String(input);
				return Encoding.UTF8.GetString(base64EncodedBytes);
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
