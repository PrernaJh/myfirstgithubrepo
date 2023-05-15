using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public class UspsEvsProcessor : IUspsEvsProcessor
	{
		private readonly ILogger<UspsEvsProcessor> logger;
		private readonly IPackageRepository packageRepository;
		private readonly IContainerRepository containerRepository;
		private readonly IBinRepository binRepository;
		private readonly ISequenceProcessor sequenceProcessor;
		private readonly ISubClientRepository subClientRepository;

		public UspsEvsProcessor(ILogger<UspsEvsProcessor> logger,
									IPackageRepository packageRepository,
									IContainerRepository containerRepository,
									IBinRepository binRepository,
									ISequenceProcessor sequenceProcessor,
									ISubClientRepository subClientRepository)
		{
			this.logger = logger;
			this.packageRepository = packageRepository;
			this.containerRepository = containerRepository;
			this.binRepository = binRepository;
			this.sequenceProcessor = sequenceProcessor;
			this.subClientRepository = subClientRepository;
		}

		public async Task<FileExportResponse> ExportUspsEvsFile(Site site, EndOfDayQueueMessage queueMessage)
		{
			var totalWatch = Stopwatch.StartNew();
			var dbReadWatch = Stopwatch.StartNew();
			var lookbacks = EndOfDayUtility.GetDefaultEndOfDayLookbacks();

			if (queueMessage.UseTargetDate)
			{
				lookbacks = EndOfDayUtility.GetLookbacksFromTargetDate(queueMessage.TargetDate, site.TimeZone);
			}

			var siteTimeNow = TimeZoneUtility.GetLocalTime(site.TimeZone);
			var lookbackStartDate = siteTimeNow.AddDays(-lookbacks.LookbackStart).Date;
			var lookbackEndDate = siteTimeNow.AddDays(-lookbacks.LookbackEnd).Date;

			var packages = await packageRepository.GetPackagesForUspsEvsFile(site.SiteName, lookbackStartDate, lookbackEndDate);
			var containers = await containerRepository.GetClosedContainersForPackageEvsFile(site.SiteName, lookbackStartDate, lookbackEndDate);

			dbReadWatch.Stop();
			var response = await CreateUspsRecords(site, packages, containers);
			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			var dbWriteWatch = Stopwatch.StartNew();
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
			response.IsSuccessful = true;
			return response;
		}

		public async Task<FileExportResponse> ExportUspsEvsFileForPMODContainers(Site site, EndOfDayQueueMessage queueMessage)
		{
			var totalWatch = Stopwatch.StartNew();
			var dbReadWatch = Stopwatch.StartNew();
			var lookbacks = EndOfDayUtility.GetDefaultEndOfDayLookbacks();

			if (queueMessage.UseTargetDate)
			{
				lookbacks = EndOfDayUtility.GetLookbacksFromTargetDate(queueMessage.TargetDate, site.TimeZone);
			}

			var siteTimeNow = TimeZoneUtility.GetLocalTime(site.TimeZone);
			var lookbackStartDate = siteTimeNow.AddDays(-lookbacks.LookbackStart).Date;
			var lookbackEndDate = siteTimeNow.AddDays(-lookbacks.LookbackEnd).Date;

			var containers = await containerRepository.GetContainersForUspsEvsFileAsync(site.SiteName, lookbackStartDate, lookbackEndDate);

			dbReadWatch.Stop();
			var response = await CreateUspsRecordsForContainers(site, containers);
			response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
			var dbWriteWatch = Stopwatch.StartNew();
			response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
			response.IsSuccessful = true;
			return response;
		}

		public async Task<FileExportResponse> CreateUspsRecordsForContainers(Site site, IEnumerable<ShippingContainer> containers)
		{
			var response = new FileExportResponse();
			if (containers.Any())
			{
				try
				{
					var EFN = await GetH1ElectronicFileNumber(site.MailProducerMid, site.SiteName, site.EvsId);
					var headerRecord = new UspsEvsRecord
					{
						H1HeaderRecordID = "H1",
						H1ElectronicFileNumber = EFN,//validate
						H1ElectronicFileType = ShippingServiceFileConstants.PostageandTrackingFile,
						H1DateofMailing = DateTime.Now.ToString("yyyyMMdd"),
						H1TimeofMailing = DateTime.Now.ToString("HHmmss"),

						H1EntryFacilityType = " ",//need entry site info
						H1EntryFacilityZIPCode = site.Zip,
						H1EntryFacilityZIPplus4Code = string.Empty,
						H1DirectEntryOriginCountryCode = string.Empty,

						H1ShipmentFeeCode = string.Empty,
						H1ExtraFeeforShipment = "000000",
						H1ContainerizationIndicator = string.Empty,
						H1USPSElectronicFileVersionNumber = "020",
						H1TransactionID = DateTime.Now.ToString("yyyyMMdd") + site.Zip.Substring(0, 3) + "1",//the last 4 digts need to be unique in a given day
						H1SoftwareVendorCode = string.Empty,
						H1SoftwareVendorProductVersionNumber = string.Empty,
						H1FileRecordCount = AddLeadingZeros((containers.Count() + 1).ToString()),
						H1MailerID = site.MailProducerMid
					};
					response.FileContents.Add(BuildHeaderString(headerRecord));
				}
				catch (Exception ex)
				{
					//response.BadOutputDocuments.Add(item);
					logger.Log(LogLevel.Error, $"Failed to add USPS evs Header. Exception: {ex}");
				}

				var sortedContainers = containers.OrderBy(c => c.BinCode).ThenBy(c => c.IsSecondaryCarrier);
				foreach (var container in sortedContainers)
				{
					try
					{
						var bin = await binRepository.GetBinByBinCodeAsync(container.BinCode, container.BinActiveGroupId);
						string entryZip = ParseZip(container.IsSecondaryCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary);


						string classOfMail = ShippingServiceFileConstants.UspsPriority;
						string processingCategory = ShippingServiceFileConstants.OpenAndDistribute;
						string rateIndicator = container.ContainerType == "BAG" ? "SP" : "O5";
						var zipCode = entryZip.Substring(0, 5);
						var zip4 = entryZip.Trim().Length >= 9 ? entryZip.Trim().Substring(entryZip.Trim().Length - 4) : string.Empty;
						var record = new UspsEvsRecord
						{
							D1DetailRecordID = "D1",
							D1TrackingNumber = container.CarrierBarcode,
							D1ClassOfMail = classOfMail,
							D1ServiceTypeCode = "123",//container.ContainerId.Substring(10, 3),//need this.
							D1BarcodeConstructCode = container.CarrierBarcode.Length == 30 ? "C03" : "C02",
							D1DestinationZIPCode = zipCode,
							D1DestinationZIPplus4 = zip4,
							D1DestinationFacilityType = string.Empty,
							D1DestinationCountryCode = string.Empty,
							D1ForeignPostalCode = string.Empty,
							D1CarrierRoute = string.Empty,
							D1LogisticsManagerMailerID = site.MailProducerMid,
							D1MailOwnerMailerID = site.MailProducerMid,
							D1ContainerID1 = container.ContainerId,
							D1ContainerType1 = string.Empty,
							D1ContainerID2 = string.Empty,
							D1ContainerType2 = string.Empty,
							D1ContainerID3 = string.Empty,
							D1ContainerType3 = string.Empty,
							D1CRID = site.MailProducerCrid,
							D1CustomerReferenceNumber1 = string.Empty,
							D1FASTReservationNumber = string.Empty,
							D1FASTScheduledInductionDate = "00000000",
							D1FASTScheduledInductionTime = "000000",

							D1PaymentAccountNumber = AddLeadingZerosForFormat(container.BinLabelType == ContainerConstants.PmodBag ? site.PermitNumber : site.PmodPalletPermitNumber, 10),
							D1MethodofPayment = site.UspsPaymentMethod,
							D1PostOfficeofAccountZIPCode = site.UspsPermitNoZip,

							D1MeterSerialNumber = string.Empty,
							D1ChargebackCode = string.Empty,
							D1Postage = GetFormattedPostage(container.Cost.ToString()),//needed? doesn't exist right now
							D1PostageType = site.UspsPostageType,
							D1CustomizedShippingServicesContractsNumber = site.UspsCsscNo,
							D1CustomizedShippingServicesContractsProductID = site.UspsCsscProductNo,
							D1UnitofMeasureCode = ShippingServiceFileConstants.LBS,
							D1Weight = GetFormattedWeight(decimal.Parse(container.Weight == "" ? "0" : container.Weight).ToString()),

							D1ProcessingCategory = processingCategory,
							D1RateIndicator = rateIndicator,
							D1DestinationRateIndicator = GetDestinationRateIndicator(container.BinCode),//Constant?

							D1DomesticZone = container.BinCode.Substring(0, 1) == "D" || container.BinCode.Substring(0, 1) == "S" ? "00" : container.Zone.ToString().PadLeft(2, '0'),
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
							D1POBoxIndicator = "N",//containers don't have a PO box indicator.  Should they?  
							D1WaiverofSignatureOrCarrierReleaseOrMerchantOverrideOrCustomerDeliveryPreference = string.Empty,
							D1DeliveryOptionIndicator = "1",
							D1DestinationDeliveryPoint = string.Empty,
							D1RemovalIndicator = string.Empty,
							D1TrackingIndicator = string.Empty,
							D1OriginalLabelTrackingNumberBarcodeConstructCode = string.Empty,
							D1OriginalTrackingNumber = string.Empty,
							D1CustomerReferenceNumber2 = string.Empty,
							D1RecipientNameDestination = "",//container.RecipientName,
							D1DeliveryAddress = "",//container.AddressLine1,
							D1AncillaryServiceEndorsement = string.Empty,
							D1AddressServiceParticipantCod = string.Empty,
							D1KeyLine = string.Empty,
							D1ReturnAddress = "",//container.ReturnAddressLine1,
							D1ReturnAddressCity = "",//container.ReturnCity,
							D1ReturnAddressState = "",//container.ReturnState,
							D1ReturnAddressZIPCode = "",//container.ReturnZip,
							D1LogisticMailingFacilityCRID = site.MailProducerCrid
						};

						response.FileContents.Add(BuildRecordString(record));
					}
					catch (Exception ex)
					{
						response.BadOutputDocuments.Add(container);
						logger.Log(LogLevel.Error, $"Failed to add USPS evs Record for containerId: {container.Id}. Exception: {ex}");
					}
				}
			}
			response.NumberOfRecords = response.FileContents.Count;
			return response;
		}

		public async Task<FileExportResponse> CreateUspsRecords(Site site, IEnumerable<Package> packages, IEnumerable<ShippingContainer> containers)
		{
			var response = new FileExportResponse();
			if (packages.Any())
			{
				string lastBinCode = "";
				bool lastIsSecondary = false;
				string lastSubClientName = "";
				string lastContainerId = "";
				var sortedPackages = packages.OrderBy(p => p.BinCode).ThenBy(p => p.IsSecondaryContainerCarrier).ThenBy(p => p.SubClientName).ThenBy(p => p.ContainerId);
				List<SubClient> subClients = new List<SubClient>();
				List<ShippingContainer> createdContainers = new List<ShippingContainer>();
				SubClient subClient = null;
				string EFN = "";
				string entryZip = "";
				ShippingContainer container = null;
				var containerType = "";
				var containerId = "";

				foreach (var package in sortedPackages)
				{
					try
					{
						//write Header Record
						if (package.BinCode != lastBinCode || package.IsSecondaryContainerCarrier != lastIsSecondary || package.SubClientName != lastSubClientName)
						{
							if (!subClients.FindAll(x => x.Name == package.SubClientName).Any())
							{
								subClient = await subClientRepository.GetSubClientByNameAsync(package.SubClientName);
								subClients.Add(subClient);
							}
							else
							{
								subClient = subClients.Find(x => x.Name == package.SubClientName);
							}

							EFN = await GetH1ElectronicFileNumber(
								subClient.ParentMailOwnerMid, site.SiteName, site.EvsId);

							try
							{
								var bin = await binRepository.GetBinByBinCodeAsync(package.BinCode, package.BinGroupId);
								entryZip = ParseZip(package.IsSecondaryContainerCarrier ? bin.DropShipSiteCszSecondary : bin.DropShipSiteCszPrimary);
								var headerRecord = new UspsEvsRecord
								{
									H1HeaderRecordID = "H1",
									H1ElectronicFileNumber = EFN,//validate
									H1ElectronicFileType = ShippingServiceFileConstants.PostageandTrackingFile,
									H1DateofMailing = DateTime.Now.ToString("yyyyMMdd"),
									H1TimeofMailing = DateTime.Now.ToString("HHmmss"),

									H1EntryFacilityType = bin.BinCode.StartsWith("S") || bin.BinCode.StartsWith("D") ? bin.BinCode.Substring(0, 1) : "",//need entry site info
									H1EntryFacilityZIPCode = entryZip.Substring(0, 5),//need entry site info
									H1EntryFacilityZIPplus4Code = entryZip.Trim().Length >= 9 ? entryZip.Trim().Substring(entryZip.Trim().Length - 4) : string.Empty,//need entry site info
									H1DirectEntryOriginCountryCode = string.Empty,

									H1ShipmentFeeCode = string.Empty,
									H1ExtraFeeforShipment = "000000",
									H1ContainerizationIndicator = string.Empty,
									H1USPSElectronicFileVersionNumber = "020",
									H1TransactionID = DateTime.Now.ToString("yyyyMMdd") + site.Zip.Substring(0, 3) + "1",//the last 4 digts need to be unique in a given day
									H1SoftwareVendorCode = string.Empty,
									H1SoftwareVendorProductVersionNumber = string.Empty,
									//H1FileRecordCount = AddLeadingZeros((packages.Count() + containers.Count() + 1).ToString()),
									H1FileRecordCount = AddLeadingZeros((packages.Count(p => p.BinCode == package.BinCode && p.IsSecondaryContainerCarrier == package.IsSecondaryContainerCarrier && p.SubClientName == package.SubClientName) + 1).ToString()),
									H1MailerID = subClient.ParentMailOwnerMid
								};
								response.FileContents.Add(BuildHeaderString(headerRecord));
							}
							catch (Exception ex)
							{
								//response.BadOutputDocuments.Add(item);
								logger.Log(LogLevel.Error, $"Failed to add USPS evs Header. Exception: {ex}");
							}
						}

						if (package.BinCode != lastBinCode || package.IsSecondaryContainerCarrier != lastIsSecondary)
						{
							createdContainers.Clear();
						}

						lastBinCode = package.BinCode;
						lastIsSecondary = package.IsSecondaryContainerCarrier;
						lastSubClientName = package.SubClientName;

						//write Container Record
						if (lastContainerId != package.ContainerId && package.ContainerId != null)
						{
							try
							{
								if (createdContainers.FindAll(c => c.ContainerId == package.ContainerId).Count == 0)
								{
									var filteredContainers = containers.Where(c => c.ContainerId == package.ContainerId);
									if (filteredContainers.Count() > 0)
									{
										container = filteredContainers.First();
										containerType = GetContainerType(container);
										containerId = container.ContainerId;
										createdContainers.Add(container);
										var containerRecord = new UspsEvsRecord
										{
											C1ContainerRecordID = "C1",
											C1ContainerID = containerId,
											C1ContainerType = containerType,
											C1ElectronicFileNumber = EFN,
											C1DestinationZIPCode = entryZip.Substring(0, 5)
										};
										//response.FileContents.Add(BuildContainerString(containerRecord));
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
									var filteredContainers = containers.Where(c => c.ContainerId == package.ContainerId);
									if (filteredContainers.Count() > 0)
									{
										container = filteredContainers.First();
										containerType = GetContainerType(container);
										containerId = package.ContainerId;
									}
								}
							}
							catch (Exception ex)
							{
								//response.BadOutputDocuments.Add(item);
								logger.Log(LogLevel.Error, $"Failed to add USPS evs Container. Exception: {ex}");
							}
						}

						if (string.IsNullOrEmpty(package.ContainerId))
						{
							container = null;
							containerType = "";
							containerId = "";
						}

						lastContainerId = package.ContainerId;

						string classOfMail = string.Empty;
						string processingCategory = string.Empty;
						string rateIndicator = ShippingServiceFileConstants.SinglePiece;
						switch (package.ShippingMethod)
						{
							case ShippingMethodConstants.UspsFirstClass:
								classOfMail = ShippingServiceFileConstants.UspsFirstClass;
								processingCategory = ShippingServiceFileConstants.MachinableParcel;
								break;
							case ShippingMethodConstants.UspsPriority:
								classOfMail = ShippingServiceFileConstants.UspsPriority;
								processingCategory = ShippingServiceFileConstants.MachinableParcel;
								break;
							case ShippingMethodConstants.UspsParcelSelectLightWeight:
								classOfMail = ShippingServiceFileConstants.UspsLightWeight;
								processingCategory = ShippingServiceFileConstants.IrregularParcel;
								rateIndicator = GetParselectRateIndicator(package);
								break;
							case ShippingMethodConstants.UspsParcelSelect:
								classOfMail = ShippingServiceFileConstants.UspsParcelSelect;
								processingCategory = ShippingServiceFileConstants.IrregularParcel;
								rateIndicator = GetParselectRateIndicator(package);
								break;
						}
						var record = new UspsEvsRecord
						{
							D1DetailRecordID = "D1",
							D1TrackingNumber = package.Barcode,
							D1ClassOfMail = classOfMail,
							D1ServiceTypeCode = package.Barcode.Substring(10, 3),//need this.
							D1BarcodeConstructCode = package.MailerId.Length == 9 ? package.Barcode.Length == 30 ? "C03" : "C02" : package.Barcode.Length == 30 ? "C07" : "C06",
							D1DestinationZIPCode = package.Zip.Trim().Length >= 6 ? package.Zip.Substring(0, 5) : package.Zip,
							D1DestinationZIPplus4 = package.Zip.Trim().Length >= 9 ? package.Zip.Substring(package.Zip.Trim().Length - 4) : string.Empty,//need to standardize format.  Currently only one zip field with multiple formats.  Some 5 and some 9 digit with hyphen.
							D1DestinationFacilityType = string.Empty,
							D1DestinationCountryCode = string.Empty,
							D1ForeignPostalCode = string.Empty,
							D1CarrierRoute = string.Empty,
							D1LogisticsManagerMailerID = site.MailProducerMid,
							D1MailOwnerMailerID = subClient.UspsMailOwnerMid,
							D1ContainerID1 = (container?.ShippingMethod == ContainerConstants.UspsPmodBag || container?.ShippingMethod == ContainerConstants.UspsPmodPallet)
								? container.CarrierBarcode : containerId,
							D1ContainerType1 = containerType,
							D1ContainerID2 = string.Empty,
							D1ContainerType2 = string.Empty,
							D1ContainerID3 = string.Empty,
							D1ContainerType3 = string.Empty,
							D1CRID = subClient.ParentMailOwnerCrid,
							D1CustomerReferenceNumber1 = string.Empty,
							D1FASTReservationNumber = string.Empty,
							D1FASTScheduledInductionDate = "00000000",
							D1FASTScheduledInductionTime = "000000",

							D1PaymentAccountNumber = AddLeadingZerosForFormat(subClient.UspsPermitNo, 10),
							D1MethodofPayment = subClient.UspsPaymentMethod,
							D1PostOfficeofAccountZIPCode = subClient.UspsPermitNoZip,

							D1MeterSerialNumber = string.Empty,
							D1ChargebackCode = string.Empty,
							D1Postage = GetFormattedPostage(package.Cost.ToString()),//needed? doesn't exist right now
							D1PostageType = subClient.UspsPostageType,
							D1CustomizedShippingServicesContractsNumber = subClient.UspsCsscNo,
							D1CustomizedShippingServicesContractsProductID = subClient.UspsCsscProductNo,
							D1UnitofMeasureCode = ShippingServiceFileConstants.LBS,
							D1Weight = GetFormattedWeight((package.Weight).ToString()),

							D1ProcessingCategory = processingCategory,
							D1RateIndicator = rateIndicator,
							D1DestinationRateIndicator = GetDestinationRateIndicator(package.BinCode),//Constant?

							D1DomesticZone = package.BinCode.Substring(0, 1) == "D" || package.BinCode.Substring(0, 1) == "S" ? "00" : package.Zone.ToString().PadLeft(2, '0'),
							D1Length = "00000",
							D1Width = "00000",
							D1Height = "00000",
							D1DimensionalWeight = "000000",
							D1ExtraServiceCode1stService = package.Barcode.Substring(10, 3) == "021" ? "921" : "920",
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
							D1LogisticMailingFacilityCRID = site.MailProducerCrid
						};

						response.FileContents.Add(BuildRecordString(record));
					}
					catch (Exception ex)
					{
						response.BadOutputDocuments.Add(package);
						logger.Log(LogLevel.Error, $"Failed to add USPS evs Record for packageId: {package.PackageId}. Exception: {ex}");
					}
				}
			}
			response.NumberOfRecords = response.FileContents.Count;
			return response;
		}

		private string ParseZipFromCSZ(string csz)
		{
			string[] splitString = csz.Split(' ');
			string zip = splitString[splitString.Length - 1].ToString();
			if (zip.Length > 5)
			{
				zip = zip.Substring(0, 5);
			}
			return zip;
		}

		private string BuildRecordString(UspsEvsRecord record)
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
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}

		private static string BuildContainerString(UspsEvsRecord record)
		{
			var delimiter = "|";
			var recordBuilder = new StringBuilder();

			recordBuilder.Append(record.C1ContainerRecordID);
			recordBuilder.Append(delimiter + record.C1ContainerID);
			recordBuilder.Append(delimiter + record.C1ContainerType);
			recordBuilder.Append(delimiter + record.C1ElectronicFileNumber);
			recordBuilder.Append(delimiter + record.C1DestinationZIPCode);
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}

		private static string BuildHeaderString(UspsEvsRecord record)
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
			recordBuilder.AppendLine();

			return recordBuilder.ToString();
		}

		private string GetParselectRateIndicator(Package package)
		{
			if (package.BinCode.Trim().Length == 0)
			{
				logger.Log(LogLevel.Information, $"Package has no BinCode. Barcode: {package.Barcode}");
				return ShippingServiceFileConstants.FiveDigitPrice;
			}

			if (package.BinCode.Substring(0, 1) == "D")
			{
				return ShippingServiceFileConstants.FiveDigitPrice;
			}
			else
			{
				if (package.BinCode.Substring(0, 1) != "S")
				{
					logger.Log(LogLevel.Information, $"Parselect bincode does not start with a D or an S. Barcode: {package.Barcode}");
					return ShippingServiceFileConstants.FiveDigitPrice;
				}
				return ShippingServiceFileConstants.ThreeDigitPrice;
			}
		}

		private async Task<string> GetH1ElectronicFileNumber(string mailerid, string siteName, string EvsId)
		{
			if (StringHelper.Exists(mailerid) && StringHelper.Exists(EvsId))
			{
				string stc = "750";
				string serialNumber = string.Empty;
				string AI = BarcodeConstants.ChannelApplicationIdentifierSixDigitMid;
				EvsId = EvsId.Length == 0 ? "0" : EvsId;
				EvsId = EvsId.Substring(0, 1);
				if (mailerid.Length == 6)
				{
					Sequence sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(siteName, SequenceTypeConstants.EvsFile, SequenceTypeConstants.NineDigitMaxSequence);
					serialNumber = sequence.Number.ToString().PadLeft(9, '0');
				}
				else if (mailerid.Length == 9)
				{
					Sequence sequence = await sequenceProcessor.ExecuteGetSequenceProcedure(siteName, SequenceTypeConstants.EvsFile, SequenceTypeConstants.SixDigitMaxSequence);
					serialNumber = sequence.Number.ToString().PadLeft(6, '0');
					AI = BarcodeConstants.ChannelApplicationIdentifierNineDigitMid;
				}
				return AI + stc + mailerid + EvsId + serialNumber + GetCheckDigit(AI + stc + mailerid + EvsId + serialNumber);
			}
			return string.Empty; // Some data missing in test.
		}

		private string ParseZip(string csz)
		{
			return new string(csz.Where(c => char.IsDigit(c)).ToArray());
		}

		private string GetCheckDigit(string EFN)
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

		private static string AddLeadingZeros(string packagesCount)
		{
			int length = packagesCount.Length + 1;
			if (length < 9)
			{
				packagesCount = packagesCount.PadLeft(9, '0');
			}
			return packagesCount.ToString();
		}

		private static string GetDestinationRateIndicator(string binCode)
		{
			switch (binCode.Substring(0, 1))
			{
				case "D":
					return ShippingServiceFileConstants.DestinationDeliveryUnit;
				case "S":
					return ShippingServiceFileConstants.DestinationSectionalCenterFacility;
				default:
					return ShippingServiceFileConstants.None;
			}


		}

		private static string GetContainerType(ShippingContainer container)
		{
			string containerType = "";
			if (container.ShippingCarrier == ShippingCarrierConstants.Usps)
			{
				if (container.ContainerType == ContainerConstants.ContainerTypePallet)
				{
					containerType = "OP";
				}
				else if (container.ContainerType == ContainerConstants.ContainerTypeBag)
				{
					containerType = "OT";
				}
			}
			else
			{
				if (container.ContainerType == ContainerConstants.ContainerTypePallet)
				{
					containerType = "PT";
				}
				else if (container.ContainerType == ContainerConstants.ContainerTypeBag)
				{
					containerType = "SK";
				}
			}
			return containerType;
		}

		private string AddLeadingZerosForFormat(string str, int definedLength)
		{
			if (str == null)
				str = string.Empty;
			int length = str.Length;
			if (length < definedLength)
			{
				str = str.PadLeft(definedLength, '0');
			}
			return str.ToString();
		}

		private string GetFormattedWeight(string weightWithDecimal)
		{
			string[] splitWeight = weightWithDecimal.Split('.');
			string wholePart = splitWeight[0];
			string decimalPart = splitWeight.Length == 2 ? splitWeight[1] : "0000";

			wholePart = wholePart.PadLeft(5, '0');
			decimalPart = decimalPart.PadRight(4, '0').Substring(0, 4);

			return wholePart + decimalPart;
		}

		private string GetFormattedPostage(string postageWithDecimal)
		{
			string[] splitPostage = postageWithDecimal.Split('.');
			string wholePart = splitPostage[0];
			string decimalPart = splitPostage.Length == 2 ? splitPostage[1] : "000";

			wholePart = wholePart.PadLeft(4, '0');
			decimalPart = decimalPart.PadRight(3, '0').Substring(0, 3);

			return wholePart + decimalPart;
		}

		private string GetContainerType(string type)
		{
			//BL Truck bedload
			//OA Open & Distribute Full Postal Paks
			//OE Open & Distribute EMM Tray Box
			//OF Open and Distribute Full Tray Box
			//OH Open and Distribute Half Tray Box
			//OK Open & Distribute Half Postal Paks
			//OP Open & Distribute Pallet
			//OT Open & Distribute Flat Tub Tray Box
			//PT Pallet
			//RP Receptacle
			//SK Sack
			return "SK";
		}
	}
}
