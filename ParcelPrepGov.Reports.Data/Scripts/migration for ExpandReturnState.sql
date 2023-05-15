DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PackageDatasets]') AND [c].[name] = N'ReturnState');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [PackageDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [PackageDatasets] ALTER COLUMN [ReturnState] nvarchar(30) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210304152100_ExpandReturnState', N'3.1.6');

GO

