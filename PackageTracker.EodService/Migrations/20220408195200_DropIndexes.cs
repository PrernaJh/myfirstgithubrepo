using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class DropIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_PackageDetailRecords_PackageId",
                  table: "PackageDetailRecords");
            migrationBuilder.DropIndex(
                 name: "IX_ReturnAsnRecords_ParcelId",
                  table: "ReturnAsnRecords");
            migrationBuilder.DropIndex(
                 name: "IX_InvoiceRecords_PackageId",
                  table: "InvoiceRecords");
            migrationBuilder.DropIndex(
                 name: "IX_ExpenseRecords_BillingReference1_Product",
                  table: "ExpenseRecords");
            migrationBuilder.DropIndex(
                 name: "IX_EvsPackage_TrackingNumber",
                  table: "EvsPackage");

            migrationBuilder.DropIndex(
                  name: "IX_EvsContainer_ContainerId",
                   table: "EvsContainer");
            migrationBuilder.DropIndex(
                  name: "IX_ContainerDetailRecords_TrackingNumber",
                   table: "ContainerDetailRecords");
            migrationBuilder.DropIndex(
                 name: "IX_PmodContainerDetailRecords_ContainerId",
                  table: "PmodContainerDetailRecords");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
