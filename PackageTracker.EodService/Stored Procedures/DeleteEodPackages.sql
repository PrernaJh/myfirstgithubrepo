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


