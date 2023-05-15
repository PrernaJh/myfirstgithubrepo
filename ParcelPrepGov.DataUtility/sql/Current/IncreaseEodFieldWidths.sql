ALTER TABLE [ReturnAsnRecords] ALTER COLUMN [ParcelId] varchar(100) NULL;
ALTER TABLE [PmodContainerDetailRecords] ALTER COLUMN [PdWeight] varchar(32) NULL;
ALTER TABLE [PmodContainerDetailRecords] ALTER COLUMN [ContainerId] varchar(100) NULL;
ALTER TABLE [PackageDetailRecords] ALTER COLUMN [Weight] varchar(32) NULL;
ALTER TABLE [PackageDetailRecords] ALTER COLUMN [PackageId] varchar(100) NULL;
ALTER TABLE [PackageDetailRecords] ALTER COLUMN [BillingWeight] varchar(32) NULL;
ALTER TABLE [InvoiceRecords] ALTER COLUMN [Weight] varchar(32) NULL;
ALTER TABLE [InvoiceRecords] ALTER COLUMN [PackageId] varchar(100) NULL;
ALTER TABLE [InvoiceRecords] ALTER COLUMN [BillingWeight] varchar(32) NULL;
ALTER TABLE [EvsPackage] ALTER COLUMN [ContainerId] varchar(100) NULL;
ALTER TABLE [EvsContainer] ALTER COLUMN [ContainerId] varchar(100) NULL;
ALTER TABLE [EodPackages] ALTER COLUMN [PackageId] varchar(100) NULL;
ALTER TABLE [EodContainers] ALTER COLUMN [ContainerId] varchar(100) NULL;
ALTER TABLE [ContainerDetailRecords] ALTER COLUMN [Weight] varchar(32) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220328150410_IncreaseFieldWidths', N'3.1.6');

GO
