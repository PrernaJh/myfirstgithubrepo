using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.Identity.Data.Migrations
{
    public partial class AddSendRecallReleaseAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendRecallReleaseAlerts",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendRecallReleaseAlerts",
                table: "Users");
        }
    }
}
