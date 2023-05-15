using System;
using System.Collections.Generic;
using System.Text;

namespace ManifestBuilder
{
    class EvsRecordBuilder
	{
		public static string BuildRecordString(UspsEvsRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.D1DetailRecordID);
			recordBuilder.Append(delimiter + record.D1TrackingNumber);
			recordBuilder.Append(delimiter + record.D1ClassOfMail);
			recordBuilder.Append(delimiter + record.D1ServiceTypeCode);
			recordBuilder.Append(delimiter + record.D1BarcodeConstructCode);
			recordBuilder.Append(delimiter + record.D1DestinationZIPCode);
			recordBuilder.Append(delimiter + record.D1DestinationZIPplus4);
			recordBuilder.Append(delimiter + record.D1DestinationFacilityType);
			recordBuilder.Append(delimiter + record.D1DestinationCountryCode);
			recordBuilder.Append(delimiter + record.D1ForeignPostalCode);
			recordBuilder.Append(delimiter + record.D1CarrierRoute);
			recordBuilder.Append(delimiter + record.D1LogisticsManagerMailerID);
			recordBuilder.Append(delimiter + record.D1MailOwnerMailerID);
			recordBuilder.Append(delimiter + record.D1ContainerID1);
			recordBuilder.Append(delimiter + record.D1ContainerType1);
			recordBuilder.Append(delimiter + record.D1ContainerID2);
			recordBuilder.Append(delimiter + record.D1ContainerType2);
			recordBuilder.Append(delimiter + record.D1ContainerID3);
			recordBuilder.Append(delimiter + record.D1ContainerType3);
			recordBuilder.Append(delimiter + record.D1CRID);
			recordBuilder.Append(delimiter + record.D1CustomerReferenceNumber1);
			recordBuilder.Append(delimiter + record.D1FASTReservationNumber);
			recordBuilder.Append(delimiter + record.D1FASTScheduledInductionDate);
			recordBuilder.Append(delimiter + record.D1FASTScheduledInductionTime);
			recordBuilder.Append(delimiter + record.D1PaymentAccountNumber);
			recordBuilder.Append(delimiter + record.D1MethodofPayment);
			recordBuilder.Append(delimiter + record.D1PostOfficeofAccountZIPCode);
			recordBuilder.Append(delimiter + record.D1MeterSerialNumber);
			recordBuilder.Append(delimiter + record.D1ChargebackCode);
			recordBuilder.Append(delimiter + record.D1Postage);
			recordBuilder.Append(delimiter + record.D1PostageType);
			recordBuilder.Append(delimiter + record.D1CustomizedShippingServicesContractsNumber);
			recordBuilder.Append(delimiter + record.D1CustomizedShippingServicesContractsProductID);
			recordBuilder.Append(delimiter + record.D1UnitofMeasureCode);
			recordBuilder.Append(delimiter + record.D1Weight);
			recordBuilder.Append(delimiter + record.D1ProcessingCategory);
			recordBuilder.Append(delimiter + record.D1RateIndicator);
			recordBuilder.Append(delimiter + record.D1DestinationRateIndicator);
			recordBuilder.Append(delimiter + record.D1DomesticZone);
			recordBuilder.Append(delimiter + record.D1Length);
			recordBuilder.Append(delimiter + record.D1Width);
			recordBuilder.Append(delimiter + record.D1Height);
			recordBuilder.Append(delimiter + record.D1DimensionalWeight);
			recordBuilder.Append(delimiter + record.D1ExtraServiceCode1stService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceFee1stService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceCode2ndService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceFee2ndService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceCode3rdService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceFee3rdService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceCode4thService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceFee4thService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceCode5thService);
			recordBuilder.Append(delimiter + record.D1ExtraServiceFee5thService);
			recordBuilder.Append(delimiter + record.D1ValueofArticle);
			recordBuilder.Append(delimiter + record.D1CODAmountDueSender);
			recordBuilder.Append(delimiter + record.D1HandlingCharge);
			recordBuilder.Append(delimiter + record.D1SurchargeType);
			recordBuilder.Append(delimiter + record.D1SurchargeAmount);
			recordBuilder.Append(delimiter + record.D1DiscountType);
			recordBuilder.Append(delimiter + record.D1DiscountAmount);
			recordBuilder.Append(delimiter + record.D1NonIncidentalEnclosureRateIndicator);
			recordBuilder.Append(delimiter + record.D1NonIncidentalEnclosureClass);
			recordBuilder.Append(delimiter + record.D1NonIncidentalEnclosurePostage);
			recordBuilder.Append(delimiter + record.D1NonIncidentalEnclosureWeight);
			recordBuilder.Append(delimiter + record.D1NonIncidentalEnclosureProcessingCategory);
			recordBuilder.Append(delimiter + record.D1PostalRoutingBarcode);
			recordBuilder.Append(delimiter + record.D1OpenandDistributeContentsIndicator);
			recordBuilder.Append(delimiter + record.D1POBoxIndicator);
			recordBuilder.Append(delimiter + record.D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference);
			recordBuilder.Append(delimiter + record.D1DeliveryOptionIndicator);
			recordBuilder.Append(delimiter + record.D1DestinationDeliveryPoint);
			recordBuilder.Append(delimiter + record.D1RemovalIndicator);
			recordBuilder.Append(delimiter + record.D1TrackingIndicator);
			recordBuilder.Append(delimiter + record.D1OriginalLabelTrackingNumberBarcodeConstructCode);
			recordBuilder.Append(delimiter + record.D1OriginalTrackingNumber);
			recordBuilder.Append(delimiter + record.D1CustomerReferenceNumber2);
			recordBuilder.Append(delimiter + record.D1RecipientNameDestination);
			recordBuilder.Append(delimiter + record.D1DeliveryAddress);
			recordBuilder.Append(delimiter + record.D1AncillaryServiceEndorsement);
			recordBuilder.Append(delimiter + record.D1AddressServiceParticipantCod);
			recordBuilder.Append(delimiter + record.D1KeyLine);
			recordBuilder.Append(delimiter + record.D1ReturnAddress);
			recordBuilder.Append(delimiter + record.D1ReturnAddressCity);
			recordBuilder.Append(delimiter + record.D1ReturnAddressState);
			recordBuilder.Append(delimiter + record.D1ReturnAddressZIPCode);
			recordBuilder.Append(delimiter + record.D1LogisticMailingFacilityCRID);
			recordBuilder.Append("\r\n");

