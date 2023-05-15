DROP INDEX [IX_TrackPackageDatasets_PackageDatasetId] ON [TrackPackageDatasets];

GO

DROP INDEX [IX_TrackPackageDatasets_ShippingContainerDatasetId] ON [TrackPackageDatasets];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrackPackageDatasets]') AND [c].[name] = N'ShippingContainerDatasetId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TrackPackageDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [TrackPackageDatasets] ALTER COLUMN [ShippingContainerDatasetId] int NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrackPackageDatasets]') AND [c].[name] = N'PackageDatasetId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [TrackPackageDatasets] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [TrackPackageDatasets] ALTER COLUMN [PackageDatasetId] int NULL;

GO


               UPDATE [dbo].[TrackPackageDatasets]
                    SET PackageDatasetId = NULL
                        WHERE PackageDatasetId = 0
            

GO


                UPDATE [dbo].[TrackPackageDatasets]
                    SET ShippingContainerDatasetId = NULL
                        WHERE ShippingContainerDatasetId = 0
            

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210128194636_FixTrackPackageDataset', N'3.1.6');

GO

CREATE INDEX [IX_TrackPackageDatasets_PackageDatasetId] ON [TrackPackageDatasets] ([PackageDatasetId]);

GO

CREATE INDEX [IX_TrackPackageDatasets_ShippingContainerDatasetId] ON [TrackPackageDatasets] ([ShippingContainerDatasetId]);

GO

ALTER TABLE [TrackPackageDatasets] ADD CONSTRAINT [FK_TrackPackageDatasets_PackageDatasets_PackageDatasetId] FOREIGN KEY ([PackageDatasetId]) REFERENCES [PackageDatasets] ([Id]) ON DELETE NO ACTION;

GO

ALTER TABLE [TrackPackageDatasets] ADD CONSTRAINT [FK_TrackPackageDatasets_ShippingContainerDatasets_ShippingContainerDatasetId] FOREIGN KEY ([ShippingContainerDatasetId]) REFERENCES [ShippingContainerDatasets] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210128203020_AddForeignKeys', N'3.1.6');

GO

