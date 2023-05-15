using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddLastKnownContainerFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastKnownEventDate",
                table: "ShippingContainerDatasets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventDescription",
                table: "ShippingContainerDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventLocation",
                table: "ShippingContainerDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventZip",
                table: "ShippingContainerDatasets",
                maxLength: 10,
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.CreateIndex(
                 name: "IX_ShippingContainerDatasets_LastKnownEventDate",
                 table: "ShippingContainerDatasets",
                 column: "LastKnownEventDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastKnownEventDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventDescription",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventLocation",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventZip",
                table: "ShippingContainerDatasets");
        }
    }
}
