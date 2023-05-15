using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddNewDatasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    JobBarcode = table.Column<string>(maxLength: 24, nullable: true),
                    SubClientName = table.Column<string>(maxLength: 30, nullable: true),
                    ManifestDate = table.Column<string>(maxLength: 10, nullable: true),
                    MarkUpType = table.Column<string>(maxLength: 24, nullable: true),
                    MarkUp = table.Column<string>(maxLength: 24, nullable: true),
                    Product = table.Column<string>(maxLength: 16, nullable: true),
                    PackageType = table.Column<string>(maxLength: 16, nullable: true),
                    PackageDescription = table.Column<string>(maxLength: 100, nullable: true),
                    Length = table.Column<decimal>(nullable: false),
                    Width = table.Column<decimal>(nullable: false),
                    Depth = table.Column<decimal>(nullable: false),
                    MailTypeCode = table.Column<string>(maxLength: 16, nullable: true),
                    Username = table.Column<string>(maxLength: 32, nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 18, nullable: true),
                    BinCode = table.Column<string>(maxLength: 24, nullable: true),
                    BinCodeSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(maxLength: 60, nullable: true),
                    ShippingCarrier = table.Column<string>(maxLength: 24, nullable: true),
                    ContainerType = table.Column<string>(maxLength: 24, nullable: true),
                    Grouping = table.Column<string>(maxLength: 2, nullable: true),
                    Weight = table.Column<string>(maxLength: 16, nullable: true),
                    UpdatedBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    Cost = table.Column<decimal>(maxLength: 32, nullable: false),
                    Charge = table.Column<decimal>(nullable: false),
                    Zone = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingContainerDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackPackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    PackageId = table.Column<string>(maxLength: 32, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "JobContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    JobDatasetId = table.Column<int>(nullable: false),
                    JobBarcode = table.Column<string>(maxLength: 24, nullable: true),
                    NumberOfContainers = table.Column<int>(nullable: false),
                    Weight = table.Column<string>(maxLength: 16, nullable: true)
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

            migrationBuilder.CreateIndex(
               name: "IX_JobContainerDatasets_JobBarcode",
               table: "JobContainerDatasets",
               column: "JobBarcode");

            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_SiteName",
                table: "JobContainerDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_CosmosId",
                table: "JobContainerDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_JobBarcode",
                table: "JobDatasets",
                column: "JobBarcode");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_SiteName",
                table: "JobDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_CosmosId",
                table: "JobDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_ContainerId",
                table: "ShippingContainerDatasets",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_SiteName",
                table: "ShippingContainerDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_CosmosId",
                table: "ShippingContainerDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_PackageId",
                table: "TrackPackageDatasets",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_TrackingNumber",
                table: "TrackPackageDatasets",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDataSets_SiteName",
                table: "TrackPackageDataSets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_CosmosId",
                table: "TrackPackageDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_PackageId",
                table: "PackageEventDatasets",
                column: "PackageId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
