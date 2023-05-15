using MMS.API.Domain.Models;
using MMS.API.Domain.Models.Containers;
using System.Diagnostics;

namespace MMS.API.Domain.Utilities
{
	public static class LogUtility
	{
		public static string GenerateScanPackageTimerMessage(PackageTimer timer)
		{
			return $"Total Time: {timer.TotalWatch.ElapsedMilliseconds} " +
				   $"| Select Query: {timer.SelectQueryWatch.ElapsedMilliseconds} " +
				   $"| Service Data Time: {timer.ServiceWatch.ElapsedMilliseconds} " +
				   $"| Container Query Time: {timer.ContainerWatch.ElapsedMilliseconds} " +
				   $"| Shipping Data Time: {timer.ShippingWatch.ElapsedMilliseconds} " +
				   $"| Update Scans: {timer.UpdateQueryWatch.ElapsedMilliseconds}";
		}

		public static string GenerateCreatePackageTimerMessage(PackageTimer timer)
		{
			return $"Total Time: {timer.TotalWatch.ElapsedMilliseconds} " +
				   $"| Sequence Time: {timer.SequenceWatch.ElapsedMilliseconds} " +
				   $"| Service Data Time: {timer.ServiceWatch.ElapsedMilliseconds} " +
				   $"| Shipping Data Time: {timer.ShippingWatch.ElapsedMilliseconds} " +
				   $"| Container Query Time: {timer.ContainerWatch.ElapsedMilliseconds} " +
				   $"| Bin Validation Query Time: {timer.BinValidationTimer.ElapsedMilliseconds} " +
				   $"| Add Query: {timer.AddQueryWatch.ElapsedMilliseconds}";
		}

		public static string GenerateScanPackageRequestLog(ScanPackageRequest request)
		{
			return $"Incoming ScanPackage Request | PackageId: {request.PackageId} " +
				   $"| Username: {request.Username} " +
				   $"| MachineId: {request.MachineId} " +
				   $"| Weight: {request.Weight} " +
				   $"| JobBarcode: {request.JobId} " +
				   $"| SiteName: {request.SiteName} ";
		}

		public static string GenerateScanPackageResponseLog(ScanPackageResponse response)
		{
			var labelFieldValues = string.Empty;
			response.LabelFieldValues.ForEach(x => labelFieldValues += $"({x.Position},{x.FieldValue})");

			return $"Outgoing ScanPackage Response | PackageId: {response.PackageId} " +
				   $"| Success: {response.Succeeded.ToString() ?? string.Empty} " +
				   $"| Weight: {response.Weight}" +
				   $"| Carrier: {response.Carrier} " +
				   $"| ServiceType: {response.ServiceType} " +
				   $"| Bin: {response.Bin} " +
				   $"| Barcode: {response.Barcode} " +
				   $"| PrintLabel: {response.PrintLabel} " +
				   $"| LabelTypeId: {response.LabelTypeId} " +
				   $"| LabelFieldValues: {labelFieldValues} " +
				   $"| Base64: {response.Base64Label} " +
				   $"| Message: {response.Message} " +
				   $"| ErrorLabelMessage: {response.ErrorLabelMessage}";
		}

		public static string GenerateCreateContainersRequestLog(CreateContainersRequest request)
		{
			var sortCodes = string.Empty;
			var isSecondaryCarrier = request.IsSecondaryCarrier ? "true" : "false";

			foreach (var sortCode in request.BinCodes)
			{
				sortCodes += $"{sortCode} ";
			}

			return $"Incoming CreateContainers Request | SortCodes: {sortCodes} " +
				   $"| IsSecondaryCarrier: {isSecondaryCarrier} " +
				   $"| NumberOfCopies: {request.NumberOfCopies} " +
				   $"| SiteName: {request.SiteName} ";
		}

