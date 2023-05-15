using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddDateIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_StopTheClockEventDate",
                table: "PackageDatasets",
                column: "StopTheClockEventDate");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_LastKnownEventDate",
                table: "PackageDatasets",
                column: "LastKnownEventDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_StopTheClockEventDate",
                table: "PackageDatasets");
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_LastKnownEventDate",
                table: "PackageDatasets");
        }
    }
}
