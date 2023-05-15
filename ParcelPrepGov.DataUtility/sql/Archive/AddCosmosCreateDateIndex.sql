CREATE INDEX [IX_PackageDatasets_CosmosCreateDate] ON [PackageDatasets] ([CosmosCreateDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211227163416_AddCosmosCreateDateIndex', N'3.1.6');

GO
