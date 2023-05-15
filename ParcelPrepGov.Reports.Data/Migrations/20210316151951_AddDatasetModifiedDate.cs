using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddDatasetModifiedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "TrackPackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "SubClientDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "ShippingContainerEventDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "PackageEventDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "PackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "JobDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "JobContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetModifiedDate",
                table: "BinDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_DatasetCreateDate",
                table: "TrackPackageDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_DatasetModifiedDate",
                table: "TrackPackageDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SubClientDatasets_DatasetCreateDate",
                table: "SubClientDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_SubClientDatasets_DatasetModifiedDate",
                table: "SubClientDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDatasets_DatasetCreateDate",
                table: "ShippingContainerEventDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDatasets_DatasetModifiedDate",
                table: "ShippingContainerEventDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_DatasetCreateDate",
                table: "ShippingContainerDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_DatasetModifiedDate",
                table: "ShippingContainerDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_DatasetCreateDate",
                table: "PackageEventDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_DatasetModifiedDate",
                table: "PackageEventDatasets",
                column: "DatasetModifiedDate");
            
            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_DatasetCreateDate",
                table: "PackageDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_DatasetModifiedDate",
                table: "PackageDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_DatasetCreateDate",
                table: "JobDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_DatasetModifiedDate",
                table: "JobDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_DatasetCreateDate",
                table: "JobContainerDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_DatasetModifiedDate",
                table: "JobContainerDatasets",
                column: "DatasetModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BinDatasets_DatasetCreateDate",
                table: "BinDatasets",
                column: "DatasetCreateDate");
            migrationBuilder.CreateIndex(
                name: "IX_BinDatasets_DatasetModifiedDate",
                table: "BinDatasets",
                column: "DatasetModifiedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_DatasetCreateDate",
                table: "TrackPackageDatasets");
            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_DatasetModifiedDate",
                table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_SubClientDatasets_DatasetCreateDate",
                table: "SubClientDatasets");
            migrationBuilder.DropIndex(
                name: "IX_SubClientDatasets_DatasetModifiedDate",
                table: "SubClientDatasets");

            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerEventDatasets_DatasetCreateDate",
                table: "ShippingContainerEventDatasets");
            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerEventDatasets_DatasetModifiedDate",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerDatasets_DatasetCreateDate",
                table: "ShippingContainerDatasets");
            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerDatasets_DatasetModifiedDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropIndex(
                name: "IX_PackageEventDatasets_DatasetCreateDate",
                table: "PackageEventDatasets");
            migrationBuilder.DropIndex(
                name: "IX_PackageEventDatasets_DatasetModifiedDate",
                table: "PackageEventDatasets");

            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_DatasetCreateDate",
                table: "PackageDatasets");
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_DatasetModifiedDate",
                table: "PackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_JobDatasets_DatasetCreateDate",
                table: "JobDatasets");
            migrationBuilder.DropIndex(
                name: "IX_JobDatasets_DatasetModifiedDate",
                table: "JobDatasets");

            migrationBuilder.DropIndex(
                name: "IX_JobContainerDatasets_DatasetCreateDate",
                table: "JobContainerDatasets");
            migrationBuilder.DropIndex(
                name: "IX_JobContainerDatasets_DatasetModifiedDate",
                table: "JobContainerDatasets");

            migrationBuilder.DropIndex(
                name: "IX_BinDatasets_DatasetCreateDate",
                table: "BinDatasets");
            migrationBuilder.DropIndex(
                name: "IX_BinDatasets_DatasetModifiedDate",
                table: "BinDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "SubClientDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "PackageEventDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "JobDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "JobContainerDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetModifiedDate",
                table: "BinDatasets");
        }
    }
}
