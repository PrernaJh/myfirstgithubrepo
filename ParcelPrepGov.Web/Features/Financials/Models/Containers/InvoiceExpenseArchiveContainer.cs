using Microsoft.Extensions.Configuration;
using ParcelPrepGov.Web.Features.Common;

namespace ParcelPrepGov.Web.Features.Financials.Models.Containers
{
    public class InvoiceExpenseArchiveContainer : AzureCustomContainer
    { 
        public InvoiceExpenseArchiveContainer(IConfiguration config)
        {
            ContainerName = "invoiceexpense-archive";             

            AzureFileProvider = new AzureBlobFileProvider(ContainerName, "aspxAzureEmptyFolderBlob", config);
        }
    }
}