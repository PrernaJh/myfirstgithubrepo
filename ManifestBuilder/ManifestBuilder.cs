using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace ManifestBuilder
{
    public static class ManifestBuilder
    {
		public static CreateManifestResponse CreateManifestFile(CreateManifestRequest request)
		{
			CreateManifestResponse response = UspsEvsFileProcessor.CreateUspsRecords(request);

			return response;
		}

		public static string GetManifestFileName(int FileNameStartSequence)
		{
			if (FileNameStartSequence > 9999)
            {
				throw new Exception("Serial number must be less than 4 digits.");
            }
			return $"USPS_eVs_{System.DateTime.Now:yyyMMdd}{FileNameStartSequence.ToString().PadLeft(4, '0')}.ssf.manifest";
		}
	}
}
