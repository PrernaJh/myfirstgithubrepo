using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class RemoveOldPackageDatasetEtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "JobDatasets");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "TrackPackageDatasets",
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BinCode",
                table: "ShippingContainerDatasets",
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubClientName",
                table: "JobDatasets",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CosmosId",
                table: "JobContainerDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteName",
                table: "JobContainerDatasets",
                maxLength: 24,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CosmosId",
                table: "JobContainerDatasets");

            migrationBuilder.DropColumn(
                name: "SiteName",
                table: "JobContainerDatasets");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "TrackPackageDatasets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 24,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "TrackPackageDatasets",
                type: "datetime2",
                maxLength: 24,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "BinCode",
                table: "ShippingContainerDatasets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 24,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ShippingContainerDatasets",
                type: "datetime2",
                maxLength: 24,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "SubClientName",
                table: "JobDatasets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "JobDatasets",
                type: "datetime2",
                maxLength: 30,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BinCode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    CosmosId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryUnitName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EntryUnitState = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    EntryUnitType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntryUnitZip = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    ManifestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PackageId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    ShippingBarcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingCarrier = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ShippingServiceLevel = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    SiteName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    SubClientName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDatasets", x => x.Id);
                });
        }
    }
}
