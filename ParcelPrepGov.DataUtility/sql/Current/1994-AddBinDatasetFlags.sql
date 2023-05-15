IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE  [MigrationId] = N'20220902153134_AddBinDatasetFlags')
BEGIN
	ALTER TABLE [BinDatasets] ADD [IsAptb] bit NOT NULL DEFAULT CAST(0 AS bit);
	ALTER TABLE [BinDatasets] ADD [IsScsc] bit NOT NULL DEFAULT CAST(0 AS bit);
	INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
	VALUES (N'20220902153134_AddBinDatasetFlags', N'3.1.6');
END

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE  [MigrationId] = N'20220906142307_AddProcessedCols')
BEGIN
	ALTER TABLE [PackageDatasets] ADD [ProcessedEventType] nvarchar(50) NULL;
	ALTER TABLE [PackageDatasets] ADD [ProcessedMachineId] nvarchar(50) NULL;
	ALTER TABLE [PackageDatasets] ADD [ProcessedUsername] nvarchar(50) NULL;

	INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
	VALUES (N'20220906142307_AddProcessedCols', N'3.1.6');
END

GO
CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
	@CUSTOMER VARCHAR(250) = NULL,
	@CONT_CARRIER VARCHAR(250) = NULL,
	@CONT_METHOD VARCHAR(250) = NULL,
	@CONT_TYPE VARCHAR(250) = NULL,
	@PKG_CARRIER VARCHAR(250) = NULL,
	@PKG_SHIPPINGMETHOD VARCHAR(250) = NULL,
	@SINGLE_BAG_SORT VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON

        SELECT 
			Sitename AS [SITE],
			MAX(ClientName) AS [CUSTOMER],
            BinCode AS BINCODE, 
            MIN(LabelListDescription) AS DESTINATION,
            ContainerId AS CONTAINER_ID, 
			MIN(ContainerType) as CONT_TYPE,
            MIN(containerbarcode) AS CONT_BARCODE,
            MIN(cont_carrier) AS CONT_CARRIER,
            MIN(cont_method) AS CONT_METHOD, 
            PackageId AS PACKAGE_ID, 
            MIN(ShippingBarcode) AS TRACKING_NUMBER,
            MIN(ShippingCarrier) AS PKG_CARRIER,
            MIN(ShippingMethod) AS PKG_SHIPPINGMETHOD,
			MAX(FORMAT(LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS PKG_PROCESSED_DATE,	
			MAX(PKG_PROCESSED_BY) AS [PKG_PROCESSED_BY],
			MAX(FORMAT(LocalCreatedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS CONT_OPENED_DATE,			
			MAX(OPENED_BY_NAME) AS OPENED_BY_NAME,
			MAX(FORMAT(LocalClosedDate, 'MM/dd/yyyy hh:mm:ss tt')) as CONT_CLOSED_DATE,
			MAX(CLOSED_BY_NAME) AS CLOSED_BY_NAME,
			MAX([PROCESSED_EVENT_TYPE]) AS [EVENT_TYPE],
			MAX(MACHINE_ID) AS [MACHINE_ID],
			MAX([DESTINATION_ZIP]) AS [DESTINATION_ZIP],
			MAX(SITE_KEY) AS [SITE_KEY],
			CAST(MAX(CAST(SINGLE_BAG_SORT AS INT) )AS BIT) AS [SINGLE_BAG_SORT]			
        FROM (SELECT
		p.SiteName,
             p.PackageId,
             p.LocalProcessedDate,
             p.ContainerId,
			 p.ClientName,
             p.BinCode,
			 c.ContainerType,
             p.ShippingBarcode,
             p.ShippingCarrier,
             p.ShippingMethod,
             c.UpdatedBarcode AS containerbarcode,
             c.ShippingCarrier AS cont_carrier,
             c.ShippingMethod AS cont_method,
             b.LabelListDescription,
			 (SELECT TOP 1 e.username FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'CREATED') AS OPENED_BY,
			 (SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username WHERE e.ContainerId = c.ContainerId AND e.EventType = 'CREATED') AS OPENED_BY_NAME,		     
			 (SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username WHERE e.ContainerId = c.ContainerId AND e.EventType = 'SCANCLOSED') AS CLOSED_BY_NAME,
			 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') AS PKG_PROCESSED_BY,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'SCANCLOSED') AS LocalClosedDate,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'CREATED') AS LocalCreatedDate,
			 p.ProcessedEventType AS [PROCESSED_EVENT_TYPE], 
			 p.ProcessedMachineId AS [MACHINE_ID], 
			 p.Zip AS [DESTINATION_ZIP],
			 ISNULL(b.IsScsc,0) AS [SINGLE_BAG_SORT], 
			 s.[Key] as [SITE_KEY]
         FROM [dbo].[PackageDatasets] p
			 LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
			 LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId = b.ActiveGroupId AND c.BinCode = b.BinCode
			 LEFT JOIN [dbo].[SubClientDatasets] s ON p.SubClientName = s.[Name]
			 LEFT JOIN [dbo].UserLookups u on u.Username = p.ProcessedUsername
         WHERE 				 		 
		 p.SiteName = @siteName 
			AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) 
			AND PackageStatus = 'PROCESSED'
			AND (@CONT_TYPE IS NULL OR c.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@CONT_TYPE, '|', 1 )))
			AND (@CONT_CARRIER IS NULL OR c.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@CONT_CARRIER, '|', 1)))
			AND (@CONT_METHOD IS NULL OR c.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@CONT_METHOD, '|', 1)))
			AND (@PKG_CARRIER IS NULL OR  p.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@PKG_CARRIER, '|', 1)))
			AND (@PKG_SHIPPINGMETHOD IS NULL OR p.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@PKG_SHIPPINGMETHOD, '|', 1)))
			AND (@SINGLE_BAG_SORT IS NULL OR ISNULL(b.IsScsc,0) IN (SELECT VALUE FROM STRING_SPLIT(@SINGLE_BAG_SORT, '|', 1)))
			AND (@CUSTOMER IS NULL OR p.ClientName IN (SELECT VALUE FROM STRING_SPLIT(@CUSTOMER, '|', 1)))
			) s
         GROUP BY BinCode, ContainerId, PackageId, opened_by, SiteName
         ORDER BY BinCode, ContainerId, PackageId
END


