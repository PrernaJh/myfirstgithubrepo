using System;
using System.Collections.Generic;
using System.Text;

namespace ManifestBuilder
{
    class EvsTypeConverter
	{
		public static string GetContainerType(ShippingContainer container)
		{
			string containerType = "";
			if (container.ShippingCarrier == ShippingCarrier.Usps)
			{
				if (container.ContainerType == ContainerType.Pallet)
				{
					containerType = "OP";
				}
				else if (container.ContainerType == ContainerType.Bag)
				{
					containerType = "OT";
				}
			}
			else
			{
				if (container.ContainerType == ContainerType.Pallet)
				{
					containerType = "PT";
				}
				else if (container.ContainerType == ContainerType.Bag)
				{
					containerType = "SK";
				}
			}
			return containerType;
		}

		public static string GetRateIndicator(Package package)
		{
			if (package.DestinationRateIndicator == "D")
			{
				return ShippingServiceFileConstants.FiveDigitPrice;
			}
			else if (package.DestinationRateIndicator == "S")
			{
				return ShippingServiceFileConstants.ThreeDigitPrice;
			}		
			return ShippingServiceFileConstants.FiveDigitPrice;
		}

		public static string GetFormattedPostage(string postageWithDecimal)
		{
			string[] splitPostage = postageWithDecimal.Split('.');
			string wholePart = splitPostage[0];
			string decimalPart = splitPostage.Length == 2 ? splitPostage[1] : "000";

			wholePart = wholePart.PadLeft(4, '0');
			decimalPart = decimalPart.PadRight(3, '0').Substring(0, 3);

			return wholePart + decimalPart;
		}

		public static string GetFormattedWeight(string weightWithDecimal)
		{
			string[] splitWeight = weightWithDecimal.Split('.');
			string wholePart = splitWeight[0];
			string decimalPart = splitWeight.Length == 2 ? splitWeight[1] : "0000";

			wholePart = wholePart.PadLeft(5, '0');
			decimalPart = decimalPart.PadRight(4, '0').Substring(0, 4);

			return wholePart + decimalPart;
		}

		public static string GetEntryFacilityType(EntryFacilityType entryFacilityType)
        {
			switch (entryFacilityType)
			{
				case EntryFacilityType.ADC:
					return "A";
				case EntryFacilityType.NDC:
					return "B";
				case EntryFacilityType.SCF:
					return "S";
				case EntryFacilityType.DDU:
					return "D";
				case EntryFacilityType.ASF:
					return "F";
				case EntryFacilityType.ISC:
					return "I";
				default:
					return string.Empty;
			}
		}

		public static string GetProcessingCategory(ProcessingCategory processingCategory)
        {
			switch (processingCategory)
            {
				case ProcessingCategory.Cards:
					return "0";
				case ProcessingCategory.Letters:
					return "1";
				case ProcessingCategory.Flats:
					return "2";
				case ProcessingCategory.MachinableParcel:
					return "3";
				case ProcessingCategory.IrregularParcel:
					return "4";
				case ProcessingCategory.NonMachinableParcel:
					return "5";
				case ProcessingCategory.Catalogs:
					return "C";
				case ProcessingCategory.OpenAndDistribute:
					return "O";
				case ProcessingCategory.Returns:
					return "R";
				default:
					return string.Empty;
	}
        }
	}
}
