using Microsoft.Azure.Cosmos;
using System;
using System.Text.RegularExpressions;

namespace PackageTracker.Data.Utilities
{
	public static class PartitionKeyUtility
	{
		private const int PartitionKeyLength = 6;

		public static PartitionKey GeneratePartitionKeyFromString(string partitionKeyString)
		{
			var keyValue = GeneratePartitionKeyLiteralString(partitionKeyString);

			return new PartitionKey(keyValue);
		}

		public static PartitionKey GenerateConstantLengthPartitionKey(string input)
		{
			var keyValue = GenerateConstantLengthPartitionKeyString(input);
			return new PartitionKey(keyValue);
		}

		public static string GenerateConstantLengthPartitionKeyString(string input)
		{
			if (StringUtility.HasValue(input))
			{
				if (input.Length > PartitionKeyLength)
				{
					return input.Substring(input.Length - PartitionKeyLength, PartitionKeyLength);
				}
				else
				{
					return input;
				}
			}
			else
			{
				return string.Empty;
			}
		}

        public static string GeneratePartitionKeyLiteralString(string inputString)
		{
			var keyValue = string.Empty;
			if (StringUtility.HasValue(inputString))
			{
				keyValue = inputString;
			}
			return keyValue;
		}

		public static string GenerateDefaultPartitionKeyString()
		{
			return "none";
		}

		public static string GeneratePackagePartitionKeyString(string packageId)
		{
			if (packageId.Length == 32) // 32 digit CMOP ASN
			{
				var orderNumberLastDigit = packageId.Substring(21, 1);
				var zip = packageId.Substring(27);
				int.TryParse(orderNumberLastDigit, out var orderLastOne);
				int.TryParse(zip, out var zipInt);

				var partitionInt = (orderLastOne + 1) * zipInt;
				return partitionInt.ToString();
			}
			else if (packageId.Length == 50 || packageId.Length == 51) // New format CMOP ASN
			{
				var orderNumberLastDigit = packageId.Substring(33, 1);
				var zip = packageId.Substring(43, 5);
				int.TryParse(orderNumberLastDigit, out var orderLastOne);
				int.TryParse(zip, out var zipInt);

				var partitionInt = (orderLastOne + 1) * zipInt;
				return partitionInt.ToString();
			}
            else  // 33 digit CMOP ASNs and non-CMOP ASNs
            {
				var digits = string.Join("", Regex.Split(packageId, @"\D+")); // Remove non-digits then truncate to last 16 digits.
				return digits.Length <= 16 ? digits : digits.Substring(digits.Length - 16);
			}		
		}

		public static string GenerateEndOfDayPartitionKeyString(string siteName, DateTime manifestDate)
		{
			return $"{siteName}{manifestDate.Date:yyyyMMdd}";     
		}

	}
}
