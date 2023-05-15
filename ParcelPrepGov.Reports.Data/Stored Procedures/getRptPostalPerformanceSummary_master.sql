/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 6/2/2022 4:05:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_master]
(    
	@subClients VARCHAR(300),
	@beginDate AS DATE,
	@endDate AS DATE,
    @product AS VARCHAR(300) = null,   
    @postalArea AS VARCHAR(300) = null,   
    @entryUnitName AS VARCHAR(500) = null,   
    @entryUnitType as VARCHAR(200) = null,   
    @entryUnitCsz AS VARCHAR(500) = null 
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT 'ID' AS ID, 
		pd.SubClientName AS CUST_LOCATION,
		CASE  WHEN LEFT(bd.BinCode, 1) = 'D' THEN 'DDU' ELSE 'SCF' END AS ENTRY_UNIT_TYPE,
		pd.ShippingMethod AS USPS_PRODUCT,
		p.PostalArea AS USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME,
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(pd.StopTheClockEventDate) AS TOTAL_PCS_STC,
		COUNT(pd.PackageId)-COUNT(pd.StopTheClockEventDate) AS TOTAL_PCS_NO_STC,
		[dbo].[Percent](COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS STC_SCAN_PCT,
		[dbo].[Fraction](SUM(pd.PostalDays), COUNT(pd.StopTheClockEventDate)) AS AVG_DEL_DAYS,
		SUM(CASE WHEN pd.PostalDays = 0  AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END) AS DAY0_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 0   AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY0_PCT,
		SUM(CASE WHEN pd.PostalDays = 1 THEN 1 ELSE 0 END) AS DAY1_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 1 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY1_PCT,
		SUM(CASE WHEN pd.PostalDays = 2 THEN 1 ELSE 0 END) AS DAY2_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 2 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY2_PCT,
		SUM(CASE WHEN pd.PostalDays = 3 THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 3 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT
	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	--LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
	--	OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
	--		FROM dbo.TrackPackageDatasets 
	--					LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
	--					LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
	--				WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) 
	--					AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
	--			ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	--LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, '|')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))			
			AND (@entryUnitType IS NULL OR IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') IN (SELECT * FROM [dbo].[SplitString](@entryUnitType, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
	
	GROUP BY 
		pd.SubClientName,
		LEFT(bd.BinCode, 1), 
		pd.ShippingMethod,
		p.PostalArea, 
		bd.DropShipSiteDescriptionPrimary,
		bd.DropShipSiteCszPrimary

END