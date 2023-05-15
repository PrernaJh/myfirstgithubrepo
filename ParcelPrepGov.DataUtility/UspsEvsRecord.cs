﻿namespace ParcelPrepGov.DataUtility
{
	public class UspsEvsRecord
	{
		public string TrackingNumber { get; set; }
		public string ShipmentType { get; set; }
		public string PickupDate { get; set; }
		public string ShipReferenceNumber { get; set; }
		public string ShipperAccount { get; set; }
		public string H1HeaderRecordID { get; set; }
		public string H1ElectronicFileNumber { get; set; }
		public string H1ElectronicFileType { get; set; }
		public string H1DateofMailing { get; set; }
		public string H1TimeofMailing { get; set; }
		public string H1EntryFacilityType { get; set; }
		public string H1EntryFacilityZIPCode { get; set; }
		public string H1EntryFacilityZIPplus4Code { get; set; }
		public string H1DirectEntryOriginCountryCode { get; set; }
		public string H1ShipmentFeeCode { get; set; }
		public string H1ExtraFeeforShipment { get; set; }
		public string H1ContainerizationIndicator { get; set; }
		public string H1USPSElectronicFileVersionNumber { get; set; }
		public string H1TransactionID { get; set; }
		public string H1SoftwareVendorCode { get; set; }
		public string H1SoftwareVendorProductVersionNumber { get; set; }
		public string H1FileRecordCount { get; set; }
		public string H1MailerID { get; set; }
		public string C1ContainerRecordID { get; set; }
		public string C1ContainerID { get; set; }
		public string C1ContainerType { get; set; }
		public string C1ElectronicFileNumber { get; set; }
		public string C1DestinationZIPCode { get; set; }
		public string D1DetailRecordID { get; set; }
		public string D1TrackingNumber { get; set; }
		public string D1ClassOfMail { get; set; }
		public string D1ServiceTypeCode { get; set; }
		public string D1BarcodeConstructCode { get; set; }
		public string D1DestinationZIPCode { get; set; }
		public string D1DestinationZIPplus4 { get; set; }
		public string D1DestinationFacilityType { get; set; }
		public string D1DestinationCountryCode { get; set; }
		public string D1ForeignPostalCode { get; set; }
		public string D1CarrierRoute { get; set; }
		public string D1LogisticsManagerMailerID { get; set; }
		public string D1MailOwnerMailerID { get; set; }
		public string D1ContainerID1 { get; set; }
		public string D1ContainerType1 { get; set; }
		public string D1ContainerID2 { get; set; }
		public string D1ContainerType2 { get; set; }
		public string D1ContainerID3 { get; set; }
		public string D1ContainerType3 { get; set; }
		public string D1CRID { get; set; }
		public string D1CustomerReferenceNumber1 { get; set; }
		public string D1FASTReservationNumber { get; set; }
		public string D1FASTScheduledInductionDate { get; set; }
		public string D1FASTScheduledInductionTime { get; set; }
		public string D1PaymentAccountNumber { get; set; }
		public string D1MethodofPayment { get; set; }
		public string D1PostOfficeofAccountZIPCode { get; set; }
		public string D1MeterSerialNumber { get; set; }
		public string D1ChargebackCode { get; set; }
		public string D1Postage { get; set; }
		public string D1PostageType { get; set; }
		public string D1CustomizedShippingServicesContractsNumber { get; set; }
		public string D1CustomizedShippingServicesContractsProductID { get; set; }
		public string D1UnitofMeasureCode { get; set; }
		public string D1Weight { get; set; }
		public string D1ProcessingCategory { get; set; }
		public string D1RateIndicator { get; set; }
		public string D1DestinationRateIndicator { get; set; }
		public string D1DomesticZone { get; set; }
		public string D1Length { get; set; }
		public string D1Width { get; set; }
		public string D1Height { get; set; }
		public string D1DimensionalWeight { get; set; }
		public string D1ExtraServiceCode1stService { get; set; }
		public string D1ExtraServiceFee1stService { get; set; }
		public string D1ExtraServiceCode2ndService { get; set; }
		public string D1ExtraServiceFee2ndService { get; set; }
		public string D1ExtraServiceCode3rdService { get; set; }
		public string D1ExtraServiceFee3rdService { get; set; }
		public string D1ExtraServiceCode4thService { get; set; }
		public string D1ExtraServiceFee4thService { get; set; }
		public string D1ExtraServiceCode5thService { get; set; }
		public string D1ExtraServiceFee5thService { get; set; }
		public string D1ValueofArticle { get; set; }
		public string D1CODAmountDueSender { get; set; }
		public string D1HandlingCharge { get; set; }
		public string D1SurchargeType { get; set; }
		public string D1SurchargeAmount { get; set; }
		public string D1DiscountType { get; set; }
		public string D1DiscountAmount { get; set; }
		public string D1NonIncidentalEnclosureRateIndicator { get; set; }
		public string D1NonIncidentalEnclosureClass { get; set; }
		public string D1NonIncidentalEnclosurePostage { get; set; }
		public string D1NonIncidentalEnclosureWeight { get; set; }
		public string D1NonIncidentalEnclosureProcessingCategory { get; set; }
		public string D1PostalRoutingBarcode { get; set; }
		public string D1OpenandDistributeContentsIndicator { get; set; }
		public string D1POBoxIndicator { get; set; }
		public string D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference { get; set; }
		public string D1DeliveryOptionIndicator { get; set; }
		public string D1DestinationDeliveryPoint { get; set; }
		public string D1RemovalIndicator { get; set; }
		public string D1TrackingIndicator { get; set; }
		public string D1OriginalLabelTrackingNumberBarcodeConstructCode { get; set; }
		public string D1OriginalTrackingNumber { get; set; }
		public string D1CustomerReferenceNumber2 { get; set; }
		public string D1RecipientNameDestination { get; set; }
		public string D1DeliveryAddress { get; set; }
		public string D1AncillaryServiceEndorsement { get; set; }
		public string D1AddressServiceParticipantCod { get; set; }
		public string D1KeyLine { get; set; }
		public string D1ReturnAddress { get; set; }
		public string D1ReturnAddressCity { get; set; }
		public string D1ReturnAddressState { get; set; }
		public string D1ReturnAddressZIPCode { get; set; }
		public string D1LogisticMailingFacilityCRID { get; set; }
	}
}

