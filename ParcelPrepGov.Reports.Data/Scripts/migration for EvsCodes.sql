CREATE TABLE [EvsCodes] (
    [Id] int NOT NULL IDENTITY,
    [CreateDate] datetime2 NOT NULL,
    [Code] nvarchar(2) NULL,
    [Description] nvarchar(50) NULL,
    [IsStopTheClock] int NOT NULL,
    [IsUndeliverable] int NOT NULL,
    CONSTRAINT [PK_EvsCodes] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210226171456_EvsCodes', N'3.1.6');

GO