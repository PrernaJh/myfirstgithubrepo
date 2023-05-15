using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class ZipMapProcessor : IZipMapProcessor
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly ILogger<ZipMapProcessor> logger;
		private readonly IZipMapRepository zipMapRepository;

		public ZipMapProcessor(IActiveGroupProcessor activeGroupProcessor, ILogger<ZipMapProcessor> logger, IZipMapRepository zipMapRepository)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.logger = logger;
			this.zipMapRepository = zipMapRepository;
		}

		public async Task<ZipMap> GetZipMapAsync(string activeGroupType, string zipCode)
		{
			var activeGroupId = await activeGroupProcessor.GetZipMapActiveGroup(activeGroupType);
			var response = await zipMapRepository.GetZipMapValueAsync(activeGroupId, zipCode);
			return response;
		}

		public async Task<ZipMap> GetZipMapByGroupAsync(string activeGroupId, string zipCode)
		{
			var response = await zipMapRepository.GetZipMapValueAsync(activeGroupId, zipCode);
			return response;
		}
	}
}
