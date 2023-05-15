/****** Object:  StoredProcedure [dbo].[getRptUpsDailyPieceDetail]    Script Date: 11/18/2021 1:26:14 PM By:Chong Vang ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptUpsDailyPieceDetail]
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
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.Weight AS WEIGHT, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC, 
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
		IIF(pd.LastKnownEventDate IS NULL, 0.00, 
			[dbo].[DateTimeDiff](pd.LastKnownEventDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS DAYS_FROM_LAST_KNOWN_STATUS,
		IIF(pd.StopTheClockEventDate >= @manifestDate, [dbo].[CalendarDateDiff](pd.LocalProcessedDate, pd.StopTheClockEventDate), 
			[dbo].[CalendarDateDiff](pd.LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS CALENDAR_DAYS
	FROM dbo.PackageDatasets pd
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'UPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsDailyPieceDetail]    Script Date: 11/18/2021 1:26:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDailyPieceDetail]
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
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CONTAINER_CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.Weight AS WEIGHT, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC, 
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
		IIF(pd.LastKnownEventDate IS NULL, 0.00, 
			[dbo].[DateTimeDiff](pd.LastKnownEventDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS DAYS_FROM_LAST_KNOWN_STATUS,
		IIF(pd.StopTheClockEventDate >= @manifestDate, [dbo].[CalendarDateDiff](pd.LocalProcessedDate, pd.StopTheClockEventDate), 
			[dbo].[CalendarDateDiff](pd.LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS CALENDAR_DAYS
	FROM dbo.PackageDatasets pd --WITH (NOLOCK, FORCESEEK)
	--LEFT JOIN(SELECT PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
	--	OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
	--		FROM dbo.TrackPackageDatasets 
	--					LEFT JOIN dbo.EvsCodes AS ec ON dbo.TrackPackageDatasets.EventCode = ec.Code
	--					LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
	--				WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) 
	--					AND p.PackageStatus = 'PROCESSED' AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)) t
	--			ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	--LEFT JOIN dbo.EvsCodes e ON e.Code = t .EventCode
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

GO

/****** Object:  StoredProcedure [dbo].[getRptFedExDailyPieceDetail]    Script Date: 11/18/2021 1:26:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptFedExDailyPieceDetail]
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
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.Weight AS WEIGHT, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC, 
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
		IIF(pd.LastKnownEventDate IS NULL, 0.00, 
			[dbo].[DateTimeDiff](pd.LastKnownEventDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS DAYS_FROM_LAST_KNOWN_STATUS,
		IIF(pd.StopTheClockEventDate >= @manifestDate, [dbo].[CalendarDateDiff](pd.LocalProcessedDate, pd.StopTheClockEventDate), 
			[dbo].[CalendarDateDiff](pd.LocalProcessedDate, [dbo].[SiteLocalTime](pd.ProcessedDate, pd.LocalProcessedDate))) AS CALENDAR_DAYS
	FROM dbo.PackageDatasets pd
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier <> 'USPS' AND pd.ShippingCarrier <> 'UPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
			AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					pd.LastKnownEventDescription) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

