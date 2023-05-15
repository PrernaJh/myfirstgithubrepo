ALTER TABLE [PackageDatasets] ADD [LastKnownEventDate] datetime2 NULL;

GO

ALTER TABLE [PackageDatasets] ADD [LastKnownEventDescription] varchar(120) NULL;

GO

ALTER TABLE [PackageDatasets] ADD [LastKnownEventLocation] varchar(120) NULL;

GO

ALTER TABLE [PackageDatasets] ADD [LastKnownEventZip] varchar(10) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211110163520_AddLastKnowFields', N'3.1.6');

GO

