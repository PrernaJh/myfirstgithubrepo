CREATE TABLE [PackageInquiries] (
    [Id] int NOT NULL IDENTITY,
    [SiteName] varchar(24) NULL,
    [InquiryId] int NOT NULL,
    [PackageDatasetId] int NOT NULL,
    [PackageId] varchar(50) NULL,
    CONSTRAINT [PK_PackageInquiries] PRIMARY KEY ([Id])
);

GO

CREATE INDEX [IX_PackageInquiries_SiteName] ON [PackageInquiries] ([SiteName]);

GO

CREATE INDEX [IX_PackageInquiries_InquiryId] ON [PackageInquiries] ([InquiryId]);

GO

CREATE INDEX [IX_PackageInquiries_PackageId] ON [PackageInquiries] ([PackageId]);

GO

ALTER TABLE [PackageInquiries] ADD CONSTRAINT [FK_PackageInquiries_PackageDatasets_PackageDatasetId] FOREIGN KEY ([PackageDatasetId]) REFERENCES [PackageDatasets] ([Id]) ON DELETE CASCADE;

GO

CREATE INDEX [IX_PackageInquiries_PackageDatasetId] ON [PackageInquiries] ([PackageDatasetId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210916150901_AddPackageInquiries', N'3.1.6');

GO

