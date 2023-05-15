using PackageTracker.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface ISequenceProcessor
	{
		Task<Sequence> GetSequenceAsync(string siteName, string sequenceType);
		Task AssignSequencesToListOfPackages(List<Package> packages);
		Task<Sequence> ExecuteGetSequenceProcedure(string siteName, string sequenceType, string maxSequence, string startSequence = null);

		Task<Sequence> UpdateItemAsync(Sequence sequence);
    }
}
