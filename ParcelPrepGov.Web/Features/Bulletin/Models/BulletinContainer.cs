using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Financials.Models;
using ParcelPrepGov.Web.Features.Financials.Models.Containers;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Bulletin.Models
{
    public class BulletinContainer : AzureCustomContainer
    {
        private readonly IConfiguration config;

        public BulletinContainer(IConfiguration config)
        {
            this.config = config;
            ContainerName = "bulletin";
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}