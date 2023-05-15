using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParcelPrepGov.Reports.Models.SprocModels;
using ParcelPrepGov.Web.Features.PackageSearch.Models;

namespace ParcelPrepGov.Web.Features.ContainerSearch.Models
{
    public class ContainerSearchViewModel
    {
        public string ContainerId { get; set; }
        public string Barcode { get; set; }
        public ContainerSearchResultViewModel Result { get; set; } = new ContainerSearchResultViewModel();
    }
}
