/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_master]    Script Date: 10/15/2021 9:43:43 AM ******/
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
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitType as VARCHAR(MAX) = null,
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
	LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code
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
		p.PostalArea, 
		bd.DropShipSiteDescriptionPrimary

END

