using PackageTracker.Communications.Models;
using System.Threading.Tasks;

namespace PackageTracker.Communications.Interfaces
{
	public interface ISmsService
	{
		Task<SmsResponse> SendAsync(SmsMessage smsMessage);
	}
}
