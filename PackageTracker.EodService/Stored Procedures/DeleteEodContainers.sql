/****** Object:  StoredProcedure [dbo].[DeleteEodContainers]    Script Date: 4/8/2022 11:45:32 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[DeleteEodContainers]
(
    @site AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS

BEGIN
	BEGIN TRANSACTION

	DELETE c
		FROM ContainerDetailRecords c
			JOIN EodContainers p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM PmodContainerDetailRecords c
			JOIN EodContainers p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM ExpenseRecords c
			JOIN EodContainers p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM EvsContainer c
			JOIN EodContainers p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM EvsPackage c
			JOIN EodContainers p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE p			
		FROM [dbo].[EodContainers] p
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	COMMIT TRANSACTION 
	CHECKPOINT 
END


