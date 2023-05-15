CREATE INDEX [IX_PackageDatasets_BinGroupId] ON [PackageDatasets] ([BinGroupId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210505204143_AddPackageBinGroupIdIndex', N'3.1.6');

GO
