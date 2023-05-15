ALTER TABLE [ShippingContainerDatasets] ADD [StopTheClockEventDate] datetime2 NULL;

GO

CREATE INDEX [IX_ShippingContainerDatasets_StopTheClockEventDate] ON [ShippingContainerDatasets] ([StopTheClockEventDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220209170346_AddContainerStopTheClockEventDate', N'3.1.6');

GO
