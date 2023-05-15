using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddJobAndShippingContainerDatasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "TrackPackageDatasets",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(24)",
                oldMaxLength: 24,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "TrackPackageDatasets",
                maxLength: 24,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SiteName",
                table: "TrackPackageDatasets",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "PackageDatasets",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubClientName",
                table: "PackageDatasets",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BinCode",
                table: "PackageDatasets",
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "PackageDatasets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "JobDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobBarcode = table.Column<string>(maxLength: 24, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    CreateDate = table.Column<DateTime>(maxLength: 30, nullable: false),
                    SubClientName = table.Column<string>(nullable: true),
                    ManifestDate = table.Column<string>(maxLength: 10, nullable: true),
                    MarkUpType = table.Column<string>(maxLength: 24, nullable: true),
                    MarkUp = table.Column<string>(maxLength: 24, nullable: true),
                    Product = table.Column<string>(maxLength: 16, nullable: true),
                    PackageType = table.Column<string>(maxLength: 16, nullable: true),
                    PackageDescription = table.Column<string>(maxLength: 100, nullable: true),
                    Length = table.Column<decimal>(nullable: false),
                    Width = table.Column<decimal>(nullable: false),
                    Depth = table.Column<decimal>(nullable: false),
                    MailTypeCode = table.Column<string>(maxLength: 16, nullable: true),
                    Username = table.Column<string>(maxLength: 32, nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerId = table.Column<string>(maxLength: 18, nullable: true),
                    SiteName = table.Column<string>(maxLength: 24, nullable: true),
                    CreateDate = table.Column<DateTime>(maxLength: 24, nullable: false),
                    BinCode = table.Column<string>(nullable: true),
                    BinCodeSecondary = table.Column<string>(maxLength: 24, nullable: true),
                    ShippingMethod = table.Column<string>(maxLength: 60, nullable: true),
                    ShippingCarrier = table.Column<string>(maxLength: 24, nullable: true),
                    ContainerType = table.Column<string>(maxLength: 24, nullable: true),
                    Grouping = table.Column<string>(maxLength: 2, nullable: true),
                    Weight = table.Column<string>(maxLength: 16, nullable: true),
                    UpdatedBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    Cost = table.Column<decimal>(maxLength: 32, nullable: false),
                    Charge = table.Column<decimal>(nullable: false),
                    Zone = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    MachineId = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingContainerDatasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobContainerDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobDatasetId = table.Column<int>(nullable: false),
                    NumberOfContainers = table.Column<int>(nullable: false),
                    Weight = table.Column<string>(maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobContainerDatasets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobContainerDatasets_JobDatasets_JobDatasetId",
                        column: x => x.JobDatasetId,
                        principalTable: "JobDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

           migrationBuilder.CreateIndex(
                name: "IX_JobContainerDatasets_JobDatasetId",
                table: "JobContainerDatasets",
                column: "JobDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_JobBarcode",
                table: "JobDatasets",
                column: "JobBarcode");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_ContainerId",
                table: "ShippingContainerDatasets",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDatasets_TrackingNumber",
                table: "TrackPackageDatasets",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_JobDatasets_SiteName",
                table: "JobDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDatasets_SiteName",
                table: "PackageDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingContainerDatasets_SiteName",
                table: "ShippingContainerDatasets",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageDataSets_SiteName",
                table: "TrackPackageDataSets",
                column: "SiteName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                 name: "IX_JobDatasets_JobBarcode",
                 table: "JobDatasets");
            
            migrationBuilder.DropIndex(
                 name: "IX_ShippingContainerDatasets_ContainerId",
                 table: "ShippingContainerDatasets");
            
            migrationBuilder.DropIndex(
                 name: "IX_TrackPackageDatasets_TrackingNumber",
                 table: "TrackPackageDatasets");

            migrationBuilder.DropIndex(
                 name: "IX_JobDatasets_SiteName",
                 table: "JobDatasets");
            
            migrationBuilder.DropIndex(
                 name: "IX_PackageDatasets_SiteName",
                 table: "PackageDatasets");

            migrationBuilder.DropIndex(
                 name: "IX_ShippingContainerDatasets_SiteName",
                 table: "ShippingContainerDatasets");

            migrationBuilder.DropIndex(
                 name: "IX_TrackPackageDataSets_SiteName",
                 table: "TrackPackageDataSets");

            migrationBuilder.DropTable(
                name: "JobContainerDatasets");

            migrationBuilder.DropTable(
                name: "ShippingContainerDatasets");

            migrationBuilder.DropTable(
                name: "JobDatasets");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "SiteName",
                table: "TrackPackageDatasets");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "PackageDatasets");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCarrier",
                table: "TrackPackageDatasets",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "PackageDatasets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubClientName",
                table: "PackageDatasets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BinCode",
                table: "PackageDatasets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 24,
                oldNullable: true);
        }
    }
}
