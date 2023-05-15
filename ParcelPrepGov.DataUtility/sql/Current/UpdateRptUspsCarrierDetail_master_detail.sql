/****** Object:  StoredProcedure [dbo].[getRptUspsCarrierDetail_master]    Script Date: 4/22/2022 8:17:33 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER     PROCEDURE [dbo].[getRptUspsCarrierDetail_master]
(
    -- Add the parameters for the stored procedure here
	@subClientName VARCHAR(50),
	@beginDate AS DATE, 
	@endDate AS DATE,
	@product AS VARCHAR(MAX) = null
	
)
AS

BEGIN
    SET NOCOUNT ON

	DECLARE @site VARCHAR(50);

    SELECT @site = (SELECT SiteName FROM SubClientDatasets WHERE SubClientDatasets.Name = @subClientName); 

	SELECT        
		MAX(CONVERT(varchar, scd.SiteName) + 
			CONVERT(varchar, scd.LocalProcessedDate, 101) + 
				IIF(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
                IIF(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary))   
		)AS ID,

		scd.SiteName AS LOCATION,
		CONVERT(varchar, scd.LocalProcessedDate, 101) AS MANIFEST_DATE, 
        MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) AS ENTRY_UNIT_NAME, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary)) as ENTRY_UNIT_KEY,
        MAX(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary)) AS ENTRY_UNIT_CSZ,  
		MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		MAX(scd.ContainerType) AS PRODUCT, 
		MAX(IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary)) AS CARRIER,  
		COUNT(scd.LastKnownEventDate) AS CONT_NO_SCAN, 
		COUNT(scd.ContainerId) AS TOTAL_CONT
	FROM dbo.ShippingContainerDatasets scd
	LEFT JOIN dbo.BinDatasets bd ON scd.BinCode = bd.BinCode AND scd.BinActiveGroupId = bd.ActiveGroupId 
	LEFT JOIN dbo.SubClientDatasets sbcd on scd.SiteName = sbcd.SiteName

	WHERE 
		sbcd.Name = @subClientName AND scd.Status = 'CLOSED'
		AND scd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND scd.ShippingCarrier = 'USPS' AND (@product IS NULL OR @product = scd.ContainerType)
	GROUP BY 
		scd.SiteName,
		CONVERT(varchar, scd.LocalProcessedDate, 101),
		scd.BinCode
		       
	
END

GO

CREATE OR ALTER   PROCEDURE [dbo].[getRptUspsCarrierDetail_detail]
(
    -- Add the parameters for the stored procedure here
	@subClientName VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE = @beginDate,
	@product AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON

	DECLARE @site VARCHAR(50);

    SELECT @site = (SELECT SiteName FROM SubClientDatasets WHERE SubClientDatasets.Name = @subClientName); 

	SELECT
			CONVERT(varchar, sbcd.Name) + 
				CONVERT(varchar, scd.CosmosCreateDate, 101) + 
				IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
				IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary)
			AS ID, 
		sbcd.Name AS LOCATION, 
		scd.CosmosCreateDate AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME,
		bd.DropShipSiteKeyPrimary AS ENTRY_UNIT_KEY,
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		scd.ContainerType AS PRODUCT, 
		bd.ShippingCarrierPrimary AS CARRIER, 
		scd.UpdatedBarcode AS TRACKING_NUMBER, 
		scd.ContainerId AS CONTAINER_LABEL, 
		scd.Weight AS WEIGHT, 
		t.EventDate AS LAST_KNOWN_DATE, 
		t.EventDescription AS LAST_KNOWN_DESC, 
		t.EventLocation AS LAST_KNOWN_LOCATION, 
		t.EventZip AS LAST_KNOWN_ZIP, 
		CASE WHEN e.IsStopTheClock = 1 THEN CAST(DATEDIFF(DAY, scd.CosmosCreateDate, t.EventDate) AS varchar) ELSE '' END AS NUM_DAYS
	FROM dbo.ShippingContainerDatasets scd 
	LEFT JOIN (SELECT ShippingContainerDatasetId, SiteName, EventDate, EventDescription, EventLocation, EventCode, EventZip, 
				ROW_NUMBER() OVER (PARTITION BY ShippingContainerDatasetId ORDER BY EventDate DESC) AS ROW_NUM
		FROM dbo.TrackPackageDatasets WHERE EventDate >= @beginDate and SiteName = @site and ShippingContainerDataSetId<>'') t 
			ON scd.Id = t.ShippingContainerDatasetId AND t .ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t .EventCode
	LEFT JOIN dbo.BinDatasets bd ON scd.BinCode = bd.BinCode AND scd.BinActiveGroupId = bd.ActiveGroupId
	LEFT JOIN dbo.SubClientDatasets sbcd on scd.SiteName = sbcd.SiteName

	WHERE sbcd.Name = @subClientName AND scd.Status = 'CLOSED'
		AND scd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND scd.ShippingCarrier = 'USPS' AND (@product IS NULL OR @product = scd.ContainerType)
		
END