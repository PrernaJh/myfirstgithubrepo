DECLARE @beginDate DATETIME2;
SET @beginDate = '1/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '12/01/2021';
DECLARE @sites varchar(MAX);
SET @sites = 'TUCSON,CHARLESTON,CHELMSFORD,CHICAGO,DALLAS,LEAVENWORTH,MURFREESBORO'
SELECT ShippingContainerDatasetId, EventDate, IIF(ec.Id IS NULL, t.EventDescription, ec.Description) AS EventDescription, EventLocation, EventCode, EventZip, c.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
        OVER (PARTITION BY ShippingContainerDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
            into #items
            FROM dbo.TrackPackageDatasets t
                        LEFT JOIN dbo.EvsCodes AS ec ON t.EventCode = ec.Code
                        LEFT JOIN dbo.ShippingContainerDatasets AS c ON t.ShippingContainerDatasetId = c.Id
                    WHERE  c.SiteName IN (SELECT * FROM [dbo].[SplitString](@sites, ',')) 
                    AND c.LastKnownEventDate IS NULL
                    AND t.EventDate >= @beginDate 
                    AND c.Status = 'CLOSED' AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
order by ROW_NUM
GO 
UPDATE dbo.ShippingContainerDatasets
SET LastKnownEventDate = i.EventDate, 
LastKnownEventDescription = i.EventDescription, 
LastKnownEventLocation = i.EventLocation,
LastKnownEventZip = i.EventZip
FROM #items i
WHERE i.ROW_NUM = 1 AND Id = i.ShippingContainerDatasetId;
GO
select * from #items i
where i.ROW_NUM = 1 
GO
DROP TABLE #items