/****** Object:  StoredProcedure [dbo].[getRptDailyRevenueFile]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getRptDailyRevenueFile]
(
    @subClients AS VARCHAR(MAX),
    @manifestDate AS DATE
)
AS
BEGIN

    SET NOCOUNT ON

	SELECT        
		CONVERT(Date, LocalProcessedDate, 101) AS MANIFEST_DATE, 
		SubClientName AS CUST_NAME, ShippingCarrier + '-' + ShippingMethod AS PRODUCT, ServiceLevel AS TRACKING_TYPE, 
		COUNT(PackageId) AS PIECES, 
		SUM(CAST(ExtraCost AS decimal(18, 2))) AS ASSESSORIAL_COST, 
		SUM(Charge) AS COST, 
		SUM(CAST(ExtraCost AS decimal(18, 2))) + SUM(Charge) AS TOTAL_COST

	FROM dbo.PackageDatasets

	WHERE PackageStatus = 'PROCESSED' AND SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)

	GROUP BY 
		CONVERT(Date, LocalProcessedDate, 101), 
		SubClientName, 
		ShippingCarrier, 
		ShippingMethod, 
		ServiceLevel
END


GO
