using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PostalAreasAndDistricts_CreateDate",
                table: "PostalAreasAndDistricts",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_PostalAreasAndDistricts_ZipCode3Zip",
                table: "PostalAreasAndDistricts",
                column: "ZipCode3Zip");

            migrationBuilder.CreateIndex(
                name: "IX_VisnSites_CreateDate",
                table: "VisnSites",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_VisnSites_Visn",
                table: "VisnSites",
                column: "Visn");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostalAreasAndDistricts_CreateDate",
                table: "PostalAreasAndDistricts");

            migrationBuilder.DropIndex(
                name: "IX_PostalAreasAndDistricts_ZipCode3Zip",
                table: "PostalAreasAndDistricts");

            migrationBuilder.DropIndex(
                name: "IX_VisnSites_CreateDate",
                table: "CreateDate");

            migrationBuilder.DropIndex(
                name: "IX_VisnSites_Visn",
                table: "VisnSites");
        }
    }
}
