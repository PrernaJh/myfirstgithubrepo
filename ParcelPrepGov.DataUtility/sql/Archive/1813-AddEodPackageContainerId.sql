ALTER TABLE [EodPackages] ADD [ContainerId] varchar(100) NULL;

GO

CREATE INDEX [IX_EodPackages_ContainerId] ON [EodPackages] ([PackageId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220525170827_AddEodPackageContainerId', N'3.1.6');

GO