			return recordBuilder.ToString();
		}

		public static string BuildContainerString(UspsEvsRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.C1ContainerRecordID);
			recordBuilder.Append(delimiter + record.C1ContainerID);
			recordBuilder.Append(delimiter + record.C1ContainerType);
			recordBuilder.Append(delimiter + record.C1ElectronicFileNumber);
			recordBuilder.Append(delimiter + record.C1DestinationZIPCode);
			recordBuilder.Append("\r\n");

			return recordBuilder.ToString();
		}

		public static string BuildHeaderString(UspsEvsRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.H1HeaderRecordID);
			recordBuilder.Append(delimiter + record.H1ElectronicFileNumber);
			recordBuilder.Append(delimiter + record.H1ElectronicFileType);
			recordBuilder.Append(delimiter + record.H1DateofMailing);
			recordBuilder.Append(delimiter + record.H1TimeofMailing);
			recordBuilder.Append(delimiter + record.H1EntryFacilityType);
			recordBuilder.Append(delimiter + record.H1EntryFacilityZIPCode);
			recordBuilder.Append(delimiter + record.H1EntryFacilityZIPplus4Code);
			recordBuilder.Append(delimiter + record.H1DirectEntryOriginCountryCode);
			recordBuilder.Append(delimiter + record.H1ShipmentFeeCode);
			recordBuilder.Append(delimiter + record.H1ExtraFeeforShipment);
			recordBuilder.Append(delimiter + record.H1ContainerizationIndicator);
			recordBuilder.Append(delimiter + record.H1USPSElectronicFileVersionNumber);
			recordBuilder.Append(delimiter + record.H1TransactionID);
			recordBuilder.Append(delimiter + record.H1SoftwareVendorCode);
			recordBuilder.Append(delimiter + record.H1SoftwareVendorProductVersionNumber);
			recordBuilder.Append(delimiter + record.H1FileRecordCount);
			recordBuilder.Append(delimiter + record.H1MailerID);
			recordBuilder.Append("\r\n");

			return recordBuilder.ToString();
		}
	}
}
