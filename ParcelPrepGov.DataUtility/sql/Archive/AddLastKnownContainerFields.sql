ALTER TABLE [ShippingContainerDatasets] ADD [LastKnownEventDate] datetime2 NULL;

GO

ALTER TABLE [ShippingContainerDatasets] ADD [LastKnownEventDescription] varchar(120) NULL;

GO

ALTER TABLE [ShippingContainerDatasets] ADD [LastKnownEventLocation] varchar(120) NULL;

GO

ALTER TABLE [ShippingContainerDatasets] ADD [LastKnownEventZip] varchar(10) NULL;

GO

CREATE INDEX [IX_ShippingContainerDatasets_LastKnownEventDate] ON [ShippingContainerDatasets] ([LastKnownEventDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211117172454_AddLastKnownContainerFields', N'3.1.6');

GO

