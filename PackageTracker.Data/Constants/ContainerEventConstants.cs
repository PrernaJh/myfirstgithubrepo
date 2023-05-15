namespace PackageTracker.Data.Constants
{
    public class ContainerEventConstants
    {
        // container status

        public const string Active = "ACTIVE";
        public const string Closed = "CLOSED";
        public const string Replaced = "REPLACED";
        public const string Deleted = "DELETED";

        // container event type
        public const string Created = "CREATED";
        public const string Reprint = "REPRINT";
        public const string CloseScan = "SCANCLOSED";
        public const string WeightUpdate = "WEIGHTUPDATE";
        public const string ReplaceScan = "REPLACESCAN";
        public const string DeleteScan = "SCANDELETED";
        public const string UpdateScan = "UPDATESCAN";
        public const string PackagesAdded = "PACKAGESADDED";
        public const string RateAssigned = "RATEASSIGNED";
        public const string EodProcessed = "EODPROCESSED";
    }
}
