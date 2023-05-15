using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_CosmosId",
                table: "EodPackages",
                column: "CosmosId");
            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_SiteName",
                table: "EodPackages",
                column: "SiteName");
            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_LocalProcessedDate",
                table: "EodPackages",
                column: "LocalProcessedDate");
            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_IsPackageProcessed",
                table: "EodPackages",
                column: "IsPackageProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_CosmosId",
                table: "EodContainers",
                column: "CosmosId");
            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_SiteName",
                table: "EodContainers",
                column: "SiteName");
            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_LocalProcessedDate",
                table: "EodContainers",
                column: "LocalProcessedDate");
            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_IsContainerClosed",
                table: "EodContainers",
                column: "IsContainerClosed");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDetailRecords_CosmosId",
                table: "PackageDetailRecords",
                column: "CosmosId");
            migrationBuilder.CreateIndex(
                name: "IX_ReturnAsnRecords_CosmosId",
                table: "ReturnAsnRecords",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerDetailRecords_CosmosId",
                table: "ContainerDetailRecords",
                column: "CosmosId");
            migrationBuilder.CreateIndex(
                name: "IX_PmodContainerDetailRecords_CosmosId",
                table: "PmodContainerDetailRecords",
                column: "CosmosId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRecords_CosmosId",
                table: "InvoiceRecords",
                column: "CosmosId");
            migrationBuilder.CreateIndex(
                name: "IX_ExpenseRecords_CosmosId",
                table: "ExpenseRecords",
                column: "CosmosId");


            migrationBuilder.CreateIndex(
                 name: "IX_EvsPackage_CosmosId",
                 table: "EvsPackage",
                 column: "CosmosId");
            migrationBuilder.CreateIndex(
                 name: "IX_EvsContainer_CosmosId",
                 table: "EvsContainer",
                 column: "CosmosId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EodPackages_CosmosId",
                 table: "EodPackages");
            migrationBuilder.DropIndex(
                name: "IX_EodPackages_SiteName",
                 table: "EodPackages");
            migrationBuilder.DropIndex(
                 name: "IX_EodPackages_LocalProcessedDate",
                 table: "EodPackages");

            migrationBuilder.DropIndex(
                 name: "IX_EodContainers_CosmosId",
                  table: "EodContainers");
            migrationBuilder.DropIndex(
                name: "IX_EodContainers_SiteName",
                 table: "EodContainers");
            migrationBuilder.DropIndex(
                 name: "IX_EodContainers_LocalProcessedDate",
                 table: "EodContainers");

            migrationBuilder.DropIndex(
                 name: "IX_PackageDetailRecords_CosmosId",
                  table: "PackageDetailRecords");
            migrationBuilder.DropIndex(
                 name: "IX_ReturnAsnRecords_CosmosId",
                  table: "ReturnAsnRecords");

            migrationBuilder.DropIndex(
                 name: "IX_InvoiceRecords_CosmosId",
                  table: "InvoiceRecords");
            migrationBuilder.DropIndex(
                 name: "IX_ExpenseRecords_CosmosId",
                  table: "ExpenseRecords");

            migrationBuilder.DropIndex(
                  name: "IX_ContainerDetailRecords_CosmosId",
                   table: "ContainerDetailRecordRecords");
            migrationBuilder.DropIndex(
                 name: "IX_PmodContainerDetailRecords_CosmosId",
                  table: "PmodContainerDetailRecords");

            migrationBuilder.DropIndex(
                  name: "IX_EvsPackage_CosmosId",
                   table: "EvsPackage");
            migrationBuilder.DropIndex(
                  name: "IX_EvsContainer_CosmosId",
                   table: "EvsContainer");
        }
    }
}
