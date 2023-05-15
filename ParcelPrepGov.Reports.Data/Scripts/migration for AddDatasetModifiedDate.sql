ALTER TABLE [TrackPackageDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [SubClientDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [ShippingContainerEventDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [ShippingContainerDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [PackageEventDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [PackageDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [JobDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [JobContainerDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [BinDatasets] ADD [DatasetModifiedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

CREATE INDEX [IX_TrackPackageDatasets_DatasetCreateDate] ON [TrackPackageDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_TrackPackageDatasets_DatasetModifiedDate] ON [TrackPackageDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_SubClientDatasets_DatasetCreateDate] ON [SubClientDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_SubClientDatasets_DatasetModifiedDate] ON [SubClientDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_ShippingContainerEventDatasets_DatasetCreateDate] ON [ShippingContainerEventDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_ShippingContainerEventDatasets_DatasetModifiedDate] ON [ShippingContainerEventDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_ShippingContainerDatasets_DatasetCreateDate] ON [ShippingContainerDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_ShippingContainerDatasets_DatasetModifiedDate] ON [ShippingContainerDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_PackageEventDatasets_DatasetCreateDate] ON [PackageEventDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_PackageEventDatasets_DatasetModifiedDate] ON [PackageEventDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_PackageDatasets_DatasetCreateDate] ON [PackageDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_PackageDatasets_DatasetModifiedDate] ON [PackageDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_JobDatasets_DatasetCreateDate] ON [JobDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_JobDatasets_DatasetModifiedDate] ON [JobDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_JobContainerDatasets_DatasetCreateDate] ON [JobContainerDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_JobContainerDatasets_DatasetModifiedDate] ON [JobContainerDatasets] ([DatasetModifiedDate]);

GO

CREATE INDEX [IX_BinDatasets_DatasetCreateDate] ON [BinDatasets] ([DatasetCreateDate]);

GO

CREATE INDEX [IX_BinDatasets_DatasetModifiedDate] ON [BinDatasets] ([DatasetModifiedDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210316151951_AddDatasetModifiedDate', N'3.1.6');

GO

