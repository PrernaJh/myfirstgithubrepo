using ManifestBuilder;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContainerConstants = PackageTracker.Data.Constants.ContainerConstants;
using IEvsFileProcessor = PackageTracker.EodService.Interfaces.IEvsFileProcessor;
using IEodContainerRepository = PackageTracker.EodService.Interfaces.IEodContainerRepository;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;
using PackageTracker.EodService.Data.Models;

namespace PackageTracker.EodService.Processors
{
	public class EvsFileProcessor : IEvsFileProcessor
	{
		private ILogger<EvsFileProcessor> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinProcessor binProcessor;
		private readonly IEodContainerRepository eodContainerRepository;
		private readonly IEodPackageRepository eodPackageRepository;
		private readonly ISequenceProcessor sequenceProcessor;
		private readonly ISubClientProcessor subClientProcessor;

		public EvsFileProcessor(ILogger<EvsFileProcessor> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IBinProcessor binProcessor,
			IEodContainerRepository eodContainerRepository,
			IEodPackageRepository eodPackageRepository,
			ISequenceProcessor sequenceProcessor,
			ISubClientProcessor subClientProcessor)
		{
			this.logger = logger;

			this.activeGroupProcessor = activeGroupProcessor;
			this.binProcessor = binProcessor;
			this.eodContainerRepository = eodContainerRepository;
			this.eodPackageRepository = eodPackageRepository;
			this.sequenceProcessor = sequenceProcessor;
			this.subClientProcessor = subClientProcessor;
		}

		public async Task<FileExportResponse> GenerateEvsEodFile(Site site, DateTime manifestDate)
		{
			var response = new FileExportResponse();
			var eodPackages = await eodPackageRepository.GetEvsEodPackages(site.SiteName, manifestDate);
			var eodContainers = await eodContainerRepository.GetEvsEodContainers(site.SiteName, manifestDate);
			var manifestPackages = new List<ManifestBuilder.Package>();
			var manifestContainers = new List<ManifestBuilder.ShippingContainer>();
			var mailerIdSequenceMaps = await GenerateMailerIdSequenceMap(sequenceProcessor, site);
			var fileNameSequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.EvsFileName, SequenceTypeConstants.FourDigitMaxSequence);

			foreach (var eodPackage in eodPackages.Where(p => p.EvsPackage != null))
			{
				manifestPackages.Add(EvsPackage.GetManifestBuilderPackage(eodPackage.EvsPackage));
			}
			foreach (var eodContainer in eodContainers.Where(c => c.EvsContainerRecord != null))
			{
				manifestContainers.Add(EvsContainer.GetManifestBuilderShippingContainer(eodContainer.EvsContainerRecord));
			}
			var request = new CreateManifestRequest
			{
				Packages = manifestPackages,
				Containers = manifestContainers,
				EFNStartSequenceByMID = mailerIdSequenceMaps,
				MailDate = manifestDate,
				MailProducerMid = site.MailProducerMid,
				Site = new ManifestBuilder.Models.Site
				{
					SiteName = site.SiteName,
					EvsId = site.EvsId,
					Zip = site.Zip
				}
			};

			var evsFileName = $"USPS_eVs_{manifestDate:yyyMMdd}{fileNameSequence.Number.ToString().PadLeft(4, '0')}.ssf.manifest";
			var createManifestResponse = ManifestBuilder.ManifestBuilder.CreateManifestFile(request);
			if (createManifestResponse.IsSuccessful)
			{

				response.FileName = evsFileName;
				response.FileContents.AddRange(createManifestResponse.EvsRecords);
				response.IsSuccessful = createManifestResponse.IsSuccessful;
				response.NumberOfRecords = response.FileContents.Count;

				await UpdateEvsHeaderSequenceNumbers(mailerIdSequenceMaps);
			}
			return response;
		}

