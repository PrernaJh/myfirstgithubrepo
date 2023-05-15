/****** Object:  StoredProcedure [dbo].[deleteOlderContainers]    Script Date: 8/3/2022 9:36:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[deleteOlderContainers]
(
    @site AS VARCHAR(MAX),
	@createDate AS DATE
)
AS
BEGIN
	DECLARE @count INT
	SET @count = 
		(SELECT COUNT(*)
			FROM [dbo].[ShippingContainerDatasets] 
				WHERE SiteName = @site
					AND CosmosCreateDate < @createDate
		)
	SELECT @count					
	SET @count = @count / 10000
	WHILE @count >= 0
	BEGIN
		BEGIN TRANSACTION
		SET @count = @count -1
		DELETE TOP (10000)			
			FROM [dbo].[ShippingContainerDatasets] 
				WHERE SiteName = @site
					AND CosmosCreateDate < @createDate
		COMMIT TRANSACTION 
		CHECKPOINT 
	END
END
