/****** Object:  StoredProcedure [dbo].[getRptUspsGtr5Detail]    Script Date: 10/11/2021 3:15:15 PM ******/
-- =============================================
-- Author:      Jeff Johnson Story 1202
-- Modified Date: 10/16/21
-- Description: Update to Gtr5 days report showing packages and events it should not be. Added line 79 to resolve this.
-- =============================================
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
			AND e.IsStopTheClock = 1 AND [dbo].CalendarDateDiff(LocalProcessedDate, t.EventDate) > 5
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@postalArea IS NULL OR p.PostalArea IN (SELECT * FROM [dbo].[SplitString](@postalArea, ',')))
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
			AND (@lastKnowDesc IS NULL OR IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
					IIF(e.Id IS NULL, t.EventDescription, e.Description)) IN (SELECT * FROM [dbo].[SplitString](@lastKnowDesc, '|')))

END

