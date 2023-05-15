/****** Object:  StoredProcedure [dbo].[getRptDailyPackageSummary]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getRptDailyPackageSummary]
(
    @site AS VARCHAR(50),
    @manifestDate AS DATE
)
AS
BEGIN

    SET NOCOUNT ON

	SELECT        
		CONVERT(Date, LocalProcessedDate, 101) AS MANIFEST_DATE, 
		s.Description AS CUST_NAME, 
		ShippingCarrier + '-' + ShippingMethod AS PRODUCT, 
		COUNT(PackageId) AS PIECES, 
		SUM(CAST(Weight AS decimal(18, 2))) AS WEIGHT

	FROM dbo.PackageDatasets pd
	INNER JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE PackageStatus = 'PROCESSED' AND pd.SiteName = @site 
		AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)

	GROUP BY 
		CONVERT(Date, LocalProcessedDate, 101),
		S.Description,
		ShippingCarrier, 
		ShippingMethod

END
GO
