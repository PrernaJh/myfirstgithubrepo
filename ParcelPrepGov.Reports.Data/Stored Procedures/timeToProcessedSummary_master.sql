/****** Object:  StoredProcedure [dbo].[timeToProcessedSummary_master]    Script Date: 11/17/2021 2:22:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[timeToProcessedSummary_master]
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
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		AVG(CAST(DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate) AS DECIMAL)) AS AVG_DEL_DAYS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=0 THEN 1 Else 0 END) AS DAY0_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=0 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY0_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=1 THEN 1 Else 0 END) AS DAY1_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=1 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY1_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=2 THEN 1 Else 0 END) AS DAY2_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=2 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY2_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=3 THEN 1 Else 0 END) AS DAY3_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=3 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY3_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=4 THEN 1 Else 0 END) AS DAY4_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=4 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY4_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=5 THEN 1 Else 0 END) AS DAY5_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)=5 THEN 1 Else 0 END)/CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY5_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)>=6 THEN 1 Else 0 END) AS DAY6_PCS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.CosmosCreateDate, pd.ProcessedDate)>=6 THEN 1 Else 0 END) /CONVERT(DECIMAL, COUNT(pd.PackageId)) AS DAY6_PCT

	FROM dbo.PackageDatasets pd
	
	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
		AND pd.SiteName = @site AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
	
	GROUP BY 
		pd.SubClientName

END

GO
