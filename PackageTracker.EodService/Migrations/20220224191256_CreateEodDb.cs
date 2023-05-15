using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PackageTracker.EodService.Migrations
{
    public partial class CreateEodDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerDetailRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ShipmentType = table.Column<string>(maxLength: 10, nullable: true),
                    PickupDate = table.Column<string>(maxLength: 10, nullable: true),
                    ShipReferenceNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ShipperAccount = table.Column<string>(maxLength: 10, nullable: true),
                    DestinationName = table.Column<string>(maxLength: 60, nullable: true),
                    DestinationAddress1 = table.Column<string>(maxLength: 120, nullable: true),
                    DestinationAddress2 = table.Column<string>(maxLength: 120, nullable: true),
                    DestinationCity = table.Column<string>(maxLength: 30, nullable: true),
                    DestinationState = table.Column<string>(maxLength: 30, nullable: true),
                    DestinationZip = table.Column<string>(maxLength: 10, nullable: true),
                    DropSiteKey = table.Column<string>(maxLength: 10, nullable: true),
                    OriginName = table.Column<string>(maxLength: 60, nullable: true),
                    OriginAddress1 = table.Column<string>(maxLength: 120, nullable: true),
                    OriginAddress2 = table.Column<string>(maxLength: 120, nullable: true),
                    OriginCity = table.Column<string>(maxLength: 30, nullable: true),
                    OriginState = table.Column<string>(maxLength: 30, nullable: true),
                    OriginZip = table.Column<string>(maxLength: 10, nullable: true),
                    Reference1 = table.Column<string>(maxLength: 24, nullable: true),
                    Reference2 = table.Column<string>(maxLength: 24, nullable: true),
                    Reference3 = table.Column<string>(maxLength: 24, nullable: true),
                    CarrierRoute1 = table.Column<string>(maxLength: 24, nullable: true),
                    CarrierRoute2 = table.Column<string>(maxLength: 24, nullable: true),
                    CarrierRoute3 = table.Column<string>(maxLength: 24, nullable: true),
                    Weight = table.Column<string>(maxLength: 24, nullable: true),
                    DeliveryDate = table.Column<string>(maxLength: 10, nullable: true),
                    ExtraSvcs1 = table.Column<string>(maxLength: 10, nullable: true),
                    ExtraSvcs2 = table.Column<string>(maxLength: 10, nullable: true),
                    ExtraSvcs3 = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerDetailRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvsContainer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 50, nullable: true),
                    ShippingCarrier = table.Column<int>(maxLength: 24, nullable: false),
                    ShippingMethod = table.Column<string>(maxLength: 60, nullable: true),
                    ContainerType = table.Column<int>(nullable: false),
                    CarrierBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    EntryZip = table.Column<string>(maxLength: 10, nullable: true),
                    EntryFacilityType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvsContainer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvsPackage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 50, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ServiceType = table.Column<int>(nullable: false),
                    ProcessingCategory = table.Column<int>(nullable: false),
                    Zone = table.Column<int>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false),
                    MailerId = table.Column<string>(maxLength: 24, nullable: true),
                    Cost = table.Column<decimal>(nullable: false),
                    IsPoBox = table.Column<bool>(nullable: false),
                    RecipientName = table.Column<string>(maxLength: 60, nullable: true),
                    AddressLine1 = table.Column<string>(maxLength: 120, nullable: true),
                    Zip = table.Column<string>(maxLength: 10, nullable: true),
                    ReturnAddressLine1 = table.Column<string>(maxLength: 120, nullable: true),
                    ReturnCity = table.Column<string>(maxLength: 60, nullable: true),
                    ReturnState = table.Column<string>(maxLength: 30, nullable: true),
                    ReturnZip = table.Column<string>(maxLength: 10, nullable: true),
                    EntryZip = table.Column<string>(maxLength: 10, nullable: true),
                    DestinationRateIndicator = table.Column<string>(maxLength: 1, nullable: true),
                    EntryFacilityType = table.Column<int>(nullable: false),
                    MailProducerCrid = table.Column<string>(maxLength: 10, nullable: true),
                    ParentMailOwnerMid = table.Column<string>(maxLength: 10, nullable: true),
                    UspsMailOwnerMid = table.Column<string>(maxLength: 10, nullable: true),
                    ParentMailOwnerCrid = table.Column<string>(maxLength: 10, nullable: true),
                    UspsMailOwnerCrid = table.Column<string>(maxLength: 10, nullable: true),
                    UspsPermitNo = table.Column<string>(maxLength: 10, nullable: true),
                    UspsPermitNoZip = table.Column<string>(maxLength: 10, nullable: true),
                    UspsPaymentMethod = table.Column<string>(maxLength: 2, nullable: true),
                    UspsPostageType = table.Column<string>(maxLength: 1, nullable: true),
                    UspsCsscNo = table.Column<string>(maxLength: 10, nullable: true),
                    UspsCsscProductNo = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvsPackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SubClientName = table.Column<string>(maxLength: 50, nullable: true),
                    ProcessingDate = table.Column<string>(maxLength: 10, nullable: true),
                    BillingReference1 = table.Column<string>(maxLength: 10, nullable: true),
                    Product = table.Column<string>(maxLength: 60, nullable: true),
                    TrackingType = table.Column<string>(maxLength: 60, nullable: true),
                    Cost = table.Column<decimal>(nullable: false),
                    ExtraServiceCost = table.Column<decimal>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false),
                    Zone = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    SubClientName = table.Column<string>(maxLength: 50, nullable: true),
                    PackageId = table.Column<string>(maxLength: 50, nullable: true),
                    BillingDate = table.Column<string>(maxLength: 10, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 100, nullable: true),
                    BillingReference1 = table.Column<string>(maxLength: 10, nullable: true),
                    BillingProduct = table.Column<string>(maxLength: 60, nullable: true),
                    BillingWeight = table.Column<string>(maxLength: 24, nullable: true),
                    Zone = table.Column<string>(maxLength: 10, nullable: true),
                    SigCost = table.Column<string>(maxLength: 24, nullable: true),
                    BillingCost = table.Column<string>(maxLength: 24, nullable: true),
                    Weight = table.Column<string>(maxLength: 24, nullable: true),
                    TotalCost = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageDetailRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    MmsLocation = table.Column<string>(maxLength: 50, nullable: true),
                    Customer = table.Column<string>(maxLength: 10, nullable: true),
                    ShipDate = table.Column<string>(maxLength: 10, nullable: true),
                    VamcId = table.Column<string>(maxLength: 10, nullable: true),
                    PackageId = table.Column<string>(maxLength: 50, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ShipMethod = table.Column<string>(maxLength: 60, nullable: true),
                    BillMethod = table.Column<string>(maxLength: 60, nullable: true),
                    EntryUnitType = table.Column<string>(maxLength: 10, nullable: true),
                    ShipCost = table.Column<string>(maxLength: 24, nullable: true),
                    BillingCost = table.Column<string>(maxLength: 24, nullable: true),
                    SignatureCost = table.Column<string>(maxLength: 24, nullable: true),
                    ShipZone = table.Column<string>(maxLength: 10, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 10, nullable: true),
                    Weight = table.Column<string>(maxLength: 24, nullable: true),
                    BillingWeight = table.Column<string>(maxLength: 24, nullable: true),
                    SortCode = table.Column<string>(maxLength: 60, nullable: true),
                    MarkupReason = table.Column<string>(maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDetailRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PmodContainerDetailRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    Site = table.Column<string>(maxLength: 50, nullable: true),
                    PdCust = table.Column<string>(maxLength: 50, nullable: true),
                    PdShipDate = table.Column<string>(maxLength: 10, nullable: true),
                    PdVamcId = table.Column<string>(maxLength: 10, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 50, nullable: true),
                    PdTrackingNum = table.Column<string>(maxLength: 100, nullable: true),
                    PdShipMethod = table.Column<string>(maxLength: 24, nullable: true),
                    PdBillMethod = table.Column<string>(maxLength: 24, nullable: true),
                    PdEntryUnitType = table.Column<string>(maxLength: 24, nullable: true),
                    PdShipCost = table.Column<string>(maxLength: 24, nullable: true),
                    PdBillingCost = table.Column<string>(maxLength: 24, nullable: true),
                    PdSigCost = table.Column<string>(maxLength: 24, nullable: true),
                    PdShipZone = table.Column<string>(maxLength: 10, nullable: true),
                    PdZip5 = table.Column<string>(maxLength: 10, nullable: true),
                    PdWeight = table.Column<string>(maxLength: 24, nullable: true),
                    PdBillingWeight = table.Column<string>(maxLength: 24, nullable: true),
                    PdSortCode = table.Column<string>(maxLength: 50, nullable: true),
                    PdMarkupReason = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PmodContainerDetailRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnAsnRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    ParcelId = table.Column<string>(maxLength: 50, nullable: true),
                    SiteCode = table.Column<string>(maxLength: 10, nullable: true),
                    PackageWeight = table.Column<string>(maxLength: 24, nullable: true),
                    ProductCode = table.Column<string>(maxLength: 10, nullable: true),
                    Over84Flag = table.Column<string>(maxLength: 1, nullable: true),
                    Over108Flag = table.Column<string>(maxLength: 1, nullable: true),
                    NonMachinableFlag = table.Column<string>(maxLength: 1, nullable: true),
                    DelCon = table.Column<string>(maxLength: 1, nullable: true),
                    Signature = table.Column<string>(maxLength: 1, nullable: true),
                    CustomerNumber = table.Column<string>(maxLength: 10, nullable: true),
                    BolNumber = table.Column<string>(maxLength: 50, nullable: true),
                    PackageCreateDateDayMonthYear = table.Column<string>(maxLength: 10, nullable: true),
                    PackageCreateDateHourMinuteSecond = table.Column<string>(maxLength: 10, nullable: true),
                    ZipDestination = table.Column<string>(maxLength: 10, nullable: true),
                    PackageBarcode = table.Column<string>(maxLength: 100, nullable: true),
                    Zone = table.Column<string>(maxLength: 10, nullable: true),
                    TotalShippingCharge = table.Column<string>(maxLength: 24, nullable: true),
                    ConfirmationSurcharge = table.Column<string>(maxLength: 24, nullable: true),
                    NonMachinableSurcharge = table.Column<string>(maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnAsnRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EodContainers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalProcessedDate = table.Column<DateTime>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(maxLength: 50, nullable: true),
                    ContainerId = table.Column<string>(maxLength: 50, nullable: true),
                    IsContainerClosed = table.Column<bool>(nullable: false),
                    ContainerDetailRecordId = table.Column<int>(nullable: true),
                    PmodContainerDetailRecordId = table.Column<int>(nullable: true),
                    ExpenseRecordId = table.Column<int>(nullable: true),
                    EvsContainerRecordId = table.Column<int>(nullable: true),
                    EvsPackageRecordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EodContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EodContainers_ContainerDetailRecords_ContainerDetailRecordId",
                        column: x => x.ContainerDetailRecordId,
                        principalTable: "ContainerDetailRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodContainers_EvsContainer_EvsContainerRecordId",
                        column: x => x.EvsContainerRecordId,
                        principalTable: "EvsContainer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodContainers_EvsPackage_EvsPackageRecordId",
                        column: x => x.EvsPackageRecordId,
                        principalTable: "EvsPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodContainers_ExpenseRecords_ExpenseRecordId",
                        column: x => x.ExpenseRecordId,
                        principalTable: "ExpenseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodContainers_PmodContainerDetailRecords_PmodContainerDetailRecordId",
                        column: x => x.PmodContainerDetailRecordId,
                        principalTable: "PmodContainerDetailRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EodPackages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalProcessedDate = table.Column<DateTime>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    CosmosId = table.Column<string>(maxLength: 36, nullable: true),
                    CosmosCreateDate = table.Column<DateTime>(nullable: false),
                    SiteName = table.Column<string>(maxLength: 50, nullable: true),
                    PackageId = table.Column<string>(maxLength: 50, nullable: true),
                    Barcode = table.Column<string>(maxLength: 100, nullable: true),
                    SubClientName = table.Column<string>(maxLength: 50, nullable: true),
                    IsPackageProcessed = table.Column<bool>(nullable: false),
                    PackageDetailRecordId = table.Column<int>(nullable: true),
                    ReturnAsnRecordId = table.Column<int>(nullable: true),
                    EvsPackageId = table.Column<int>(nullable: true),
                    InvoiceRecordId = table.Column<int>(nullable: true),
                    ExpenseRecordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EodPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EodPackages_EvsPackage_EvsPackageId",
                        column: x => x.EvsPackageId,
                        principalTable: "EvsPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodPackages_ExpenseRecords_ExpenseRecordId",
                        column: x => x.ExpenseRecordId,
                        principalTable: "ExpenseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodPackages_InvoiceRecords_InvoiceRecordId",
                        column: x => x.InvoiceRecordId,
                        principalTable: "InvoiceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodPackages_PackageDetailRecords_PackageDetailRecordId",
                        column: x => x.PackageDetailRecordId,
                        principalTable: "PackageDetailRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EodPackages_ReturnAsnRecords_ReturnAsnRecordId",
                        column: x => x.ReturnAsnRecordId,
                        principalTable: "ReturnAsnRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_ContainerDetailRecordId",
                table: "EodContainers",
                column: "ContainerDetailRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_EvsContainerRecordId",
                table: "EodContainers",
                column: "EvsContainerRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_EvsPackageRecordId",
                table: "EodContainers",
                column: "EvsPackageRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_ExpenseRecordId",
                table: "EodContainers",
                column: "ExpenseRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodContainers_PmodContainerDetailRecordId",
                table: "EodContainers",
                column: "PmodContainerDetailRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_EvsPackageId",
                table: "EodPackages",
                column: "EvsPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_ExpenseRecordId",
                table: "EodPackages",
                column: "ExpenseRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_InvoiceRecordId",
                table: "EodPackages",
                column: "InvoiceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_PackageDetailRecordId",
                table: "EodPackages",
                column: "PackageDetailRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EodPackages_ReturnAsnRecordId",
                table: "EodPackages",
                column: "ReturnAsnRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EodContainers");

            migrationBuilder.DropTable(
                name: "EodPackages");

            migrationBuilder.DropTable(
                name: "ContainerDetailRecords");

            migrationBuilder.DropTable(
                name: "EvsContainer");

            migrationBuilder.DropTable(
                name: "PmodContainerDetailRecords");

            migrationBuilder.DropTable(
                name: "EvsPackage");

            migrationBuilder.DropTable(
                name: "ExpenseRecords");

            migrationBuilder.DropTable(
                name: "InvoiceRecords");

            migrationBuilder.DropTable(
                name: "PackageDetailRecords");

            migrationBuilder.DropTable(
                name: "ReturnAsnRecords");
        }
    }
}
