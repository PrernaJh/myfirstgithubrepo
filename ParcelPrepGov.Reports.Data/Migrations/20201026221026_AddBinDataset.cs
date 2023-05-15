using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddBinDataset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    ActiveGroupId = table.Column<string>(maxLength: 36, nullable: true),
                    BinCode = table.Column<string>(maxLength: 24, nullable: true),
                    LabelListSiteKey = table.Column<string>(maxLength: 24, nullable: true),
                    LabelListDescription = table.Column<string>(maxLength: 100, nullable: true),
                    LabelListZip = table.Column<string>(maxLength: 5, nullable: true),
                    OriginPointSiteKey = table.Column<string>(maxLength: 24, nullable: true),
                    OriginPointDescription = table.Column<string>(maxLength: 100, nullable: true),
                    DropShipSiteKeyPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    DropShipSiteDescriptionPrimary = table.Column<string>(maxLength: 100, nullable: true),
                    DropShipSiteAddressPrimary = table.Column<string>(maxLength: 120, nullable: true),
                    DropShipSiteCszPrimary = table.Column<string>(maxLength: 120, nullable: true),
                    ShippingCarrierPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingMethodPrimary = table.Column<string>(maxLength: 60, nullable: true),
                    ContainerTypePrimary = table.Column<string>(maxLength: 24, nullable: true),
                    LabelTypePrimary = table.Column<string>(maxLength: 24, nullable: true),
                    RegionalCarrierHubPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    DaysOfTheWeekPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    ScacPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    AccountIdPrimary = table.Column<string>(maxLength: 24, nullable: true),
                    BinCodeSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    DropShipSiteKeySecondary = table.Column<string>(maxLength: 24, nullable: true),
                    DropShipSiteDescriptionSecondary = table.Column<string>(maxLength: 100, nullable: true),
                    DropShipSiteAddressSecondary = table.Column<string>(maxLength: 120, nullable: true),
                    DropShipSiteCszSecondary = table.Column<string>(maxLength: 120, nullable: true),
                    ShippingMethodSecondary = table.Column<string>(maxLength: 60, nullable: true),
                    ContainerTypeSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    LabelTypeSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    RegionalCarrierHubSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    DaysOfTheWeekSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    ScacSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    AccountIdSecondary = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinDatasets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                 name: "IX_BinDatasets_ActiveGroupId",
                 table: "BinDatasets",
                 column: "ActiveGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BinDatasets_BinCode",
                table: "BinDatasets",
                column: "BinCode");

            migrationBuilder.CreateIndex(
                 name: "IX_BinDatasets_CosmosId",
                 table: "BinDatasets",
                 column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_BinDatasets_SiteName",
                table: "BinDatasets",
                column: "SiteName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinDatasets");
        }
    }
}
