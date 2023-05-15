using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddUndeliverableEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UndeliverableEventDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetCreateDate = table.Column<DateTime>(nullable: false),
                    DatasetModifiedDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(maxLength: 36, type: "varchar(36)", nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(maxLength: 24, type: "varchar(24)", nullable: true),
                    PackageId = table.Column<string>(maxLength: 100, type: "varchar(100)", nullable: true),
                    PackageDatasetId = table.Column<int>(nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    EventCode = table.Column<string>(maxLength: 24, type: "varchar(24)", nullable: true),
                    EventDescription = table.Column<string>(maxLength: 120, type: "varchar(120)", nullable: true),
                    EventLocation = table.Column<string>(maxLength: 120, type: "varchar(120)", nullable: true),
                    EventZip = table.Column<string>(maxLength: 10, type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndeliverableEventDatasets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UndeliverableEventDatasets_PackageDatasets_PackageDatasetId",
                        column: x => x.PackageDatasetId,
                        principalTable: "PackageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UndeliverableEventDatasets_PackageDatasetId",
                table: "UndeliverableEventDatasets",
                column: "PackageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_UndeliverableEventDatasets_CosmosId",
                table: "UndeliverableEventDatasets",
                column: "CosmosId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UndeliverableEventDatasets");
        }
    }
}
