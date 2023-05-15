/****** Object:  StoredProcedure [dbo].[getRptBasicContainerPackageNesting]    Script Date: 7/27/2022 10:47:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[getRptBasicContainerPackageNesting]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
	@CONT_CARRIER VARCHAR(250) = NULL,
	@CONT_METHOD VARCHAR(250) = NULL,
	@PKG_CARRIER VARCHAR(250) = NULL,
	@PKG_SHIPPINGMETHOD VARCHAR(250) = NULL,
	@CONT_TYPE VARCHAR(250) = NULL
)
AS
BEGIN
    SET NOCOUNT ON

        SELECT 
			Sitename AS [SITE],
            BinCode AS BINCODE, 
            MIN(LabelListDescription) AS DESTINATION,
            ContainerId AS CONTAINER_ID, 
            MIN(ContainerBarcode) AS CONT_BARCODE,
            MIN(cont_carrier) AS CONT_CARRIER,
            MIN(cont_method) AS CONT_METHOD, 
			MIN(ContainerType) as CONT_TYPE,
            PackageId AS PACKAGE_ID, 
            MIN(ShippingBarcode) AS TRACKING_NUMBER,
            MIN(ShippingCarrier) AS PKG_CARRIER,
            MIN(ShippingMethod) AS PKG_SHIPPINGMETHOD,
			MAX(FORMAT(LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS PKG_PROCESSED_DATE,
			MAX(FORMAT(LocalCreatedDate, 'MM/dd/yyyy hh:mm:ss tt')) AS CONT_OPENED_DATE,
			opened_by AS OPENED_BY,
			MAX(FORMAT(LocalClosedDate, 'MM/dd/yyyy hh:mm:ss tt')) as CONT_CLOSED_DATE,
			MAX(CLOSED_BY) AS CLOSED_BY
			
        FROM (SELECT
        p.PackageId,
			p.SiteName,
             p.BinCode,
             b.LabelListDescription,
             p.ContainerId,
             c.UpdatedBarcode AS ContainerBarcode,
             c.ShippingCarrier AS cont_carrier,
             c.ShippingMethod AS cont_method,
			 c.ContainerType,
             p.ShippingBarcode,
             p.ShippingCarrier,
             p.ShippingMethod,
             p.LocalProcessedDate,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e where e.ContainerId = c.ContainerId and e.EventType = 'CREATED') AS LocalCreatedDate,
			 (SELECT TOP 1 e.Username  FROM [dbo].ShippingContainerEventDatasets e where e.ContainerId = c.ContainerId and e.EventType = 'CREATED') AS OPENED_BY,
			 (SELECT TOP 1 e.LocalEventDate FROM [dbo].ShippingContainerEventDatasets e where e.ContainerId = c.ContainerId and e.EventType = 'SCANCLOSED') AS LocalClosedDate,
			 (SELECT TOP 1 e.Username  FROM [dbo].ShippingContainerEventDatasets e where e.ContainerId = c.ContainerId and e.EventType = 'SCANCLOSED') AS CLOSED_BY
         FROM [dbo].[PackageDatasets] p
			 LEFT JOIN [dbo].[ShippingContainerDatasets] c ON p.ContainerId = c.ContainerId
			 LEFT JOIN [dbo].[BinDatasets] b ON c.BinActiveGroupId = b.ActiveGroupId AND c.BinCode = b.BinCode
         WHERE 				 
		 p.SiteName = @siteName 
			AND p.LocalProcessedDate BETWEEN @manifestDate and DATEADD(day, 1, @manifestDate) 
			AND PackageStatus = 'PROCESSED'
			AND (@CONT_TYPE IS NULL OR c.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@CONT_TYPE, '|', 1 )))
			AND (@CONT_CARRIER IS NULL OR c.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@CONT_CARRIER, '|', 1)))
			AND (@CONT_METHOD IS NULL OR c.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@CONT_METHOD, '|', 1)))
			AND (@PKG_CARRIER IS NULL OR  p.ShippingCarrier IN (SELECT VALUE FROM STRING_SPLIT(@PKG_CARRIER, '|', 1)))
			AND (@PKG_SHIPPINGMETHOD IS NULL OR p.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@PKG_SHIPPINGMETHOD, '|', 1)))
			) s
         GROUP BY BinCode, ContainerId, PackageId, opened_by, SiteName
         ORDER BY BinCode, ContainerId, PackageId
END