using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PackageTracker.Domain.Utilities
{
	public static class StringHelper
	{
		public static bool Exists(string input)
		{
			return !string.IsNullOrWhiteSpace(input);
		}

		public static bool DoesNotExist(string input)
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

		public static decimal ParseIntoDecimalOrReturnZero(string stringInput)
		{
			decimal.TryParse(stringInput, out var parsedDecimal);
			return parsedDecimal;
		}

		public static string ParseNumbersFromText(string input)
		{
			return Regex.Match(input, @"\d+").Value;
		}

		/// <summary>
		/// Encode string to Base64
		/// </summary>
		/// <param name="plainText"></param>
		/// <returns></returns>
		public static string ToBase64(this string plainText)
		{
			var plainTextBytes = Encoding.ASCII.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}

		/// <summary>
		/// Decode base64 to string
		/// </summary>
		/// <param name="base64EncodedData"></param>
		/// <returns></returns>
		public static string FromBase64(this string base64EncodedData)
		{
			var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
			return Encoding.UTF8.GetString(base64EncodedBytes);
		}

		public static string Truncate(string val, int maxLength)
        {
			if (val == null || val.Length <= maxLength)
				return val;
			return val.Substring(0, maxLength);
        }

		public static string ToStringList(IEnumerable<string> list, string delimiter)
		{
			var sb = new StringBuilder();
			string separator = string.Empty;
			foreach (var value in list)
			{
				sb.Append(separator).Append(value);
				separator = delimiter;
			}
			return sb.ToString();
		}
	}
}
