using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddFieldsToTrackPackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventCode",
                table: "TrackPackageDatasets",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventZip",
                table: "TrackPackageDatasets",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "EventZip",
                table: "TrackPackageDatasets");
        }
    }
}
