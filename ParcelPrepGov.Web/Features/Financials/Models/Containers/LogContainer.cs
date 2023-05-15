using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials.Models.Containers
{
    public class LogContainer : AzureCustomContainer
    { 
        public LogContainer(IConfiguration config)
        {
            ContainerName = "$logs"; 
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}