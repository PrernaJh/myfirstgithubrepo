using FedExTrackApi;
using System.Threading.Tasks;

namespace PackageTracker.WebServices
{
    public interface IFedExTrackClient
    {
        Task<trackResponse> trackAsync(TrackRequest TrackRequest);
    }
}
