using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class RemoveOldDatasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobContainerDatasets");

            migrationBuilder.DropTable(
                name: "ShippingContainerDatasets");

            migrationBuilder.DropTable(
                name: "TrackPackageDatasets");

            migrationBuilder.DropTable(
                name: "JobDatasets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Depth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JobBarcode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MachineId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    MailTypeCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    ManifestDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MarkUp = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    MarkUpType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    PackageDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    SiteName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    SubClientName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BinCode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    BinCodeSecondary = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    Charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContainerId = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    ContainerType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    CosmosId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", maxLength: 32, nullable: false),
                    Grouping = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    MachineId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ShippingCarrier = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SiteName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    UpdatedBarcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Zone = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingContainerDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackPackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EventLocation = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PackageId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ShippingCarrier = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    SiteName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackageDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    JobBarcode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    JobDatasetId = table.Column<int>(type: "int", nullable: false),
                    NumberOfContainers = table.Column<int>(type: "int", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobContainerDatasets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobContainerDatasets_JobDatasets_JobDatasetId",
                        column: x => x.JobDatasetId,
                        principalTable: "JobDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_JobDatasetId",
                table: "JobContainerDatasets",
                column: "JobDatasetId");
        }
    }
}
