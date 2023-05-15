using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddVisnAndPostalData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostalAreasAndDistricts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ZipCode3Zip = table.Column<string>(maxLength: 3, nullable: true),
                    Scf = table.Column<int>(nullable: false),
                    PostalDistrict = table.Column<string>(maxLength: 30, nullable: true),
                    PostalArea = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalAreasAndDistricts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisnSites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Visn = table.Column<string>(maxLength: 12, nullable: true),
                    SiteParent = table.Column<string>(maxLength: 12, nullable: true),
                    SiteNumber = table.Column<string>(maxLength: 12, nullable: true),
                    SiteType = table.Column<string>(maxLength: 12, nullable: true),
                    SiteName = table.Column<string>(maxLength: 60, nullable: true),
                    SiteAddress1 = table.Column<string>(maxLength: 60, nullable: true),
                    SiteAddress2 = table.Column<string>(maxLength: 60, nullable: true),
                    SiteCity = table.Column<string>(maxLength: 30, nullable: true),
                    SiteState = table.Column<string>(maxLength: 2, nullable: true),
                    SiteZipCode = table.Column<string>(maxLength: 10, nullable: true),
                    SitePhone = table.Column<string>(maxLength: 24, nullable: true),
                    SiteShippingContact = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisnSites", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalAreasAndDistricts");

            migrationBuilder.DropTable(
                name: "VisnSites");
        }
    }
}
