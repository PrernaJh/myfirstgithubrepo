/****** Object:  UserDefinedFunction [dbo].[PostalDateDiff]    Script Date: 4/23/2021 11:40:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[PostalDateDiff]
(
	@beginDate DATE,
	@endDate DATE,
	@shippingMethod VARCHAR(60)
)
RETURNS INT
AS
BEGIN
    DECLARE @beginDays INT
    DECLARE @endDays INT
    DECLARE @diff INT
	DECLARE @dropPointDay INT = 0

	SELECT @beginDays =
		(SELECT Ordinal
			FROM [dbo].[PostalDays]
				WHERE @beginDate = PostalDate
		)

	SELECT @endDays =
		(SELECT Ordinal
			FROM [dbo].[PostalDays]
				WHERE @endDate = PostalDate
		)

	SELECT @diff = CASE WHEN @beginDays IS NULL OR @endDays IS NULL
		THEN DATEDIFF(DAY, @beginDate, @endDate)
		ELSE @endDays - @beginDays
		END

	IF @shippingMethod = 'PSLW'
		SET @dropPointDay = 1
	
	RETURN CASE WHEN @diff > 0 THEN @diff - @dropPointDay ELSE 0 END -- Don't count ship date.
END

GO 

/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 5/12/2021 8:15:16 AM ******/

ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_master]
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
		CONVERT(varchar, pd.SubClientName) + 
			IIF(LEFT(bd.BinCode, 1) IS NULL, ' ',  LEFT(bd.BinCode, 1)) + 
			IIF(p.PostalArea IS NULL, 'null',  p.PostalArea) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) 
			AS ID, 

		pd.SubClientName AS CMOP,
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
						AND p.SiteName = @site and p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SiteName = @site AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		and pd.ClientName='CMOP'
	
	GROUP BY 
		pd.SubClientName,
		LEFT(bd.BinCode, 1), 
		p.PostalArea, 
		bd.DropShipSiteDescriptionPrimary

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 5/26/2021 1:51:01 PM ******/

ALTER PROCEDURE [dbo].[getRptUspsGtr5Detail]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
    @beginDate AS DATE,
	@endDate AS DATE = @beginDate

)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		pd.SiteName AS LOCATION, 
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		pd.City as DEST_CITY,
		pd.State as DEST_STATE,
		pd.Zip as DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
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
		IIF(t.EventDate is NULL, null, [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)) AS POSTAL_DAYS,
		IIF(t.EventDate is NULL, null, [dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)) AS CAL_DAYS,
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
		LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.SiteName = @site AND pd.ShippingCarrier = 'USPS' and pd.ClientName='CMOP'
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND ((e.IsStopTheClock != 1 AND [dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate) > 5) OR
			 (e.IsStopTheClock = 1 AND [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) > 5))

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsLocationDeliverySummary]    Script Date: 5/10/2021 11:13:58 AM ******/

ALTER PROCEDURE [dbo].[getRptUspsLocationDeliverySummary]
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
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <= 3 AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <=3  AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,
		COUNT(t.PackageId) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)), COUNT(t.PackageId)) AS AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(t.PackageId) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(t.PackageId), COUNT(pd.PackageId)) AS NO_STC_PCT,
		COUNT(pd.PackageId) AS TOTAL_PCS 

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY s.Description

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsLocationTrackingSummary]    Script Date: 5/10/2021 11:14:43 AM ******/

ALTER PROCEDURE [dbo].[getRptUspsLocationTrackingSummary]
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
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(t.PackageId) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)), COUNT(t.PackageId)) AS AVG_CAL_DAYS,
		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END) AS SIGNATURE_PCS,
		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END) AS SIGNATURE_DELIVERED_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END), 
				SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END)) AS SIGNATURE_DELIVERED_PCT,
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL 
				THEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL 
				THEN [dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate) ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(t.PackageId) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(t.PackageId), COUNT(pd.PackageId)) AS NO_STC_PCT

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 				
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY s.Description

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsProductDeliverySummary]    Script Date: 5/10/2021 11:16:11 AM ******/

ALTER PROCEDURE [dbo].[getRptUspsProductDeliverySummary]
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
		pd.ShippingMethod AS PRODUCT, 
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <= 3 AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <=3  AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,
		COUNT(t.PackageId) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)), COUNT(t.PackageId)) AS AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(t.PackageId) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(t.PackageId), COUNT(pd.PackageId)) AS NO_STC_PCT,
		COUNT(pd.PackageId) AS TOTAL_PCS 

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY pd.ShippingMethod

END

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
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <= 3 AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) <=3  AND NOT t.EventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,
		COUNT(t.PackageId) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)), COUNT(t.PackageId)) AS AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(t.PackageId) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(t.PackageId), COUNT(pd.PackageId)) AS NO_STC_PCT,
		COUNT(pd.PackageId) AS TOTAL_PCS 

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	
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
		COUNT(t.PackageId) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(t.PackageId), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod)), COUNT(t.PackageId)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate)), COUNT(t.PackageId)) AS AVG_CAL_DAYS,
		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END) AS SIGNATURE_PCS,
		SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END) AS SIGNATURE_DELIVERED_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END), 
				SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' THEN 1 ELSE 0 END)) AS SIGNATURE_DELIVERED_PCT,
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL 
				THEN [dbo].[PostalDateDiff](LocalProcessedDate, t.EventDate, pd.ShippingMethod) ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL 
				THEN [dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate) ELSE 0 END),
			SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT t.PackageId IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(t.PackageId) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(t.PackageId), COUNT(pd.PackageId)) AS NO_STC_PCT

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	
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
