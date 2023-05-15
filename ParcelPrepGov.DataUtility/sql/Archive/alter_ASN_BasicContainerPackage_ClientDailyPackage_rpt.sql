/****** Object:  StoredProcedure [dbo].[getRptASNReconcilationDetail_master]    Script Date: 11/17/2021 by:Chong Vang 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER PROCEDURE [dbo].[getRptASNReconcilationDetail_master]
(
    -- Add the parameters for the stored procedure here
	@subClientName VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS

BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
    --Get all packages that have NOT been scanned but have been imported
	SELECT pd.PackageId AS PACKAGE_ID, pd.DatasetCreateDate AS IMPORT_DATE, pd.PackageStatus AS PACKAGE_STATUS, pd.SubClientName AS SUB_CLIENT_NAME
	FROM [dbo].[PackageDatasets] pd
	WHERE pd.SubClientName = @subClientName
		AND pd.PackageStatus IN('IMPORTED','RECALLED')
		AND pd.DatasetCreateDate BETWEEN @beginDate AND @endDate
		GROUP BY pd.SubClientName, pd.PackageId, pd.DatasetCreateDate, pd.PackageStatus

END
GO

/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 12/10/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 12/10/2021 8:15:16 AM ******/

CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(
    -- Add the parameters for the stored procedure here
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

        Select 
            BinCode as BINCODE, 
            Min(LabelListDescription) as DESTINATION,
            ContainerId as CONTAINER_ID, 
            Min(containerbarcode) as CONT_BARCODE,
            Min(cont_carrier) as CONT_CARRIER,
            Min(cont_method) as CONT_METHOD, 
            PackageId as PKG_ID, 
            Min(ShippingBarcode) as PKG_TRACKING,
            Min(ShippingCarrier) as PKG_CARRIER,
            Min(ShippingMethod) as PKG_SHIPPINGMETHOD  
        from (SELECT
             p.PackageId,
             p.LocalProcessedDate,
             p.ContainerId,
             p.BinCode,
             p.ShippingBarcode,
             p.ShippingCarrier,
             p.ShippingMethod,
             c.UpdatedBarcode as containerbarcode,
             c.ShippingCarrier as cont_carrier,
             c.ShippingMethod as cont_method,
             b.LabelListDescription
            
         FROM [dbo].[PackageDatasets] p
         left join [dbo].[ShippingContainerDatasets] c on p.ContainerId=c.ContainerId
         left join [dbo].[BinDatasets] b on c.BinActiveGroupId=b.ActiveGroupId AND c.BinCode=b.BinCode
         where p.SiteName=@siteName and p.LocalProcessedDate between @manifestDate and DATEADD(day, 1, @manifestDate) and PackageStatus='PROCESSED') s
         group by BinCode,ContainerId,PackageId
         order by BinCode,ContainerId,PackageId

END

GO

/****** Object:  StoredProcedure [dbo].[getRptClientDailyPackageSummary]    Script Date: 11/19/2021 10:17:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptClientDailyPackageSummary]
(
    @subClientNames AS VARCHAR(MAX),
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
	WHERE PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClientNames, ','))
		AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)

	GROUP BY 
		CONVERT(Date, LocalProcessedDate, 101),
		ShippingCarrier, 
		ShippingMethod,
		pd.SubClientName

END