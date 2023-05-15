/****** Object:  StoredProcedure [dbo].[DeleteOldEodContainers]    Script Date: 4/15/2022 10:23:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].DeleteOldEodContainers
(
    @site AS VARCHAR(MAX),
	@manifestDate AS DATE,
	@chunkSize AS INT = 10000
)
AS

BEGIN
	BEGIN TRANSACTION
		SELECT TOP (@chunkSize) p.CosmosId INTO #items
			FROM [dbo].[EodContainers] p
				WHERE p.SiteName = @site
					AND p.LocalProcessedDate < @manifestDate

		DELETE c
			FROM ContainerDetailRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM PmodContainerDetailRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM ExpenseRecords c
				JOIN #items i on i.CosmosId = c.CosmosId 
		
		DELETE c
			FROM EvsContainer c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE c
			FROM EvsPackage c
				JOIN #items i on i.CosmosId = c.CosmosId 

		DELETE p			
			FROM [dbo].[EodContainers] p
				JOIN #items i on i.CosmosId = p.CosmosId 

		DROP TABLE #items
	COMMIT TRANSACTION 
	CHECKPOINT
END
