/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_master]    Script Date: 9/28/2021 12:49:50 PM ******/
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
			IIF(IIF(e.Description IS NULL, t.EventDescription, e.Description) IS NULL, 'null', IIF(e.Description IS NULL, t.EventDescription, e.Description))
		AS ID, 
	s.Description AS CUST_LOCATION, 
	v.Visn AS VISN,
	v.SiteNumber AS MEDICAL_CENTER_NO,
	v.SiteName AS MEDICAL_CENTER_NAME, 
	IIF(e.Description IS NULL, t.EventDescription, e.Description) AS EVENT_DESC,		
	COUNT(pd.PackageId) AS TOTAL_PCS 
	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT PackageDatasetId, EventDescription, EventDate, EventCode, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
					WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
						AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
				ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE e.IsUndeliverable = 1 AND PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

	GROUP BY 
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		e.Description,
		IIF(e.Description IS NULL, t.EventDescription, e.Description)

END


GO

/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_detail]    Script Date: 9/28/2021 12:50:00 PM ******/
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
				IIF(IIF(e.Description IS NULL, t.EventDescription, e.Description) IS NULL, 'null', IIF(e.Description IS NULL, t.EventDescription, e.Description))
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
		IIF(e.Description IS NULL, t.EventDescription, e.Description) AS EVENT_DESC,
		CAST(CONVERT(varchar, t.EventDate, 101) AS DATE) AS EVENT_DATE
	FROM dbo.PackageDatasets pd
	LEFT JOIN(SELECT PackageDatasetId, EventDate, EventCode, EventDescription, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets 
						LEFT JOIN dbo.EvsCodes ec on EventCode = ec.Code
						LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
							WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
								AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t
									ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v 
		ON v.SiteParent = CAST((CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VARCHAR)
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE e.IsUndeliverable = 1 AND pd.PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS' AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

END