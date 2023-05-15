ALTER TABLE [Users] ADD [SendRecallReleaseAlerts] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220420161845_AddSendRecallReleaseAlerts', N'3.1.6');

GO
