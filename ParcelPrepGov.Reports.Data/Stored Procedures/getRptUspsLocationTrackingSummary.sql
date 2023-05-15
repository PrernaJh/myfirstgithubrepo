/****** Object:  StoredProcedure [dbo].[getRptUspsLocationTrackingSummary]    Script Date: 12/20/2021 12:39:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
		COUNT(pd.StopTheClockEventDate) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		
		[dbo].[Fraction](SUM(pd.PostalDays), COUNT(pd.StopTheClockEventDate)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM(pd.CalendarDays), COUNT(pd.StopTheClockEventDate)) AS AVG_CAL_DAYS,
		
		
		--[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod)), COUNT(pd.StopTheClockEventDate)) AS AVG_POSTAL_DAYS,
		--[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, pd.StopTheClockEventDate)), COUNT(pd.StopTheClockEventDate)) AS AVG_CAL_DAYS,
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
		
		
		--[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL 
		--		THEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) ELSE 0 END),
		--	SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_POSTAL_DAYS,
		--[dbo].[Fraction](SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL 
		--		THEN [dbo].[CalendarDateDiff](LocalProcessedDate, pd.StopTheClockEventDate) ELSE 0 END),
		--	SUM(CASE WHEN pd.ServiceLevel = 'SIGNATURE' AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END)) AS SIGNATURE_AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS NO_STC_PCT

	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	--LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
	--	OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
	--		FROM dbo.TrackPackageDatasets 
	--					LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
	--					LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
	--				WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
	--					AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 				
	--			ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY s.Description

END

