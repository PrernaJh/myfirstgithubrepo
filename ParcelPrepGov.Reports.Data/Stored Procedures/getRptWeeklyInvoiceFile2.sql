/****** Object:  StoredProcedure [dbo].[getRptWeeklyInvoiceFile2]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[getRptWeeklyInvoiceFile2]
(
    -- Add the parameters for the stored procedure here
    @subClient AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
	SELECT 
	pd.SubClientName AS SUBCLIENT, 
		CAST(pd.LocalProcessedDate AS date) AS PROCESSING_DATE, 
		LEFT(pd.PackageId, 5) AS BILLING_REFERENCE1, 
		pd.ShippingMethod AS PRODUCT, 
		'' as TRACKING_TYPE,
		Count(pd.PackageId) AS PIECES,
		SUM(pd.weight) AS WEIGHT, 
		SUM(pd.charge) AS COST, 
		'' AS EXTRA_SERVICE_COST, 
		SUM(pd.Charge) AS COMPARISON_COST, 
		AVG(pd.Weight) AS AVERAGE_WEIGHT,
		AVG(pd.Zone) as AVERAGE_ZONE


	FROM dbo.PackageDatasets pd 
	INNER JOIN dbo.JobDatasets jd ON pd.JobId = jd.CosmosId
	

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.SubClientName = @subClient 
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

	group by  pd.SubClientName, CAST(pd.LocalProcessedDate AS date),LEFT(pd.PackageId, 5),pd.ShippingMethod 


	Union Select
	    sd.sitename AS SUBCLIENT, 
		CAST(sd.LocalProcessedDate AS date) AS PROCESSING_DATE, 
		'' AS BILLING_REFERENCE1, 
		sd.ShippingMethod as PRODUCT, 
		'' as TRACKING_TYPE,
		Count(sd.ContainerId) AS PIECES,
		SUM(cast(sd.weight as decimal )) AS WEIGHT, 
		SUM(sd.charge) AS COST, 
		'' AS EXTRA_SERVICE_COST, 
		SUM(sd.Charge) AS COMPARISON_COST, 
		AVG(cast(sd.weight as decimal )) AS AVERAGE_WEIGHT,
		AVG(sd.Zone) as AVERAGE_ZONE

		from dbo.ShippingContainerDatasets sd


	WHERE sd.Status = 'CLOSED' AND sd.SiteName='MURFREESBORO'
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		--AND LocalProcessedDate BETWEEN '2/20/2021' AND DATEADD(day, 1, '2/21/2021') 
		AND sd.ShippingCarrier ='USPS'

	group by sd.SiteName , CAST(sd.LocalProcessedDate AS date),sd.ShippingMethod
END




GO
