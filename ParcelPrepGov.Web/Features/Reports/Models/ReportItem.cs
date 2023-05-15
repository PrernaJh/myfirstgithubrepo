namespace ParcelPrepGov.Web.Features.Reports.Models
{
    public class ReportItem
    { 
        public ReportItem(int Id, string reportName, string reportDescription, string updateId)
        {
            this.ID = Id;
            this.ReportName = reportName;
            this.Description = reportDescription;
            this.UpdateID = updateId;
        }

        public int ID{ get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
        public string UpdateID { get; set; }
    }
}