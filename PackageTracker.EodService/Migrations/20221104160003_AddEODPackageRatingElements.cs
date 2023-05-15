using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class AddEODPackageRatingElements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsOutside48States",
                table: "ReturnAsnRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsRural",
                table: "ReturnAsnRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkupType",
                table: "ReturnAsnRecords",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkupType",
                table: "PackageDetailRecords",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsOutside48States",
                table: "PackageDetailRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsRural",
                table: "PackageDetailRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsOutside48States",
                table: "InvoiceRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsRural",
                table: "InvoiceRecords",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkupType",
                table: "InvoiceRecords",
                maxLength: 24,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOutside48States",
                table: "ReturnAsnRecords");

            migrationBuilder.DropColumn(
                name: "IsRural",
                table: "ReturnAsnRecords");

            migrationBuilder.DropColumn(
                name: "MarkupType",
                table: "ReturnAsnRecords");

            migrationBuilder.DropColumn(
                name: "IsOutside48States",
                table: "PackageDetailRecords");

            migrationBuilder.DropColumn(
                name: "IsRural",
                table: "PackageDetailRecords");

            migrationBuilder.DropColumn(
                name: "MarkupType",
                table: "PackageDetailRecords");

            migrationBuilder.DropColumn(
                name: "IsOutside48States",
                table: "InvoiceRecords");

            migrationBuilder.DropColumn(
                name: "IsRural",
                table: "InvoiceRecords");

            migrationBuilder.DropColumn(
                name: "MarkupType",
                table: "InvoiceRecords");
        }
    }
}
