/****** Object:  StoredProcedure [dbo].[getRptASNReconcilationDetail]   Script Date: 10/22/2021 3:28:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptASNReconcilationDetail_master]
(
    -- Add the parameters for the stored procedure here
	@subClientName VARCHAR(50),
	@importDate DATETIME
)

WITH RECOMPILE
AS

BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
    --Get all packages that have NOT been scanned but have been imported
	SELECT DISTINCT pd.PackageId AS PACKAGE_ID, pd.CosmosCreateDate AS IMPORT_DATE, pd.PackageStatus
	INTO #TempScanned
	FROM [dbo].[PackageDatasets] pd
	JOIN [dbo].[PackageEventDatasets] pv ON pd.Id = pv.PackageDatasetId
	WHERE pd.SubClientName = @subClientName
		AND pd.PackageStatus = 'IMPORTED'
		AND pv.CosmosCreateDate BETWEEN DATEADD(DD,-4, DATEADD(HOUR, -6, @importDate)) AND DATEADD(HOUR, -6, @importDate)
		GROUP BY pd.PackageId, pd.CosmosCreateDate, pd.PackageStatus

	SELECT * FROM #TempScanned

	DROP TABLE #TempScanned

END
GO

GO
DROP INDEX IF EXISTS [dbo].[PackageEventDatasets].[<IX_PED_CCD_PDI>]
 
CREATE NONCLUSTERED INDEX [<IX_PED_CCD_PDI>]
ON [dbo].[PackageEventDatasets] ([CosmosCreateDate])
INCLUDE ([PackageDatasetId])
 
GO