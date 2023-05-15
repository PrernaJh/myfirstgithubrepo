using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddPackageBinGroupIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_BinGroupId",
                 table: "PackageDatasets",
                 column: "BinGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_BinGroupId",
                 table: "PackageDatasets");
        }
    }
}