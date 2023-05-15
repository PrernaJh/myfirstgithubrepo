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

GO

/****** Object:  StoredProcedure [dbo].[DeleteEodPackages]    Script Date: 4/8/2022 11:47:29 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[DeleteEodPackages]
(
    @site AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS

BEGIN
	BEGIN TRANSACTION

	DELETE c
		FROM PackageDetailRecords c
			JOIN EodPackages p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM ReturnAsnRecords c
			JOIN EodPackages p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM EvsPackage c
			JOIN EodPackages p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM InvoiceRecords c
			JOIN EodPackages p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE c
		FROM ExpenseRecords c
			JOIN EodPackages p on p.CosmosId = c.CosmosId 
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	DELETE p			
		FROM [dbo].[EodPackages] p
			WHERE p.SiteName = @site
				AND p.LocalProcessedDate = @manifestDate

	COMMIT TRANSACTION 
	CHECKPOINT 
END


GO

/****** Object:  StoredProcedure [dbo].[DeleteOldEodContainers]    Script Date: 4/12/2022 11:32:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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

GO

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

GO



