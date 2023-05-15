using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class CreatePostalDaysTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostalDays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostalDate = table.Column<DateTime>(nullable: false),
                    Ordinal = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 50, nullable: true),
                    IsHoliday = table.Column<int>(nullable: false),
                    IsSunday = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalDays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                 name: "IX_PostalDays_PostalDate",
                 table: "PostalDays",
                 column: "PostalDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalDays");
        }
    }
}
