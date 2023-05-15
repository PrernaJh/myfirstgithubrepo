using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddRecallStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecallStatus",
                type: "varchar(24)",
                table: "PackageDatasets",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecallStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(maxLength: 24, type: "varchar(24)", nullable: true),
                    Description = table.Column<string>(maxLength: 80, type: "varchar(80)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallStatuses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecallStatuses");

            migrationBuilder.DropColumn(
                name: "RecallStatus",
                table: "PackageDatasets");
        }
    }
}
