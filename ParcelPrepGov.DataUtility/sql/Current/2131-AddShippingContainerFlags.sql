IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE  [MigrationId] = N'20221108163649_AddShippingContainerFlags')
BEGIN
ALTER TABLE [ShippingContainerDatasets] ADD [IsOutside48States] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ShippingContainerDatasets] ADD [IsRural] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ShippingContainerDatasets] ADD [IsSaturdayDelivery] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221108163649_AddShippingContainerFlags', N'3.1.6');
END

GO