using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets",
                column: "PackageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                column: "ShippingContainerDatasetId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackPackageDatasets_PackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets",
                column: "PackageDatasetId",
                principalTable: "PackageDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackPackageDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                column: "ShippingContainerDatasetId",
                principalTable: "ShippingContainerDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackPackageDatasets_PackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackPackageDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets");
        }
    }
}
