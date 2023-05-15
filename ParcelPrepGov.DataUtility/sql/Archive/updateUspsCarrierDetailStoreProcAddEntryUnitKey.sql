CREATE OR ALTER   PROCEDURE [dbo].[getRptUspsCarrierDetail_master]
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
		CONVERT(varchar, sbcd.Name) + 
			CONVERT(varchar, scd.CosmosCreateDate, 101) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
			IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary)
		AS ID,
		sbcd.Name AS LOCATION,
		CAST(CONVERT(varchar, scd.CosmosCreateDate, 101) AS DATETIME2) AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteKeyPrimary AS ENTRY_UNIT_KEY,
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		MAX(scd.ContainerType) AS PRODUCT, 
		MAX(bd.ShippingCarrierPrimary) AS CARRIER, 
		COUNT(t.EventDate) AS CONT_NO_SCAN, 
		COUNT(scd.ContainerId) AS TOTAL_CONT
	FROM dbo.ShippingContainerDatasets scd
	LEFT JOIN (SELECT ShippingContainerDatasetId, EventDate, EventCode, SiteName, ROW_NUMBER() 
		OVER (PARTITION BY ShippingContainerDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets  WHERE EventDate >= @beginDate and SiteName = @site and ShippingContainerDataSetId<>'') t 
				ON scd.Id = t.ShippingContainerDatasetId AND t.ROW_NUM = 1 
	--LEFT JOIN dbo.EvsCodes e ON  t.EventCode = e.Code 
	LEFT JOIN dbo.BinDatasets bd ON scd.BinCode = bd.BinCode AND scd.BinActiveGroupId = bd.ActiveGroupId 
	LEFT JOIN dbo.SubClientDatasets sbcd on scd.SiteName = sbcd.SiteName

	WHERE 
		sbcd.Name = @subClientName AND scd.Status = 'CLOSED'
		AND scd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND scd.ShippingCarrier = 'USPS' AND (@product IS NULL OR @product = scd.ContainerType)
	GROUP BY 
		sbcd.Name,
		bd.DropShipSiteKeyPrimary,
		CONVERT(varchar, scd.CosmosCreateDate, 101), 
		bd.DropShipSiteDescriptionPrimary, 
		bd.DropShipSiteCszPrimary
	
END