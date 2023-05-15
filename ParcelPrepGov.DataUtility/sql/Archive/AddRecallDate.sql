ALTER TABLE [PackageDatasets] ADD [RecallDate] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210908145550_AddRecallDate', N'3.1.6');

GO
