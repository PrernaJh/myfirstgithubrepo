ALTER TABLE [ShippingContainerEventDatasets] ALTER COLUMN [Description] varchar(180) NULL;

GO

ALTER TABLE [PackageEventDatasets] ALTER COLUMN [Description] varchar(180) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211208194909_ExpandEventDescriptions', N'3.1.6');

GO

