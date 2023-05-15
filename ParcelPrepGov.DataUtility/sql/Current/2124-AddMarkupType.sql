IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE[MigrationId] = N'20221109153658_AddMarkupType')
BEGIN
ALTER TABLE[PackageDatasets] ADD [MarkUpType] varchar(24) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES(N'20221109153658_AddMarkupType', N'3.1.6');
END
    
GO