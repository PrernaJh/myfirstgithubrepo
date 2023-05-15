/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 2/17/2022 10:32:34 AM ******/
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
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
		pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP,
		
		IIF(pd.StopTheClockEventDate IS NOT NULL,
			pd.PostalDays,
			[dbo].[PostalDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate), pd.ShippingMethod)
			) AS POSTAL_DAYS,
		IIF(pd.StopTheClockEventDate IS NOT NULL,
			pd.CalendarDays,
			[dbo].[CalendarDateDiff](LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))
			) AS CAL_DAYS,
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME
		
	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK)
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND pd.StopTheClockEventDate > @beginDate AND pd.CalendarDays > 5
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, ',')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))


END

GO

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_gtr6]    Script Date: 2/17/2022 10:39:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_gtr6]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT        
		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary) AS ID, 

		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary)+
			CONVERT(VARCHAR, left(pd.Zip,3)) AS ID3, 

		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary)+
			CONVERT(VARCHAR, left(pd.Zip,5)) AS ID5, 


		CAST(CONVERT(VARCHAR, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED, 
		s.Description AS CUST_LOCATION,
		pd.PackageId AS PACKAGE_ID, 
		pd.ShippingBarcode AS TRACKING_NUMBER, 
		pd.City AS DEST_CITY,
		pd.State AS DEST_STATE,
		pd.Zip AS DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		bd.ShippingCarrierPrimary AS CARRIER, 
		p.PostalArea AS USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		t.EventDescription AS LAST_KNOWN_DESC, 
		t.EventDate AS LAST_KNOWN_DATE,
		t.EventLocation AS LAST_KNOWN_LOCATION, 
		t.EventZip AS LAST_KNOWN_ZIP, 
		v.Visn AS VISN,
		v.SiteNumber AS PHARM_DIV_NO,
		v.SiteName AS PHARM_DIV

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT PackageId, PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets
				JOIN dbo.EvsCodes e ON e.Code = EventCode
					WHERE e.IsStopTheClock = 1) t ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName

	WHERE CAST(DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate) AS DECIMAL) >= 6 AND pd.PackageStatus = 'PROCESSED' AND pd.SiteName = @site 
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUndeliveredReport]    Script Date: 2/17/2022 10:42:18 AM ******/
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
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME

	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE (pd.StopTheClockEventDate IS NULL) AND pd.PackageStatus = 'PROCESSED' AND s.Name IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

GO

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_nostc]    Script Date: 2/17/2022 10:49:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_nostc]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

		-- Insert statements for procedure here
	SELECT      
		CAST(CONVERT(VARCHAR, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED, 
		s.Description AS CUST_LOCATION,
		v.Visn AS VISN,
		v.SiteNumber AS PHARM_DIV_NO,
		v.SiteName AS PHARM_DIV,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.City AS DEST_CITY,
		pd.State AS DEST_STATE,
		pd.Zip AS DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		bd.ShippingCarrierPrimary AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		t.EventDescription AS LAST_KNOWN_DESC, 
		t.EventDate AS LAST_KNOWN_DATE,
		t.EventLocation AS LAST_KNOWN_LOCATION, 
		t.EventZip AS LAST_KNOWN_ZIP

	FROM dbo.PackageDatasets pd
	LEFT JOIN(SELECT PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes AS ec ON dbo.TrackPackageDatasets.EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName

	WHERE e.IsStopTheClock != 1 AND pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SiteName = @site AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND t.EventDate IS NULL 

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_detail]    Script Date: 2/17/2022 10:51:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[getRptUspsUndeliverables_detail]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
	SELECT
		CONVERT(varchar, s.Description) + 
				IIF(v.Visn IS NULL, 'null',  v.Visn) +
				IIF(v.SiteNumber IS NULL, 'null', v.SiteNumber) +
				IIF(v.SiteName IS NULL, 'null',  v.SiteName)  +
				IIF(pd.LastKnownEventDescription IS NULL, 'null', pd.LastKnownEventDescription)
			AS ID, 
		s.Description AS CUST_LOCATION, 
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME,
		pd.PackageId AS PACKAGE_ID,
		pd.Zip AS ZIP,
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		pd.LastKnownEventDescription AS EVENT_DESC,
		CAST(CONVERT(varchar, pd.LastKnownEventDate, 101) AS DATE) AS EVENT_DATE
	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.IsUndeliverable = 1 AND pd.PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS' AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_master]    Script Date: 2/17/2022 10:53:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[getRptUspsUndeliverables_master]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
SELECT
	CONVERT(varchar, s.Description) + 
			IIF(v.Visn IS NULL, 'null',  v.Visn) +
			IIF(v.SiteNumber IS NULL, 'null', v.SiteNumber) +
			IIF(v.SiteName IS NULL, 'null',  v.SiteName)  +
			IIF(pd.LastKnownEventDescription IS NULL, 'null', pd.LastKnownEventDescription)
		AS ID, 
	s.Description AS CUST_LOCATION, 
	v.Visn AS VISN,
	v.SiteNumber AS MEDICAL_CENTER_NO,
	v.SiteName AS MEDICAL_CENTER_NAME, 
	pd.LastKnownEventDescription AS EVENT_DESC,		
	COUNT(pd.PackageId) AS TOTAL_PCS 
	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.IsUndeliverable = 1 AND PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

	GROUP BY 
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		pd.LastKnownEventDescription

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsVisnDeliverySummary]    Script Date: 2/17/2022 10:54:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptUspsVisnDeliverySummary]    Script Date: 5/10/2021 11:19:09 AM ******/

ALTER PROCEDURE [dbo].[getRptUspsVisnDeliverySummary]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT      
		s.Description AS LOCATION,
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME,
		pd.ShippingMethod AS PRODUCT, 

		SUM(CASE WHEN pd.PostalDays <= 3 AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays <=3  AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
		SUM(CASE WHEN pd.PostalDays >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,
		
		
		COUNT(pd.StopTheClockEventDate) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM(pd.PostalDays), COUNT(pd.StopTheClockEventDate)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(pd.CalendarDays), COUNT(pd.StopTheClockEventDate)) AS AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS NO_STC_PCT,
		COUNT(pd.PackageId) AS TOTAL_PCS 

	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND NOT v.SiteParent IS NULL
	
	GROUP BY
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		pd.ShippingMethod


	ORDER BY
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		pd.ShippingMethod

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsVisnTrackingSummary]    Script Date: 2/17/2022 10:56:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptUspsVisnTrackingSummary]    Script Date: 5/10/2021 11:19:58 AM ******/

ALTER PROCEDURE [dbo].[getRptUspsVisnTrackingSummary]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT      
		s.Description AS LOCATION,
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME,
		pd.ShippingMethod AS PRODUCT, 
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(pd.StopTheClockEventDate) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS DELIVERED_PCT,
			
		[dbo].[Fraction](SUM(pd.PostalDays), COUNT(pd.StopTheClockEventDate)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(pd.CalendarDays), COUNT(pd.StopTheClockEventDate)) AS AVG_CAL_DAYS,

		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END) AS SIGNATURE_PCS,
		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END) AS SIGNATURE_DELIVERED_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END), 
				SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END)) AS SIGNATURE_DELIVERED_PCT,
		
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL 
				THEN pd.PostalDays ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL 
				THEN pd.CalendarDays ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_CAL_DAYS,
		
		
		COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS NO_STC_PCT

	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		pd.ShippingMethod

	ORDER BY
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		pd.ShippingMethod

END

GO