DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobContainerDatasets]') AND [c].[name] = N'JobBarcode');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [JobContainerDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [JobContainerDatasets] ALTER COLUMN [JobBarcode] nvarchar(100) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210727145010_ExpandJobBarcode2', N'3.1.6');

GO