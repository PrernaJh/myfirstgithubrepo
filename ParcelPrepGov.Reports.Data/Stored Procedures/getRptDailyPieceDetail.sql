
CREATE OR ALTER   PROCEDURE [dbo].[getRptDailyPieceDetail]
(    
	@subClients VARCHAR(MAX),
    @manifestDate AS DATE,
	@product AS VARCHAR(MAX) = NULL,
	@postalArea AS VARCHAR(MAX) = NULL,
	@entryUnitName AS VARCHAR(MAX) = NULL,
	@entryUnitCsz AS VARCHAR(MAX) = NULL,
	@lastKnowDesc AS VARCHAR(MAX) = NULL,
	@PACKAGE_CARRIER AS VARCHAR(MAX) = NULL
)
AS
BEGIN
	SELECT 
		s.Description AS CUST_LOCATION, 
		CAST(pd.LocalProcessedDate AS date) AS MANIFEST_DATE, 
		IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
		IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
		CASE WHEN LEFT(bd.BinCode, 1) = 'D' THEN 'DDU' WHEN LEFT(bd.BinCode, 1) = 'S' THEN 'SCF' ELSE '' END AS ENTRY_UNIT_TYPE, 		
		pd.ShippingMethod AS PRODUCT, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,		
		p.PostalArea AS POSTAL_AREA,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		scd.ShippingCarrier AS CONTAINER_CARRIER,		
		scd.ShippingMethod AS CONTAINER_SHIPPING_METHOD,		
		scd.UpdatedBarcode AS CONTAINER_TRACKING_NUMBER,
		pd.Weight AS WEIGHT, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC, 
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
		IIF(pd.LastKnownEventDate IS NULL, 0.00, 
			[dbo].[DateTimeDiff](pd.LastKnownEventDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS DAYS_FROM_LAST_KNOWN_STATUS,
		IIF(pd.StopTheClockEventDate >= @manifestDate, pd.CalendarDays, 
			[dbo].[CalendarDateDiff](pd.LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS CALENDAR_DAYS
	FROM dbo.PackageDatasets pd
		LEFT JOIN dbo.ShippingContainerDatasets scd ON pd.ContainerId = scd.ContainerId
		LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
		LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
		LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
			AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
			AND (@PACKAGE_CARRIER IS NULL OR pd.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@PACKAGE_CARRIER, '|', 1)))
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1)))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT VALUE FROM STRING_SPLIT(@postalArea, '|', 1)))
			AND (@entryUnitName IS NULL OR IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) 
				IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
			AND (@entryUnitCsz IS NULL OR IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) 
				IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT VALUE FROM STRING_SPLIT(@lastKnowDesc, '|')))

END