IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] varchar(150) NOT NULL,
        [ProductVersion] varchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [ContainerDetailRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [TrackingNumber] varchar(100) NULL,
    [ShipmentType] varchar(10) NULL,
    [PickupDate] varchar(10) NULL,
    [ShipReferenceNumber] varchar(100) NULL,
    [ShipperAccount] varchar(10) NULL,
    [DestinationName] varchar(60) NULL,
    [DestinationAddress1] varchar(120) NULL,
    [DestinationAddress2] varchar(120) NULL,
    [DestinationCity] varchar(30) NULL,
    [DestinationState] varchar(30) NULL,
    [DestinationZip] varchar(10) NULL,
    [DropSiteKey] varchar(10) NULL,
    [OriginName] varchar(60) NULL,
    [OriginAddress1] varchar(120) NULL,
    [OriginAddress2] varchar(120) NULL,
    [OriginCity] varchar(30) NULL,
    [OriginState] varchar(30) NULL,
    [OriginZip] varchar(10) NULL,
    [Reference1] varchar(24) NULL,
    [Reference2] varchar(24) NULL,
    [Reference3] varchar(24) NULL,
    [CarrierRoute1] varchar(24) NULL,
    [CarrierRoute2] varchar(24) NULL,
    [CarrierRoute3] varchar(24) NULL,
    [Weight] varchar(24) NULL,
    [DeliveryDate] varchar(10) NULL,
    [ExtraSvcs1] varchar(10) NULL,
    [ExtraSvcs2] varchar(10) NULL,
    [ExtraSvcs3] varchar(10) NULL,
    CONSTRAINT [PK_ContainerDetailRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [EvsContainer] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [ContainerId] varchar(50) NULL,
    [ShippingCarrier] int NOT NULL,
    [ShippingMethod] varchar(60) NULL,
    [ContainerType] int NOT NULL,
    [CarrierBarcode] varchar(100) NULL,
    [EntryZip] varchar(10) NULL,
    [EntryFacilityType] int NOT NULL,
    CONSTRAINT [PK_EvsContainer] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [EvsPackage] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [ContainerId] varchar(50) NULL,
    [TrackingNumber] varchar(100) NULL,
    [ServiceType] int NOT NULL,
    [ProcessingCategory] int NOT NULL,
    [Zone] int NOT NULL,
    [Weight] decimal(18,4) NOT NULL,
    [MailerId] varchar(24) NULL,
    [Cost] decimal(18,4) NOT NULL,
    [IsPoBox] bit NOT NULL,
    [RecipientName] varchar(60) NULL,
    [AddressLine1] varchar(120) NULL,
    [Zip] varchar(10) NULL,
    [ReturnAddressLine1] varchar(120) NULL,
    [ReturnCity] varchar(60) NULL,
    [ReturnState] varchar(30) NULL,
    [ReturnZip] varchar(10) NULL,
    [EntryZip] varchar(10) NULL,
    [DestinationRateIndicator] varchar(1) NULL,
    [EntryFacilityType] int NOT NULL,
    [MailProducerCrid] varchar(10) NULL,
    [ParentMailOwnerMid] varchar(10) NULL,
    [UspsMailOwnerMid] varchar(10) NULL,
    [ParentMailOwnerCrid] varchar(10) NULL,
    [UspsMailOwnerCrid] varchar(10) NULL,
    [UspsPermitNo] varchar(10) NULL,
    [UspsPermitNoZip] varchar(10) NULL,
    [UspsPaymentMethod] varchar(2) NULL,
    [UspsPostageType] varchar(1) NULL,
    [UspsCsscNo] varchar(10) NULL,
    [UspsCsscProductNo] varchar(10) NULL,
    CONSTRAINT [PK_EvsPackage] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [ExpenseRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [SubClientName] varchar(50) NULL,
    [ProcessingDate] varchar(10) NULL,
    [BillingReference1] varchar(10) NULL,
    [Product] varchar(60) NULL,
    [TrackingType] varchar(60) NULL,
    [Cost] decimal(18,4) NOT NULL,
    [ExtraServiceCost] decimal(18,4) NOT NULL,
    [Weight] decimal(18,4) NOT NULL,
    [Zone] int NOT NULL,
    CONSTRAINT [PK_ExpenseRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [InvoiceRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [SubClientName] varchar(50) NULL,
    [PackageId] varchar(50) NULL,
    [BillingDate] varchar(10) NULL,
    [TrackingNumber] varchar(100) NULL,
    [BillingReference1] varchar(10) NULL,
    [BillingProduct] varchar(60) NULL,
    [BillingWeight] varchar(24) NULL,
    [Zone] varchar(10) NULL,
    [SigCost] varchar(24) NULL,
    [BillingCost] varchar(24) NULL,
    [Weight] varchar(24) NULL,
    [TotalCost] varchar(24) NULL,
    CONSTRAINT [PK_InvoiceRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [PackageDetailRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [MmsLocation] varchar(50) NULL,
    [Customer] varchar(10) NULL,
    [ShipDate] varchar(10) NULL,
    [VamcId] varchar(10) NULL,
    [PackageId] varchar(50) NULL,
    [TrackingNumber] varchar(100) NULL,
    [ShipMethod] varchar(60) NULL,
    [BillMethod] varchar(60) NULL,
    [EntryUnitType] varchar(10) NULL,
    [ShipCost] varchar(24) NULL,
    [BillingCost] varchar(24) NULL,
    [SignatureCost] varchar(24) NULL,
    [ShipZone] varchar(10) NULL,
    [ZipCode] varchar(10) NULL,
    [Weight] varchar(24) NULL,
    [BillingWeight] varchar(24) NULL,
    [SortCode] varchar(60) NULL,
    [MarkupReason] varchar(180) NULL,
    CONSTRAINT [PK_PackageDetailRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [PmodContainerDetailRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [Site] varchar(50) NULL,
    [PdCust] varchar(50) NULL,
    [PdShipDate] varchar(10) NULL,
    [PdVamcId] varchar(10) NULL,
    [ContainerId] varchar(50) NULL,
    [PdTrackingNum] varchar(100) NULL,
    [PdShipMethod] varchar(24) NULL,
    [PdBillMethod] varchar(24) NULL,
    [PdEntryUnitType] varchar(24) NULL,
    [PdShipCost] varchar(24) NULL,
    [PdBillingCost] varchar(24) NULL,
    [PdSigCost] varchar(24) NULL,
    [PdShipZone] varchar(10) NULL,
    [PdZip5] varchar(10) NULL,
    [PdWeight] varchar(24) NULL,
    [PdBillingWeight] varchar(24) NULL,
    [PdSortCode] varchar(50) NULL,
    [PdMarkupReason] varchar(24) NULL,
    CONSTRAINT [PK_PmodContainerDetailRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [ReturnAsnRecords] (
    [Id] int NOT NULL IDENTITY,
    [CosmosId] varchar(36) NULL,
    [ParcelId] varchar(50) NULL,
    [SiteCode] varchar(10) NULL,
    [PackageWeight] varchar(24) NULL,
    [ProductCode] varchar(10) NULL,
    [Over84Flag] varchar(1) NULL,
    [Over108Flag] varchar(1) NULL,
    [NonMachinableFlag] varchar(1) NULL,
    [DelCon] varchar(1) NULL,
    [Signature] varchar(1) NULL,
    [CustomerNumber] varchar(10) NULL,
    [BolNumber] varchar(50) NULL,
    [PackageCreateDateDayMonthYear] varchar(10) NULL,
    [PackageCreateDateHourMinuteSecond] varchar(10) NULL,
    [ZipDestination] varchar(10) NULL,
    [PackageBarcode] varchar(100) NULL,
    [Zone] varchar(10) NULL,
    [TotalShippingCharge] varchar(24) NULL,
    [ConfirmationSurcharge] varchar(24) NULL,
    [NonMachinableSurcharge] varchar(24) NULL,
    CONSTRAINT [PK_ReturnAsnRecords] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [EodContainers] (
    [Id] int NOT NULL IDENTITY,
    [LocalProcessedDate] datetime2 NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [CosmosId] varchar(36) NULL,
    [CosmosCreateDate] datetime2 NOT NULL,
    [SiteName] varchar(50) NULL,
    [ContainerId] varchar(50) NULL,
    [IsContainerClosed] bit NOT NULL,
    [ContainerDetailRecordId] int NULL,
    [PmodContainerDetailRecordId] int NULL,
    [ExpenseRecordId] int NULL,
    [EvsContainerRecordId] int NULL,
    [EvsPackageRecordId] int NULL,
    CONSTRAINT [PK_EodContainers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EodContainers_ContainerDetailRecords_ContainerDetailRecordId] FOREIGN KEY ([ContainerDetailRecordId]) REFERENCES [ContainerDetailRecords] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodContainers_EvsContainer_EvsContainerRecordId] FOREIGN KEY ([EvsContainerRecordId]) REFERENCES [EvsContainer] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodContainers_EvsPackage_EvsPackageRecordId] FOREIGN KEY ([EvsPackageRecordId]) REFERENCES [EvsPackage] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodContainers_ExpenseRecords_ExpenseRecordId] FOREIGN KEY ([ExpenseRecordId]) REFERENCES [ExpenseRecords] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodContainers_PmodContainerDetailRecords_PmodContainerDetailRecordId] FOREIGN KEY ([PmodContainerDetailRecordId]) REFERENCES [PmodContainerDetailRecords] ([Id]) ON DELETE SET NULL
);

GO

CREATE TABLE [EodPackages] (
    [Id] int NOT NULL IDENTITY,
    [LocalProcessedDate] datetime2 NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [CosmosId] varchar(36) NULL,
    [CosmosCreateDate] datetime2 NOT NULL,
    [SiteName] varchar(50) NULL,
    [PackageId] varchar(50) NULL,
    [Barcode] varchar(100) NULL,
    [SubClientName] varchar(50) NULL,
    [IsPackageProcessed] bit NOT NULL,
    [PackageDetailRecordId] int NULL,
    [ReturnAsnRecordId] int NULL,
    [EvsPackageId] int NULL,
    [InvoiceRecordId] int NULL,
    [ExpenseRecordId] int NULL,
    CONSTRAINT [PK_EodPackages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EodPackages_EvsPackage_EvsPackageId] FOREIGN KEY ([EvsPackageId]) REFERENCES [EvsPackage] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodPackages_ExpenseRecords_ExpenseRecordId] FOREIGN KEY ([ExpenseRecordId]) REFERENCES [ExpenseRecords] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodPackages_InvoiceRecords_InvoiceRecordId] FOREIGN KEY ([InvoiceRecordId]) REFERENCES [InvoiceRecords] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodPackages_PackageDetailRecords_PackageDetailRecordId] FOREIGN KEY ([PackageDetailRecordId]) REFERENCES [PackageDetailRecords] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_EodPackages_ReturnAsnRecords_ReturnAsnRecordId] FOREIGN KEY ([ReturnAsnRecordId]) REFERENCES [ReturnAsnRecords] ([Id]) ON DELETE SET NULL
);

GO

CREATE INDEX [IX_EodContainers_ContainerDetailRecordId] ON [EodContainers] ([ContainerDetailRecordId]);

GO

CREATE INDEX [IX_EodContainers_EvsContainerRecordId] ON [EodContainers] ([EvsContainerRecordId]);

GO

CREATE INDEX [IX_EodContainers_EvsPackageRecordId] ON [EodContainers] ([EvsPackageRecordId]);

GO

CREATE INDEX [IX_EodContainers_ExpenseRecordId] ON [EodContainers] ([ExpenseRecordId]);

GO

CREATE INDEX [IX_EodContainers_PmodContainerDetailRecordId] ON [EodContainers] ([PmodContainerDetailRecordId]);

GO

CREATE INDEX [IX_EodPackages_EvsPackageId] ON [EodPackages] ([EvsPackageId]);

GO

CREATE INDEX [IX_EodPackages_ExpenseRecordId] ON [EodPackages] ([ExpenseRecordId]);

GO

CREATE INDEX [IX_EodPackages_InvoiceRecordId] ON [EodPackages] ([InvoiceRecordId]);

GO

CREATE INDEX [IX_EodPackages_PackageDetailRecordId] ON [EodPackages] ([PackageDetailRecordId]);

GO

CREATE INDEX [IX_EodPackages_ReturnAsnRecordId] ON [EodPackages] ([ReturnAsnRecordId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220224191256_CreateEodDb', N'3.1.6');

GO
