using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddTrackPackageDatasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysToLastKnownStatus",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastStatusDate",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastStatusDescription",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastStatusLocation",
                table: "PackageDatasets");

            migrationBuilder.CreateTable(
                name: "TrackPackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageDatasetId = table.Column<int>(nullable: false),
                    ShippingCarrier = table.Column<string>(maxLength: 24, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 100, nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    EventDescription = table.Column<string>(maxLength: 120, nullable: true),
                    EventLocation = table.Column<string>(maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackageDatasets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackPackageDatasets");

            migrationBuilder.AddColumn<double>(
                name: "DaysToLastKnownStatus",
                table: "PackageDatasets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusDate",
                table: "PackageDatasets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastStatusDescription",
                table: "PackageDatasets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastStatusLocation",
                table: "PackageDatasets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }
    }
}
