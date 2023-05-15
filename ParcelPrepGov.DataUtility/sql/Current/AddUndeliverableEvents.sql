CREATE TABLE [UndeliverableEventDatasets] (
    [Id] int NOT NULL IDENTITY,
    [DatasetCreateDate] datetime2 NOT NULL,
    [DatasetModifiedDate] datetime2 NOT NULL,
    [CosmosId] varchar(36) NULL,
    [CosmosCreateDate] datetime2 NOT NULL,
    [SiteName] varchar(24) NULL,
    [PackageId] varchar(100) NULL,
    [PackageDatasetId] int NULL,
    [EventDate] datetime2 NOT NULL,
    [EventCode] varchar(24) NULL,
    [EventDescription] varchar(120) NULL,
    [EventLocation] varchar(120) NULL,
    [EventZip] varchar(10) NULL,
    CONSTRAINT [PK_UndeliverableEventDatasets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UndeliverableEventDatasets_PackageDatasets_PackageDatasetId] FOREIGN KEY ([PackageDatasetId]) REFERENCES [PackageDatasets] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_UndeliverableEventDatasets_PackageDatasetId] ON [UndeliverableEventDatasets] ([PackageDatasetId]);

GO

CREATE INDEX [IX_UndeliverableEventDatasets_CosmosId] ON [UndeliverableEventDatasets] ([CosmosId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220414155759_AddUndeliverableEvents', N'3.1.6');

GO