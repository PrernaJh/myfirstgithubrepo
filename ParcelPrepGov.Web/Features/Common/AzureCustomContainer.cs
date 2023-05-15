namespace ParcelPrepGov.Web.Features.Common
{
    public class AzureCustomContainer
    {
        public AzureBlobFileProvider AzureFileProvider { get; set; }
        public string ContainerName { get; set; }
    }
}
