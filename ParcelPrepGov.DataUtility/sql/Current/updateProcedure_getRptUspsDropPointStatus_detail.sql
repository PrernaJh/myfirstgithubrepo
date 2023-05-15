SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_detail]
(
	@subClients VARCHAR(250),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(200) = null,
	@postalArea AS VARCHAR(200) = null,
	@entryUnitName AS VARCHAR(500) = null,
	@entryUnitCsz AS VARCHAR(500) = null
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		CONVERT(varchar, s.Description) + 
			FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') + 
				IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
				IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary) +
				IIF(pd.ShippingMethod IS NULL, 'null',  pd.ShippingMethod)  + 
				IIF(bd.ShippingCarrierPrimary IS NULL, 'null',  bd.ShippingCarrierPrimary)
			AS ID, 
		s.Description AS CUST_LOCATION,
		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT,  
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
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

	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.ShippingCarrier = 'USPS'
		AND pd.ShippingMethod IN ('FCZ', 'PSLW')
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (pd.StopTheClockEventDate is NULL)
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
		AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
		AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
END

