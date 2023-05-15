using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials.Models.Containers
{
    public class FinancialDataContainer : AzureCustomContainer
    {
        public FinancialDataContainer(IConfiguration config)
        { 
            ContainerName = "financial-data"; 
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}
