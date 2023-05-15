/****** Object:  StoredProcedure [dbo].[getContainerSearchByBarcode]    Script Date: 11/10/2022 10:30:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchByBarcode]     
@barcode varchar(100), 
@siteName varchar(50) = NULL
AS
BEGIN
	SELECT TOP 1 	
		sc.ContainerId AS CONTAINER_ID,
		CAST(CONVERT(varchar, sc.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		sc.Status AS [STATUS], 
		sc.ContainerType AS CONTAINER_TYPE,
        sc.ShippingCarrier AS SHIPPING_CARRIER, 
        sc.ShippingMethod AS SHIPPING_METHOD,  
		sc.UpdatedBarcode AS TRACKING_NUMBER,
		sc.BinCode AS BIN_CODE,
		sc.SiteName AS [FSC_SITE],
		sc.Zone AS [ZONE],
		IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary) AS DROP_SHIP_SITE_KEY,
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
        IIF(sc.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
        IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		sc.Weight AS CONTAINER_WEIGHT,	
		sc.IsOutside48States AS IS_OUTSIDE_48_STATES,
		sc.IsRural AS IS_RURAL,
		sc.IsSaturdayDelivery AS IS_SATURDAY,
		sc.LastKnownEventDate AS LAST_KNOWN_DATE,
        sc.LastKnownEventDescription AS LAST_KNOWN_DESCRIPTION,
        sc.LastKnownEventLocation AS LAST_KNOWN_LOCATION,
        sc.LastKnownEventZip AS LAST_KNOWN_ZIP,
		CASE WHEN sc.IsSecondaryCarrier=1 THEN 1 ELSE 0 END AS [IS_SECONDARY_CARRIER]
    FROM dbo.ShippingContainerDatasets SC WITH (NOLOCK) 	
    LEFT JOIN dbo.BinDatasets bd ON  bd.ActiveGroupId = sc.BinActiveGroupId AND  bd.BinCode = sc.BinCode
    WHERE (sc.SiteName = @siteName OR @siteName IS NULL OR @siteName = 'GLOBAL')
	AND (sc.ContainerId = @barcode OR sc.UpdatedBarcode = @barcode)
ORDER BY SC.CosmosCreateDate DESC
   
END
