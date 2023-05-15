
IF ( NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'UserLookups'))
BEGIN
    CREATE TABLE [UserLookups] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(256) NULL,
    [FirstName] nvarchar(200) NULL,
    [LastName] nvarchar(200) NULL,
    CONSTRAINT [PK_UserLookups] PRIMARY KEY ([UserId])
	);
END



GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220728211855_AddUserLookup')
BEGIN
	INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
	VALUES (N'20220728211855_AddUserLookup', N'3.1.6');
END

GO

CREATE OR ALTER PROCEDURE [dbo].[getPackageEvent_data]
(    
	@ids as VARCHAR(MAX),
	@beginDate AS DATE = '2020-06-01'
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT p.*, ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'') as DisplayName 
		FROM [dbo].[PackageEventDatasets] p LEFT JOIN UserLookups u on u.Username = p.Username
			WHERE EventDate >= @beginDate
				AND CosmosId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY EventDate DESC

END

GO

CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchEventsByContainerId]
    @containerId VARCHAR(50),
	@siteName VARCHAR(50)
AS
BEGIN
	-- get external events first and then append internal events
	DECLARE @id INT = (SELECT TOP 1 id FROM ShippingContainerDatasets WHERE ContainerId = @containerId ORDER BY CosmosCreateDate DESC)

	-- get external events first and then append internal events
	SELECT LocalEventDate AS [LOCAL_EVENT_DATE],
	sc.UpdatedBarcode AS [TRACKING_NUMBER],	
	EventType AS [EVENT_TYPE],
	EventStatus AS [EVENT_STATUS],
	scd.Username AS [USER_NAME],
	ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'') as DISPLAY_NAME,  
	scd.MachineId AS [MACHINE_ID],
	sc.ShippingCarrier AS [SHIPPING_CARRIER]
	FROM ShippingContainerEventDatasets scd 
		INNER JOIN ShippingContainerDatasets sc ON sc.Id = scd.ShippingContainerDatasetId
		LEFT JOIN UserLookups u ON u.username = scd.username
	WHERE sc.Id = @id
		AND sc.sitename = @siteName	
	UNION
		SELECT t.EventDate AS [LOCAL_EVENT_DATE], 
	t.TrackingNumber AS [TRACKING_NUMBER],	
	t.EventCode AS [EVENT_TYPE],
	t.EventDescription AS [EVENT_STATUS],
	'' AS [USER_NAME],
	'' AS [DISPLAY_NAME],
	t.SiteName AS [MACHINE_ID],
	t.ShippingCarrier AS [SHIPPING_CARRIER]
	FROM [dbo].[TrackPackageDatasets] t
		INNER JOIN dbo.ShippingContainerDatasets AS p ON t.ShippingContainerDatasetId = p.Id	
	WHERE p.Id = @id	
		AND p.siteName = @siteName
	ORDER BY [LOCAL_EVENT_DATE] DESC
END

GO

CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
	@CONT_CARRIER VARCHAR(250) = NULL,
	@CONT_METHOD VARCHAR(250) = NULL,
	@PKG_CARRIER VARCHAR(250) = NULL,
	@PKG_SHIPPINGMETHOD VARCHAR(250) = NULL,
	@CONT_TYPE VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON

        SELECT 
			Sitename AS [SITE],
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
			OPENED_BY AS OPENED_BY,
			MAX(OPENED_BY_NAME) AS OPENED_BY_NAME,
			MAX(FORMAT(LocalCreatedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS CONT_OPENED_DATE,
			MAX(CLOSED_BY) AS CLOSED_BY,
			MAX(CLOSED_BY_NAME) AS CLOSED_BY_NAME,
			MAX(FORMAT(LocalClosedDate, 'MM/dd/yyyy hh:mm:ss tt')) as CONT_CLOSED_DATE
			
        FROM (SELECT
		p.SiteName,
             p.PackageId,
             p.LocalProcessedDate,
             p.ContainerId,
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
		     (SELECT TOP 1 e.Username FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'SCANCLOSED') AS CLOSED_BY,
			 (SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username WHERE e.ContainerId = c.ContainerId AND e.EventType = 'SCANCLOSED') AS CLOSED_BY_NAME,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'SCANCLOSED') AS LocalClosedDate,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e WHERE e.ContainerId = c.ContainerId AND e.EventType = 'CREATED') AS LocalCreatedDate
         FROM [dbo].[PackageDatasets] p
			 LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
			 LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId = b.ActiveGroupId AND c.BinCode = b.BinCode
         WHERE 				 
		 p.SiteName = @siteName 
			AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) 
			AND PackageStatus = 'PROCESSED'
			AND (@CONT_TYPE IS NULL OR c.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@CONT_TYPE, '|', 1 )))
			AND (@CONT_CARRIER IS NULL OR c.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@CONT_CARRIER, '|', 1)))
			AND (@CONT_METHOD IS NULL OR c.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@CONT_METHOD, '|', 1)))
			AND (@PKG_CARRIER IS NULL OR  p.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@PKG_CARRIER, '|', 1)))
			AND (@PKG_SHIPPINGMETHOD IS NULL OR p.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@PKG_SHIPPINGMETHOD, '|', 1)))
			) s
         GROUP BY BinCode, ContainerId, PackageId, opened_by, SiteName
         ORDER BY BinCode, ContainerId, PackageId
