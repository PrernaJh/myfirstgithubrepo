using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddClientFacilityName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientFacilityName",
                table: "PackageDatasets",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
               name: "IX_PackageDatasets_ClientFacilityName",
               table: "PackageDatasets",
               column: "ClientFacilityName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientFacilityName",
                table: "PackageDatasets");
        }
    }
}
