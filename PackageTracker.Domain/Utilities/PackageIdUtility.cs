using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;

namespace PackageTracker.Domain.Utilities
{
    public static class PackageIdUtility
    {
        public static string GenerateCmopMailCode(string packageId)
        {
            return packageId.Length switch // valid CMOP barcode lengths
            {
                32 => packageId.Substring(26, 1),
                33 => packageId.Substring(26, 1),
                50 => ParseLeadingZero(packageId.Substring(41, 2)),
                51 => ParseLeadingZero(packageId.Substring(41, 2)),
                _ => "0"
            };
        }

        public static string GetRepeatScanMailCode(Package package)
        {            
            if (package.ClientName == ClientSubClientConstants.CmopClientName)
            {
                return GenerateCmopMailCode(package.PackageId);
            }
            else // MailCode is always "0" for non-CMOP
            {
                return "0";
            }
        }

        public static int GetVisnSiteParentId(string clientName, string packageId)
        {
            var response = 0;

            if (clientName == ClientSubClientConstants.CmopClientName) // VISN ID is always 0 for non-CMOP
            {
                var visnSiteParent = packageId.Length switch // valid CMOP barcode lengths
                {
                    32 => packageId.Substring(0, 5),
                    33 => packageId.Substring(0, 5),
                    50 => packageId.Substring(14, 5),
                    51 => packageId.Substring(14, 5),
                    _ => "0"
                };

                int.TryParse(visnSiteParent, out response);
            }

            return response;
        }

        public static string GetPaddedVisnSiteParentId(string clientName, string packageId, int padLength)
        {
            return GetVisnSiteParentId(clientName, packageId).ToString().PadLeft(padLength, '0');
        }

        public static string GenerateReferenceCode(Package package)
        {
            var response = string.Empty;
            if (package.ClientName == ClientSubClientConstants.CmopClientName
                && (package.PackageId.Length == 50 || package.PackageId.Length == 51)
                && !package.IsCreated)
            {
                response = package.PackageId.Substring(13, 35);
            }
            else
            {
                response = package.PackageId.Length > 35 ? package.PackageId[..35] : package.PackageId;
            }

            return response;
        }

        private static string ParseLeadingZero(string twoDigitMailCode)
        {
            int.TryParse(twoDigitMailCode, out var parsedMailCode);
            return parsedMailCode.ToString();
        }
    }
}
