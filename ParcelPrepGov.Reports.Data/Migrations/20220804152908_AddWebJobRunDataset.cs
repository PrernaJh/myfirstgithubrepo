using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddWebJobRunDataset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebJobRunDatasets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetCreateDate = table.Column<DateTime>(nullable: false),
                    DatasetModifiedDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: true),
                    ClientName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    SubClientName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    JobName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    JobType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    ProcessedDate = table.Column<DateTime>(nullable: false),
                    FileName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    FileArchiveName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    NumberOfRecords = table.Column<int>(nullable: false),
                    Username = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    LocalCreateDate = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebJobRunDatasets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebJobRunDatasets");
        }
    }
}
