/****** Object:  StoredProcedure [dbo].[getRptDailyContainer_master] by Chong Vang  Script Date: 1/25/2022 11:04:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[getRptDailyContainer_master]
(
    -- Add the parameters for the stored procedure here
    @subClients VARCHAR(MAX),
    @manifestDate AS DATE,
    @product AS VARCHAR(MAX) = null,
    @postalArea AS VARCHAR(MAX) = null,
    @entryUnitName AS VARCHAR(MAX) = null,
    @entryUnitCsz AS VARCHAR(MAX) = null
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
		sc.Status as CONTAINER_STATUS,
		MAX(sc.DatasetCreateDate) AS CONTAINER_OPEN_DATE,
        Max(sc.ContainerId) AS CONTAINER_ID,
		MAX(sc.UpdatedBarcode) AS TRACKING_NUMBER,
		MAX(bd.BinCode) as BIN_NUMBER,
		MAX(sc.Username) as USERNAME,
		MAX(sc.LocalProcessedDate) AS CONTAINER_CLOSED_DATE,
        COUNT(pd.PackageId) AS TOTAL_PACKAGES,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as DROP_SHIP_SITE_KEY,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ 
    FROM dbo.ShippingContainerDatasets sc WITH (NOLOCK)
	LEFT JOIN dbo.PackageDatasets pd on sc.BinCode = pd.BinCode 
    LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND sc.BinCode = bd.BinCode 
    LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    WHERE pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|'))) 
        AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
        AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
    GROUP BY 
		sc.ContainerId,
		sc.Status
	ORDER BY
	CONTAINER_STATUS,
	TOTAL_PACKAGES
END

GO
/****** Object:  StoredProcedure [dbo].[getRptDailyContainer_detail]    Script Date: 1/25/2022 11:37:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptDailyContainer_detail]
(
    -- Add the parameters for the stored procedure here
	@subClients VARCHAR(MAX),
    @manifestDate AS DATE,
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
				IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary))  + 
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary))
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
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId

	WHERE pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
        AND pd.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate) 
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|'))) 
        AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
        AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
END

