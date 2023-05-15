ALTER TABLE [PackageEventDatasets] ADD [TrackingNumber] varchar(100) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220413150159_AddPackageEventTrackingNumber', N'3.1.6');

GO

