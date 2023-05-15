/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 12/10/2021 2:22:39 PM  By Chong Vang******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 12/10/2021 8:15:16 AM ******/

CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(
    -- Add the parameters for the stored procedure here
	@siteName VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
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
         where p.SiteName=@siteName and p.LocalProcessedDate between @beginDate and @endDate and PackageStatus='PROCESSED') s
         group by BinCode,ContainerId,PackageId
         order by BinCode,ContainerId,PackageId

END

GO