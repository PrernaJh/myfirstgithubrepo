using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddIsUndeliverable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsUndeliverable",
                table: "PackageDatasets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_IsUndeliverable",
                table: "PackageDatasets",
                column: "IsUndeliverable");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUndeliverable",
                table: "PackageDatasets");
        }
    }
}
