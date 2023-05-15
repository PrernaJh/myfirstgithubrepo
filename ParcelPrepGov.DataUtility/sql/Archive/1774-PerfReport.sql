/****** Object:  StoredProcedure [dbo].[getRptUSPSMonthlyDeliveryPerformanceSummary]    Script Date: 5/26/2022 9:11:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptUSPSMonthlyDeliveryPerformanceSummary]
	@subclientNames varchar(MAX),
    @startDate date,
    @enddate date
AS
BEGIN


SELECT  
	a.SubClientName, 
	a.ManifestMonth, 
	@startDate AS StartDate,
	@enddate AS EndDate,
	a.Product,
	SUM(a.NumberOfPackages) AS TOTAL_PIECES,
	CONVERT(DECIMAL(18,2), SUM(a.NumberOfPackages * a.PostalDays) / CONVERT(DECIMAL(18,2), SUM(a.NumberOfPackages), 0),   0) AS AVG_POSTAL_DAYS,
	CONVERT(DECIMAL(18,2), SUM(a.NumberOfPackages * a.CalendarDays) / CONVERT(DECIMAL(18,2), SUM(a.NumberOfPackages), 0), 0) AS AVG_CAL_DAYS,
	SUM(CASE WHEN a.IsStopTheClock = 0  THEN a.NumberOfPackages ELSE 0 END) AS TOTAL_PCS_NO_STC,
	SUM(a.NumberOfPackages) - SUM(a.HasPhysicalScan) AS TOTAL_PCS_NO_SCAN,
	SUM(CASE WHEN a.CalendarDays <= 3   THEN a.NumberOfPackages ELSE 0 END) AS [LessThanOrEqualToDay3],
	SUM(CASE WHEN a.CalendarDays = 4    THEN a.NumberOfPackages ELSE 0 END) AS [Day4],
	SUM(CASE WHEN a.CalendarDays = 5    THEN a.NumberOfPackages ELSE 0 END) AS [Day5],
	SUM(CASE WHEN a.CalendarDays = 6    THEN a.NumberOfPackages ELSE 0 END) AS [Day6],
	SUM(CASE WHEN a.CalendarDays = 7    THEN a.NumberOfPackages ELSE 0 END) AS [Day7],
	SUM(CASE WHEN a.CalendarDays = 8    THEN a.NumberOfPackages ELSE 0 END) AS [Day8],
	SUM(CASE WHEN a.CalendarDays = 9    THEN a.NumberOfPackages ELSE 0 END) AS [Day9],
	SUM(CASE WHEN a.CalendarDays = 10   THEN a.NumberOfPackages ELSE 0 END) AS [Day10],
	SUM(CASE WHEN a.CalendarDays = 11   THEN a.NumberOfPackages ELSE 0 END) AS [Day11],
	SUM(CASE WHEN a.CalendarDays = 12   THEN a.NumberOfPackages ELSE 0 END) AS [Day12],
	SUM(CASE WHEN a.CalendarDays = 13   THEN a.NumberOfPackages ELSE 0 END) AS [Day13],
	SUM(CASE WHEN a.CalendarDays = 14   THEN a.NumberOfPackages ELSE 0 END) AS [Day14],
	SUM(CASE WHEN a.CalendarDays >= 15  THEN a.NumberOfPackages ELSE 0 END) AS GreaterOrEqualTo15
FROM (
	SELECT MAX(s.Description) AS SubclientName,
	MAX(FORMAT(pd.LocalProcessedDate, 'MMM-yy')) AS ManifestMonth, 
	pd.ShippingMethod AS [Product],
	ISNULL(pd.PostalDays, 0) AS PostalDays,
	ISNULL(pd.CalendarDays, 0) AS CalendarDays,
	COUNT(distinct pd.PackageId) AS NumberOfPackages,
	SUM( CASE WHEN pd.StopTheClockEventDate IS NOT NULL THEN 1 ELSE 0 END) AS IsStopTheClock
	--,SUM( CASE WHEN TPD.Id IS NOT NULL THEN 1 ELSE 0 END) AS HasPhysicalScan
	, SUM( CASE WHEN pd.LastKnownEventDate IS NOT NULL THEN 1 ELSE 0 END ) AS HasPhysicalScan
	from PackageDatasets pd WITH (NOLOCK) 
		INNER JOIN SubClientDatasets s ON s.Name = pd.SubClientName	
		--OUTER APPLY(
		--	SELECT TOP 1 * 
		--	FROM TrackPackageDatasets r
		--	WHERE r.PackageDatasetId = pd.id			
		--) TPD
		WHERE LocalProcessedDate BETWEEN @startDate AND DATEADD(day, 1, @enddate)
			AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subclientNames, ',', 1))
			AND pd.ShippingCarrier = 'USPS' 			
		GROUP BY pd.SubClientName, MONTH(pd.LocalProcessedDate), pd.ShippingMethod, ISNULL(pd.PostalDays, 0), ISNULL(pd.CalendarDays, 0) 
	) as a
	GROUP BY a.SubClientName, a.ManifestMonth, a.Product
	ORDER BY a.Product, a.SubclientName

END