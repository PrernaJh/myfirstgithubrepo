/****** Object:  StoredProcedure [dbo].[getOldestPackageForArchive]    Script Date: 12/27/2021 10:21:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER     PROCEDURE [dbo].[getOldestPackageForArchive]
(
    @subClient AS VARCHAR(MAX),
	@startDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT TOP 1 *
	FROM [dbo].[PackageDatasets] p
		WHERE p.PackageStatus = 'PROCESSED'
			AND p.SubClientName = @subClient
			AND p.LocalProcessedDate >= @startDate
	ORDER BY p.LocalProcessedDate

END