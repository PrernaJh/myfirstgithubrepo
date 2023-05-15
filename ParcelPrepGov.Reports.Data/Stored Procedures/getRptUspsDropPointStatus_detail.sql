
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

