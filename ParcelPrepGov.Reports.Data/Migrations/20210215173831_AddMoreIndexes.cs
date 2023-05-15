using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddMoreIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_SubClientName",
                 table: "PackageDatasets",
                 column: "SubClientName");
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_LocalProcessedDate",
                 table: "PackageDatasets",
                 column: "LocalProcessedDate");
            migrationBuilder.CreateIndex(
                 name: "IX_ShippingContainerDatasets_LocalProcessedDate",
                 table: "ShippingContainerDatasets",
                 column: "LocalProcessedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_SubClientName",
                 table: "PackageDatasets");
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_LocalProcessedDate",
                 table: "PackageDatasets");
            migrationBuilder.DropIndex(
                 name: "IX_ShippingContainerDatasets_LocalProcessedDate",
                 table: "ShippingContainerDatasets");
        }
    }
}
