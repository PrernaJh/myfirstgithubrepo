/*
	exec [getContainerSearchEventsByContainerId] '99M901958890000101105', 'DALLAS'
*/
CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchEventsByContainerId]
    @containerId varchar(50),
	@siteName varchar(50)
AS
BEGIN
	-- get external events first and the append internal events
	DECLARE @id int = (select top 1 id from ShippingContainerDatasets where ContainerId = @containerId ORDER BY CosmosCreateDate DESC)

	-- get external events first and the append internal events
	select LocalEventDate AS [LOCAL_EVENT_DATE],
	sc.UpdatedBarcode AS [TRACKING_NUMBER],	
	EventType AS [EVENT_TYPE],
	EventStatus AS [EVENT_STATUS],
	scd.Username AS [USER_NAME],
	scd.MachineId AS [MACHINE_ID],
	sc.ShippingCarrier AS [SHIPPING_CARRIER]
	from ShippingContainerEventDatasets scd INNER JOIN ShippingContainerDatasets sc ON sc.Id = scd.ShippingContainerDatasetId
	where sc.Id = @id
	AND sc.sitename = @siteName	
	UNION
		SELECT t.EventDate AS [LOCAL_EVENT_DATE], 
	t.TrackingNumber AS [TRACKING_NUMBER],	
	t.EventCode AS [EVENT_TYPE],
	t.EventDescription AS [EVENT_STATUS],
	'' AS [USER_NAME],
	t.SiteName AS [MACHINE_ID],
	t.ShippingCarrier AS [SHIPPING_CARRIER]
	FROM [dbo].[TrackPackageDatasets] t
		INNER JOIN dbo.ShippingContainerDatasets AS p ON  t.ShippingContainerDatasetId = p.Id	
	WHERE p.Id = @id	
	AND p.siteName = @siteName
	ORDER BY [LOCAL_EVENT_DATE] DESC
END



GO
/*
exec [getContainerSearchPackages] '99M901958890000101105', 'DALLAS'
*/
CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchPackages]
    @containerId varchar(50),
	@siteName varchar(50)
AS
BEGIN   
  SELECT 
  pd.Id,
  pd.PackageId AS [PACKAGE_ID],
  IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
  pd.ShippingCarrier AS CARRIER, 
  pd.ShippingMethod AS SHIPPING_METHOD,    
  pd.PackageStatus AS [PACKAGE_STATUS],
  pd.RecallDate AS [RECALL_DATE],
  pd.RecallStatus AS [RECALL_STATUS],
  IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, 'SHIPPED'), pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP
  FROM PackageDatasets pd 
  WHERE pd.SiteName = @siteName	
	AND pd.ContainerId = @containerId 		
	ORDER BY   IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) DESC	
END

GO

/*
	exec [getContainerSearchByBarcode] '99M901958890000101105'
*/
CREATE OR ALTER   PROCEDURE [dbo].[getContainerSearchByBarcode]     
@barcode varchar(100), 
@siteName varchar(50) = NULL
AS
BEGIN
	SELECT TOP 1 	
		sc.ContainerId AS CONTAINER_ID,
		CAST(CONVERT(varchar, sc.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		sc.Status AS [STATUS], 
		sc.ContainerType AS CONTAINER_TYPE,
        IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) AS SHIPPING_CARRIER, 
        IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) AS SHIPPING_METHOD,  
		sc.UpdatedBarcode AS TRACKING_NUMBER,
		sc.BinCode AS BIN_CODE,
		sc.SiteName AS [FSC_SITE],
		sc.Zone AS [ZONE],
		IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary) AS DROP_SHIP_SITE_KEY,
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
        IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		sc.Weight AS CONTAINER_WEIGHT,		
		sc.LastKnownEventDate AS LAST_KNOWN_DATE,
        sc.LastKnownEventDescription AS LAST_KNOWN_DESCRIPTION,
        sc.LastKnownEventLocation AS LAST_KNOWN_LOCATION,
        sc.LastKnownEventZip AS LAST_KNOWN_ZIP,
		CASE WHEN sc.IsSecondaryCarrier=1 THEN 1 ELSE 0 END AS [IS_SECONDARY_CARRIER]
    FROM dbo.ShippingContainerDatasets SC WITH (NOLOCK) 	
    LEFT JOIN dbo.BinDatasets bd ON  bd.ActiveGroupId = sc.BinActiveGroupId AND  bd.BinCode = sc.BinCode
    WHERE (sc.SiteName = @siteName OR @siteName IS NULL OR @siteName = 'GLOBAL')
	AND (sc.ContainerId = @barcode OR sc.UpdatedBarcode = @barcode)
ORDER BY SC.CosmosCreateDate DESC
   
END
