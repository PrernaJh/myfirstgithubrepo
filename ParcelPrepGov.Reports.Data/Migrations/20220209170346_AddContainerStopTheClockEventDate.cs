using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddContainerStopTheClockEventDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StopTheClockEventDate",
                table: "ShippingContainerDatasets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_StopTheClockEventDate",
                table: "ShippingContainerDatasets",
                column: "StopTheClockEventDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopTheClockEventDate",
                table: "ShippingContainerDatasets");
        }
    }
}
