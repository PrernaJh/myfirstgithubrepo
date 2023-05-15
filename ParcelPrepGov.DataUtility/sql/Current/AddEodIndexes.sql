CREATE INDEX [IX_EodPackages_CosmosId] ON [EodPackages] ([CosmosId]);

GO

CREATE INDEX [IX_EodPackages_SiteName] ON [EodPackages] ([SiteName]);

GO

CREATE INDEX [IX_EodPackages_LocalProcessedDate] ON [EodPackages] ([LocalProcessedDate]);

GO

CREATE INDEX [IX_EodPackages_IsPackageProcessed] ON [EodPackages] ([IsPackageProcessed]);

GO

CREATE INDEX [IX_EodContainers_CosmosId] ON [EodContainers] ([CosmosId]);

GO

CREATE INDEX [IX_EodContainers_SiteName] ON [EodContainers] ([SiteName]);

GO

CREATE INDEX [IX_EodContainers_LocalProcessedDate] ON [EodContainers] ([LocalProcessedDate]);

GO

CREATE INDEX [IX_EodContainers_IsContainerClosed] ON [EodContainers] ([IsContainerClosed]);

GO

CREATE INDEX [IX_PackageDetailRecords_CosmosId] ON [PackageDetailRecords] ([CosmosId]);

GO

CREATE INDEX [IX_ReturnAsnRecords_CosmosId] ON [ReturnAsnRecords] ([CosmosId]);

GO

CREATE INDEX [IX_ContainerDetailRecords_CosmosId] ON [ContainerDetailRecords] ([CosmosId]);

GO

CREATE INDEX [IX_PmodContainerDetailRecords_CosmosId] ON [PmodContainerDetailRecords] ([CosmosId]);

GO

CREATE INDEX [IX_InvoiceRecords_CosmosId] ON [InvoiceRecords] ([CosmosId]);

GO

CREATE INDEX [IX_ExpenseRecords_CosmosId] ON [ExpenseRecords] ([CosmosId]);

GO

CREATE INDEX [IX_EvsPackage_CosmosId] ON [EvsPackage] ([CosmosId]);

GO

CREATE INDEX [IX_EvsContainer_CosmosId] ON [EvsContainer] ([CosmosId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220225174049_AddIndexes', N'3.1.6');

GO