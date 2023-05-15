-- Find first processing event if there is one and update packageDatasets
DECLARE @beginDate DATETIME2;
SET @beginDate = '01/01/2022';
DECLARE @endDate DATETIME2;
SET @endDate = '09/30/2022';
DECLARE @sites varchar(MAX);
SET @sites = 'TUCSON,CHARLESTON,CHELMSFORD,CHICAGO,DALLAS,LEAVENWORTH,MURFREESBORO'
SELECT PackageDatasetId,  
            EventType, MachineId, Username, ROW_NUMBER() 
        OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
            into #items
            FROM dbo.PackageEventDatasets e
                        LEFT JOIN dbo.PackageDatasets AS p ON e.PackageDatasetId = p.Id
                    WHERE  p.SiteName IN (SELECT * FROM STRING_SPLIT(@sites, ',')) 
                        AND e.LocalEventDate >= p.LocalProcessedDate 
                        AND p.PackageStatus = 'PROCESSED' AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
						AND p.ProcessedEventType IS NULL
                        AND (e.EventType = 'AUTOSCAN' OR e.EventType = 'MANUALSCAN' OR e.EventType = 'REPEATSCAN')
			order by ROW_NUM
GO

UPDATE dbo.PackageDatasets
SET ProcessedEventType = i.EventType,
    ProcessedMachineId = i.MachineId,
	ProcessedUsername = i.Username
FROM #items i
WHERE i.ROW_NUM = 1 AND Id = i.PackageDatasetId;
GO

select * from #items i
where i.ROW_NUM = 1 
GO

DROP TABLE #items
GO