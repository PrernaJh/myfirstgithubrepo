ALTER TABLE [PackageDatasets] ADD [ClientShipDate] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220111155330_AddClientShipDate', N'3.1.6');

GO

ALTER TABLE [BinDatasets] ADD [ShippingCarrierSecondary] varchar NULL;

GO

CREATE INDEX [IX_BinDatasets_ShippingCarrierSecondary] ON [BinDatasets] ([ShippingCarrierSecondary]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220111161455_AddShippingCarrierSecondary', N'3.1.6');

GO

