using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddContainerIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_ContainerId",
                table: "PackageDatasets",
                column: "ContainerId");
            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                column: "ShippingContainerDatasetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_ContainerId",
                table: "PackageDatasets");
            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerEventDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets");
        }
    }
}
