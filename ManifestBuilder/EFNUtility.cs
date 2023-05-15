using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ManifestBuilder
{
    class EFNUtility
	{
		public static string PadSerialNumber(int length, int serialNumber, string evsId)
        {
			return string.IsNullOrEmpty(evsId)
				? serialNumber.ToString().PadLeft(length, '0').Substring(0, length)
				: evsId.Substring(0, 1) + serialNumber.ToString().PadLeft(length-1, '0').Substring(0, length-1);
        }

		public static string GetH1ElectronicFileNumber(string mailerid, int serialNumber, string evsId)
		{
			string stc = "750";
			string AI = BarcodeConstants.ChannelApplicationIdentifierSixDigitMid;
			string paddedSerial = "";
			if (mailerid.Length == 6)
			{
				paddedSerial = PadSerialNumber(10, serialNumber, evsId);
			}
			else if (mailerid.Length == 9)
			{
				paddedSerial = PadSerialNumber(7, serialNumber, evsId);
				AI = BarcodeConstants.ChannelApplicationIdentifierNineDigitMid;
			}

			return AI + stc + mailerid + paddedSerial + GetCheckDigit(AI + stc + mailerid + paddedSerial);
		}

		public static string GetCheckDigit(string EFN)
		{
			List<char> chars = EFN.Reverse().ToList();
			int Sum1 = 0;
			int Sum2 = 0;
			string checkDigit;

			for (int i = 0; i < chars.Count; i++)
			{
				if (i % 2 == 0)
				{
					Sum1 += int.Parse(chars[i].ToString());
				}
				else
				{
					Sum2 += int.Parse(chars[i].ToString());
				}
			}

			Sum1 = Sum1 * 3;

			checkDigit = (10 - ((Sum1 + Sum2) % 10)).ToString();
			if (checkDigit == "10")
			{
				checkDigit = "0";
			}

			return checkDigit;
		}
	}
}
