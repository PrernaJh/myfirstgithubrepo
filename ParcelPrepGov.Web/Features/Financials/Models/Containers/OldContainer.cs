using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials.Models.Containers
{
    public class OldContainer : AzureCustomContainer
    {
        public OldContainer(IConfiguration config)
        {
            ContainerName = "testingold";
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }

    }
}