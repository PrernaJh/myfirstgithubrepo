using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddNewPackageDatasetEtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    PackageId = table.Column<string>(maxLength: 32, nullable: true),
                    ClientName = table.Column<string>(maxLength: 30, nullable: true),
                    SubClientName = table.Column<string>(maxLength: 30, nullable: true),
                    MailCode = table.Column<string>(maxLength: 1, nullable: true),
                    PackageStatus = table.Column<string>(maxLength: 24, nullable: true),
                    ProcessedDate = table.Column<DateTime>(nullable: false),
                    LocalProcessedDate = table.Column<DateTime>(nullable: false),
                    SiteId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteZip = table.Column<string>(maxLength: 5, nullable: true),
                    SiteAddressLineOne = table.Column<string>(maxLength: 120, nullable: true),
                    SiteCity = table.Column<string>(maxLength: 30, nullable: true),
                    SiteState = table.Column<string>(maxLength: 2, nullable: true),
                    JobId = table.Column<string>(maxLength: 36, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 36, nullable: true),
                    BinCode = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    ShippingCarrier = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(maxLength: 60, nullable: true),
                    ServiceLevel = table.Column<string>(maxLength: 60, nullable: true),
                    Zone = table.Column<int>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false),
                    Length = table.Column<decimal>(nullable: false),
                    Width = table.Column<decimal>(nullable: false),
                    Depth = table.Column<decimal>(nullable: false),
                    TotalDimensions = table.Column<decimal>(nullable: false),
                    Shape = table.Column<string>(maxLength: 24, nullable: true),
                    RequestCode = table.Column<string>(maxLength: 24, nullable: true),
                    DropSiteKeyValue = table.Column<string>(maxLength: 24, nullable: true),
                    MailerId = table.Column<string>(maxLength: 24, nullable: true),
                    Cost = table.Column<decimal>(nullable: false),
                    Charge = table.Column<decimal>(nullable: false),
                    BillingWeight = table.Column<decimal>(nullable: false),
                    ExtraCost = table.Column<string>(maxLength: 100, nullable: true),
                    IsPoBox = table.Column<bool>(nullable: false),
                    IsUpsDas = table.Column<bool>(nullable: false),
                    IsOutside48States = table.Column<bool>(nullable: false),
                    IsOrmd = table.Column<bool>(nullable: false),
                    IsDuplicate = table.Column<bool>(nullable: false),
                    IsSaturday = table.Column<bool>(nullable: false),
                    IsDduScfBin = table.Column<bool>(nullable: false),
                    IsSecondaryContainerCarrier = table.Column<bool>(nullable: false),
                    IsQCRequired = table.Column<bool>(nullable: false),
                    AsnImportWebJobId = table.Column<string>(maxLength: 36, nullable: true),
                    BinGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    BinMapGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    RateId = table.Column<string>(maxLength: 36, nullable: true),
                    ServiceRuleId = table.Column<string>(maxLength: 36, nullable: true),
                    ServiceRuleGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    ZoneMapGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    FortyEightStatesGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    UpsGeoDescriptorGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    RecipientName = table.Column<string>(maxLength: 60, nullable: true),
                    AddressLine1 = table.Column<string>(maxLength: 120, nullable: true),
                    AddressLine2 = table.Column<string>(maxLength: 120, nullable: true),
                    AddressLine3 = table.Column<string>(maxLength: 120, nullable: true),
                    City = table.Column<string>(maxLength: 30, nullable: true),
                    State = table.Column<string>(maxLength: 2, nullable: true),
                    Zip = table.Column<string>(maxLength: 5, nullable: true),
                    FullZip = table.Column<string>(maxLength: 10, nullable: true),
                    Phone = table.Column<string>(maxLength: 13, nullable: true),
                    ReturnName = table.Column<string>(maxLength: 60, nullable: true),
                    ReturnAddressLine1 = table.Column<string>(maxLength: 120, nullable: true),
                    ReturnAddressLine2 = table.Column<string>(maxLength: 120, nullable: true),
                    ReturnCity = table.Column<string>(maxLength: 30, nullable: true),
                    ReturnState = table.Column<string>(maxLength: 2, nullable: true),
                    ReturnZip = table.Column<string>(maxLength: 5, nullable: true),
                    ReturnPhone = table.Column<string>(maxLength: 13, nullable: true),
                    ZipOverrides = table.Column<string>(nullable: true),
                    ZipOverrideGroupIds = table.Column<string>(nullable: true),
                    DuplicatePackageIds = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageEventDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    PackageDatasetId = table.Column<int>(nullable: false),
                    PackageId = table.Column<string>(maxLength: 32, nullable: true),
                    EventId = table.Column<int>(nullable: false),
                    EventType = table.Column<string>(maxLength: 24, nullable: true),
                    EventStatus = table.Column<string>(maxLength: 24, nullable: true),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocalEventDate = table.Column<DateTime>(nullable: false),
                    Username = table.Column<string>(maxLength: 32, nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEventDatasets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEventDatasets_PackageDatasets_PackageDatasetId",
                        column: x => x.PackageDatasetId,
                        principalTable: "PackageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_PackageDatasetId",
                table: "PackageEventDatasets",
                column: "PackageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_CosmosId",
                table: "PackageEventDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEventDatasets_SiteName",
                table: "PackageEventDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_PackageId",
                table: "PackageDatasets",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_ShippingBarcode",
                table: "PackageDatasets",
                column: "ShippingBarcode");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_CosmosId",
                table: "PackageDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_SiteName",
                table: "PackageDatasets",
                column: "SiteName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageEventDatasets");

            migrationBuilder.DropTable(
                name: "PackageDatasets");
        }
    }
}
