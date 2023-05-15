/****** Object:  StoredProcedure [dbo].[deleteArchivedPackages]    Script Date: 1/4/2022 9:36:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[deleteArchivedPackages]
(
    @subClient AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS
BEGIN
	DECLARE @count INT
	SET @count = 
		(SELECT COUNT(*)
			FROM [dbo].[PackageDatasets] 
				WHERE PackageStatus = 'PROCESSED'
					AND SubClientName = @subClient
					AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
		)
	SELECT @count					
	SET @count = @count / 10000
	WHILE @count >= 0
	BEGIN
		BEGIN TRANSACTION
		SET @count = @count -1
		DELETE TOP (10000)			
			FROM [dbo].[PackageDatasets] 
				WHERE PackageStatus = 'PROCESSED'
					AND SubClientName = @subClient
					AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
		COMMIT TRANSACTION 
		CHECKPOINT 
	END
END