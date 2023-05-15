/****** Object:  StoredProcedure [dbo].[getRptCarrierDetail]    Script Date: 4/29/2022 11:50:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[getRptCarrierDetail]
(
    -- Add the parameters for the stored procedure here
    @siteName AS VARCHAR(100),
	@beginDate AS DATE,
	@endDate AS DATE,

	@entryUnitType AS VARCHAR(200) = null,
	@containerType AS VARCHAR(200) = null,
	@carrier AS VARCHAR(200) = null,
	@containerId AS VARCHAR(200) = null,
	@containerTrackingNumber AS VARCHAR(200) = null
)
AS

BEGIN
    SET NOCOUNT ON

	SELECT        
		'' AS ID, 
		MAX(scd.SiteName) AS LOCATION, 
		MAX(CAST(CONVERT(varchar, scd.LocalProcessedDate, 101) AS DATETIME2)) AS MANIFEST_DATE, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		MAX(scd.ContainerType) AS CONTAINER_TYPE, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
		 
        COUNT(pd.PackageId)-COUNT(pd.StopTheClockEventDate) AS CONT_NO_SCAN, 
		COUNT(pd.PackageId) AS TOTAL_CONT,
		scd.ContainerId AS CONTAINER_ID,
		MAX(scd.UpdatedBarcode) AS CONTAINER_TRACKING_NUMBER,
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) AS DROP_SHIP_SITE_KEY, 
		MAX(scd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		MAX(scd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
		MAX(scd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION,
		Max(scd.LastKnownEventZip) AS LAST_KNOWN_ZIP,
		MAX(scd.Weight) AS CONTAINER_WEIGHT

	FROM dbo.PackageDatasets pd
		LEFT JOIN dbo.ShippingContainerDatasets scd ON scd.ContainerId = pd.ContainerId
		LEFT JOIN dbo.BinDatasets bd ON bd.BinCode = pd.BinCode AND bd.ActiveGroupId = pd.BinGroupId 

	WHERE pd.SiteName = @siteName
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND pd.PackageStatus = 'PROCESSED'
		AND scd.Status = 'CLOSED'
		AND scd.ShippingCarrier NOT IN ('USPS', 'USPS PMOD')
			AND (@entryUnitType IS NULL OR IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF') IN (SELECT * FROM [dbo].[SplitString](@entryUnitType, '|')))
			AND (@containerType IS NULL OR scd.ContainerType IN (SELECT * FROM [dbo].[SplitString](@containerType, '|')))
			AND (@carrier IS NULL OR IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IN (SELECT * FROM [dbo].[SplitString](@carrier, '|')))
			AND (@containerId IS NULL OR scd.ContainerId IN (SELECT * FROM [dbo].[SplitString](@containerId, '|')))
			AND (@containerTrackingNumber IS NULL OR scd.UpdatedBarcode IN (SELECT * FROM [dbo].[SplitString](@containerTrackingNumber, '|')))

	GROUP BY 
        scd.ContainerId
END