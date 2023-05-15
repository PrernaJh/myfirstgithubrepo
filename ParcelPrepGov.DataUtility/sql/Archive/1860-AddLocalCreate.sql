IF NOT EXISTS(SELECT object_id FROM sys.columns 
          WHERE Name = N'LocalCreateDate'
          AND Object_ID = Object_ID(N'dbo.ShippingContainerDatasets'))
BEGIN   
    ALTER TABLE [ShippingContainerDatasets] ADD [LocalCreateDate] datetime2 NULL;
END


GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE  [MigrationId] = N'20220623125623_AddLocalCreateDate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220623125623_AddLocalCreateDate', N'3.1.6');
END

GO

CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

        SELECT 
            BinCode AS BINCODE, 
            MIN(LabelListDescription) AS DESTINATION,
            ContainerId AS CONTAINER_ID, 
            MIN(containerbarcode) AS CONT_BARCODE,
            MIN(cont_carrier) AS CONT_CARRIER,
            MIN(cont_method) AS CONT_METHOD, 
            PackageId AS PACKAGE_ID, 
            MIN(ShippingBarcode) AS TRACKING_NUMBER,
            MIN(ShippingCarrier) AS PKG_CARRIER,
            MIN(ShippingMethod) AS PKG_SHIPPINGMETHOD,
			opened_by AS OPENED_BY,
			MAX(FORMAT(LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS PKG_PROCESSED_DATE,
			MAX(FORMAT(LocalCreateDate, 'MM/dd/yyyy hh:mm:ss tt')) AS CONT_CREATE_DATE
        FROM (SELECT
             p.PackageId,
             p.LocalProcessedDate,
             p.ContainerId,
             p.BinCode,
             p.ShippingBarcode,
             p.ShippingCarrier,
             p.ShippingMethod,
             c.UpdatedBarcode AS containerbarcode,
             c.ShippingCarrier AS cont_carrier,
             c.ShippingMethod AS cont_method,
             b.LabelListDescription,
			 c.Username AS opened_by,
			 c.LocalCreateDate
         FROM [dbo].[PackageDatasets] p
			 LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
			 LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId = b.ActiveGroupId AND c.BinCode = b.BinCode
         WHERE p.SiteName = @siteName 
			AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) 
			AND PackageStatus = 'PROCESSED') s
         GROUP BY BinCode, ContainerId, PackageId, opened_by
         ORDER BY BinCode, ContainerId, PackageId
END