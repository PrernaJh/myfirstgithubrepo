DROP INDEX [IX_EodContainers_ContainerDetailRecordId] ON [EodContainers];

GO

DROP INDEX [IX_EodContainers_EvsContainerRecordId] ON [EodContainers];

GO

DROP INDEX [IX_EodContainers_EvsPackageRecordId] ON [EodContainers];

GO

DROP INDEX [IX_EodContainers_ExpenseRecordId] ON [EodContainers];

GO

DROP INDEX [IX_EodContainers_PmodContainerDetailRecordId] ON [EodContainers];

GO

DROP INDEX [IX_EodPackages_EvsPackageId] ON [EodPackages];

GO

DROP INDEX [IX_EodPackages_InvoiceRecordId] ON [EodPackages];

GO

DROP INDEX [IX_EodPackages_ExpenseRecordId] ON [EodPackages];

GO

DROP INDEX [IX_EodPackages_PackageDetailRecordId] ON [EodPackages];

GO

DROP INDEX [IX_EodPackages_ReturnAsnRecordId] ON [EodPackages];

GO

ALTER TABLE [EodContainers] DROP CONSTRAINT [FK_EodContainers_ContainerDetailRecords_ContainerDetailRecordId];

GO

ALTER TABLE [EodContainers] DROP CONSTRAINT [FK_EodContainers_EvsContainer_EvsContainerRecordId];

GO

ALTER TABLE [EodContainers] DROP CONSTRAINT [FK_EodContainers_EvsPackage_EvsPackageRecordId];

GO

ALTER TABLE [EodContainers] DROP CONSTRAINT [FK_EodContainers_ExpenseRecords_ExpenseRecordId];

GO

ALTER TABLE [EodContainers] DROP CONSTRAINT [FK_EodContainers_PmodContainerDetailRecords_PmodContainerDetailRecordId];

GO

ALTER TABLE [EodPackages] DROP CONSTRAINT [FK_EodPackages_EvsPackage_EvsPackageId];

GO

ALTER TABLE [EodPackages] DROP CONSTRAINT [FK_EodPackages_ExpenseRecords_ExpenseRecordId];

GO

ALTER TABLE [EodPackages] DROP CONSTRAINT [FK_EodPackages_InvoiceRecords_InvoiceRecordId];

GO

ALTER TABLE [EodPackages] DROP CONSTRAINT [FK_EodPackages_PackageDetailRecords_PackageDetailRecordId];

GO

ALTER TABLE [EodPackages] DROP CONSTRAINT [FK_EodPackages_ReturnAsnRecords_ReturnAsnRecordId];

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220408201711_DropForeignKeys', N'3.1.6');

GO

