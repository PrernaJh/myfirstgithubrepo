DROP INDEX [IX_PackageDetailRecords_PackageId] ON [PackageDetailRecords];

GO

DROP INDEX [IX_ReturnAsnRecords_ParcelId] ON [ReturnAsnRecords];

GO

DROP INDEX [IX_InvoiceRecords_PackageId] ON [InvoiceRecords];

GO

DROP INDEX [IX_ExpenseRecords_BillingReference1_Product] ON [ExpenseRecords];

GO

DROP INDEX [IX_EvsPackage_TrackingNumber] ON [EvsPackage];

GO

DROP INDEX [IX_EvsContainer_ContainerId] ON [EvsContainer];

GO

DROP INDEX [IX_ContainerDetailRecords_TrackingNumber] ON [ContainerDetailRecords];

GO

DROP INDEX [IX_PmodContainerDetailRecords_ContainerId] ON [PmodContainerDetailRecords];

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220408195200_DropIndexes', N'3.1.6');

GO



