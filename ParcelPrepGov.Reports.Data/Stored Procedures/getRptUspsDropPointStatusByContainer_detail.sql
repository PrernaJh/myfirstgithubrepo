CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatusByContainer_detail]
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
		CONVERT(varchar, s.Description) + 
		CONVERT(varchar, pd.LocalProcessedDate, 101) + 
        IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
        IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
        IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary))  + 
        IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) 
			AS ID,  
		s.SiteName AS [SITE],
		CAST(pd.LocalProcessedDate AS DATE) AS MANIFEST_DATE, 
		pd.ContainerId AS CONTAINER_ID,
		pd.BinCode AS BIN_CODE,
		IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary) as DROP_SHIP_SITE_KEY,
		IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
		IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT,  
		sc.ShippingCarrier AS CARRIER, 
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP
	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK)  
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    LEFT OUTER JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId

	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.ShippingCarrier = 'USPS' 
		AND pd.ShippingMethod in ('FCZ', 'PSLW', 'PS')
		AND pd.SiteName = @site 				
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND pd.StopTheClockEventDate is NULL		
		AND (sc.Status = 'CLOSED' OR sc.ContainerId IS NULL)
        AND (@product IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
        AND (@entryUnitName IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
        AND (@entryUnitCsz IS NULL OR IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) 
			IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
END