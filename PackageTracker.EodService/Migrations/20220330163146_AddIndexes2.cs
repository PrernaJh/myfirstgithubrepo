using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class AddIndexes2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_PackageId",
                table: "EodPackages",
                column: "PackageId");
            migrationBuilder.CreateIndex(
                name: "IX_PackageDetailRecords_PackageId",
                table: "PackageDetailRecords",
                column: "PackageId");
            migrationBuilder.CreateIndex(
                name: "IX_ReturnAsnRecords_ParcelId",
                table: "ReturnAsnRecords",
                column: "ParcelId");
            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_PackageId",
                table: "InvoiceRecords",
                column: "PackageId");
            migrationBuilder.CreateIndex(
                name: "IX_ExpenseRecords_BillingReference1_Product",
                table: "ExpenseRecords",
                columns: new [] { "BillingReference1", "Product" });
            migrationBuilder.CreateIndex(
                 name: "IX_EvsPackage_TrackingNumber",
                 table: "EvsPackage",
                 column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_ContainerId",
                table: "EodContainers",
                column: "ContainerId");
            migrationBuilder.CreateIndex(
                 name: "IX_EvsContainer_ContainerId",
                 table: "EvsContainer",
                 column: "ContainerId");
            migrationBuilder.CreateIndex(
                name: "IX_ContainerDetailRecords_TrackingNumber",
                table: "ContainerDetailRecords",
                column: "TrackingNumber");
            migrationBuilder.CreateIndex(
                name: "IX_PmodContainerDetailRecords_ContainerId",
                table: "PmodContainerDetailRecords",
                column: "ContainerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EodPackages_PackageId",
                 table: "EodPackages");
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
                 name: "IX_EodContainers_ContainerId",
                  table: "EodContainers");
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
    }
}

