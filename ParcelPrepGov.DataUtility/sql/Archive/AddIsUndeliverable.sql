ALTER TABLE [PackageDatasets] ADD [IsUndeliverable] int NULL;

GO

CREATE INDEX [IX_PackageDatasets_IsUndeliverable] ON [PackageDatasets] ([IsUndeliverable]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211217162442_AddIsUndeliverable', N'3.1.6');

GO