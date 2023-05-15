SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatus_master]
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
		MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT,  
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(DISTINCT(sc.ContainerId)) AS TOTAL_BAGS, 
		COUNT(pd.PackageId)-COUNT(pd.StopTheClockEventDate) AS PCS_NO_STC, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
		(COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate)) AS PCS_NO_SCAN, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.ShippingCarrier = 'USPS' 
		AND pd.ShippingMethod IN ('FCZ', 'PSLW')
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
		AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
		AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
		
	GROUP BY 
		s.Description, 
		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy'),
		bd.DropShipSiteDescriptionPrimary, 
		bd.DropShipSiteCszPrimary, 
		pd.ShippingMethod,  
		bd.ShippingCarrierPrimary

END
