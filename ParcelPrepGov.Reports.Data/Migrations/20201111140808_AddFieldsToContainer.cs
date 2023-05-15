using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddFieldsToContainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BinActiveGroupId",
                table: "ShippingContainerDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecondaryCarrier",
                table: "ShippingContainerDatasets",
                maxLength: 32,
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BinActiveGroupId",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "IsSecondaryCarrier",
                table: "ShippingContainerDatasets");
        }
    }
}
