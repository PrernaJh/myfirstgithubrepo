/****** Object:  StoredProcedure [dbo].[DeleteOldEodPackages]    Script Date: 4/15/2022 10:01:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     PROCEDURE [dbo].[DeleteOldEodPackages]
(
    @site AS VARCHAR(MAX),
	@manifestDate AS DATE,
	@chunkSize AS INT = 10000
)
AS

BEGIN
	BEGIN TRANSACTION
		SELECT TOP (@chunkSize) p.CosmosId INTO #items
			FROM [dbo].[EodPackages] p
				WHERE p.SiteName = @site
					AND p.LocalProcessedDate < @manifestDate

		DELETE c
			FROM PackageDetailRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM ReturnAsnRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM EvsPackage c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM InvoiceRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM ExpenseRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE p			
			FROM [dbo].[EodPackages] p
				JOIN #items i on i.CosmosId = p.CosmosId 

		DROP TABLE #items
	COMMIT TRANSACTION 
	CHECKPOINT
END


