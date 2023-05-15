using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddShippingCarrierSecondary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrierSecondary",
                table: "BinDatasets",
                type: "varchar",
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BinDatasets_ShippingCarrierSecondary",
                table: "BinDatasets",
                column: "ShippingCarrierSecondary");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCarrierSecondary",
                table: "BinDatasets");
        }
    }
}
