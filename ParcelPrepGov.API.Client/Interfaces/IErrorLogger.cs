namespace ParcelPrepGov.API.Client.Interfaces
{
    public enum ErrorTypes
    {
        NoError = 0,
        PingReponse,
        Login,
        Logout,
        GetBinCodes,
        CreateContainer,
        ScanContainer,
        GetJobOptions,
        StartJob,
        AddJob,
        ReprintPackage,
        GetPackageHistory,
        ScanPackage,
        ValidatePackage,
        GetReturnOptions,
        ReturnPackage,
        GetAllSiteNames,
        GeneralHTTPResponse,
        BadRequest,
    }

    public interface IErrorLogger
    {
        void HTTPRequestError(ErrorTypes errorType, string message);
    }
}
