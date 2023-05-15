using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddTrackPackageContainerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingContainerId",
                table: "TrackPackageDatasets",
                maxLength: 18,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerId",
                table: "TrackPackageDatasets",
                column: "ShippingContainerId");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingContainerId",
                table: "TrackPackageDatasets");
        }
    }
}
