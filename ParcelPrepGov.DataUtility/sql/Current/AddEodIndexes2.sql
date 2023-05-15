CREATE INDEX [IX_EodPackages_PackageId] ON [EodPackages] ([PackageId]);

GO

CREATE INDEX [IX_PackageDetailRecords_PackageId] ON [PackageDetailRecords] ([PackageId]);

GO

CREATE INDEX [IX_ReturnAsnRecords_ParcelId] ON [ReturnAsnRecords] ([ParcelId]);

GO

CREATE INDEX [IX_InvoiceRecords_PackageId] ON [InvoiceRecords] ([PackageId]);

GO

CREATE INDEX [IX_ExpenseRecords_BillingReference1_Product] ON [ExpenseRecords] ([BillingReference1], [Product]);

GO

CREATE INDEX [IX_EvsPackage_TrackingNumber] ON [EvsPackage] ([TrackingNumber]);

GO

CREATE INDEX [IX_EodContainers_ContainerId] ON [EodContainers] ([ContainerId]);

GO

CREATE INDEX [IX_EvsContainer_ContainerId] ON [EvsContainer] ([ContainerId]);

GO

CREATE INDEX [IX_ContainerDetailRecords_TrackingNumber] ON [ContainerDetailRecords] ([TrackingNumber]);

GO

CREATE INDEX [IX_PmodContainerDetailRecords_ContainerId] ON [PmodContainerDetailRecords] ([ContainerId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220330163146_AddIndexes2', N'3.1.6');

GO

