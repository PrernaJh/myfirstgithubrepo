using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddShippingContainerEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingContainerEventDataset_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDataset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingContainerEventDataset",
                table: "ShippingContainerEventDataset");

            migrationBuilder.RenameTable(
                name: "ShippingContainerEventDataset",
                newName: "ShippingContainerEventDatasets");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingContainerEventDataset_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                newName: "IX_ShippingContainerEventDatasets_ShippingContainerDatasetId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingContainerEventDataset_CosmosId",
                table: "ShippingContainerEventDatasets",
                newName: "IX_ShippingContainerEventDatasets_CosmosId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingContainerEventDataset_ContainerId",
                table: "ShippingContainerEventDatasets",
                newName: "IX_ShippingContainerEventDatasets_ContainerId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingContainerEventDataset_SiteName",
                table: "ShippingContainerEventDatasets",
                newName: "IX_ShippingContainerEventDatasets_SiteName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingContainerEventDatasets",
                table: "ShippingContainerEventDatasets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets",
                column: "ShippingContainerDatasetId",
                principalTable: "ShippingContainerDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingContainerEventDatasets",
                table: "ShippingContainerEventDatasets");

            migrationBuilder.RenameTable(
                name: "ShippingContainerEventDatasets",
                newName: "ShippingContainerEventDataset");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingContainerEventDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDataset",
                newName: "IX_ShippingContainerEventDataset_ShippingContainerDatasetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingContainerEventDataset",
                table: "ShippingContainerEventDataset",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingContainerEventDataset_ShippingContainerDatasets_ShippingContainerDatasetId",
                table: "ShippingContainerEventDataset",
                column: "ShippingContainerDatasetId",
                principalTable: "ShippingContainerDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
