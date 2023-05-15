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
