/****** Object:  StoredProcedure [dbo].[getRptExpenseWeekly]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptExpenseWeekly]
(
    -- Add the parameters for the stored procedure here
    @subClient as varchar(50),
	@localProcessedDate as date
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
Select * from (SELECT 
	pd.SubClientName AS SUBCLIENT, 
		CAST(pd.LocalProcessedDate AS date) AS PROCESSING_DATE, 
		LEFT(pd.PackageId, 5) AS BILLING_REFERENCE1, 
		pd.ShippingMethod AS PRODUCT, 
		'' as TRACKING_TYPE,
		Count(pd.PackageId) AS PIECES,
		SUM(pd.weight) AS WEIGHT, 
		SUM(pd.Cost) AS COST, 
		--sum(cast(pd.ExtraCost as decimal)) AS EXTRA_SERVICE_COST, 
		0 AS EXTRA_SERVICE_COST,
		--SUM(pd.Cost)+sum(cast(pd.ExtraCost as decimal)) AS TOTAL_COST, 
		SUM(pd.Cost) AS TOTAL_COST, 
		AVG(pd.Weight) AS AVERAGE_WEIGHT,
		AVG(pd.Zone) as AVERAGE_ZONE
	FROM dbo.PackageDatasets pd 
	INNER JOIN dbo.JobDatasets jd ON pd.JobId = jd.CosmosId

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.SubClientName = @subClient
		AND LocalProcessedDate BETWEEN DATEADD(day, -6, @localProcessedDate) AND DATEADD(day, 1, @localProcessedDate)
	group by  pd.SubClientName, CAST(pd.LocalProcessedDate AS date),LEFT(pd.PackageId, 5),pd.ShippingMethod 


	Union Select
	    sd.sitename AS SUBCLIENT, 
		CAST(sd.LocalProcessedDate AS date) AS PROCESSING_DATE, 
		'' AS BILLING_REFERENCE1, 
		sd.ShippingMethod as PRODUCT, 
		'' as TRACKING_TYPE,
		Count(sd.ContainerId) AS PIECES,
		SUM(cast(sd.weight as decimal )) AS WEIGHT, 
		SUM(sd.cost) AS COST, 
		'' AS EXTRA_SERVICE_COST, 
		SUM(sd.Cost) AS COMPARISON_COST, 
		AVG(cast(sd.weight as decimal )) AS AVERAGE_WEIGHT,
		AVG(sd.Zone) as AVERAGE_ZONE
		from dbo.ShippingContainerDatasets sd


	WHERE sd.Status = 'CLOSED' AND sd.SiteName=
	(select sd.siteName from dbo.SubClientDatasets sd where sd.Name = @subClient)

		AND LocalProcessedDate BETWEEN DATEADD(day, -6, @localProcessedDate) AND DATEADD(day, 1, @localProcessedDate)
		AND sd.ShippingCarrier ='USPS'
	group by sd.SiteName , CAST(sd.LocalProcessedDate AS date),sd.ShippingMethod
	) s order by PROCESSING_DATE, BILLING_REFERENCE1,PRODUCT
END
GO
