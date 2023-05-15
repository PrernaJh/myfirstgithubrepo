using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddLastKnowFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastKnownEventDate",
                table: "PackageDatasets",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventDescription",
                table: "PackageDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventLocation",
                table: "PackageDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownEventZip",
                table: "PackageDatasets",
                maxLength: 10,
                type: "varchar(10)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastKnownEventDate",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventDescription",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventLocation",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "LastKnownEventZip",
                table: "PackageDatasets");
        }
    }
}
