using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class SequenceProcessor : ISequenceProcessor
	{
		private readonly ISequenceRepository sequenceRepository;

		public SequenceProcessor(ISequenceRepository sequenceRepository)
		{
			this.sequenceRepository = sequenceRepository;
		}

		public async Task AssignSequencesToListOfPackages(List<Package> packages)
		{
			var packagesGroupedBySite = packages.GroupBy(x => x.SiteName);

			foreach (var groupOfPackages in packagesGroupedBySite)
			{
				var sequence = await sequenceRepository.GetSequenceAsync(groupOfPackages.Key, SequenceTypeConstants.Package);

				foreach (var package in groupOfPackages)
				{
					if (package.Sequence == 0)
					{
						if (sequence.Number++ > SequenceTypeConstants.AsnImportMaxSequence)
						{
							sequence.Number = 1; 
						}
						package.Sequence = sequence.Number;
					}
				}
				await sequenceRepository.UpdateItemAsync(sequence);
			}
		}

		public async Task<Sequence> GetSequenceAsync(string siteName, string sequenceType)
		{
			return await sequenceRepository.GetSequenceAsync(siteName, sequenceType);
		}

		public async Task<Sequence> ExecuteGetSequenceProcedure(string siteName, string sequenceType, string maxSequence, string startSequence = null)
		{
			var validatedStartSequence = StringHelper.DoesNotExist(startSequence) ? "1" : startSequence;
			var storedProcedureName = "getSequenceNumber";
			string[] inputParams = { siteName, sequenceType, maxSequence, validatedStartSequence };

			return await sequenceRepository.ExecuteStoredProcedure(storedProcedureName, siteName, inputParams);
		}

        public async Task<Sequence> UpdateItemAsync(Sequence sequence)
        {
			return await sequenceRepository.UpdateItemAsync(sequence);
        }
    }
}
