namespace ParcelPrepGov.API.Client.Data
{
    public class ReplaceContainerResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string PackageIdUpdated { get; set; }
    }
}
