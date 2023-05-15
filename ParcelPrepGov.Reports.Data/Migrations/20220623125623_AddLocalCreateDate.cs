using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddLocalCreateDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LocalCreateDate",
                table: "ShippingContainerDatasets",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceRequestNumber",
                table: "PackageInquiries",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalCreateDate",
                table: "ShippingContainerDatasets");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceRequestNumber",
                table: "PackageInquiries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
