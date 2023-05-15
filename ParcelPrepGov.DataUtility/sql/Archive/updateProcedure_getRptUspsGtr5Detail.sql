/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 9/21/2021 8:10:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
		LEFT JOIN (SELECT Siteparent, MIN(Visn) AS Visn, MIN(Sitenumber) AS Sitenumber, MIN(Sitename) AS Sitename 
		FROM dbo.VisnSites GROUP BY Siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.SiteName = @site AND pd.ShippingCarrier = 'USPS' and pd.ClientName='CMOP'
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND e.IsStopTheClock = 1 AND [dbo].[CalendarDateDiff](LocalProcessedDate, t.EventDate) > 5

END