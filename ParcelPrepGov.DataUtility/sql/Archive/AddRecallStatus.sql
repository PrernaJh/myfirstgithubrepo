ALTER TABLE [dbo].[PackageDatasets] ADD [RecallStatus] varchar(24) NULL;

GO

CREATE TABLE  [dbo].[RecallStatuses] (
    [Id] int NOT NULL IDENTITY,
    [CreateDate] datetime2 NOT NULL,
    [Status] varchar(24) NULL,
    [Description] varchar(80) NULL,
    CONSTRAINT [PK_RecallStatuses] PRIMARY KEY ([Id])
);
GO

INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','CREATED','ENTERED – NO ASN RECORD');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','IMPORTED','ENTERED - PACKAGE NOT PROCESSED');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','PROCESSED','ENTERED - PACKAGE PROCESSED');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','SCANNED','RECALL FOUND');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','RELEASED','RECALL RELEASE');
GO

INSERT INTO  [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210903154138_AddRecallStatus', N'3.1.6');

GO