		public async Task<FileExportResponse> GeneratePmodEvsEodFile(Site site, DateTime manifestDate)
		{
			var response = new FileExportResponse();
			var eodContainers = await eodContainerRepository.GetEvsEodContainers(site.SiteName, manifestDate);
			var manifestPackages = new List<ManifestBuilder.Package>();
			var manifestContainers = new List<ManifestBuilder.ShippingContainer>();
			var mailerIdSequenceMaps = await GenerateMailerIdSequenceMap(sequenceProcessor, site);
			var fileNameSequence = await sequenceProcessor.ExecuteGetSequenceProcedure(site.SiteName, SequenceTypeConstants.EvsFileName, SequenceTypeConstants.FourDigitMaxSequence);

			foreach (var eodContainer in eodContainers.Where(c => c.EvsPackageRecord != null))
			{
				manifestPackages.Add(EvsPackage.GetManifestBuilderPackage(eodContainer.EvsPackageRecord));
			}
			var request = new CreateManifestRequest
			{
				Packages = manifestPackages,
				Containers = manifestContainers,
				EFNStartSequenceByMID = mailerIdSequenceMaps,
				MailDate = manifestDate,
				MailProducerMid = site.MailProducerMid,
				IsForPmodContainers = true,
				Site = new ManifestBuilder.Models.Site
				{
					SiteName = site.SiteName,
					EvsId = site.EvsId,
					Zip = site.Zip
				}
			};

			var evsFileName = $"USPS_eVs_{manifestDate:yyyMMdd}{fileNameSequence.Number.ToString().PadLeft(4, '0')}.ssf.manifest";
			var createManifestResponse = ManifestBuilder.ManifestBuilder.CreateManifestFile(request);
			if (createManifestResponse.IsSuccessful)
			{

				response.FileName = evsFileName;
				response.FileContents.AddRange(createManifestResponse.EvsRecords);
				response.IsSuccessful = createManifestResponse.IsSuccessful;
				response.NumberOfRecords = response.FileContents.Count;

				await UpdateEvsHeaderSequenceNumbers(mailerIdSequenceMaps);
			}
			return response;
		}

		public async Task<Dictionary<string, string>> GenerateBinEntryZipMap(Site site, List<PackageTracker.Data.Models.Package> packages, bool isSecondaryContainerCarrier)
		{
			var response = new Dictionary<string, string>();
			var binCodeList = new List<string>();
			var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(site.SiteName, site.TimeZone);

			foreach (var group in packages.Where(p => p.IsSecondaryContainerCarrier == isSecondaryContainerCarrier).GroupBy(x => x.BinCode))
			{
				var bin = await binProcessor.GetBinByBinCodeAsync(group.Key, binGroupId);
				if (isSecondaryContainerCarrier && StringHelper.Exists(bin.DropShipSiteCszSecondary))
					response.Add(bin.BinCode, AddressUtility.ParseCityStateZip(bin.DropShipSiteCszSecondary).FullZip);
				else if (!isSecondaryContainerCarrier && StringHelper.Exists(bin.DropShipSiteCszPrimary))
					response.Add(bin.BinCode, AddressUtility.ParseCityStateZip(bin.DropShipSiteCszPrimary).FullZip);
			}
			return response;
		}

		public async Task<Dictionary<string, string>> GenerateBinEntryZipMap(Site site, List<PackageTracker.Data.Models.ShippingContainer> containers, bool isSecondaryCarrier)
		{
			var response = new Dictionary<string, string>();
			var binCodeList = new List<string>();
			var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(site.SiteName, site.TimeZone);

			foreach (var group in containers.Where(p => p.IsSecondaryCarrier == isSecondaryCarrier).GroupBy(x => x.BinCode))
			{
				var bin = await binProcessor.GetBinByBinCodeAsync(group.Key, binGroupId);
				if (isSecondaryCarrier && StringHelper.Exists(bin.DropShipSiteCszSecondary))
					response.Add(bin.BinCode, AddressUtility.ParseCityStateZip(bin.DropShipSiteCszSecondary).FullZip);
				else if (!isSecondaryCarrier && StringHelper.Exists(bin.DropShipSiteCszPrimary))
					response.Add(bin.BinCode, AddressUtility.ParseCityStateZip(bin.DropShipSiteCszPrimary).FullZip);
			}
			return response;
		}

