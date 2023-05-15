using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddHumanReadableBarcode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HumanReadableBarcode",
                table: "PackageDatasets",
                maxLength: 100,
                nullable: true);
            migrationBuilder.CreateIndex(
                 name: "IX_PackageDatasets_HumanReadableBarcode",
                 table: "PackageDatasets",
                 column: "HumanReadableBarcode");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[PackageDatasets]
	            SET HumanReadableBarcode = SUBSTRING(ShippingBarcode, 9, 26)
		            WHERE HumanReadableBarcode IS NULL AND LEN(ShippingBarcode) = 34
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PackageDatasets_HumanReadableBarcode",
                table: "PackageDatasets");
            migrationBuilder.DropColumn(
                    name: "HumanReadableBarcode",
                    table: "PackageDatasets");
        }
    }
}
