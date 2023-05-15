using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelPrepGov.Reports.Migrations
{
    public partial class AddReleaseDateIndex : Migration
    {
        static string[] columns1 = { "SubClientName", "LocalProcesedDate" };
        static string[] columns2 = { "SubClientName", "RecallDate" };
        static string[] columns3 = { "SubClientName", "ReleaseDate" };
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_LocalProcesedDate",
                  table: "PackageDatasets",
                  columns: columns1);

            migrationBuilder.CreateIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_RecallDate",
                  table: "PackageDatasets",
                  columns: columns2);

            migrationBuilder.CreateIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_ReleaseDate",
                  table: "PackageDatasets",
                  columns: columns3);
       }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_LocalProcesedDate");
            migrationBuilder.DropIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_RecallDate");
            migrationBuilder.DropIndex(
                  name: "NCL_IDX_PackageDatasets_SubClientName_ReleaseDate");
        }
    }
}
