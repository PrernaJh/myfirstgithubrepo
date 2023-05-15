/****** Object:  StoredProcedure [dbo].[getRptUspsStcDetail]    Script Date: 10/7/2021 2:47:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[getRptUspsStcDetail]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
    @manifestDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null,
	@lastKnowDesc AS VARCHAR(MAX) = null
)
AS
BEGIN
	SELECT 
		s.Description AS CUST_LOCATION, 
		CAST(pd.LocalProcessedDate AS date) AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT, 
		p.PostalArea AS USPS_AREA,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.Weight AS WEIGHT, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC, 
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP, 
		IIF(t.EventDate IS NULL, 0.00, 
			[dbo].[DateTimeDiff](t.EventDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS DAYS_FROM_LAST_KNOWN_STATUS,
		IIF(e.IsStopTheClock = 1, [dbo].[CalendarDateDiff](pd.LocalProcessedDate, t.EventDate), 
			[dbo].[CalendarDateDiff](pd.LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS CALENDAR_DAYS
	FROM dbo.PackageDatasets pd
	LEFT JOIN(SELECT PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes AS ec ON dbo.TrackPackageDatasets.EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) 
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END





GO




/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_master]    Script Date: 10/7/2021 10:41:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_master]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		CONVERT(varchar, s.Description) + 
			CONVERT(varchar, pd.LocalProcessedDate, 101) + 
				IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
				IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary) +
				IIF(pd.ShippingMethod IS NULL, 'null',  pd.ShippingMethod)  +
				IIF(p.PostalArea IS NULL, 'null',  p.PostalArea)  +
				IIF(bd.ShippingCarrierPrimary IS NULL, 'null',  bd.ShippingCarrierPrimary)
			AS ID, 
		s.Description AS CUST_LOCATION, 
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT, 
		p.PostalArea AS USPS_AREA,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(DISTINCT(sc.ContainerId)) AS TOTAL_BAGS, 
		(COUNT(pd.PackageId) - SUM(CONVERT(int, ISNULL(e.IsStopTheClock,0)))) AS PCS_NO_STC, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - SUM(CONVERT(int, ISNULL(e.IsStopTheClock,0))))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
		(COUNT(pd.PackageId) - COUNT(t.EventDate)) AS PCS_NO_SCAN, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(t.EventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
	FROM dbo.PackageDatasets pd
	LEFT JOIN dbo.ShippingContainerDatasets sc ON sc.ContainerId = pd.ContainerId
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON EventCode = ec.Code 
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) 
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS'
						AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
		
	GROUP BY 
		s.Description, 
		CONVERT(varchar, pd.LocalProcessedDate, 101),
		bd.DropShipSiteDescriptionPrimary, 
		bd.DropShipSiteCszPrimary, 
		pd.ShippingMethod, 
		p.PostalArea, 
		bd.ShippingCarrierPrimary

END
