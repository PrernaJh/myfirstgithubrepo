using PackageTracker.Data.Constants;
using System;
using System.Linq;

namespace PackageTracker.Domain.Utilities
{
	public static class BarcodeUtility
	{
		public static int GenerateUspsCheckDigit(string barcodeInput, int multiplier)
		{
			var chars = barcodeInput.ToCharArray().ToList();
			var evenNumbers = 0;
			var oddNumbers = 0;

			for (int i = 0; i < chars.Count; i++)
			{
				// counts from the right
				if (i % 2 == 0)
				{
					evenNumbers += int.Parse(chars[i].ToString());
				}
				else
				{
					oddNumbers += int.Parse(chars[i].ToString());
				}
			}

			var total = (evenNumbers * multiplier) + oddNumbers;
			var checkDigit = 10 - (total % 10);

			if (checkDigit == 10)
			{
				checkDigit = 0;
			}

			return checkDigit;
		}

		public static int GenerateOnTracCheckDigit(string barcodeInput)
		{
			var chars = barcodeInput.ToCharArray().ToList();
			var evenNumbers = 0;
			var oddNumbers = 0;

			for (int i = 0; i < chars.Count; i++)
			{
				// counts from the left
				if (i % 2 == 0)
				{
					oddNumbers += int.Parse(chars[i].ToString());
				}
				else
				{
					evenNumbers += int.Parse(chars[i].ToString());
				}
			}

			var total = (evenNumbers * 2) + oddNumbers; // ontrac multiplier is 2
			var checkDigit = 10 - (total % 10);

			if (checkDigit == 10)
			{
				checkDigit = 0;
			}

			return checkDigit;
		}

		public static string GenerateUspsSerialNumberByMidLength(int sequenceNumber, int midLength)
		{
			var paddedSequenceNumber = string.Empty;

			if (midLength == 6)
			{
				paddedSequenceNumber = sequenceNumber.ToString().PadLeft(14, '0');
			}
			else if (midLength == 9)
			{
				paddedSequenceNumber = sequenceNumber.ToString().PadLeft(11, '0');
			}
			return paddedSequenceNumber;
		}

		public static string GeneratePmodContainerSerialNumberByMidLength(int sequenceNumber, int midLength)
		{
			var paddedSequenceNumber = string.Empty;

			if (midLength == 6)
			{
				paddedSequenceNumber = sequenceNumber.ToString().PadLeft(12, '0');
			}
			else if (midLength == 9)
			{
				paddedSequenceNumber = sequenceNumber.ToString().PadLeft(9, '0');
			}
			return paddedSequenceNumber;
		}
	}
}
