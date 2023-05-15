ALTER TABLE [PackageDatasets] ADD [VisnSiteParent] int NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220217152848_AddVisnSiteParent', N'3.1.6');

GO
