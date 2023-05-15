CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
	@CONT_CARRIER VARCHAR(250) = NULL,
	@CONT_METHOD VARCHAR(250) = NULL,
	@PKG_CARRIER VARCHAR(250) = NULL,
	@PKG_SHIPPINGMETHOD VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON

        SELECT 
			Sitename AS [SITE],
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
			opened_by AS OPENED_BY,
			MAX(FORMAT(LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS PKG_PROCESSED_DATE,
			MAX(FORMAT(LocalCreateDate, 'MM/dd/yyyy hh:mm:ss tt')) AS CONT_CREATE_DATE
        FROM (SELECT
		p.SiteName,
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
			 c.Username AS opened_by,
			 c.LocalCreateDate
         FROM [dbo].[PackageDatasets] p
			 LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
			 LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId = b.ActiveGroupId AND c.BinCode = b.BinCode
         WHERE 		 
		 p.SiteName = @siteName 
			AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) 
			AND PackageStatus = 'PROCESSED'
			AND (@CONT_CARRIER IS NULL OR c.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@CONT_CARRIER, '|', 1)))
			AND (@CONT_METHOD IS NULL OR c.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@CONT_METHOD, '|', 1)))
			AND (@PKG_CARRIER IS NULL OR  p.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@PKG_CARRIER, '|', 1)))
			AND (@PKG_SHIPPINGMETHOD IS NULL OR p.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@PKG_SHIPPINGMETHOD, '|', 1)))
			) s
         GROUP BY BinCode, ContainerId, PackageId, opened_by, SiteName
         ORDER BY BinCode, ContainerId, PackageId
END