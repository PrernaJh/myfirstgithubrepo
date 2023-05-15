/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatusByContainer]    Script Date: 12/22/2021 1:00:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatusByContainer]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null,
	@postalArea AS VARCHAR(MAX) = null,
	@entryUnitName AS VARCHAR(MAX) = null,
	@entryUnitCsz AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		CONVERT(varchar, s.Description) + 
			CONVERT(varchar, pd.LocalProcessedDate, 101) + 
				IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
				IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary) +
				IIF(pd.ShippingMethod IS NULL, 'null',  pd.ShippingMethod)  + 
				IIF(bd.ShippingCarrierPrimary IS NULL, 'null',  bd.ShippingCarrierPrimary)
			AS ID,
		pd.ContainerId AS CONTAINER_ID,
		pd.LastKnownEventDate as LAST_KNOWN_DATE,
		pd.LastKnownEventDescription as LAST_KNOWN_DESCRIPTION,
		pd.LastKnownEventLocation as LAST_KNOWN_LOCATION,
		pd.LastKnownEventZip as LAST_KNOWN_ZIP,
		pd.DropSiteKeyValue as DROP_SHIP_SITE_KEY,
		s.Description AS CUST_LOCATION, 
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		pd.ShippingMethod AS PRODUCT,  
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(pd.PackageId)-COUNT(pd.StopTheClockEventDate) AS PCS_NO_STC, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
		(COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate)) AS PCS_NO_SCAN, 
		CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
	FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|'))) 
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
		
	GROUP BY 
		pd.ContainerId,
		s.Description, 
		CONVERT(varchar, pd.LocalProcessedDate, 101),
		pd.ShippingMethod,  
		bd.ShippingCarrierPrimary,
		pd.LastKnownEventDate,
		pd.LastKnownEventDescription,
		pd.LastKnownEventLocation,
		pd.LastKnownEventZip,
		pd.DropSiteKeyValue,
		bd.DropShipSiteDescriptionPrimary,
		bd.DropShipSiteCszPrimary

END