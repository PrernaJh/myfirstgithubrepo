/****** Object:  StoredProcedure [dbo].[deleteOlderPackages]    Script Date: 1/4/2022 9:36:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[deleteOlderPackages]
(
    @subClient AS VARCHAR(MAX),
	@processed AS BIT,
	@createDate AS DATE
)
AS
BEGIN
	DECLARE @count INT
	SET @count = 
		(SELECT COUNT(*)
			FROM [dbo].[PackageDatasets] 
				WHERE SubClientName = @subClient
					AND ((@processed = 1 AND PackageStatus = 'PROCESSED')
						OR (@processed = 0 AND PackageStatus != 'PROCESSED'))
					AND CosmosCreateDate < @createDate
		)
	SELECT @count					
	SET @count = @count / 10000
	WHILE @count >= 0
	BEGIN
		BEGIN TRANSACTION
		SET @count = @count -1
		DELETE TOP (10000)			
			FROM [dbo].[PackageDatasets] 
				WHERE SubClientName = @subClient
					AND ((@processed = 1 AND PackageStatus = 'PROCESSED')
						OR (@processed = 0 AND PackageStatus != 'PROCESSED'))
					AND CosmosCreateDate < @createDate
		COMMIT TRANSACTION 
		CHECKPOINT 
	END
END
