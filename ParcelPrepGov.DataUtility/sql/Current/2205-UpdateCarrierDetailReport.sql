/****** Object:  StoredProcedure [dbo].[getRptCarrierDetail]    Script Date: 12/20/2022 8:58:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	EXEC getRptCarrierDetail 'CHARLESTON', '2022-12-12', '2022-12-12'
*/
CREATE OR ALTER PROCEDURE [dbo].[getRptCarrierDetail]
(    
    @siteName AS VARCHAR(100),
	@beginDate AS DATE,
	@endDate AS DATE,
	@entryUnitType AS VARCHAR(200) = NULL,
	@containerType AS VARCHAR(200) = NULL,
	@carrier AS VARCHAR(200) = NULL,
	@containerId AS VARCHAR(200) = NULL,
	@containerTrackingNumber AS VARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON
    SELECT        
            '' AS ID, 
        MAX(CASE WHEN scd.id IS NULL THEN @sitename ELSE scd.SiteName END) AS LOCATION, 
        MAX(CASE WHEN scd.id IS NULL THEN pd.LocalProcessedDate ELSE scd.LocalProcessedDate END) AS MANIFEST_DATE, 
        MAX(IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ, 
        MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
        MAX(scd.ContainerType) AS CONTAINER_TYPE, 
        MAX(IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.ShippingCarrierPrimary, bd.ShippingCarrierSecondary)) AS CARRIER,
        COUNT(pd.PackageId) AS TOTAL_PIECES,
        COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate) AS TOTAL_PCS_NO_SCAN, 
        pd.BinCode AS BIN_CODE,
        MAX (scd.ContainerId) AS CONTAINER_ID,
        MAX(scd.UpdatedBarcode) AS CONTAINER_TRACKING_NUMBER,
        MAX(IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY, 
        MAX(scd.LastKnownEventDate) AS LAST_KNOWN_DATE,
        MAX(scd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
        MAX(scd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION,
        Max(scd.LastKnownEventZip) AS LAST_KNOWN_ZIP,
        MAX(scd.Weight) AS CONTAINER_WEIGHT
    FROM dbo.PackageDatasets pd
        LEFT JOIN dbo.ShippingContainerDatasets scd ON scd.ContainerId = pd.ContainerId
        LEFT JOIN dbo.BinDatasets bd ON bd.BinCode = pd.BinCode AND bd.ActiveGroupId = pd.BinGroupId 
    WHERE pd.SiteName = @siteName
        AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
        AND pd.PackageStatus = 'PROCESSED'
        AND (LEFT(pd.BinCode, 1) IN ('D', 'S'))
        AND (scd.Status = 'CLOSED' OR scd.Id IS NULL)
        AND (scd.ShippingCarrier NOT IN ('USPS', 'USPS PMOD') OR scd.ShippingCarrier IS NULL)
            AND (@entryUnitType IS NULL OR IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF') IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitType, '|', 1)))
            AND (@containerType IS NULL OR ISNULL(scd.ContainerType,'') IN (SELECT VALUE FROM STRING_SPLIT(@containerType, '|', 1)))
            AND (@carrier IS NULL OR IIF(ISNULL(scd.IsSecondaryCarrier,0) = 0, bd.ShippingCarrierPrimary, bd.ShippingCarrierSecondary) IN (SELECT VALUE FROM STRING_SPLIT(@carrier, '|', 1)))
            AND (@containerId IS NULL OR pd.ContainerId IN (SELECT VALUE FROM STRING_SPLIT(@containerId, '|', 1)))
            AND (@containerTrackingNumber IS NULL OR ISNULL(scd.UpdatedBarcode,'') IN (SELECT VALUE FROM STRING_SPLIT(@containerTrackingNumber, '|', 1)))
    GROUP BY 
        scd.ContainerId, pd.BinCode        

END