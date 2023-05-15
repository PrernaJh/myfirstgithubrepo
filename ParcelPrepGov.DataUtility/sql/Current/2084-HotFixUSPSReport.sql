
CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_detail]
(
	@subClients VARCHAR(250),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(200) = NULL,
	@postalArea AS VARCHAR(200) = NULL,
	@entryUnitName AS VARCHAR(500) = NULL,
	@entryUnitCsz AS VARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
CONVERT(varchar, s.Description) +
        FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') +
        IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
	    IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
	    ISNULL(sc.ShippingMethod, 'null')  + 
		ISNULL(sc.ShippingCarrier, 'null') AS ID, 
		s.Description AS CUST_LOCATION,
		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') AS MANIFEST_DATE, 
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		pd.ContainerId AS CONTAINER_ID,
		pd.ShippingMethod AS PRODUCT,  
		sc.ShippingCarrier AS CARRIER, 
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
		LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
		LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
		LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.ShippingCarrier = 'USPS'
		AND (sc.status='CLOSED' OR sc.ContainerId IS NULL)		
		AND pd.ShippingMethod IN ('FCZ', 'PSLW', 'PS')
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (pd.StopTheClockEventDate is NULL)
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
		AND (@entryUnitName IS NULL OR IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
		AND (@entryUnitCsz IS NULL OR IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
END

GO

CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_master]
(
	@subClients VARCHAR(250),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(200) = NULL,
	@postalArea AS VARCHAR(200) = NULL,
	@entryUnitName AS VARCHAR(500) = NULL,
	@entryUnitCsz AS VARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON
		
			SELECT MAX(ID) AS ID, 
				MAX(CUST_LOCATION) AS CUST_LOCATION,
				MANIFEST_DATE,
				ENTRY_UNIT_NAME,
				ENTRY_UNIT_CSZ, 
				ENTRY_UNIT_TYPE,
				MAX(PRODUCT) AS PRODUCT,
				CARRIER, 
				SUM(TOTAL_BAGS)  AS TOTAL_BAGS,
				SUM(TOTAL_PCS)   AS TOTAL_PCS,
				SUM(PCS_NO_STC)  AS PCS_NO_STC,
				SUM(PCT_NO_STC) as PCT_NO_STC,
				SUM(PCS_NO_SCAN) AS PCS_NO_SCAN,
				SUM(PCT_NO_SCAN) AS PCT_NO_SCAN FROM (
				SELECT
					CONVERT(varchar, s.Description) 
					+ FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') +
					IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',
						IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) 
					+ IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  
						IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) 
					+ ISNULL(sc.ShippingMethod, 'null')   
					+ ISNULL(sc.ShippingCarrier, 'null') AS ID,		 
					s.Description AS CUST_LOCATION, 
					FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') AS MANIFEST_DATE,
					IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
					IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
					MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
					MAX(pd.ShippingMethod) AS PRODUCT,  
					MAX(sc.ShippingCarrier) AS CARRIER, 
					COUNT(pd.PackageId) AS TOTAL_PCS, 
					COUNT(DISTINCT(sc.ContainerId)) AS TOTAL_BAGS, 
					COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS PCS_NO_STC, 
					CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
					(COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate)) AS PCS_NO_SCAN, 
					CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
				FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
					LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
					LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
					LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
				WHERE pd.PackageStatus = 'PROCESSED' 
					AND pd.ShippingCarrier = 'USPS' 
					AND pd.ShippingMethod IN ('FCZ', 'PSLW', 'PS')
					AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @enddate) 
					AND (sc.status='CLOSED' OR sc.ContainerId IS NULL)				
					AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))				
					AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
					AND (@entryUnitName IS NULL OR IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
						IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
					AND (@entryUnitCsz IS NULL OR IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)
						IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
				GROUP BY 
					s.Description, 
					FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy'),
					IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary), 
					IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary), 
					sc.ShippingMethod,  
					sc.ShippingCarrier

		UNION
			SELECT * FROM 
					(SELECT -- give me closed containers that do not have any packages inside!
					MAX(sc.ContainerId) AS ID,		 
					MAX(sc.SiteName) AS CUST_LOCATION, 
					FORMAT (sc.LocalProcessedDate, 'MM/dd/yyyy') AS MANIFEST_DATE,
					IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
					IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
					IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
					'' AS PRODUCT,  
					sc.ShippingCarrier AS CARRIER, 
					count(DISTINCT(pd.id)) AS TOTAL_PCS, 
					COUNT(DISTINCT(sc.ContainerId)) AS TOTAL_BAGS, 
					0 AS PCS_NO_STC, 
					0 AS PCT_NO_STC, 
					0 AS PCS_NO_SCAN, 
					0 AS PCT_NO_SCAN
				FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK, FORCESEEK) 
					LEFT JOIN dbo.BinDatasets bd ON sc.BinActiveGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
					LEFT JOIN dbo.PackageDatasets pd on sc.ContainerId = pd.ContainerId
					WHERE sc.status = 'CLOSED'						
						AND sc.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @enddate) 		
						AND sc.SiteName IN (SELECT DISTINCT SITENAME FROM SubClientDatasets where Name IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1)))	
				GROUP BY bd.BinCode, sc.ShippingCarrier, FORMAT(sc.LocalProcessedDate, 'MM/dd/yyyy'),
					IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary), 
					IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) s WHERE TOTAL_PCS = 0) x 
		GROUP BY MANIFEST_DATE, ENTRY_UNIT_NAME, ENTRY_UNIT_CSZ, ENTRY_UNIT_TYPE, CARRIER
END
