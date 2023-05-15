ALTER TABLE [PackageDatasets] ADD [HumanReadableBarcode] nvarchar(100) NULL;

GO

CREATE INDEX [IX_PackageDatasets_HumanReadableBarcode] ON [PackageDatasets] ([HumanReadableBarcode]);

GO


UPDATE [dbo].[PackageDatasets]
	SET HumanReadableBarcode = SUBSTRING(ShippingBarcode, 9, 26)
		WHERE HumanReadableBarcode IS NULL AND LEN(ShippingBarcode) = 34
            

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210322144050_AddHumanReadableBarcode', N'3.1.6');

GO

