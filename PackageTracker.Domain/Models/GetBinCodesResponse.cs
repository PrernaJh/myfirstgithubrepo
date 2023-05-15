using System.Collections.Generic;

namespace PackageTracker.Domain.Models
{
	public class GetBinCodesResponse
	{
		public GetBinCodesResponse()
		{
			BinCodes = new List<BinCodeResponse>();
			Groups = new List<string>();
		}

		public List<BinCodeResponse> BinCodes { get; set; }
		public List<string> Groups { get; set; }
	}

	public class BinCodeResponse
	{
		public string BinCode { get; set; }
		public string Group { get; set; }
		public bool HasActiveContainer { get; set; }
	}
}
