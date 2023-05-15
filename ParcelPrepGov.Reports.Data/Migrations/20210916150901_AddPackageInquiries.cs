using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddPackageInquiries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true, type: "varchar(24)"),
                    InquiryId = table.Column<int>(nullable: false),
                    PackageDatasetId = table.Column<int>(nullable: false),
                    PackageId = table.Column<string>(maxLength: 50, nullable: true, type: "varchar(50)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageInquiries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                 name: "IX_PackageInquiries_SiteName",
                 table: "PackageInquiries",
                 column: "SiteName");

            migrationBuilder.CreateIndex(
                   name: "IX_PackageInquiries_InquiryId",
                   table: "PackageInquiries",
                   column: "InquiryId");

            migrationBuilder.CreateIndex(
                 name: "IX_PackageInquiries_PackageId",
                 table: "PackageInquiries",
                 column: "PackageId");

            migrationBuilder.AddForeignKey(
               name: "FK_PackageInquiries_PackageDatasets_PackageDatasetId",
               table: "PackageInquiries",
               column: "PackageDatasetId",
               principalTable: "PackageDatasets",
               principalColumn: "Id",
               onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                  name: "IX_PackageInquiries_PackageDatasetId",
                  table: "PackageInquiries",
                  column: "PackageDatasetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageInquiries");
        }
    }
}
