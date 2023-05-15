using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddTrackPackageDatasets2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.AddColumn<string>(
                name: "PackageId",
                table: "TrackPackageDatasets",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_PackageId",
                 table: "PackageDatasets",
                 column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_PackageId",
                table: "TrackPackageDatasets",
                column: "PackageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_PackageId",
                 table: "PackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_PackageId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "TrackPackageDatasets");

            migrationBuilder.AddColumn<int>(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
