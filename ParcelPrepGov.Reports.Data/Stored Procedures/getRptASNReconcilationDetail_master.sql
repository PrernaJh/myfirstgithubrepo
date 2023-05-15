/****** Object:  StoredProcedure [dbo].[getRptASNReconcilationDetail_master]    Script Date: 12/23/2021 7:34:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[getRptASNReconcilationDetail_master]
(    
	@subClientName VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS

BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON
 
    -- Get all packages that have NOT been scanned but have been imported
	Select PACKAGE_ID, IMPORT_DATE, PACKAGE_STATUS,SUB_CLIENT_NAME
	From (SELECT pd.PackageId AS PACKAGE_ID, pd.CosmosCreateDate AS IMPORT_DATE, pd.PackageStatus AS PACKAGE_STATUS
	,pd.SubClientName AS SUB_CLIENT_NAME, ROW_NUMBER() 
		OVER (PARTITION BY pd.PackageId ORDER BY pd.CosmosCreateDate DESC) AS ROW_NUM
		FROM [dbo].[PackageDatasets] pd
		WHERE pd.SubClientName = @subClientName
		AND pd.DatasetCreateDate BETWEEN @beginDate AND @endDate) t
		WHERE ROW_NUM = 1 AND t.PACKAGE_STATUS IN('IMPORTED', 'RECALLED', 'CREATED')

END