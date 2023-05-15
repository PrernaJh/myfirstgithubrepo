/****** Object:  StoredProcedure [dbo].[getPackageTracking_data]    Script Date: 10/8/2021 11:56:28 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      Jeff Johnson Story 1202
-- Modified Date: 10/8/21
-- Description: Update to fix package search history not showing after the first Stop the Clock event.
-- =============================================
ALTER PROCEDURE [dbo].[getPackageTracking_data]
(
    -- Add the parameters for the stored procedure here
	@ids as VARCHAR(MAX),
	@beginDate AS DATE = '2020-06-01'
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT * 
		FROM [dbo].[TrackPackageDatasets] t
		LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
		LEFT JOIN dbo.PackageDatasets AS p ON p.Id = t.PackageDatasetId
			WHERE cast(EventDate as datetime2(7)) >= cast(p.LocalProcessedDate as datetime2(7)) 
				AND t.PackageDatasetId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY EventDate DESC, ec.IsStopTheClock DESC

END