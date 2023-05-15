CREATE INDEX [IX_PackageDatasets_StopTheClockEventDate] ON [PackageDatasets] ([StopTheClockEventDate]);

GO

CREATE INDEX [IX_PackageDatasets_LastKnownEventDate] ON [PackageDatasets] ([LastKnownEventDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211111172139_AddDateIndexes', N'3.1.6');

GO