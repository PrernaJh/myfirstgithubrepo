DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ShippingContainerDatasets]') AND [c].[name] = N'Weight');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ShippingContainerDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ShippingContainerDatasets] ALTER COLUMN [Weight] nvarchar(24) NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobContainerDatasets]') AND [c].[name] = N'Weight');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [JobContainerDatasets] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [JobContainerDatasets] ALTER COLUMN [Weight] nvarchar(24) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210216015708_ExpandWeightFields', N'3.1.6');

GO

