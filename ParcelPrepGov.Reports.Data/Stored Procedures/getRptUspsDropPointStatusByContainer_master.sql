CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatusByContainer_master]
(    
    @site VARCHAR(50),
    @beginDate AS DATE,
    @endDate AS DATE,
    @product AS VARCHAR(MAX) = NULL,
    @postalArea AS VARCHAR(MAX) = NULL,
    @entryUnitName AS VARCHAR(MAX) = NULL,
    @entryUnitCsz AS VARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON
			SELECT
				MAX(CONVERT(varchar, s.Description) + 
					CONVERT(varchar, pd.LocalProcessedDate, 101) + 					
						IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
						IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
						IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary))  + 
						IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)))
					AS ID,
				MAX(s.SiteName) AS [Site], 
				MAX(FORMAT(pd.LocalProcessedDate, 'MM/dd/yyyy' )) AS MANIFEST_DATE, 
				pd.BinCode AS BIN_CODE,
				MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary) ) AS DROP_SHIP_SITE_KEY,
				MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) ) AS ENTRY_UNIT_NAME, 
				MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) ) AS ENTRY_UNIT_CSZ, 
				MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') ) AS ENTRY_UNIT_TYPE, 
				MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) ) AS PRODUCT,  
				MAX(sc.ShippingCarrier) AS CARRIER, 
				MAX(sc.ContainerType) AS CONTAINER_TYPE,
				pd.ContainerId AS CONTAINER_ID,
				MAX(sc.UpdatedBarcode) as TRACKING_NUMBER,
				MAX(sc.LastKnownEventDate) as LAST_KNOWN_DATE,
				MAX(sc.LastKnownEventDescription) as LAST_KNOWN_DESCRIPTION,
				MAX(sc.LastKnownEventLocation) as LAST_KNOWN_LOCATION,
				MAX(sc.LastKnownEventZip) as LAST_KNOWN_ZIP,
				COUNT(pd.PackageId) AS TOTAL_PCS, 
				COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS PCS_NO_STC, 
				CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
				(COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate)) AS PCS_NO_SCAN, 
				CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
			FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
				LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
				LEFT JOIN dbo.BinDatasets bd ON  bd.ActiveGroupId = pd.BinGroupId AND  bd.BinCode = pd.BinCode
				LEFT JOIN dbo.ShippingContainerDatasets sc ON sc.ContainerId = pd.ContainerId 
			WHERE pd.PackageStatus = 'PROCESSED' 
				AND pd.ShippingCarrier = 'USPS' 
				AND pd.ShippingMethod in ('FCZ', 'PSLW', 'PS')
				AND pd.SiteName = @site 		
				AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 				
				AND (sc.Status = 'CLOSED' OR sc.ContainerId IS NULL)
				AND (@product IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) 
					IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
				AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
					IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
				AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) 
					IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
			GROUP BY 
				pd.ContainerId, pd.BinCode
		UNION
			
			SELECT -- give me closed containers that do not have any packages inside!
			sc.ContainerId AS ID,		 
			MAX(sc.SiteName) AS CUST_LOCATION, 
			FORMAT (MAX(sc.LocalProcessedDate), 'MM/dd/yyyy') AS MANIFEST_DATE,
			MAX(sc.BinCode) AS BIN_CODE,
			MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY,
			MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
			MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ, 
			MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
			MAX(sc.ShippingMethod) AS PRODUCT,  
			MAX(sc.ShippingCarrier) AS CARRIER, 
			MAX(sc.ContainerType) AS CONTAINER_TYPE,
			sc.ContainerId AS CONTAINER_ID,
			MAX(sc.UpdatedBarcode) as TRACKING_NUMBER,
			MAX(sc.LastKnownEventDate) as LAST_KNOWN_DATE,
			MAX(sc.LastKnownEventDescription) as LAST_KNOWN_DESCRIPTION,
			MAX(sc.LastKnownEventLocation) as LAST_KNOWN_LOCATION,
			MAX(sc.LastKnownEventZip) as LAST_KNOWN_ZIP,
			COUNT(DISTINCT(pd.id)) AS TOTAL_PCS, 
			0 AS PCS_NO_STC, 
			0 AS PCT_NO_STC, 
			0 AS PCS_NO_SCAN, 
			0 AS PCT_NO_SCAN
		FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK, FORCESEEK) 
			LEFT JOIN dbo.BinDatasets bd ON sc.BinActiveGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
			LEFT JOIN dbo.PackageDatasets pd on sc.ContainerId = pd.ContainerId
			WHERE sc.status = 'CLOSED'						
				AND sc.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @enddate) 		
				AND sc.SiteName IN (SELECT VALUE FROM STRING_SPLIT(@site, ',', 1))
		GROUP BY sc.ContainerId
		HAVING COUNT(DISTINCT(pd.id)) = 0					
END