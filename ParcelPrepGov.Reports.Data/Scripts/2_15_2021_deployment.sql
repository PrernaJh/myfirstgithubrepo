CREATE INDEX [IX_PackageDatasets_SubClientName] ON [PackageDatasets] ([SubClientName]);

GO

CREATE INDEX [IX_PackageDatasets_LocalProcessedDate] ON [PackageDatasets] ([LocalProcessedDate]);

GO

CREATE INDEX [IX_ShippingContainerDatasets_LocalProcessedDate] ON [ShippingContainerDatasets] ([LocalProcessedDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210215173831_AddMoreIndexes', N'3.1.6');

GO

