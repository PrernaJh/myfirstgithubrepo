DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TrackPackageDatasets]') AND [c].[name] = N'PackageId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TrackPackageDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [TrackPackageDatasets] ALTER COLUMN [PackageId] nvarchar(50) NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PackageEventDatasets]') AND [c].[name] = N'PackageId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PackageEventDatasets] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PackageEventDatasets] ALTER COLUMN [PackageId] nvarchar(50) NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PackageDatasets]') AND [c].[name] = N'PackageId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PackageDatasets] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [PackageDatasets] ALTER COLUMN [PackageId] nvarchar(50) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201210150357_ExpandPackageId', N'3.1.6');

GO

CREATE INDEX [IX_PackageDatasets_PackageStatus] ON [PackageDatasets] ([PackageStatus]);

GO

CREATE INDEX [IX_ShippingContainerDatasets_Status] ON [ShippingContainerDatasets] ([Status]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201210172240_AddIndexes', N'3.1.6');

GO

