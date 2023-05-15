IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE  [MigrationId] = N'20221104160003_AddEODPackageRatingElements')
BEGIN
ALTER TABLE [ReturnAsnRecords] ADD [IsOutside48States] varchar(5) NULL;

ALTER TABLE [ReturnAsnRecords] ADD [IsRural] varchar(5) NULL;

ALTER TABLE [ReturnAsnRecords] ADD [MarkupType] varchar(24) NULL;

ALTER TABLE [PackageDetailRecords] ADD [IsOutside48States] varchar(5) NULL;

ALTER TABLE [PackageDetailRecords] ADD [IsRural] varchar(5) NULL;

ALTER TABLE [PackageDetailRecords] ADD [MarkupType] varchar(24) NULL;

ALTER TABLE [PackageDetailRecords] DROP COLUMN [MarkupReason];

ALTER TABLE [InvoiceRecords] ADD [IsOutside48States] varchar(5) NULL;

ALTER TABLE [InvoiceRecords] ADD [IsRural] varchar(5) NULL;

ALTER TABLE [InvoiceRecords] ADD [MarkupType] varchar(24) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221104160003_AddEODPackageRatingElements', N'3.1.6');
END

GO

