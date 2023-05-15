using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ManifestBuilder
{
    class UspsEvsFileProcessor
	{
		private static int GetSequenceNumber(CreateManifestRequest request, string mailOwnerMid)
        {
			var sequenceNumber = 0;
			if (request.EFNStartSequenceByMID.ContainsKey(request.Site?.SiteName))
			{
				if (request.EFNStartSequenceByMID[request.Site?.SiteName]++ > 999999998)
					request.EFNStartSequenceByMID[request.Site?.SiteName] = 1;
				sequenceNumber = request.EFNStartSequenceByMID[request.Site?.SiteName];
			}
			else
			{
				if (request.EFNStartSequenceByMID[mailOwnerMid]++ > 999999998)
					request.EFNStartSequenceByMID[mailOwnerMid] = 1;
				sequenceNumber = request.EFNStartSequenceByMID[mailOwnerMid];
			}
			return sequenceNumber;
		}
			
		public static CreateManifestResponse CreateUspsRecords(CreateManifestRequest request)
		{
			CreateManifestResponse response = new CreateManifestResponse();
			response.EvsRecords = new List<string>();

			bool successful = true;
			string message = string.Empty;

			try
			{
				if (request.Packages.Any())
                {
					// For backward compatibility populate parent fields ...
					foreach (var package in request.Packages)
                    {
						if (string.IsNullOrEmpty(package.ParentMailOwnerMid))
							package.ParentMailOwnerMid = package.UspsMailOwnerMid;
						if (string.IsNullOrEmpty(package.ParentMailOwnerCrid))
							package.ParentMailOwnerCrid = package.UspsMailOwnerCrid;
                    }

					if (request.IsForPmodContainers)
                    {
						var packagesCount = request.Packages.Count();
						var sequenceNumber = GetSequenceNumber(request, request.MailProducerMid);
						var EFN = EFNUtility.GetH1ElectronicFileNumber(request.MailProducerMid, sequenceNumber, request.Site?.EvsId);
						var site = request.Site;
						var headerRecord = new UspsEvsRecord
						{
							H1HeaderRecordID = "H1",
							H1ElectronicFileNumber = EFN,//validate
							H1ElectronicFileType = ShippingServiceFileConstants.PostageandTrackingFile,
							H1DateofMailing = request.MailDate.ToString("yyyyMMdd"),
							H1TimeofMailing = request.MailDate.ToString("HHmmss"),

							H1EntryFacilityType = " ",
							H1EntryFacilityZIPCode = site.Zip,
							H1EntryFacilityZIPplus4Code = string.Empty,
							H1DirectEntryOriginCountryCode = string.Empty,

							H1ShipmentFeeCode = string.Empty,
							H1ExtraFeeforShipment = "000000",
							H1ContainerizationIndicator = string.Empty,
							H1USPSElectronicFileVersionNumber = "020",
							H1TransactionID = DateTime.Now.ToString("yyyyMMdd") + "0001",//the last 4 digts need to be unique in a given day
							H1SoftwareVendorCode = "8150",
							H1SoftwareVendorProductVersionNumber = "1",
							H1FileRecordCount = AddLeadingZeros((packagesCount + 1).ToString()),
							H1MailerID = request.MailProducerMid
						};
						response.EvsRecords.Add(EvsRecordBuilder.BuildHeaderString(headerRecord));

						var sortedPackages = request.Packages.OrderBy(p => p.EntryZip).ThenBy(p => p.EntryFacilityType);
						foreach (var package in sortedPackages)
                        {
							(var classOfMail, var rateIndicator) = GetClassAndRate(package);
							var record = new UspsEvsRecord
							{
								D1DetailRecordID = "D1",
								D1TrackingNumber = package.TrackingNumber,
								D1ClassOfMail = classOfMail,
								D1ServiceTypeCode = package.TrackingNumber.Substring(10, 3),//need this.
								D1BarcodeConstructCode = package.MailerId.Length == 9 ? package.TrackingNumber.Length == 30 ? "C03" : "C02" : package.TrackingNumber.Length == 30 ? "C07" : "C06",
								D1DestinationZIPCode = package.Zip.Trim().Length >= 6 ? package.Zip.Substring(0, 5) : package.Zip,
								D1DestinationZIPplus4 = package.Zip.Trim().Length >= 9 ? package.Zip.Substring(package.Zip.Trim().Length - 4) : string.Empty,//need to standardize format.  Currently only one zip field with multiple formats.  Some 5 and some 9 digit with hyphen.
								D1DestinationFacilityType = string.Empty,
								D1DestinationCountryCode = string.Empty,
								D1ForeignPostalCode = string.Empty,
								D1CarrierRoute = string.Empty,
								D1LogisticsManagerMailerID = request.MailProducerMid,
								D1MailOwnerMailerID = package.ParentMailOwnerMid,
								D1ContainerID1 = package.ContainerId,
								D1ContainerType1 = string.Empty,
								D1ContainerID2 = string.Empty,
								D1ContainerType2 = string.Empty,
								D1ContainerID3 = string.Empty,
								D1ContainerType3 = string.Empty,
								D1CRID = package.ParentMailOwnerCrid,
								D1CustomerReferenceNumber1 = string.Empty,
								D1FASTReservationNumber = string.Empty,
								D1FASTScheduledInductionDate = "00000000",
								D1FASTScheduledInductionTime = "000000",

								D1PaymentAccountNumber = AddLeadingZerosForFormat(package.UspsPermitNo, 10),
								D1MethodofPayment = package.UspsPaymentMethod,
								D1PostOfficeofAccountZIPCode = package.UspsPermitNoZip,

								D1MeterSerialNumber = string.Empty,
								D1ChargebackCode = string.Empty,
								D1Postage = EvsTypeConverter.GetFormattedPostage(package.Cost.ToString()),
								D1PostageType = package.UspsPostageType,
								D1CustomizedShippingServicesContractsNumber = package.UspsCsscNo,
								D1CustomizedShippingServicesContractsProductID = package.UspsCsscProductNo,
								D1UnitofMeasureCode = ShippingServiceFileConstants.LBS,
								D1Weight = EvsTypeConverter.GetFormattedWeight((package.Weight).ToString()),

								D1ProcessingCategory = EvsTypeConverter.GetProcessingCategory(package.ProcessingCategory),
								D1RateIndicator = rateIndicator,
								D1DestinationRateIndicator = package.DestinationRateIndicator,

								D1DomesticZone =
										package.DestinationRateIndicator == "D" ||
										package.DestinationRateIndicator == "S" ||
										package.DestinationRateIndicator == "B"
									? "00"
									: package.Zone.ToString().PadLeft(2, '0'),
								D1Length = "00000",
								D1Width = "00000",
								D1Height = "00000",
								D1DimensionalWeight = "000000",
								D1ExtraServiceCode1stService = "430",
								D1ExtraServiceFee1stService = "000000",
								D1ExtraServiceCode2ndService = string.Empty,
								D1ExtraServiceFee2ndService = "000000",
								D1ExtraServiceCode3rdService = string.Empty,
								D1ExtraServiceFee3rdService = "000000",
								D1ExtraServiceCode4thService = string.Empty,
								D1ExtraServiceFee4thService = "000000",
								D1ExtraServiceCode5thService = string.Empty,
								D1ExtraServiceFee5thService = "000000",
								D1ValueofArticle = "0000000",
								D1CODAmountDueSender = "000000",
								D1HandlingCharge = "0000",
								D1SurchargeType = string.Empty,
								D1SurchargeAmount = "0000000",
								D1DiscountType = string.Empty,
								D1DiscountAmount = "0000000",
								D1NonIncidentalEnclosureRateIndicator = string.Empty,
								D1NonIncidentalEnclosureClass = string.Empty,
								D1NonIncidentalEnclosurePostage = "0000000",
								D1NonIncidentalEnclosureWeight = "000000000",
								D1NonIncidentalEnclosureProcessingCategory = string.Empty,
								D1PostalRoutingBarcode = ShippingServiceFileConstants.GS1128BARCODE,
								D1OpenandDistributeContentsIndicator = "EP",
								D1POBoxIndicator = package.IsPoBox ? "Y" : "N",//needed?  
								D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference = string.Empty,
								D1DeliveryOptionIndicator = "1",
								D1DestinationDeliveryPoint = string.Empty,//needed
								D1RemovalIndicator = string.Empty,
								D1TrackingIndicator = string.Empty,
								D1OriginalLabelTrackingNumberBarcodeConstructCode = string.Empty,
								D1OriginalTrackingNumber = string.Empty,
								D1CustomerReferenceNumber2 = string.Empty,
								D1RecipientNameDestination = package.RecipientName,
								D1DeliveryAddress = package.AddressLine1,
								D1AncillaryServiceEndorsement = string.Empty,
								D1AddressServiceParticipantCod = string.Empty,
								D1KeyLine = string.Empty,
								D1ReturnAddress = package.ReturnAddressLine1,
								D1ReturnAddressCity = package.ReturnCity,
								D1ReturnAddressState = package.ReturnState,
								D1ReturnAddressZIPCode = package.ReturnZip,
								D1LogisticMailingFacilityCRID = package.MailProducerCrid
							};
							response.EvsRecords.Add(EvsRecordBuilder.BuildRecordString(record));
						}
					}
					else
					{
						string lastEntryZip = "";
						EntryFacilityType lastEntryFacilityType = (EntryFacilityType)(-1); // Unknown
						string lastParentMailOwnerMid = "";
						string lastContainerId = "";
						var sortedPackages = request.Packages
							.OrderBy(p => p.EntryZip)
							.ThenBy(p => p.EntryFacilityType)
							.ThenBy(p => p.ParentMailOwnerMid)
							.ThenBy(p => p.ContainerId);
						List<ShippingContainer> createdContainers = new List<ShippingContainer>();
						string EFN = "";
						ShippingContainer container = null;
						var containerType = "";
						var containerId = "";

						foreach (var package in sortedPackages)
						{
							//write Header Record
							if (package.EntryZip != lastEntryZip
								|| package.EntryFacilityType != lastEntryFacilityType
								|| package.ParentMailOwnerMid != lastParentMailOwnerMid)
							{
								var sequenceNumber = GetSequenceNumber(request, package.ParentMailOwnerMid);
								EFN = EFNUtility.GetH1ElectronicFileNumber(package.ParentMailOwnerMid, sequenceNumber, request.Site?.EvsId);
								var packagesCount = request.Packages.Count(p =>
									p.EntryZip == package.EntryZip &&
									p.EntryFacilityType == package.EntryFacilityType &&
									p.ParentMailOwnerMid == package.ParentMailOwnerMid
								);
								var containersCount = request.Containers.Count(c =>
									request.Packages.Count(p =>
										p.EntryZip == package.EntryZip &&
										p.EntryFacilityType == package.EntryFacilityType &&
										p.ParentMailOwnerMid == package.ParentMailOwnerMid &&
										c.ContainerId == p.ContainerId) > 0
								);
								var headerRecord = new UspsEvsRecord
								{
									H1HeaderRecordID = "H1",
									H1ElectronicFileNumber = EFN,//validate
									H1ElectronicFileType = ShippingServiceFileConstants.PostageandTrackingFile,
									H1DateofMailing = request.MailDate.ToString("yyyyMMdd"),
									H1TimeofMailing = request.MailDate.ToString("HHmmss"),

									H1EntryFacilityType = EvsTypeConverter.GetEntryFacilityType(package.EntryFacilityType),
									H1EntryFacilityZIPCode = package.EntryZip.Substring(0, 5),//need entry site info
									H1EntryFacilityZIPplus4Code = package.EntryZip.Trim().Length >= 9 ? package.EntryZip.Trim().Substring(package.EntryZip.Trim().Length - 4) : string.Empty,
									H1DirectEntryOriginCountryCode = string.Empty,

									H1ShipmentFeeCode = string.Empty,
									H1ExtraFeeforShipment = "000000",
									H1ContainerizationIndicator = string.Empty,
									H1USPSElectronicFileVersionNumber = "020",
									H1TransactionID = DateTime.Now.ToString("yyyyMMdd") + "0001",//the last 4 digts need to be unique in a given day
									H1SoftwareVendorCode = "8150",
									H1SoftwareVendorProductVersionNumber = "1",
									//H1FileRecordCount = AddLeadingZeros((packagesCount + containersCount + 1).ToString()),
									H1FileRecordCount = AddLeadingZeros((packagesCount + 1).ToString()),
									H1MailerID = package.ParentMailOwnerMid
								};
								response.EvsRecords.Add(EvsRecordBuilder.BuildHeaderString(headerRecord));
							}

							if (package.EntryZip != lastEntryZip || package.EntryFacilityType != lastEntryFacilityType)
							{
								createdContainers.Clear();
							}

							lastEntryZip = package.EntryZip;
							lastEntryFacilityType = package.EntryFacilityType;
							lastParentMailOwnerMid = package.ParentMailOwnerMid;

							//write Container Record
							if (lastContainerId != package.ContainerId && !string.IsNullOrEmpty(package.ContainerId))
							{
								if (createdContainers.FindAll(c => c.ContainerId == package.ContainerId).Count == 0)
								{
									var filteredContainers = request.Containers.Where(c => c.ContainerId == package.ContainerId);
									if (filteredContainers.Count() > 0)
									{
										container = filteredContainers.First();
										containerType = EvsTypeConverter.GetContainerType(container);
										containerId = container.ContainerId;
										createdContainers.Add(container);
										var containerRecord = new UspsEvsRecord
										{
											C1ContainerRecordID = "C1",
											C1ContainerID = containerId,
											C1ContainerType = containerType,
											C1ElectronicFileNumber = EFN,
											C1DestinationZIPCode = lastEntryZip.Substring(0, 5)
										};
										//response.EvsRecords.Add(EvsRecordBuilder.BuildContainerString(containerRecord));
									}
									else
									{
										container = null;
										containerType = "";
										containerId = "";
									}
								}
								else
								{
									var filteredContainers = request.Containers.Where(c => c.ContainerId == package.ContainerId);
									if (filteredContainers.Count() > 0)
									{
										container = filteredContainers.First();
										containerType = EvsTypeConverter.GetContainerType(container);
										containerId = package.ContainerId;
									}
								}
							}

							if (string.IsNullOrEmpty(package.ContainerId))
							{
								container = null;
								containerType = "";
								containerId = "";
							}

							lastContainerId = package.ContainerId;

							(var classOfMail, var rateIndicator) = GetClassAndRate(package);
							var record = new UspsEvsRecord
							{
								D1DetailRecordID = "D1",
								D1TrackingNumber = package.TrackingNumber,
								D1ClassOfMail = classOfMail,
								D1ServiceTypeCode = package.TrackingNumber.Substring(10, 3),//need this.
								D1BarcodeConstructCode = package.MailerId.Length == 9 ? package.TrackingNumber.Length == 30 ? "C03" : "C02" : package.TrackingNumber.Length == 30 ? "C07" : "C06",
								D1DestinationZIPCode = package.Zip.Trim().Length >= 6 ? package.Zip.Substring(0, 5) : package.Zip,
								D1DestinationZIPplus4 = package.Zip.Trim().Length >= 9 ? package.Zip.Substring(package.Zip.Trim().Length - 4) : string.Empty,//need to standardize format.  Currently only one zip field with multiple formats.  Some 5 and some 9 digit with hyphen.
								D1DestinationFacilityType = string.Empty,
								D1DestinationCountryCode = string.Empty,
								D1ForeignPostalCode = string.Empty,
								D1CarrierRoute = string.Empty,
								D1LogisticsManagerMailerID = request.MailProducerMid,
								D1MailOwnerMailerID = package.UspsMailOwnerMid,
								D1ContainerID1 = container?.ShippingMethod == ServiceTypeConstants.UspsPmodBag || container?.ShippingMethod == ServiceTypeConstants.UspsPmodPallet ?
									container.CarrierBarcode : containerId,
								D1ContainerType1 = containerType,
								D1ContainerID2 = string.Empty,
								D1ContainerType2 = string.Empty,
								D1ContainerID3 = string.Empty,
								D1ContainerType3 = string.Empty,
								D1CRID = package.ParentMailOwnerCrid,
								D1CustomerReferenceNumber1 = string.Empty,
								D1FASTReservationNumber = string.Empty,
								D1FASTScheduledInductionDate = "00000000",
								D1FASTScheduledInductionTime = "000000",

								D1PaymentAccountNumber = AddLeadingZerosForFormat(package.UspsPermitNo, 10),
								D1MethodofPayment = package.UspsPaymentMethod,
								D1PostOfficeofAccountZIPCode = package.UspsPermitNoZip,

								D1MeterSerialNumber = string.Empty,
								D1ChargebackCode = string.Empty,
								D1Postage = EvsTypeConverter.GetFormattedPostage(package.Cost.ToString()),
								D1PostageType = package.UspsPostageType,
								D1CustomizedShippingServicesContractsNumber = package.UspsCsscNo,
								D1CustomizedShippingServicesContractsProductID = package.UspsCsscProductNo,
								D1UnitofMeasureCode = ShippingServiceFileConstants.LBS,
								D1Weight = EvsTypeConverter.GetFormattedWeight((package.Weight).ToString()),

								D1ProcessingCategory = EvsTypeConverter.GetProcessingCategory(package.ProcessingCategory),
								D1RateIndicator = rateIndicator,
								D1DestinationRateIndicator = package.DestinationRateIndicator,

								D1DomesticZone =
										package.DestinationRateIndicator == "D" ||
										package.DestinationRateIndicator == "S" ||
										package.DestinationRateIndicator == "B"
									? "00"
									: package.Zone.ToString().PadLeft(2, '0'),
								D1Length = "00000",
								D1Width = "00000",
								D1Height = "00000",
								D1DimensionalWeight = "000000",
								D1ExtraServiceCode1stService = package.TrackingNumber.Substring(10, 3) == "021" ? "921" : "920",
								D1ExtraServiceFee1stService = "000000",
								D1ExtraServiceCode2ndService = string.Empty,
								D1ExtraServiceFee2ndService = "000000",
								D1ExtraServiceCode3rdService = string.Empty,
								D1ExtraServiceFee3rdService = "000000",
								D1ExtraServiceCode4thService = string.Empty,
								D1ExtraServiceFee4thService = "000000",
								D1ExtraServiceCode5thService = string.Empty,
								D1ExtraServiceFee5thService = "000000",
								D1ValueofArticle = "0000000",
								D1CODAmountDueSender = "000000",
								D1HandlingCharge = "0000",
								D1SurchargeType = string.Empty,
								D1SurchargeAmount = "0000000",
								D1DiscountType = string.Empty,
								D1DiscountAmount = "0000000",
								D1NonIncidentalEnclosureRateIndicator = string.Empty,
								D1NonIncidentalEnclosureClass = string.Empty,
								D1NonIncidentalEnclosurePostage = "0000000",
								D1NonIncidentalEnclosureWeight = "000000000",
								D1NonIncidentalEnclosureProcessingCategory = string.Empty,
								D1PostalRoutingBarcode = ShippingServiceFileConstants.GS1128BARCODE,
								D1OpenandDistributeContentsIndicator = string.Empty,
								D1POBoxIndicator = package.IsPoBox ? "Y" : "N",//needed?  
								D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference = string.Empty,
								D1DeliveryOptionIndicator = "1",
								D1DestinationDeliveryPoint = string.Empty,//needed
								D1RemovalIndicator = string.Empty,
								D1TrackingIndicator = string.Empty,
								D1OriginalLabelTrackingNumberBarcodeConstructCode = string.Empty,
								D1OriginalTrackingNumber = string.Empty,
								D1CustomerReferenceNumber2 = string.Empty,
								D1RecipientNameDestination = package.RecipientName,
								D1DeliveryAddress = package.AddressLine1,
								D1AncillaryServiceEndorsement = string.Empty,
								D1AddressServiceParticipantCod = string.Empty,
								D1KeyLine = string.Empty,
								D1ReturnAddress = package.ReturnAddressLine1,
								D1ReturnAddressCity = package.ReturnCity,
								D1ReturnAddressState = package.ReturnState,
								D1ReturnAddressZIPCode = package.ReturnZip,
								D1LogisticMailingFacilityCRID = package.MailProducerCrid
							};
							response.EvsRecords.Add(EvsRecordBuilder.BuildRecordString(record));
						}
					}
				}
			}
			catch (Exception e)
			{
				successful = false;
				message = e.Message;
				message += e.StackTrace;
			}

			response.EFNEndSequenceByMID = request.EFNStartSequenceByMID;
			response.IsSuccessful = successful;
			response.errorMessage = message;

			return response;
		}

		private static (string classOfMail, string rateIndicator) GetClassAndRate(Package package)
        {
			string classOfMail = string.Empty;
			string rateIndicator = ShippingServiceFileConstants.SinglePiece;
			switch (package.ServiceType)
			{
				case ServiceType.UspsFirstClass:
					classOfMail = ShippingServiceFileConstants.UspsFirstClass;
					break;
				case ServiceType.UspsPriority:
					classOfMail = ShippingServiceFileConstants.UspsPriority;
					break;
				case ServiceType.UspsPriorityExpress:
					classOfMail = ShippingServiceFileConstants.UspsPriority;
					break;
				case ServiceType.UspsPSLW:
					classOfMail = ShippingServiceFileConstants.UspsLightWeight;
					rateIndicator = EvsTypeConverter.GetRateIndicator(package);
					break;
				case ServiceType.BPM:
					classOfMail = ShippingServiceFileConstants.UspsBPM;
					rateIndicator = EvsTypeConverter.GetRateIndicator(package);
					break;
				case ServiceType.UspsParcelSelect:
					classOfMail = ShippingServiceFileConstants.UspsParcelSelect;
					rateIndicator = EvsTypeConverter.GetRateIndicator(package);
					break;
				case ServiceType.UspsPmod:
					classOfMail = ShippingServiceFileConstants.UspsPriority;
					rateIndicator = EvsTypeConverter.GetRateIndicator(package);
					break;

				case ServiceType.UspsPmodBag:
					classOfMail = ShippingServiceFileConstants.UspsPriority;
					break;
				case ServiceType.UspsPmodPallet:
					classOfMail = ShippingServiceFileConstants.UspsPriority;
					rateIndicator = ShippingServiceFileConstants.Pallet;
					break;
			}
			return (classOfMail, rateIndicator);
		}

		private static string AddLeadingZeros(string packagesCount)
		{
			int length = packagesCount.Length + 1;
			if (length < 9)
			{
				packagesCount = packagesCount.PadLeft(9, '0');
			}
			return packagesCount.ToString();
		}

		private static string AddLeadingZerosForFormat(string str, int definedLength)
		{
			int length = str.Length;
			if (length < definedLength)
			{
				str = str.PadLeft(definedLength, '0');
			}
			return str.ToString();
		}

		private static string GetFormattedWeight(string weightWithDecimal)
		{
			string[] splitWeight = weightWithDecimal.Split('.');
			string wholePart = splitWeight[0];
			string decimalPart = splitWeight.Length == 2 ? splitWeight[1] : "0000";

			wholePart = wholePart.PadLeft(5, '0');
			decimalPart = decimalPart.PadRight(4, '0').Substring(0, 4);

			return wholePart + decimalPart;
		}

		private static string GetFormattedPostage(string postageWithDecimal)
		{
			string[] splitPostage = postageWithDecimal.Split('.');
			string wholePart = splitPostage[0];
			string decimalPart = splitPostage.Length == 2 ? splitPostage[1] : "000";

			wholePart = wholePart.PadLeft(4, '0');
			decimalPart = decimalPart.PadRight(3, '0').Substring(0, 3);

			return wholePart + decimalPart;
		}
	}
}
