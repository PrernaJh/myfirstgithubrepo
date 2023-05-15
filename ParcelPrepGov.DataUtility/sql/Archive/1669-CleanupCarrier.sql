-- Clean up procedure that is not used anywhere
IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.getRptUspsCarrierDetail'))
BEGIN
	DROP PROCEDURE getRptUspsCarrierDetail	
END

GO

-- Clean up procedure that is not used anywhere
IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.getRptUspsDropPointStatus'))
BEGIN
	DROP PROCEDURE getRptUspsDropPointStatus	
END

GO

/****** Object:  StoredProcedure [dbo].[getRptCarrierDetail]    Script Date: 5/24/2022 8:34:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	EXEC getRptCarrierDetail 'CHARLESTON', '2022-01-03', '2022-01-03'
*/
CREATE OR ALTER PROCEDURE [dbo].[getRptCarrierDetail]
(    
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
			MAX(scd.SiteName + CONVERT(varchar, scd.LocalProcessedDate, 101) + IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) ) AS ID, 
		MAX(scd.SiteName) AS LOCATION, 
		MAX(scd.LocalProcessedDate) AS MANIFEST_DATE, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		MAX(scd.ContainerType) AS CONTAINER_TYPE, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER, 
		 
        COUNT(pd.PackageId) - COUNT(pd.StopTheClockEventDate) AS TOTAL_PCS_NO_SCAN, 
		COUNT(pd.PackageId) AS TOTAL_PIECES,
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
			AND (@entryUnitType IS NULL OR IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF') IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitType, '|', 1)))
			AND (@containerType IS NULL OR scd.ContainerType IN (SELECT VALUE FROM STRING_SPLIT(@containerType, '|', 1)))
			AND (@carrier IS NULL OR IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) IN (SELECT VALUE FROM STRING_SPLIT(@carrier, '|', 1)))
			AND (@containerId IS NULL OR scd.ContainerId IN (SELECT VALUE FROM STRING_SPLIT(@containerId, '|', 1)))
			AND (@containerTrackingNumber IS NULL OR scd.UpdatedBarcode IN (SELECT VALUE FROM STRING_SPLIT(@containerTrackingNumber, '|', 1)))
	GROUP BY 
        scd.ContainerId
END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_master]    Script Date: 5/25/2022 8:50:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_master]    Script Date: 5/25/2022 8:50:52 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
	exec [getRptUspsDropPointStatus_master] 'CMOPCHARLESTON', '02/01/2022', '02/01/2022'
*/
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
        IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
	    IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
	    ISNULL(sc.ShippingMethod, 'null')  + --, --IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.ShippingMethodPrimary,  bd.ShippingMethodSecondary))  +
		ISNULL(sc.ShippingCarrier, 'null') AS ID,		 
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
		AND pd.ShippingMethod IN ('FCZ', 'PSLW')
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
		AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
		AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
		
	GROUP BY 
		s.Description, 
		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy'),
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary), 
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary), 
		sc.ShippingMethod,  
		sc.ShippingCarrier

END


GO 


/****** Object:  StoredProcedure [dbo].[getRptUspsDropPointStatus_detail]    Script Date: 5/25/2022 2:15:14 PM ******/
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
        IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
	    IIF(IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) +
	    ISNULL(sc.ShippingMethod, 'null')  + 
		ISNULL(sc.ShippingCarrier, 'null') AS ID, 
		s.Description AS CUST_LOCATION,
		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') AS MANIFEST_DATE, 
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
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
		AND pd.ShippingMethod IN ('FCZ', 'PSLW')
		AND pd.SubClientName IN (SELECT VALUE FROM STRING_SPLIT(@subClients, ',', 1))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
		AND (pd.StopTheClockEventDate is NULL)
		AND (@product IS NULL OR pd.ShippingMethod IN (SELECT VALUE FROM STRING_SPLIT(@product, '|', 1))) 
		AND (@entryUnitName IS NULL OR bd.DropShipSiteDescriptionPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitName, '|', 1)))
		AND (@entryUnitCsz IS NULL OR bd.DropShipSiteCszPrimary IN (SELECT VALUE FROM STRING_SPLIT(@entryUnitCsz, '|', 1)))
END





