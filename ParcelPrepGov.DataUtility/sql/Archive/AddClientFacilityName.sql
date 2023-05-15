ALTER TABLE [PackageDatasets] ADD [ClientFacilityName] varchar(30) NULL;

GO

CREATE INDEX [IX_PackageDatasets_ClientFacilityName] ON [PackageDatasets] ([ClientFacilityName]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211217171650_AddClientFacilityName', N'3.1.6');

GO