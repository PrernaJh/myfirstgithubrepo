ALTER TABLE [PackageDatasets] ADD [ReleaseDate] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210914162937_AddReleaseDate', N'3.1.6');

GO