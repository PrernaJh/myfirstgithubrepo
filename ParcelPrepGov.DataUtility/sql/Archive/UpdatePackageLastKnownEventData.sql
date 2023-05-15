DECLARE @beginDate DATETIME2;
SET @beginDate = '1/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '12/01/2021';
DECLARE @subClients varchar(MAX);
--SET @subClients = 'OPUSTUCSON,OPUSCHARLESTON,OPUSCHELMSFORD,OPUSCHICAGO,OPUSDALLAS,OPUSLEAVENWORTH,OPUSMURFREESBORO'
--SET @subClients = 'CMOPTUCSON,CMOPCHARLESTON,CMOPCHELMSFORD,CMOPCHICAGO,CMOPDALLAS,CMOPLEAVENWORTH,CMOPMURFREESBORO'
SET @subClients = 'DALCCHICAGO,DALCLEAVENWORTH'
SELECT PackageDatasetId, EventDate, IIF(ec.Id IS NULL, t.EventDescription, ec.Description) AS EventDescription, EventLocation, EventCode, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
        OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
            into #items
            FROM dbo.TrackPackageDatasets t
                        LEFT JOIN dbo.EvsCodes AS ec ON t.EventCode = ec.Code
                        LEFT JOIN dbo.PackageDatasets AS p ON t.PackageDatasetId = p.Id
                    WHERE  p.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ',')) 
                    AND p.LastKnownEventDate IS NULL
                    AND t.EventDate >= p.LocalProcessedDate 
                    AND p.PackageStatus = 'PROCESSED' AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
                    AND (p.StopTheClockEventDate IS NULL OR (t.EventDate = p.StopTheClockEventDate AND ec.IsStopTheClock = 1))
order by ROW_NUM
GO 
UPDATE dbo.PackageDatasets
SET LastKnownEventDate = i.EventDate, 
LastKnownEventDescription = i.EventDescription, 
LastKnownEventLocation = i.EventLocation,
LastKnownEventZip = i.EventZip
FROM #items i
WHERE i.ROW_NUM = 1 AND Id = i.PackageDatasetId;
GO
select * from #items i
where i.ROW_NUM = 1 
GO
DROP TABLE #items