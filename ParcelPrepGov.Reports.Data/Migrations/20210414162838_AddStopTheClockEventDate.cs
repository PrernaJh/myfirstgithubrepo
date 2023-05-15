using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddStopTheClockEventDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StopTheClockEventDate",
                table: "PackageDatasets",
                nullable: true);

            migrationBuilder.Sql(@"
                    DECLARE @Id int
   

   
                    DECLARE MY_data CURSOR FOR SELECT Id from PackageDatasets where StopTheClockEventDate is null
	                    and PackageStatus = 'PROCESSED' and ShippingCarrier = 'USPS'  
                    OPEN MY_data  

                    FETCH NEXT FROM MY_data INTO @Id

                    WHILE @@FETCH_STATUS = 0  
                        BEGIN  
				                    Declare @date datetime;
				                     set @date = (SELECT TOP 1 t.EventDate
					                    FROM dbo.TrackPackageDatasets AS t INNER JOIN dbo.EvsCodes AS ec ON t.EventCode = ec.Code
					                    WHERE (ec.IsStopTheClock = 1) AND (t.PackageDatasetId = @Id)
					                    ORDER BY t.EventDate)

			                    update PackageDatasets set StopTheClockEventDate = @date where Id = @Id
   
                            FETCH NEXT FROM MY_data INTO  @Id
                        END  
                    CLOSE MY_data  
                    DEALLOCATE MY_data
             ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopTheClockEventDate",
                table: "PackageDatasets");
        }
    }
}