		public static string GenerateCreateContainersResponseLog(CreateContainersResponse response)
		{
			var containersMessage = "CreateContainersResponse all new Containers: ";

			foreach (var container in response.Containers)
			{
				var labelFieldValues = string.Empty;
				container.LabelFieldValues.ForEach(x => labelFieldValues += $"({x.Position},{x.FieldValue})");

				containersMessage += $"CreateContainer | ContainerId: {container.ContainerId} " +
				$"| LabelTypeId: {container.LabelTypeId} " +
				$"| LabelFieldValues: {labelFieldValues} ";
			}

			return containersMessage;
		}

		public static string GenerateScanContainerRequestLog(CloseContainerRequest request)
		{
			return $"Incoming ScanContainer Request | ContainerId: {request.ContainerId} " +
				   $"| Weight: {request.Weight} " +
				   $"| SiteName: {request.SiteName} " +
				   $"| Username: {request.Username} " +
				   $"| MachineId: {request.MachineId} ";
		}

		public static string GenerateScanContainerResponseLog(CloseContainerResponse response)
		{
			var labelFieldValues = string.Empty;
			response.LabelFieldValues.ForEach(x => labelFieldValues += $"({x.Position},{x.FieldValue})");

			return $"Incoming ScanContainer Response | LabelTypeId: {response.LabelTypeId} " +
				   $"| LabelFieldValues: {labelFieldValues} ";
		}
		public static string GenerateValidatePackageRequestLog(ValidatePackageRequest request)
		{
			return $"Incoming ValidatePackage Request | PackageId: {request.PackageId} " +
				   $"| Username: {request.Username} " +
				   $"| MachineId: {request.MachineId} " +
				   $"| ShippingBarcode: {request.ShippingBarcode} ";
		}
		public static string GenerateValidatePackageTimerMessage(Stopwatch timer)
		{
			return $"Total Time: {timer.ElapsedMilliseconds} ";

		}
		public static string GenerateValidatePackageResponseLog(ValidatePackageResponse response)
		{
			return $"Outgoing ScanPackage Response | PackageId: {response.PackageId} " +
				   $"| IsValid: {response.IsValid.ToString() ?? string.Empty} " +
				   $"| Shipping Barcode: {response.ShippingBarcode}" +
				   $"| Full Address: {response.FullAddress} " +
				   $"| Sort Code: {response.BinCode} " +
				   $"| Recipient Name: {response.RecipientName} ";
		}

		public static string GenerateAddJobRequestLog(AddJobRequest request)
		{
			var jobContainers = string.Empty;

			foreach (var container in request.JobContainers)
			{
				var jobContainerTypes = string.Empty;
				foreach (var type in container.JobContainerTypes)
				{
					jobContainerTypes += $"{type} ";
				}

				jobContainers += $"{jobContainerTypes} {container.NumberOfContainers} {container.Weight} ";
			}

			return $"Incoming AddJobRequest | SiteName: {request.SiteName} " +
				   $"| ManifestDate: {request.ManifestDate} " +
				   $"| CmopLocation: {request.CustomerLocation.Value}" +
				   $"| Full Address: {request.MarkUpType.Value} " +
				   $"| MarkUp: {request.MarkUp.Value} " +
				   $"| Product: {request.Product.Value} " +
				   $"| PackageType: {request.PackageType.Value} " +
				   $"| PackageDescription: {request.PackageDescription.Value} " +
				   $"| Reference: {request.Reference} " +
				   $"| BillOfLading: {request.BillOfLading} " +
				   $"| SerialNumber: {request.SerialNumber} " +
				   $"| JobContainers: {jobContainers} " +
				   $"| Username: {request.Username} " +
				   $"| MachineId: {request.MachineId} ";
		}

		public static string GenerateAddJobResponseLog(AddJobResponse response)
		{
			var labelFieldValues = string.Empty;
			response.LabelFieldValues.ForEach(x => labelFieldValues += $"({x.Position},{x.FieldValue})");

			return $"Outgoing AddJobResponse | JobBarcode: {response.JobBarcode} " +
				   $"| LabelTypeId: {response.LabelTypeId} " +
				   $"| LabelFieldValues: {labelFieldValues} ";
		}
	}
}
