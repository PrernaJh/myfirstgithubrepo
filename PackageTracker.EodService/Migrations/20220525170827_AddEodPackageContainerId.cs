using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class AddEodPackageContainerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContainerId",
                table: "EodPackages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_ContainerId",
                table: "EodPackages",
                column: "PackageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContainerId",
                table: "EodPackages");
        }
    }
}
