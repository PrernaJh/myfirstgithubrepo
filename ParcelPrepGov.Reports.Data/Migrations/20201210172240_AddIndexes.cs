using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_PackageStatus",
                 table: "PackageDatasets",
                 column: "PackageStatus");
            migrationBuilder.CreateIndex(
                 name: "IX_ShippingContainerDatasets_Status",
                 table: "ShippingContainerDatasets",
                 column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_PackageStatus",
                 table: "PackageDatasets");
            migrationBuilder.DropIndex(
                 name: "IX_ShippingContainerDatasets_Status",
                 table: "ShippingContainerDatasets");
        }
    }
}
