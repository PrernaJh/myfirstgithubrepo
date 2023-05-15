using AutoMapper;
using PackageTracker.Data.Models;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Web.Features.Reports.Models;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class AutoMapProfile : Profile
	{
		public AutoMapProfile()
		{
			CreateMap<ServiceRule, ServiceRuleExcel>();
			CreateMap<ServiceRuleExtension, ServiceRuleExtensionExcel>();
			CreateMap<ActiveGroup, ActiveGroupViewModel>();
			CreateMap<ZipOverride, ZipExcel>();
			CreateMap<ZipMap, ZipMapViewModel>().ForMember(g => g.SortCode, z => z.MapFrom(z => z.Value));
			CreateMap<ZoneMap, ZoneMapViewModel>();
			CreateMap<Rate, RateViewModel>();
		}
	}
}
