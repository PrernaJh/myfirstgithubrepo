using Microsoft.Extensions.Logging;
using ParcelPrepGov.API.Client.Interfaces;
using System.Diagnostics;

namespace ParcelPrepGov.API.Client.Services
{
    public class ErrorLogger : IErrorLogger
    {
        ILogger<ErrorLogger> logger;

        public ErrorLogger(ILogger<ErrorLogger> logger)
        {
            this.logger = logger;
        }

        public void HTTPRequestError(ErrorTypes errorType, string message)
        {
            StackTrace stackTrace = new StackTrace();

            // get calling method name
            var methodName = stackTrace.GetFrame(1).GetMethod().Name;

            string strErrType = "General or Unknown";
            switch (errorType)
            {
                case ErrorTypes.PingReponse: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot receive ping response"; break;
                case ErrorTypes.Login: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process login request"; break;
                case ErrorTypes.Logout: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process logout request"; break;
                case ErrorTypes.GetBinCodes: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process get bin codes request"; break;
                case ErrorTypes.CreateContainer: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process create container request"; break;
                case ErrorTypes.ScanContainer: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process scan container request"; break;
                case ErrorTypes.GetJobOptions: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process get job options request"; break;
                case ErrorTypes.StartJob: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process start job request"; break;
                case ErrorTypes.AddJob: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process add job request"; break;
                case ErrorTypes.ReprintPackage: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process reprint package request"; break;
                case ErrorTypes.GetPackageHistory: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process get package history request"; break;
                case ErrorTypes.ScanPackage: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process scan package request"; break;
                case ErrorTypes.ValidatePackage: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process validate package request"; break;
                case ErrorTypes.GetReturnOptions: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process get return options request"; break;
                case ErrorTypes.ReturnPackage: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot process return package request"; break;
                case ErrorTypes.GetAllSiteNames: strErrType = "HTTP Status Code 422-Unprocessable response-Cannot get all site namese request"; break;

                case ErrorTypes.BadRequest: strErrType = "HTTP Status Code 400-Request could not be understood by the server"; break;
                default: break;
            }
            logger.LogError("HTTP Request Error - Callling Method: " + methodName + " - " + strErrType + " : " + message);
        }
    }
}
