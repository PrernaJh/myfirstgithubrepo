ALTER TABLE [PackageDatasets] ADD [CalendarDays] int NULL;

GO

ALTER TABLE [PackageDatasets] ADD [PostalDays] int NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211208181940_AddPostalAndCalendarDays', N'3.1.6');

GO
