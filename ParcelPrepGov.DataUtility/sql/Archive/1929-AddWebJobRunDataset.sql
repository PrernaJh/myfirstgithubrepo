CREATE TABLE [WebJobRunDatasets] (
    [Id] int NOT NULL IDENTITY,
    [DatasetCreateDate] datetime2 NOT NULL,
    [DatasetModifiedDate] datetime2 NOT NULL,
    [CosmosId] varchar(36) NULL,
    [CosmosCreateDate] datetime2 NOT NULL,
    [SiteName] varchar(24) NULL,
    [ClientName] varchar(30) NULL,
    [SubClientName] varchar(30) NULL,
    [JobName] varchar(100) NULL,
    [JobType] varchar(30) NULL,
    [ProcessedDate] datetime2 NOT NULL,
    [FileName] varchar(100) NULL,
    [FileArchiveName] varchar(100) NULL,
    [NumberOfRecords] int NOT NULL,
    [Username] varchar(30) NULL,
    [LocalCreateDate] datetime2 NOT NULL,
    [IsSuccessful] bit NOT NULL,
    [Message] varchar(100) NULL,
    CONSTRAINT [PK_WebJobRunDatasets] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220804152908_AddWebJobRunDataset', N'3.1.6');

GO

