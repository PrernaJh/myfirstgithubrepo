using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddPostalAndCalendarDays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalendarDays",
                table: "PackageDatasets",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostalDays",
                table: "PackageDatasets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarDays",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "PostalDays",
                table: "PackageDatasets");
        }
    }
}
