DECLARE @beginDate DATETIME2;
SET @beginDate = '1/1/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '12/31/2022';
DECLARE @sites varchar(MAX);
SET @sites = 'TUCSON,CHARLESTON,CHELMSFORD,CHICAGO,DALLAS,LEAVENWORTH,MURFREESBORO'
SELECT pd.Id AS PackageDatasetId, (CASE WHEN ISNUMERIC(LEFT(pd.PackageId,5)) = 1 THEN CAST(LEFT(pd.PackageId,5) AS INT) ELSE 0 END) AS VisnSiteParent
    INTO #items
    FROM dbo.PackageDatasets pd 
        WHERE  pd.SiteName IN (SELECT * FROM [dbo].[SplitString](@sites, ',')) 
            AND pd.VisnSiteParent IS NULL
			AND pd.DatasetCreateDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
	ORDER BY pd.PackageId

GO 
UPDATE dbo.PackageDatasets
SET VisnSiteParent = i.VisnSiteParent
FROM #items i
WHERE Id = i.PackageDatasetId;
GO
select * from #items i
GO
DROP TABLE #items