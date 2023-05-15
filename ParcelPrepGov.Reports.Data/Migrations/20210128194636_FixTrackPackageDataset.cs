using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class FixTrackPackageDataset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets");

            migrationBuilder.AlterColumn<int>(
                name: "ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
               UPDATE [dbo].[TrackPackageDatasets]
                    SET PackageDatasetId = NULL
                        WHERE PackageDatasetId = 0
            ");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[TrackPackageDatasets]
                    SET ShippingContainerDatasetId = NULL
                        WHERE ShippingContainerDatasetId = 0
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
               UPDATE [dbo].[TrackPackageDatasets]
                    SET PackageDatasetId = 0
                        WHERE PackageDatasetId IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[TrackPackageDatasets]
                    SET ShippingContainerDatasetId = 0
                        WHERE ShippingContainerDatasetId IS NULL
            ");
            
            migrationBuilder.AlterColumn<int>(
                name: "ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PackageDatasetId",
                table: "TrackPackageDatasets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_PackageDatasetId",
                table: "TrackPackageDatasets",
                column: "PackageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_ShippingContainerDatasetId",
                table: "TrackPackageDatasets",
                column: "ShippingContainerDatasetId");
        }
    }
}
