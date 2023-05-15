using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddShippingContainerFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOutside48States",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRural",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSaturdayDelivery",
                table: "ShippingContainerDatasets",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOutside48States",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "IsRural",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "IsSaturdayDelivery",
                table: "ShippingContainerDatasets");
        }
    }
}