END

GO


CREATE OR ALTER   PROCEDURE [dbo].[getRptDailyContainer_master]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
    @product AS VARCHAR(MAX) = NULL,
    @postalArea AS VARCHAR(MAX) = NULL,
    @entryUnitName AS VARCHAR(MAX) = NULL,
    @entryUnitCsz AS VARCHAR(MAX) = NULL
) AS
BEGIN
    SET NOCOUNT ON
    SELECT
        MAX(s.SiteName + ',' + ISNULL(sc.ContainerId, '')) AS ID,
        MAX(sc.ContainerId) AS CONTAINER_ID,
        sc.Status as CONTAINER_STATUS,
        MAX(FORMAT(sc.DatasetCreateDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_OPEN_DATE,
        MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
        MAX(bd.BinCode) as BIN_NUMBER,
        MAX(sc.Username) as USERNAME,
        MAX(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'')) as DISPLAY_NAME,
        MAX(FORMAT(sc.LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_CLOSED_DATE,
        COUNT(pd.PackageId) AS TOTAL_PACKAGES,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as DROP_SHIP_SITE_KEY,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ 
    FROM dbo.PackageDatasets pd WITH (NOLOCK)
    	LEFT JOIN dbo.ShippingContainerDatasets sc on pd.ContainerId = sc.ContainerId
   		LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
   		LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
		LEFT JOIN dbo.UserLookups u on u.Username = sc.Username
    WHERE 
		pd.SiteName = @siteName
		AND PackageStatus = 'PROCESSED'
		AND pd.ShippingMethod IN('PSLW','FCZ', 'PS')			
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
        AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
    GROUP BY 
        sc.ContainerId,
        sc.Status
UNION 
		SELECT -- give me closed containers that do not have any packages inside!
		sc.ContainerId AS ID,		 
		sc.ContainerId AS CONTAINER_ID,
		sc.Status as CONTAINER_STATUS,
		MAX(FORMAT(sc.DatasetCreateDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_OPEN_DATE,
		MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
		MAX(sc.BinCode) AS BIN_CODE,
		MAX(sc.Username) as USERNAME,
        MAX(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'')) as DISPLAY_NAME,
        MAX(FORMAT(sc.LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_CLOSED_DATE,
		COUNT(DISTINCT(pd.id)) AS TOTAL_PCS, 
		MAX(sc.ShippingCarrier) AS CARRIER, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY,
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ
	FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK, FORCESEEK) 
			LEFT JOIN dbo.BinDatasets bd ON sc.BinActiveGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
			LEFT JOIN dbo.PackageDatasets pd on sc.ContainerId = pd.ContainerId
			LEFT JOIN dbo.UserLookups u on u.Username = sc.Username
		WHERE sc.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 		
			AND sc.SiteName = @siteName
		AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
	GROUP BY sc.ContainerId, sc.Status
	HAVING COUNT(DISTINCT(pd.id)) = 0		
ORDER BY
CONTAINER_STATUS,
TOTAL_PACKAGES

END






