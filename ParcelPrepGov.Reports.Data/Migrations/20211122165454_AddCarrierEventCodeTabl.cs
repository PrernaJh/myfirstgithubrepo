using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddCarrierEventCodeTabl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarrierEventCodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ShippingCarrier = table.Column<string>(maxLength: 50, type: "varchar(50)", nullable: true),
                    Code = table.Column<string>(maxLength: 10, type: "varchar(10)", nullable: true),
                    Description = table.Column<string>(maxLength: 50, type: "varchar(50)", nullable: true),
                    IsStopTheClock = table.Column<int>(nullable: false),
                    IsUndeliverable = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierEventCodes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarrierEventCodes");
        }
    }
}
