
CREATE OR ALTER PROCEDURE [dbo].[getRptDailyContainer_master]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
    @product AS VARCHAR(MAX) = null,
    @postalArea AS VARCHAR(MAX) = null,
    @entryUnitName AS VARCHAR(MAX) = null,
    @entryUnitCsz AS VARCHAR(MAX) = null
) AS
BEGIN
    SET NOCOUNT ON
    SELECT
        sc.ContainerId AS ID,
        MAX(sc.ContainerId) AS CONTAINER_ID,
        sc.Status as CONTAINER_STATUS,
        MAX(sc.DatasetCreateDate) AS CONTAINER_OPEN_DATE,
        MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
        MAX(bd.BinCode) as BIN_NUMBER,
        MAX(sc.Username) as USERNAME,
        MAX(sc.LocalProcessedDate) AS CONTAINER_CLOSED_DATE,
        COUNT(pd.PackageId) AS TOTAL_PACKAGES,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as DROP_SHIP_SITE_KEY,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ 
    FROM dbo.PackageDatasets pd WITH (NOLOCK)
    LEFT JOIN dbo.ShippingContainerDatasets sc on pd.ContainerId = sc.ContainerId
    LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
    LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    WHERE pd.SiteName = @siteName
		AND pd.ShippingMethod IN('PSLW','FCZ')			
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
        AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
    GROUP BY 
        sc.ContainerId,
        sc.Status
UNION 
		SELECT -- give me closed containers that do not have any packages inside!
		'' AS ID,		 
		sc.ContainerId AS CONTAINER_ID,
		sc.Status as CONTAINER_STATUS,
		MAX(sc.DatasetCreateDate) AS CONTAINER_OPEN_DATE,
		MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
		MAX(sc.BinCode) AS BIN_CODE,
		MAX(sc.Username) as USERNAME,
        MAX(sc.LocalProcessedDate) AS CONTAINER_CLOSED_DATE,
		COUNT(DISTINCT(pd.id)) AS TOTAL_PCS, 
		MAX(sc.ShippingCarrier) AS CARRIER, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY,
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ
	FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK, FORCESEEK) 
		LEFT JOIN dbo.BinDatasets bd ON sc.BinActiveGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
		LEFT JOIN dbo.PackageDatasets pd on sc.ContainerId = pd.ContainerId
		WHERE sc.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 		
			AND sc.SiteName = @siteName
	GROUP BY sc.ContainerId, sc.Status
	HAVING COUNT(DISTINCT(pd.id)) = 0		
ORDER BY
CONTAINER_STATUS,
TOTAL_PACKAGES

END

GO

CREATE OR ALTER PROCEDURE [dbo].[getRptDailyContainer_detail]
(    
    @siteName VARCHAR(MAX),
    @manifestDate AS DATE,
    @product AS VARCHAR(MAX) = null,
    @postalArea AS VARCHAR(MAX) = null,
    @entryUnitName AS VARCHAR(MAX) = null,
    @entryUnitCsz AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON
    SELECT        
        pd.ContainerId AS ID,  
        s.Description AS CUST_LOCATION,
        CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
        IIF(LEFT(pd.bincode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
        pd.ShippingMethod AS PRODUCT,  
        IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) AS CARRIER, 
        pd.ShippingCarrier AS PACKAGE_CARRIER,
        IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
            pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
        IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
        IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
        IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP, 
        pd.PackageId AS PACKAGE_ID, 
        IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
        pd.Zip
    FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
    LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
    LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
    LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]     
    WHERE pd.SiteName = @siteName
		AND pd.ShippingMethod IN('PSLW','FCZ')    
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
        AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
END