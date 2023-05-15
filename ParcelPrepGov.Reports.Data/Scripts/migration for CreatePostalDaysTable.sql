CREATE TABLE [PostalDays] (
    [Id] int NOT NULL IDENTITY,
    [PostalDate] datetime2 NOT NULL,
    [Ordinal] int NOT NULL,
    [Description] nvarchar(50) NULL,
    [IsHoliday] int NOT NULL,
    [IsSunday] int NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    CONSTRAINT [PK_PostalDays] PRIMARY KEY ([Id])
);

GO

CREATE INDEX [IX_PostalDays_PostalDate] ON [PostalDays] ([PostalDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210420190230_CreatePostalDaysTable', N'3.1.6');

GO