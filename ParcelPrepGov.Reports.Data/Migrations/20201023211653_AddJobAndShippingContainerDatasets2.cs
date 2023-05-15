using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddJobAndShippingContainerDatasets2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobBarcode",
                table: "JobContainerDatasets",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_JobBarcode",
                table: "JobContainerDatasets",
                column: "JobBarcode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_JobContainerDatasets_JobBarcode",
                 table: "JobContainerDatasets");

            migrationBuilder.DropColumn(
                name: "JobBarcode",
                table: "JobContainerDatasets");
        }
    }
}
