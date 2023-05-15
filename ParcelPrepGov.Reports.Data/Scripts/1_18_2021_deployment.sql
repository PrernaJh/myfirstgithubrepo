ALTER TABLE [TrackPackageDatasets] ADD [PackageDatasetId] int NOT NULL DEFAULT 0;

GO

ALTER TABLE [TrackPackageDatasets] ADD [ShippingContainerDatasetId] int NOT NULL DEFAULT 0;

GO

CREATE INDEX [IX_TrackPackageDatasets_PackageDatasetId] ON [TrackPackageDatasets] ([PackageDatasetId]);

GO

CREATE INDEX [IX_TrackPackageDatasets_ShippingContainerDatasetId] ON [TrackPackageDatasets] ([ShippingContainerDatasetId]);

GO

CREATE INDEX [IX_ShippingContainerDatasets_UpdatedBarcode] ON [ShippingContainerDatasets] ([UpdatedBarcode]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210118180130_IndexUpdatedBarcode', N'3.1.6');

GO

