ALTER TABLE [ShippingContainerDatasets] ADD [LocalProcessedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [ShippingContainerDatasets] ADD [ProcessedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201117162214_AddContainerProcessedDateTime', N'3.1.6');

GO

