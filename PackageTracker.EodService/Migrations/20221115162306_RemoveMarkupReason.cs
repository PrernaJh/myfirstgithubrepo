using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class RemoveMarkupReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkupReason",
                table: "PackageDetailRecords");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MarkupReason",
                table: "PackageDetailRecords",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);
        }
    }
}
