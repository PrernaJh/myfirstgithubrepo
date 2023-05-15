using AutoMapper;
using PackageTracker.Data.Models;

namespace ParcelPrepGov.Web.Features.RecallRelease.Models
{
	public class AutoMapProfile : Profile
	{
		public AutoMapProfile()
		{
			CreateMap<Package, RecalledPackageViewModel>();
			CreateMap<Package, ReleasedPackageViewModel>();
		}
	}
}
