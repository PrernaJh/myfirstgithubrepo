GO
/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 12/10/2021 8:15:16 AM ******/
ALTER   PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(
    -- Add the parameters for the stored procedure here
    @siteName VARCHAR(MAX),
    @manifestDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON
        SELECT 
            BinCode AS BINCODE, 
            MIN(LabelListDescription) AS DESTINATION,
            ContainerId AS CONTAINER_ID, 
            MIN(containerbarcode) AS CONT_BARCODE,
            MIN(cont_carrier) AS CONT_CARRIER,
            MIN(cont_method) AS CONT_METHOD, 
            PackageId AS PACKAGE_ID, 
            MIN(ShippingBarcode) AS TRACKING_NUMBER,
            MIN(ShippingCarrier) AS PKG_CARRIER,
            MIN(ShippingMethod) AS PKG_SHIPPINGMETHOD,
            MIN(opened_by) AS OPENED_BY
        FROM (SELECT
             p.PackageId,
             p.LocalProcessedDate,
             p.ContainerId,
             p.BinCode,
             p.ShippingBarcode,
             p.ShippingCarrier,
             p.ShippingMethod,
             c.UpdatedBarcode AS containerbarcode,
             c.ShippingCarrier AS cont_carrier,
             c.ShippingMethod AS cont_method,
             b.LabelListDescription,
             c.Username AS opened_by            
         FROM [dbo].[PackageDatasets] p
         LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
         LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId=b.ActiveGroupId AND c.BinCode=b.BinCode
         WHERE p.SiteName=@siteName AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) AND PackageStatus='PROCESSED') s
         GROUP BY BinCode, ContainerId, PackageId
         ORDER BY BinCode, ContainerId, PackageId
END