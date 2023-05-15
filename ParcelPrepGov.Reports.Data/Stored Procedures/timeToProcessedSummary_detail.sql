/****** Object:  StoredProcedure [dbo].[timeToProcessedSummary_detail]    Script Date: 11/17/2021 2:22:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[timeToProcessedSummary_detail]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT        
		pd.SubClientName AS LOCATION,
		DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate) AS DEL_DAYS

	FROM dbo.PackageDatasets pd
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SiteName = @site AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	ORDER BY DEL_DAYS DESC

END

GO
