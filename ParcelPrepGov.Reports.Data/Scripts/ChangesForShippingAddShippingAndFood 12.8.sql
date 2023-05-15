ALTER TABLE [ShippingContainerDatasets] ADD [Status] nvarchar(24) NULL;

GO

CREATE TABLE [ShippingContainerEventDataset] (
    [Id] int NOT NULL IDENTITY,
    [DatasetCreateDate] datetime2 NOT NULL,
    [CosmosId] nvarchar(36) NULL,
    [CosmosCreateDate] datetime2 NOT NULL,
    [SiteName] nvarchar(24) NULL,
    [ContainerDatasetId] int NOT NULL,
    [ContainerId] nvarchar(32) NULL,
    [EventId] int NOT NULL,
    [EventType] nvarchar(24) NULL,
    [EventStatus] nvarchar(24) NULL,
    [Description] nvarchar(100) NULL,
    [EventDate] datetime2 NOT NULL,
    [LocalEventDate] datetime2 NOT NULL,
    [Username] nvarchar(32) NULL,
    [MachineId] nvarchar(32) NULL,
    [ShippingContainerDatasetId] int NULL,
    CONSTRAINT [PK_ShippingContainerEventDataset] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShippingContainerEventDataset_ShippingContainerDatasets_ShippingContainerDatasetId] FOREIGN KEY ([ShippingContainerDatasetId]) REFERENCES [ShippingContainerDatasets] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_ShippingContainerEventDataset_ShippingContainerDatasetId] ON [ShippingContainerEventDataset] ([ShippingContainerDatasetId]);

GO

CREATE INDEX [IX_ShippingContainerEventDataset_CosmosId] ON [ShippingContainerEventDataset] ([CosmosId]);

GO

CREATE INDEX [IX_ShippingContainerEventDataset_ContainerId] ON [ShippingContainerEventDataset] ([ContainerId]);

GO

CREATE INDEX [IX_ShippingContainerEventDataset_SiteName] ON [ShippingContainerEventDataset] ([SiteName]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201112202309_ChangesForShippingContainers', N'3.1.6');

GO

ALTER TABLE [ShippingContainerEventDataset] DROP CONSTRAINT [FK_ShippingContainerEventDataset_ShippingContainerDatasets_ShippingContainerDatasetId];

GO

ALTER TABLE [ShippingContainerEventDataset] DROP CONSTRAINT [PK_ShippingContainerEventDataset];

GO

EXEC sp_rename N'[ShippingContainerEventDataset]', N'ShippingContainerEventDatasets';

GO

EXEC sp_rename N'[ShippingContainerEventDatasets].[IX_ShippingContainerEventDataset_ShippingContainerDatasetId]', N'IX_ShippingContainerEventDatasets_ShippingContainerDatasetId', N'INDEX';

GO

EXEC sp_rename N'[ShippingContainerEventDatasets].[IX_ShippingContainerEventDataset_CosmosId]', N'IX_ShippingContainerEventDatasets_CosmosId', N'INDEX';

GO

EXEC sp_rename N'[ShippingContainerEventDatasets].[IX_ShippingContainerEventDataset_ContainerId]', N'IX_ShippingContainerEventDatasets_ContainerId', N'INDEX';

GO

EXEC sp_rename N'[ShippingContainerEventDatasets].[IX_ShippingContainerEventDataset_SiteName]', N'IX_ShippingContainerEventDatasets_SiteName', N'INDEX';

GO

ALTER TABLE [ShippingContainerEventDatasets] ADD CONSTRAINT [PK_ShippingContainerEventDatasets] PRIMARY KEY ([Id]);

GO

ALTER TABLE [ShippingContainerEventDatasets] ADD CONSTRAINT [FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId] FOREIGN KEY ([ShippingContainerDatasetId]) REFERENCES [ShippingContainerDatasets] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201112204928_AddShippingContainerEvents', N'3.1.6');

GO

ALTER TABLE [ShippingContainerEventDatasets] DROP CONSTRAINT [FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ShippingContainerEventDatasets]') AND [c].[name] = N'ContainerDatasetId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ShippingContainerEventDatasets] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ShippingContainerEventDatasets] DROP COLUMN [ContainerDatasetId];

GO

DROP INDEX [IX_ShippingContainerEventDatasets_ShippingContainerDatasetId] ON [ShippingContainerEventDatasets];
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ShippingContainerEventDatasets]') AND [c].[name] = N'ShippingContainerDatasetId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ShippingContainerEventDatasets] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ShippingContainerEventDatasets] ALTER COLUMN [ShippingContainerDatasetId] int NOT NULL;
CREATE INDEX [IX_ShippingContainerEventDatasets_ShippingContainerDatasetId] ON [ShippingContainerEventDatasets] ([ShippingContainerDatasetId]);

GO

ALTER TABLE [ShippingContainerEventDatasets] ADD CONSTRAINT [FK_ShippingContainerEventDatasets_ShippingContainerDatasets_ShippingContainerDatasetId] FOREIGN KEY ([ShippingContainerDatasetId]) REFERENCES [ShippingContainerDatasets] ([Id]) ON DELETE CASCADE;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201112210918_foo', N'3.1.6');

GO

