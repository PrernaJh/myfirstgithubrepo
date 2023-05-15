using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class IncreaseEventDescriptionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ShippingContainerEventDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PackageEventDatasets",
                maxLength: 120,
                type: "varchar(120)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldType: "varchar(100)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ShippingContainerEventDatasets",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 120,
                oldType: "varchar(120)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PackageEventDatasets",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 120,
                oldType: "varchar(120)",
                oldNullable: true);
        }
    }
}
