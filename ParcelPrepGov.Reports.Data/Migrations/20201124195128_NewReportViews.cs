using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class NewReportViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "SubClientName",
            //    table: "getRptVwUspsStcDetail",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "getRptVwDailyAdvanceWarning",
            //    columns: table => new
            //    {
            //        LOCATION = table.Column<string>(nullable: true),
            //        MANIFEST_DATE = table.Column<DateTime>(nullable: false),
            //        PACKAGE_ID = table.Column<string>(nullable: true),
            //        TRACKING_NUMBER = table.Column<string>(nullable: true),
            //        PRODUCT = table.Column<string>(nullable: true),
            //        Zip = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_TYPE = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_NAME = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_CSZ = table.Column<string>(nullable: true),
            //        LAST_KNOWN_DESC = table.Column<string>(nullable: true),
            //        LAST_KNOWN_DATE = table.Column<DateTime>(nullable: false),
            //        LAST_KNOWN_LOCATION = table.Column<string>(nullable: true),
            //        LAST_KNOWN_ZIP = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });

            //migrationBuilder.CreateTable(
            //    name: "getRptVwUspsCarrierDetail",
            //    columns: table => new
            //    {
            //        LOCATION = table.Column<string>(nullable: true),
            //        MANIFEST_DATE = table.Column<DateTime>(nullable: false),
            //        ENTRY_UNIT_NAME = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_CSZ = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_TYPE = table.Column<string>(nullable: true),
            //        PRODUCT = table.Column<string>(nullable: true),
            //        CARRIER = table.Column<string>(nullable: true),
            //        TRACKING_NUMBER = table.Column<string>(nullable: true),
            //        WEIGHT = table.Column<decimal>(nullable: false),
            //        LAST_KNOWN_DATE = table.Column<DateTime>(nullable: false),
            //        LAST_KNOWN_DESC = table.Column<string>(nullable: true),
            //        LAST_KNOWN_LOCATION = table.Column<string>(nullable: true),
            //        LAST_KNOWN_ZIP = table.Column<string>(nullable: true),
            //        NUM_DAYS = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });

            //migrationBuilder.CreateTable(
            //    name: "getRptVwUspsDropPointStatus",
            //    columns: table => new
            //    {
            //        LOCATION = table.Column<string>(nullable: true),
            //        MANIFEST_DATE = table.Column<DateTime>(nullable: false),
            //        ENTRY_UNIT_NAME = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_CSZ = table.Column<string>(nullable: true),
            //        ENTRY_UNIT_TYPE = table.Column<string>(nullable: true),
            //        PRODUCT = table.Column<string>(nullable: true),
            //        CARRIER = table.Column<string>(nullable: true),
            //        LAST_KNOWN_DESC = table.Column<string>(nullable: true),
            //        LAST_KNOWN_DATE = table.Column<DateTime>(nullable: false),
            //        LAST_KNOWN_LOCATION = table.Column<string>(nullable: true),
            //        LAST_KNOWN_ZIP = table.Column<string>(nullable: true),
            //        PACKAGE_ID = table.Column<string>(nullable: true),
            //        TRACKING_NUMBER = table.Column<string>(nullable: true),
            //        Zip = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "getRptVwDailyAdvanceWarning");

            //migrationBuilder.DropTable(
            //    name: "getRptVwUspsCarrierDetail");

            //migrationBuilder.DropTable(
            //    name: "getRptVwUspsDropPointStatus");

            //migrationBuilder.DropColumn(
            //    name: "SubClientName",
            //    table: "getRptVwUspsStcDetail");
        }
    }
}
