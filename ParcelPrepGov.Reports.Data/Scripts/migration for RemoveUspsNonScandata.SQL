
DELETE FROM [dbo].[TrackPackageDatasets]
    WHERE EventCode = 'MA'
            

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210331161944_RemoveUspsNonScandata', N'3.1.6');

GO

