DECLARE @beginDate DATETIME2;
SET @beginDate = '01/01/2022';
DECLARE @endDate DATETIME2;
SET @endDate = '12/31/2022';
DECLARE @sites varchar(MAX);
SET @sites = 'TUCSON,CHARLESTON,CHELMSFORD,CHICAGO,DALLAS,LEAVENWORTH,MURFREESBORO'
SELECT p.Id AS PackageDatasetId, 
                p.PackageId,
                (IIF(j.MarkUpType = 'COMPANY', 'FSC',
                    IIF(j.MarkUpType = 'CUSTOMER', 'CUST', j.MarkupType) ) ) AS MarkUpType 
            into #items
            FROM dbo.PackageDatasets p
                        LEFT JOIN dbo.JobDatasets AS j ON j.CosmosId = p.JobId
                    WHERE  p.SiteName IN (SELECT * FROM [dbo].[SplitString](@sites, ',')) 
                    AND p.PackageStatus = 'PROCESSED' AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
order by PackageId
GO 
UPDATE dbo.PackageDatasets
SET MarkUpType = i.MarkUpType
FROM #items i
WHERE Id = i.PackageDatasetId;
GO
select * from #items i
GO

DROP TABLE #items