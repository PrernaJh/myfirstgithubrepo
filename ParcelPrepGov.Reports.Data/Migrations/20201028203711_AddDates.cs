using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "BinDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "BinDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "JobDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "JobDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "JobContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "JobContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "PackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "PackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "PackageEventDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "PackageEventDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DatasetCreateDate",
                table: "TrackPackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CosmosCreateDate",
                table: "TrackPackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "BinDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "BinDatasets");

            migrationBuilder.DropColumn(
               name: "DatasetCreateDate",
               table: "JobDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "JobDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "JobContainerDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "JobContainerDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "PackageEventDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "PackageEventDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "DatasetCreateDate",
                table: "TrackPackageDataset");

            migrationBuilder.DropColumn(
                name: "CosmosCreateDate",
                table: "TrackPackageDatasets");
        }
    }
}
