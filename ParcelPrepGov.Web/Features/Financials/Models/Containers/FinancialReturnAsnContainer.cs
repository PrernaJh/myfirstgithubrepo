using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials
{
    internal class FinancialReturnAsnContainer : AzureCustomContainer
    {
        private IConfiguration config;

        public FinancialReturnAsnContainer(IConfiguration config)
        {
            this.config = config;
            ContainerName = "financial-return-asn";
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}