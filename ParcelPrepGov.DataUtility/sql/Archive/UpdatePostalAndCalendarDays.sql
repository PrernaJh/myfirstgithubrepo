DECLARE @beginDate DATETIME2;
SET @beginDate = '12/1/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '12/09/2021';
SELECT Id as PackageDatasetId, StopTheClockEventDate, LocalProcessedDate, ShippingMethod
	INTO #items
	FROM dbo.PackageDatasets 
	WHERE LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	AND StopTheClockEventDate >= @beginDate
	AND PostalDays IS NULL
GO 
UPDATE dbo.PackageDatasets
SET PostalDays = [dbo].[PostalDateDiff](i.LocalProcessedDate, i.StopTheClockEventDate, i.ShippingMethod),
	CalendarDays = [dbo].[CalendarDateDiff](i.LocalProcessedDate, i.StopTheClockEventDate)
	
FROM #items i
WHERE Id = i.PackageDatasetId;
GO
select * from #items i
where i.ROW_NUM = 1 
GO
DROP TABLE #items
