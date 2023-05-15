-- Find first STC event if there is one and update STC event date and calendar/postal days
DECLARE @beginDate DATETIME2;
SET @beginDate = '01/01/2022';
DECLARE @endDate DATETIME2;
SET @endDate = '08/01/2022';
DECLARE @sites varchar(MAX);
SET @sites = 'TUCSON,CHARLESTON,CHELMSFORD,CHICAGO,DALLAS,LEAVENWORTH,MURFREESBORO'
SELECT PackageDatasetId, LocalProcessedDate, ShippingMethod, 
            EventDate, IIF(ec.Id IS NULL, t.EventDescription, ec.Description) AS EventDescription, 
			EventLocation, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ec.IsUndeliverable, ROW_NUMBER() 
        OVER (PARTITION BY PackageDatasetId ORDER BY EventDate, ec.IsStopTheClock DESC) AS ROW_NUM
            into #items
            FROM dbo.TrackPackageDatasets t
                        LEFT JOIN dbo.EvsCodes AS ec ON t.EventCode = ec.Code
                        LEFT JOIN dbo.PackageDatasets AS p ON t.PackageDatasetId = p.Id
                    WHERE  p.SiteName IN (SELECT * FROM STRING_SPLIT(@sites, ',')) 
                        AND t.EventDate >= p.LocalProcessedDate 
                        AND p.PackageStatus = 'PROCESSED' AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
                        AND (ec.IsStopTheClock = 1)
			order by ROW_NUM
GO

UPDATE dbo.PackageDatasets
SET StopTheClockEventDate = i.EventDate,
    PostalDays = [dbo].[PostalDateDiff](i.LocalProcessedDate, i.EventDate, i.ShippingMethod),
    CalendarDays = [dbo].[CalendarDateDiff](i.LocalProcessedDate, i.EventDate)
FROM #items i
WHERE i.ROW_NUM = 1 AND Id = i.PackageDatasetId;
GO

select * from #items i
where i.ROW_NUM = 1 
GO

DROP TABLE #items
GO