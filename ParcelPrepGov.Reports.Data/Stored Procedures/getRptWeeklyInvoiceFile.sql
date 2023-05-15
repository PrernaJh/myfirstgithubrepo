/****** Object:  StoredProcedure [dbo].[getRptWeeklyInvoiceFile]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[getRptWeeklyInvoiceFile]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
	SELECT 
		pd.SubClientName AS SUBCLIENT, 
		CAST(pd.LocalProcessedDate AS date) AS BILLING_DATE, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		LEFT(pd.PackageId, 5) AS BILLING_REFERENCE, 
		pd.ShippingMethod AS BILLING_PRODUCT, 
		jd.Product AS MARKUP_DESC, 
		jd.MarkUpType AS MARKUP_TYPE_DESC, 
		pd.BillingWeight AS BILLING_WEIGHT, 
		pd.Zone, 
		'' AS SIG_COST, 
		'' AS PIECE_COST, 
		pd.Charge AS BILLING_COST, 
		pd.Weight AS Weight, 
		'' AS TOTAL_CUST

	FROM dbo.PackageDatasets pd 
	INNER JOIN dbo.JobDatasets jd ON pd.JobId = jd.CosmosId

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
END









GO
