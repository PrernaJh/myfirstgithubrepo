using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class IndexUpdatedBarcode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets",
                column: "PackageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                column: "ShippingContainerDatasetId");

            migrationBuilder.CreateIndex(
               name: "IX_ShippingContainerDatasets_UpdatedBarcode",
               table: "ShippingContainerDatasets",
               column: "UpdatedBarcode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_ShippingContainerDatasets_UpdatedBarcode",
                 table: "ShippingContainerDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "ShippingContainerDatasetId",
                table: "TrackPackageDatasets");
        }
    }
}
