CREATE INDEX [IX_PackageDatasets_ContainerId] ON [PackageDatasets] ([ContainerId]);
CREATE INDEX [IX_ShippingContainerEventDatasets_ShippingContainerDatasetId] ON [ShippingContainerEventDatasets] ([ShippingContainerDatasetId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220429132017_AddContainerIdIndex', N'3.1.6');

GO
