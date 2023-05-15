/****** Object:  StoredProcedure [dbo].[getRptClientDailyPackageSummary]    Script Date: 11/19/2021 10:17:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptClientDailyPackageSummary]
(
    @subClientNames AS VARCHAR(50),
    @manifestDate AS DATE,
	@product AS VARCHAR(MAX) = null
)
AS
BEGIN

    SET NOCOUNT ON

	SELECT        
		CONVERT(Date, LocalProcessedDate, 101) AS MANIFEST_DATE, 
		ShippingCarrier + '-' + ShippingMethod AS PRODUCT, 
		COUNT(PackageId) AS PIECES, 
		SUM(CAST(Weight AS decimal(18, 2))) AS WEIGHT,
		pd.SubClientName AS CUST_LOCATION

	FROM dbo.PackageDatasets pd
	INNER JOIN dbo.SubClientDatasets s ON  pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClientNames, ','))

	WHERE PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClientNames, ','))
		AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|')))

	GROUP BY 
		CONVERT(Date, LocalProcessedDate, 101),
		ShippingCarrier, 
		ShippingMethod,
		pd.SubClientName

END