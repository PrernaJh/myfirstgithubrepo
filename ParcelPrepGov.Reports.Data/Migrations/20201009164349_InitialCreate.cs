using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Site = table.Column<string>(maxLength: 24, nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    DashboardXml = table.Column<string>(nullable: true),
                    DashboardName = table.Column<string>(maxLength: 256, nullable: true),
                    IsGlobal = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<string>(maxLength: 32, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    ManifestDate = table.Column<DateTime>(nullable: false),
                    EntryUnitName = table.Column<string>(maxLength: 120, nullable: true),
                    EntryUnitState = table.Column<string>(maxLength: 2, nullable: true),
                    EntryUnitZip = table.Column<string>(maxLength: 9, nullable: true),
                    EntryUnitType = table.Column<string>(maxLength: 100, nullable: true),
                    ShippingCarrier = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(maxLength: 60, nullable: true),
                    ShippingServiceLevel = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    Product = table.Column<string>(maxLength: 16, nullable: true),
                    Weight = table.Column<string>(maxLength: 16, nullable: true),
                    LastStatusDate = table.Column<DateTime>(nullable: false),
                    LastStatusDescription = table.Column<string>(maxLength: 120, nullable: true),
                    LastStatusLocation = table.Column<string>(maxLength: 120, nullable: true),
                    DaysToLastKnownStatus = table.Column<double>(nullable: false),
                    SubClientName = table.Column<string>(maxLength: 120, nullable: true),
                    BinCode = table.Column<string>(maxLength: 120, nullable: true),
                    Username = table.Column<string>(maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportName = table.Column<string>(maxLength: 256, nullable: false),
                    Site = table.Column<string>(maxLength: 32, nullable: false),
                    Client = table.Column<string>(maxLength: 32, nullable: false),
                    SubClient = table.Column<string>(maxLength: 32, nullable: false),
                    Role = table.Column<string>(maxLength: 32, nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ChangeDate = table.Column<DateTime>(nullable: false),
                    ReportXml = table.Column<string>(nullable: true),
                    IsGlobal = table.Column<bool>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dashboards");

            migrationBuilder.DropTable(
                name: "PackageDatasets");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
