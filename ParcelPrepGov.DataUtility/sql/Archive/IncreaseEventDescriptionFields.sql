DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ShippingContainerEventDatasets]') AND [c].[name] = N'Description');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ShippingContainerEventDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ShippingContainerEventDatasets] ALTER COLUMN [Description] varchar(120) NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PackageEventDatasets]') AND [c].[name] = N'Description');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PackageEventDatasets] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PackageEventDatasets] ALTER COLUMN [Description] varchar(120) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211118164731_IncreaseEventDescriptionFields', N'3.1.6');

GO

