using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddProcessedCols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessedEventType",
                table: "PackageDatasets",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedMachineId",
                table: "PackageDatasets",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedUsername",
                table: "PackageDatasets",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedEventType",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "ProcessedMachineId",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "ProcessedUsername",
                table: "PackageDatasets");
        }
    }
}
