using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials
{
    internal class FinancialExpenseInvoice : AzureCustomContainer
    {
        private IConfiguration config;

        public FinancialExpenseInvoice(IConfiguration config)
        {
            this.config = config;
            ContainerName = "financial-expense-invoice";
            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}