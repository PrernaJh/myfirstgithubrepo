using System;

namespace ParcelPrepGov.Web.Features.RecallRelease.Models
{
    public interface IPackageViewModel
    {
        string Address { get; }
        string AddressLine1 { get; set; }
        string AddressLine2 { get; set; }
        string AddressLine3 { get; set; }
        string BinCode { get; set; }
        string City { get; set; }
        string ContainerId { get; set; }
        string LocalProcessedDate { get; set; }
        string ClientName { get; set; }
        string PackageId { get; set; }
        string PackageStatus { get; set; }
        string ShippingCarrier { get; set; }
        string ShippingMethod { get; set; }
        string State { get; set; }
        string Zip { get; set; }
        string RecallDate { get; set; }
    }
}