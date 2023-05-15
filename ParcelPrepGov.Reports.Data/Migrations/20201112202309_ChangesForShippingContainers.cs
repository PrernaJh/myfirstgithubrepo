using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class ChangesForShippingContainers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ShippingContainerDatasets",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingContainerEventDataset",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetCreateDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    ContainerDatasetId = table.Column<int>(nullable: false),
                    ContainerId = table.Column<string>(maxLength: 32, nullable: true),
                    EventId = table.Column<int>(nullable: false),
                    EventType = table.Column<string>(maxLength: 24, nullable: true),
                    EventStatus = table.Column<string>(maxLength: 24, nullable: true),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocalEventDate = table.Column<DateTime>(nullable: false),
                    Username = table.Column<string>(maxLength: 32, nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true),
                    ShippingContainerDatasetId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingContainerEventDataset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingContainerEventDataset_ShippingContainerDatasets_ShippingContainerDatasetId",
                        column: x => x.ShippingContainerDatasetId,
                        principalTable: "ShippingContainerDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDataset_ShippingContainerDatasetId",
                table: "ShippingContainerEventDataset",
                column: "ShippingContainerDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDataset_CosmosId",
                table: "ShippingContainerEventDataset",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDataset_ContainerId",
                table: "ShippingContainerEventDataset",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerEventDataset_SiteName",
                table: "ShippingContainerEventDataset",
                column: "SiteName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingContainerEventDataset");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ShippingContainerDatasets");
        }
    }
}
