/****** Object:  StoredProcedure [dbo].[getRptUspsProductDeliverySummary]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptUspsProductDeliverySummary]    Script Date: 5/10/2021 11:16:11 AM ******/

CREATE OR ALTER PROCEDURE [dbo].[getRptUspsProductDeliverySummary]
(
    -- Add the parameters for the stored procedure here
    @subClient AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT      
		pd.ShippingMethod AS PRODUCT, 
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) <= 3 AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) <=3  AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
		SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
		[dbo].[Percent](SUM(CASE WHEN [dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod) >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,
		COUNT(pd.StopTheClockEventDate) AS DELIVERED_PCS,
		[dbo].[Percent](COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS DELIVERED_PCT,
		[dbo].[Fraction](SUM([dbo].[PostalDateDiff](LocalProcessedDate, pd.StopTheClockEventDate, pd.ShippingMethod)), COUNT(pd.StopTheClockEventDate)) AS AVG_POSTAL_DAYS,
		[dbo].[Fraction](SUM([dbo].[CalendarDateDiff](LocalProcessedDate, pd.StopTheClockEventDate)), COUNT(pd.StopTheClockEventDate)) AS AVG_CAL_DAYS,
		COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS NO_STC_PCS,
		[dbo].[Percent](COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate), COUNT(pd.PackageId)) AS NO_STC_PCT,
		COUNT(pd.PackageId) AS TOTAL_PCS 

	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	--LEFT JOIN (SELECT p.PackageId, PackageDatasetId, EventDate, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
	--	OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
	--		FROM dbo.TrackPackageDatasets 
	--					LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
	--					LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
	--				WHERE ec.IsStopTheClock = 1 AND EventDate >= @beginDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
	--					AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS' AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
	--			ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SubClientName = @subClient
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY pd.ShippingMethod

END

GO
