/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatusByContainer_detail]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER PROCEDURE [dbo].[getRptUspsDropPointStatusByContainer_detail]
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
		s.Description AS CUST_LOCATION,
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
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
	--LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
	--	OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM
	--		FROM dbo.TrackPackageDatasets tpd
	--					LEFT JOIN dbo.EvsCodes ec ON tpd.EventCode = ec.Code 
	--					LEFT JOIN dbo.PackageDatasets AS p ON tpd.PackageDatasetId = p.Id
	--				WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL)
	--					AND p.PackageStatus = 'PROCESSED' AND p.ShippingCarrier = 'USPS'
	--					AND p.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)) t 
	--			ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	--LEFT JOIN dbo.EvsCodes e ON t.EventCode = e.Code  
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]

	WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) AND (pd.StopTheClockEventDate is NULL)
			AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|'))) 
			AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
			AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
END

GO
