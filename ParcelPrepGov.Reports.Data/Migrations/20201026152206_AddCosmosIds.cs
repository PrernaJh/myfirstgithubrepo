using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddCosmosIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CosmosId",
                table: "JobDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CosmosId",
                table: "PackageDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CosmosId",
                table: "ShippingContainerDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CosmosId",
                table: "TrackPackageDatasets",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_CosmosId",
                table: "JobDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_CosmosId",
                table: "PackageDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_CosmosId",
                table: "ShippingContainerDatasets",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_CosmosId",
                table: "TrackPackageDatasets",
                column: "CosmosId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobDatasets_CosmosId",
                table: "JobDatasets");

            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_CosmosId",
                table: "PackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_ShippingContainerDatasets_CosmosId",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDataSets_CosmosId",
                table: "TrackPackageDataSets");

            migrationBuilder.DropColumn(
                name: "CosmosId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosId",
                table: "ShippingContainerDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosId",
                table: "PackageDatasets");

            migrationBuilder.DropColumn(
                name: "CosmosId",
                table: "JobDatasets");
        }
    }
}