		public static async Task<Dictionary<string, int>> GenerateMailerIdSequenceMap(ISequenceProcessor sequenceProcessor, Site site)
		{
			var response = new Dictionary<string, int>();
			var sequence = await sequenceProcessor.GetSequenceAsync(site.SiteName, SequenceTypeConstants.EvsFile);
			response.Add(site.SiteName, sequence.Number);
			return response;
		}

		private async Task UpdateEvsHeaderSequenceNumbers(Dictionary<string, int> mailerIdSequenceMaps)
		{
			foreach (var item in mailerIdSequenceMaps)
			{
				var sequence = await sequenceProcessor.GetSequenceAsync(item.Key, SequenceTypeConstants.EvsFile);
				sequence.Number = item.Value;
				await sequenceProcessor.UpdateItemAsync(sequence);
			}
		}
		private static EntryFacilityType GetEntryFacilityType(string binCode)
		{
			var prefix = binCode.Substring(0, 1).ToUpper();
			if (prefix == "S")
				return EntryFacilityType.SCF;
			else if (prefix == "D")
				return EntryFacilityType.DDU;
			else
				return (EntryFacilityType)(-1); // Unknown
		}

		public EvsPackage GenerateManifestPackage(Site site, SubClient subClient, PackageTracker.Data.Models.Package package,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps)
		{
			var shouldNotGenerateEvs = package.ShippingCarrier != PackageTracker.Data.Constants.ShippingCarrierConstants.Usps 
									|| package.ShippingMethod == ShippingMethodConstants.UspsFcz;
			
			if (shouldNotGenerateEvs)
            {
				return null;
			}

			var entryZip = package.IsSecondaryContainerCarrier
				? secondaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == package.BinCode).Value
				: primaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == package.BinCode).Value;

			if (StringHelper.DoesNotExist(entryZip))
			{
				// This shouldn't happen, but will if the binCode was valid when the package was imported, but is missing from the active bin file.
				var bin = binProcessor.GetBinByBinCodeAsync(package.BinCode, package.BinGroupId).Result;
				entryZip = AddressUtility.ParseCityStateZip(package.IsSecondaryContainerCarrier
					? bin.DropShipSiteCszSecondary
					: bin.DropShipSiteCszPrimary
					).FullZip;
				if (StringHelper.DoesNotExist(entryZip))
				{
					logger.LogError($"Can't find binCode: {package.BinCode}, packageId: {package.PackageId}");
					return null;
				}
			}
			return new EvsPackage
			{
				CosmosId = package.Id,
				UspsPostageType = subClient.UspsPostageType,
				UspsPaymentMethod = subClient.UspsPaymentMethod,
				UspsPermitNoZip = subClient.UspsPermitNoZip,
				UspsPermitNo = subClient.UspsPermitNo,
				ParentMailOwnerMid = subClient.ParentMailOwnerMid,
				ParentMailOwnerCrid = subClient.ParentMailOwnerCrid,
				UspsMailOwnerMid = subClient.UspsMailOwnerMid,
				UspsMailOwnerCrid = subClient.UspsMailOwnerCrid,
				UspsCsscNo = subClient.UspsCsscNo,
				UspsCsscProductNo = subClient.UspsCsscProductNo,
				MailProducerCrid = site.MailProducerCrid,
				EntryFacilityType = (int) GetEntryFacilityType(package.BinCode),
				EntryZip = entryZip,
				ReturnZip = package.ReturnZip,
				ReturnState = package.ReturnState,
				ReturnCity = package.ReturnCity,
				ReturnAddressLine1 = package.ReturnAddressLine1,
				Zip = package.Zip,
				AddressLine1 = package.AddressLine1,
				RecipientName = package.RecipientName,
				IsPoBox = package.IsPoBox,
				DestinationRateIndicator = GetDestinationRateIndicator(package.BinCode),
				ProcessingCategory = (int) GetProcessingCategory(package.ShippingMethod),
				Cost = package.Cost,
				MailerId = package.MailerId,
				Weight = package.Weight,
				Zone = package.Zone,
				TrackingNumber = package.Barcode,
				ContainerId = package.ContainerId,
				ServiceType = (int) GetServiceType(package.ShippingMethod)
			};
		}
		public EvsPackage GenerateManifestPackageForContainer(Site site, PackageTracker.Data.Models.ShippingContainer container,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps)
		{
			if (container.ShippingCarrier != PackageTracker.Data.Constants.ShippingCarrierConstants.Usps)
				return null;
			var entryZip = container.IsSecondaryCarrier
				? secondaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == container.BinCode).Value
				: primaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == container.BinCode).Value;
			if (StringHelper.DoesNotExist(entryZip))
			{
				logger.LogError($"Can't find binCode: {container.BinCode}, containerId: {container.ContainerId}");
				return null;
			}
			var mailerId = container.BinLabelType == ContainerConstants.PmodBag ? site.SackMailerId : site.PalletMailerId;
			var permitNumber = container.BinLabelType == ContainerConstants.PmodBag ? site.PermitNumber : site.PmodPalletPermitNumber;
			return new EvsPackage
			{
				CosmosId = container.Id,
				UspsPostageType = site.UspsPostageType,
				UspsPaymentMethod = site.UspsPaymentMethod,
				UspsPermitNoZip = site.UspsPermitNoZip,
				UspsPermitNo = permitNumber,
				ParentMailOwnerMid = site.MailProducerMid,
				ParentMailOwnerCrid = site.MailProducerCrid,
				UspsMailOwnerMid = site.MailProducerMid,
				UspsMailOwnerCrid = site.MailProducerCrid,
				UspsCsscNo = site.UspsCsscNo,
				UspsCsscProductNo = site.UspsCsscProductNo,
				MailProducerCrid = site.MailProducerCrid,
				EntryFacilityType = (int) GetEntryFacilityType(container.BinCode),
				EntryZip = entryZip,
				ReturnZip = string.Empty,
				ReturnState = string.Empty,
				ReturnCity = string.Empty,
				ReturnAddressLine1 = string.Empty,
				Zip = entryZip,
				AddressLine1 = string.Empty,
				RecipientName = string.Empty,
				IsPoBox = false,
				DestinationRateIndicator = GetDestinationRateIndicator(container.BinCode),
				ProcessingCategory = (int) GetProcessingCategory(container.ShippingMethod),
				Cost = container.Cost,
				MailerId = mailerId,
				Weight = decimal.Parse(container.Weight),
				Zone = container.Zone,
				TrackingNumber = container.CarrierBarcode,
				ContainerId = container.ContainerId,
				ServiceType = (int) GetServiceType(container.ShippingMethod)
			};
		}
		public ContainerType GetContainerType(string containerType)
		{
			ContainerType result = (ContainerType)(-1); // Unknown
			if (Enum.TryParse<ContainerType>(containerType, ignoreCase: true, out var value))
				result = value;
			return result;
		}

		public ShippingCarrier GetShippingCarrier(string shippingCarrier)
		{
			ShippingCarrier result = (ShippingCarrier)(-1); // Unknown
			if (Enum.TryParse<ShippingCarrier>(shippingCarrier, ignoreCase: true, out var value))
				result = value;
			return result;
		}

		public EvsContainer GenerateManifestContainer(Site site, PackageTracker.Data.Models.ShippingContainer container,
			Dictionary<string, string> primaryCarrierBinEntryZipMaps, Dictionary<string, string> secondaryCarrierBinEntryZipMaps)
		{
			var entryZip = container.IsSecondaryCarrier
				? secondaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == container.BinCode).Value
				: primaryCarrierBinEntryZipMaps.FirstOrDefault(x => x.Key == container.BinCode).Value;
			if (StringHelper.DoesNotExist(entryZip))
			{
				logger.LogError($"Can't find binCode: {container.BinCode}, containerId: {container.ContainerId}");
				return null;
			}
			return new EvsContainer
			{
				CosmosId = container.Id,
				ContainerId = container.ContainerId,
				ContainerType = (int) GetContainerType(container.ContainerType),
				ShippingCarrier = (int) GetShippingCarrier(container.ShippingCarrier),
				ShippingMethod = container.ShippingMethod,
				CarrierBarcode = container.CarrierBarcode,
				EntryZip = entryZip,
				EntryFacilityType = (int) GetEntryFacilityType(container.BinCode),
			};
		}

		private static ServiceType GetServiceType(string shippingMethod)
		{
			var serviceType = (ServiceType)(-1); // Unknown
			switch (shippingMethod)
			{
				case ServiceTypeConstants.UspsFirstClass:
					serviceType = ServiceType.UspsFirstClass;
					break;
				case ServiceTypeConstants.UspsPriority:
					serviceType = ServiceType.UspsPriority;
					break;
				case ServiceTypeConstants.UspsPriorityExpress:
					serviceType = ServiceType.UspsPriorityExpress;
					break;
				case ServiceTypeConstants.UspsParcelSelectLightWeight:
					serviceType = ServiceType.UspsPSLW;
					break;
				case ServiceTypeConstants.UspsParcelSelect:
					serviceType = ServiceType.UspsParcelSelect;
					break;
				case ServiceTypeConstants.UspsPmod:
					serviceType = ServiceType.UspsPmod;
					break;

				case ContainerConstants.UspsPmodBag:
					serviceType = ServiceType.UspsPmodBag;
					break;
				case ContainerConstants.UspsPmodPallet:
					serviceType = ServiceType.UspsPmodPallet;
					break;
			}
			return serviceType;
		}

		private static ProcessingCategory GetProcessingCategory(string shippingMethod)
		{
			var processingCategory = (ProcessingCategory)(-1); // Unknown
			switch (shippingMethod)
			{
				case ShippingMethodConstants.UspsFirstClass:
					processingCategory = ProcessingCategory.MachinableParcel;
					break;
				case ShippingMethodConstants.UspsPriority:
					processingCategory = ProcessingCategory.MachinableParcel;
					break;
				case ShippingMethodConstants.UspsParcelSelectLightWeight:
					processingCategory = ProcessingCategory.IrregularParcel;
					break;
				case ShippingMethodConstants.UspsParcelSelect:
					processingCategory = ProcessingCategory.IrregularParcel;
					break;

				case ContainerConstants.PmodBag:
					processingCategory = ProcessingCategory.OpenAndDistribute;
					break;
				case ContainerConstants.PmodPallet:
					processingCategory = ProcessingCategory.OpenAndDistribute;
					break;
			}
			return processingCategory;
		}

		static string GetDestinationRateIndicator(string binCode)
		{
			if (StringHelper.DoesNotExist(binCode))
				return PackageTracker.Data.Constants.ShippingServiceFileConstants.None;
			switch (binCode.Substring(0, 1))
			{
				case "D":
					return PackageTracker.Data.Constants.ShippingServiceFileConstants.DestinationDeliveryUnit;
				case "S":
					return PackageTracker.Data.Constants.ShippingServiceFileConstants.DestinationSectionalCenterFacility;
				default:
					return PackageTracker.Data.Constants.ShippingServiceFileConstants.None;
			}
		}
	}
}
