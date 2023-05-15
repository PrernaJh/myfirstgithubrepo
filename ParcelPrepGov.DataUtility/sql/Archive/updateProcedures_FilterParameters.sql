/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 10/5/2021 11:05:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[getRptUspsGtr5Detail]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
    @beginDate AS DATE,
    @endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null,
	@lastKnowDesc AS VARCHAR(MAX) = null

)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		s.Description AS CUST_LOCATION, 
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		pd.City as DEST_CITY,
		pd.State as DEST_STATE,
		pd.Zip as DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		p.PostalArea AS USPS_AREA,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		--IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), t.EventDescription) AS LAST_KNOWN_DESC, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP,
		IIF(e.IsStopTheClock = 1,
			[dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod),
			[dbo].[PostalDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate), pd.ShippingMethod)
			) AS POSTAL_DAYS,
		IIF(e.IsStopTheClock = 1,
			[dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate),
			[dbo].[CalendarDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))
			) AS CAL_DAYS,
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME
	FROM dbo.PackageDatasets pd 
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS'
						AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, ',')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsStcDetail]    Script Date: 10/5/2021 11:06:07 AM ******/
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
	LEFT JOIN dbo.EvsCodes e ON e.Code = t .EventCode
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
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
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

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_detail]    Script Date: 10/7/2021 10:52:36 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_detail]
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
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT, 
		p.PostalArea AS USPS_AREA,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.Zip
	FROM dbo.PackageDatasets pd 
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS'
						AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
END

GO

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 10/7/2021 11:24:26 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 10/7/2021 11:24:26 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 5/12/2021 8:15:16 AM ******/

ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_master]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT        
		CONVERT(varchar, pd.SubClientName) + 
			IIF(LEFT(bd.BinCode, 1) IS NULL, ' ',  LEFT(bd.BinCode, 1)) + 
			IIF(p.PostalArea IS NULL, 'null',  p.PostalArea) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) 
			AS ID, 

		pd.SubClientName AS CUST_LOCATION,
		MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		p.PostalArea AS USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(t.PackageId) AS TOTAL_PCS_STC,
		COUNT(pd.PackageId)-COUNT(t.PackageId) AS TOTAL_PCS_NO_STC,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS STC_SCAN_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_DEL_DAYS,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 0  AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END) AS DAY0_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 0   AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY0_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 1 THEN 1 ELSE 0 END) AS DAY1_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 1 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY1_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 2 THEN 1 ELSE 0 END) AS DAY2_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 2 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY2_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 3 THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 3 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT
	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) 
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
	
	GROUP BY 
		pd.SubClientName,
		LEFT(bd.BinCode, 1), 
		p.PostalArea, 
		bd.DropShipSiteDescriptionPrimary

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUndeliveredReport]    Script Date: 10/7/2021 11:44:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[getRptUndeliveredReport]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null,
	@lastKnowDesc AS VARCHAR(MAX) = null
)
AS
BEGIN
	SELECT
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED,
		s.Description as CUST_LOCATION,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.City as DEST_CITY,
		pd.State as DEST_STATE,
		pd.Zip as DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		p.PostalArea as USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		--IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), t.EventDescription) AS LAST_KNOWN_DESC, 
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP, 
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME

	FROM dbo.PackageDatasets pd
	LEFT JOIN(SELECT PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes AS ec ON dbo.TrackPackageDatasets.EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE (e.IsStopTheClock != 1 or e.IsStopTheClock IS NULL) AND pd.PackageStatus = 'PROCESSED' AND s.Name IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

GO












/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 10/11/2021 3:15:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[getRptUspsGtr5Detail]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
    @beginDate AS DATE,
    @endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null,
	@lastKnowDesc AS VARCHAR(MAX) = null

)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		s.Description AS CUST_LOCATION, 
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		pd.City as DEST_CITY,
		pd.State as DEST_STATE,
		pd.Zip as DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		p.PostalArea AS USPS_AREA,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
		IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP,
		IIF(e.IsStopTheClock = 1,
			[dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod),
			[dbo].[PostalDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate), pd.ShippingMethod)
			) AS POSTAL_DAYS,
		IIF(e.IsStopTheClock = 1,
			[dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate),
			[dbo].[CalendarDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))
			) AS CAL_DAYS,
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME
	FROM dbo.PackageDatasets pd 
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets tpd
						LEFT JOIN dbo.EvsCodes ec ON tpd.EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON tpd.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS'
						AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, ',')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END



GO


/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_master]    Script Date: 10/11/2021 1:04:56 PM ******/
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
	LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets tpd
						LEFT JOIN dbo.EvsCodes ec ON tpd.EventCode = ec.Code 
						LEFT JOIN dbo.PackageDatasets AS p ON tpd.PackageDatasetId = p.Id
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


/****** Object:  StoredProcedure [dbo].[getRptUndeliveredReport]    Script Date: 10/12/2021 11:13:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[getRptUndeliveredReport]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null,
	@lastKnowDesc AS VARCHAR(MAX) = null
)
AS
BEGIN
	SELECT
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED,
		s.Description as CUST_LOCATION,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.City as DEST_CITY,
		pd.State as DEST_STATE,
		pd.Zip as DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		p.PostalArea as USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		--IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), t.EventDescription) AS LAST_KNOWN_DESC, 
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP, 
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME

	FROM dbo.PackageDatasets pd
	LEFT JOIN(SELECT PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes AS ec ON dbo.TrackPackageDatasets.EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON  t.EventCode = e.Code
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE (e.IsStopTheClock != 1 or e.IsStopTheClock IS NULL) AND pd.PackageStatus = 'PROCESSED' AND s.Name IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END