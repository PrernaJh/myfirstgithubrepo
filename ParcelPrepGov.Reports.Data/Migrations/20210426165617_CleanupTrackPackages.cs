using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class CleanupTrackPackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                WITH cte AS (
                    SELECT 
                        PackageDatasetId, EventCode, EventDate, EventDescription,
                        ROW_NUMBER() OVER (
                            PARTITION BY 
                                PackageDatasetId, EventCode, EventDate, EventDescription
                            ORDER BY 
                                PackageDatasetId, EventCode, EventDate DESC
                        ) AS row_num
                     FROM 
                        TrackPackageDatasets
                )
                DELETE FROM cte
                WHERE row_num > 1 AND NOT PackageDatasetId IS NULL;
           ");

           migrationBuilder.Sql(@"
                 WITH cte AS (
                    SELECT 
                        ShippingContainerDatasetId, EventCode, EventDate, EventDescription,
                        ROW_NUMBER() OVER (
                            PARTITION BY 
                                ShippingContainerDatasetId, EventCode, EventDate, EventDescription
                            ORDER BY 
                                ShippingContainerDatasetId, EventCode, EventDate DESC
                        ) AS row_num
                        FROM 
                        TrackPackageDatasets
                )
                DELETE FROM cte
                WHERE row_num > 1 AND NOT ShippingContainerDatasetId IS NULL;            
             ");        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
