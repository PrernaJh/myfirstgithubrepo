using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddSubClientDatasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubClientDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetCreateDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 50, nullable: true),
                    Key = table.Column<string>(maxLength: 50, nullable: true),
                    ClientName = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubClientDatasets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubClientDatasets");
        }
    }
}
