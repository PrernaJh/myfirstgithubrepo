using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class DropForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EodContainers_ContainerDetailRecordId",
                table: "EodContainers");

            migrationBuilder.DropIndex(
                name: "IX_EodContainers_EvsContainerRecordId",
                table: "EodContainers");

            migrationBuilder.DropIndex(
                name: "IX_EodContainers_EvsPackageRecordId",
                table: "EodContainers");

            migrationBuilder.DropIndex(
                name: "IX_EodContainers_ExpenseRecordId",
                table: "EodContainers");

            migrationBuilder.DropIndex(
                name: "IX_EodContainers_PmodContainerDetailRecordId",
                table: "EodContainers");

            migrationBuilder.DropIndex(
                name: "IX_EodPackages_EvsPackageId",
                table: "EodPackages");

            migrationBuilder.DropIndex(
                name: "IX_EodPackages_InvoiceRecordId",
                table: "EodPackages");

            migrationBuilder.DropIndex(
                name: "IX_EodPackages_ExpenseRecordId",
                table: "EodPackages");

            migrationBuilder.DropIndex(
                name: "IX_EodPackages_PackageDetailRecordId",
                table: "EodPackages");

            migrationBuilder.DropIndex(
                name: "IX_EodPackages_ReturnAsnRecordId",
                table: "EodPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_EodContainers_ContainerDetailRecords_ContainerDetailRecordId",
                table: "EodContainers");

            migrationBuilder.DropForeignKey(
               name: "FK_EodContainers_EvsContainer_EvsContainerRecordId",
               table: "EodContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_EodContainers_EvsPackage_EvsPackageRecordId",
                table: "EodContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_EodContainers_ExpenseRecords_ExpenseRecordId",
                table: "EodContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_EodContainers_PmodContainerDetailRecords_PmodContainerDetailRecordId",
                table: "EodContainers");

            migrationBuilder.DropForeignKey(
               name: "FK_EodPackages_EvsPackage_EvsPackageId",
               table: "EodPackages");

            migrationBuilder.DropForeignKey(
               name: "FK_EodPackages_ExpenseRecords_ExpenseRecordId",
               table: "EodPackages");

            migrationBuilder.DropForeignKey(
               name: "FK_EodPackages_InvoiceRecords_InvoiceRecordId",
               table: "EodPackages");

            migrationBuilder.DropForeignKey(
               name: "FK_EodPackages_PackageDetailRecords_PackageDetailRecordId",
               table: "EodPackages");

            migrationBuilder.DropForeignKey(
               name: "FK_EodPackages_ReturnAsnRecords_ReturnAsnRecordId",
               table: "EodPackages");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
