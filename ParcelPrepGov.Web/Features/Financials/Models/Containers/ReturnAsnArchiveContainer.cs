using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials.Models.Containers
{
    public class ReturnAsnArchiveContainer : AzureCustomContainer
    {  
        public ReturnAsnArchiveContainer(IConfiguration config)
        { 
            ContainerName = "returnasn-archive";
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }

    }
}
