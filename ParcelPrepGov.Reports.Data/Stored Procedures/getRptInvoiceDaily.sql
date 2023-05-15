/****** Object:  StoredProcedure [dbo].[getRptInvoiceDaily]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptInvoiceDaily]
(
    -- Add the parameters for the stored procedure here
    @subClient as varchar(50),
	@localProcessedDate as varchar(20)
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
Select * from (SELECT
CAST(pd.LocalProcessedDate AS date) AS BILLING_DATE,
pd.PackageId as PACKAGE_ID,
pd.ShippingBarcode as TRACKINGNUMBER,
LEFT(pd.PackageId, 5) as BILLING_REFERENCE1,
pd.ShippingMethod as BILLING_PRODUCT,
pd.BillingWeight as BILLING_WEIGHT,
pd.zone as ZONE,
pd.ExtraCost as SIG_COST,
pd.Charge as BILLING_COST,
pd.Weight as WEIGHT,
pd.ExtraCost+pd.Charge as TOTAL_CUST

FROM dbo.PackageDatasets pd
	WHERE  pd.SubClientName=@subClient and pd.PackageStatus='PROCESSED'
		--AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND LocalProcessedDate BETWEEN @localProcessedDate AND DATEADD(day, 1, @localProcessedDate)
) s order by BILLING_DATE, BILLING_REFERENCE1,BILLING_PRODUCT

END
GO
