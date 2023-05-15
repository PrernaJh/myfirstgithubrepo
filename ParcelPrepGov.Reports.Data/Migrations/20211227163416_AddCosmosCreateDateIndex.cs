using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddCosmosCreateDateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_CosmosCreateDate",
                table: "PackageDatasets",
                column: "CosmosCreateDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_CosmosCreateDate",
                table: "PackageDatasets");
        }
    }
}
