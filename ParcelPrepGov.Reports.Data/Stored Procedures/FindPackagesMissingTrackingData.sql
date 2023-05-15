/****** Object:  StoredProcedure [dbo].[FindPackagesMissingTrackingData]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[FindPackagesMissingTrackingData]
(
	@beginDate AS DATE,
	@EndDate AS DATE
)
AS
BEGIN
	SELECT pd.PackageId, pd.LocalProcessedDate
	FROM PackageDatasets pd
	JOIN [dbo].[TrackPackageDatasets] t
		ON pd.Id = t.PackageDatasetId

	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.LocalProcessedDate BETWEEN @beginDate AND @EndDate

	ORDER BY pd.LocalProcessedDate
END

GO
