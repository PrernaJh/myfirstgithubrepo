
CREATE OR ALTER PROCEDURE [dbo].[getRptDailyContainer_master]
(    
	@siteName VARCHAR(MAX),
    @manifestDate AS DATE,
	@containerType as VARCHAR(100) = NULL,
    @carrier AS VARCHAR(MAX) = NULL,
	@entryKey AS VARCHAR(150) = NULL,
    @entryUnitName AS VARCHAR(MAX) = NULL,
    @entryUnitCsz AS VARCHAR(MAX) = NULL
) AS
BEGIN
    SET NOCOUNT ON
    SELECT
        MAX(s.SiteName + ',' + ISNULL(sc.ContainerId, '')) AS ID,
        sc.Status as CONTAINER_STATUS,
        MAX(FORMAT(sc.LocalCreateDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_OPEN_DATE,
		(SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username 
				WHERE e.ContainerId = sc.ContainerId AND e.EventType = 'CREATED') AS OPENED_BY_NAME,
        MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
        MAX(sc.ContainerId) AS CONTAINER_ID,
        MAX(bd.BinCode) as BIN_NUMBER,
		MAX(sc.ContainerType) as CONT_TYPE,
		MAX(sc.Weight) AS CONT_WEIGHT,
        COUNT(pd.PackageId) AS TOTAL_PACKAGES,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as DROP_SHIP_SITE_KEY,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ,
        MAX(FORMAT(sc.LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_CLOSED_DATE,
		(SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username 
				WHERE e.ContainerId = sc.ContainerId AND e.EventType = 'SCANCLOSED') AS CLOSED_BY_NAME
    FROM dbo.PackageDatasets pd WITH (NOLOCK)
    	LEFT JOIN dbo.ShippingContainerDatasets sc on pd.ContainerId = sc.ContainerId
   		LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
   		LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    WHERE 
		pd.SiteName = @siteName
		AND PackageStatus = 'PROCESSED'
		AND pd.ShippingMethod IN('PSLW','FCZ', 'PS')			
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
		AND (@containerType IS NULL OR sc.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@containerType, '|', 1)))
		AND (@carrier IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@carrier, '|', 1)))
    	AND (@entryKey IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryKey, '|', 1)))
        AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
    GROUP BY 
        sc.ContainerId,
        sc.Status
UNION 
		SELECT -- give me closed containers that do not have any packages inside!
		sc.ContainerId AS ID,		 
		sc.Status as CONTAINER_STATUS,
		MAX(FORMAT(sc.LocalCreateDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_OPEN_DATE,
		(SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username 
				WHERE e.ContainerId = sc.ContainerId AND e.EventType = 'CREATED') AS OPENED_BY_NAME,
		MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
		sc.ContainerId AS CONTAINER_ID,
		MAX(sc.BinCode) AS BIN_NUMBER,
		MAX(sc.ContainerType) AS CONT_TYPE,
		MAX(sc.Weight) AS CONT_WEIGHT,
		COUNT(DISTINCT(pd.id)) AS TOTAL_PACKAGES, 
		MAX(sc.ShippingCarrier) AS CARRIER, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY,
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ,
        MAX(FORMAT(sc.LocalProcessedDate, 'MM/dd/yyyy hh:mm:ss')) AS CONTAINER_CLOSED_DATE,
		(SELECT TOP 1 ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') 
				FROM [dbo].ShippingContainerEventDatasets e LEFT OUTER JOIN UserLookups u ON u.Username = e.Username 
				WHERE e.ContainerId = sc.ContainerId AND e.EventType = 'SCANCLOSED') AS CLOSED_BY_NAME
	FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK, FORCESEEK) 
			LEFT JOIN dbo.BinDatasets bd ON sc.BinActiveGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
			LEFT JOIN dbo.PackageDatasets pd on sc.ContainerId = pd.ContainerId
		WHERE sc.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 		
			AND sc.SiteName = @siteName
		AND (@containerType IS NULL OR sc.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@containerType, '|', 1)))
		AND (@carrier IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@carrier, '|', 1)))
		AND (@entryKey IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryKey, '|', 1)))
		AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
	GROUP BY sc.ContainerId, sc.Status
	HAVING COUNT(DISTINCT(pd.id)) = 0		
ORDER BY
CONTAINER_STATUS,
TOTAL_PACKAGES

END

