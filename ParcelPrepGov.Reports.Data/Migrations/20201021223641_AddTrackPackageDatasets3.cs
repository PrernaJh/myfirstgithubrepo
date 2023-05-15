using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddTrackPackageDatasets3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_ShippingBarcode",
                 table: "PackageDatasets",
                 column: "ShippingBarcode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_ShippingBarcode",
                 table: "PackageDatasets");
        }
    }
}
