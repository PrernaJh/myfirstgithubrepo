-- Clean up procedure that is not used anywhere
IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.getRptUspsDropPointStatusByContainer'))
BEGIN
	DROP PROCEDURE getRptUspsDropPointStatusByContainer	
END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatusByContainer_master]    Script Date: 5/10/2022 11:39:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	exec [getRptUspsDropPointStatusByContainer_master] 'CHICAGO', '1/3/2022','1/3/2022'
*/
ALTER     PROCEDURE [dbo].[getRptUspsDropPointStatusByContainer_master]
(
    -- Add the parameters for the stored procedure here
    @site VARCHAR(50),
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
        MAX(CONVERT(varchar, s.Description) + 
            CONVERT(varchar, pd.LocalProcessedDate, 101) + 
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary))  + 
                IIF(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IS NULL, 'null',  IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)))
            AS ID,
		MAX(s.SiteName) AS [Site], 
		MAX(CAST(CONVERT(varchar, sc.LocalProcessedDate, 101) AS DATE)) AS MANIFEST_DATE, 
		MAX(pd.BinCode) AS BIN_CODE,
		MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as DROP_SHIP_SITE_KEY,
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ, 
        MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary)) AS PRODUCT,  
        MAX(IIF(sc.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
		MAX(sc.ContainerType) AS CONTAINER_TYPE,
		MAX(sc.ContainerId) AS CONTAINER_ID,
		MAX(sc.UpdatedBarcode) as TRACKING_NUMBER,
		MAX(sc.LastKnownEventDate) as LAST_KNOWN_DATE,
        MAX(sc.LastKnownEventDescription) as LAST_KNOWN_DESCRIPTION,
        MAX(sc.LastKnownEventLocation) as LAST_KNOWN_LOCATION,
        MAX(sc.LastKnownEventZip) as LAST_KNOWN_ZIP,
        COUNT(pd.PackageId) AS TOTAL_PCS, 
        COUNT(pd.PackageId)-COUNT(pd.StopTheClockEventDate) AS PCS_NO_STC, 
        CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_STC, 
        (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate)) AS PCS_NO_SCAN, 
        CONVERT(DECIMAL, (COUNT(pd.PackageId) - COUNT(pd.LastKnownEventDate))) / CONVERT(DECIMAL,COUNT(pd.PackageId)) AS PCT_NO_SCAN
    FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
    LEFT JOIN dbo.ShippingContainerDatasets sc ON pd.ContainerId = sc.ContainerId
    LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
    
    WHERE pd.PackageStatus = 'PROCESSED' 
		AND pd.ShippingCarrier = 'USPS' 
		AND pd.ShippingMethod in ('FCZ', 'PSLW')
		AND pd.SiteName = @site 		
        AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (sc.Id IS NOT NULL AND sc.[Status] = 'CLOSED')
        AND (@product IS NULL OR pd.ShippingMethod IN (SELECT * FROM [dbo].[SplitString](@product, '|'))) 
        AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitName, '|')))
        AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT * FROM [dbo].[SplitString](@entryUnitCsz, '|')))
    GROUP BY 
        sc.ContainerId
END


