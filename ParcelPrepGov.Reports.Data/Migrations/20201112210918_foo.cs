using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class foo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.DropColumn(
                name: "ContainerDatasetId",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.AlterColumn<int>(
                name: "ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                column: "ShippingContainerDatasetId",
                principalTable: "ShippingContainerDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.AlterColumn<int>(
                name: "ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                column: "ShippingContainerDatasetId",
                principalTable: "ShippingContainerDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
