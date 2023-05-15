namespace ParcelPrepGov.API.Client.Data
{
    public class LoginResponse
    {
        public bool Succeeded { get; set; }
        public string AuthorizationTier { get; set; }
        public string Site { get; set; }
        public int ConsecutiveScansAllowed { get; set; }
    }
}