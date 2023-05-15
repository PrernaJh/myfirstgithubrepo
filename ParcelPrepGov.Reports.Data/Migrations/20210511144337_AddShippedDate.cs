using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddShippedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedDate",
                table: "PackageDatasets",
                nullable: true);

            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_ShippedDate",
                 table: "PackageDatasets",
                 column: "ShippedDate");

            migrationBuilder.Sql(@"
                DECLARE @Id VARCHAR(MAX)
 
                DECLARE MY_data CURSOR FOR SELECT CosmosId from PackageDatasets WHERE ShippedDate IS NULL 
  	                AND PackageStatus = 'PROCESSED' AND ShippingCarrier = 'USPS' 
                OPEN MY_data  

                FETCH NEXT FROM MY_data INTO @Id
                WHILE @@FETCH_STATUS = 0  
                    BEGIN  
		                DECLARE @date DATETIME;
		                SET @date = (SELECT TOP 1 e.EventDate
			                FROM dbo.PackageEventDatasets AS e
				                WHERE e.CosmosId = @Id AND e.EventId = 999
		                )
		                UPDATE PackageDatasets set ShippedDate = @date
			                WHERE CosmosId = @Id
   
                        FETCH NEXT FROM MY_data INTO  @Id
                    END  
                CLOSE MY_data  
                DEALLOCATE MY_data
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippedDate",
                table: "PackageDatasets");
        }
    }
}
