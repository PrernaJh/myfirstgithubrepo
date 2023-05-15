/****** Object:  StoredProcedure [dbo].[getRptRegionalCarrierDetail_master]    Script Date: 11/22/2021 10:47:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getRptRegionalCarrierDetail_master]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE,

	@location AS VARCHAR(MAX) = null,
	@manifestDateString AS VARCHAR(MAX) = null,
	@carrier AS VARCHAR(MAX) = null,
	@product AS VARCHAR(MAX) = null
)
AS

BEGIN
    SET NOCOUNT ON

	SELECT        
		CONVERT(varchar, scd.SiteName) + 
			CONVERT(varchar, scd.CosmosCreateDate, 101) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
			IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary)
		AS ID, 
		scd.SiteName AS LOCATION, 
		CAST(CONVERT(varchar, scd.CosmosCreateDate, 101) AS DATETIME2) AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
		MAX(IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		MAX(scd.ContainerType) AS PRODUCT, 
		MAX(bd.ShippingCarrierPrimary) AS CARRIER, 
		COUNT(t .EventDate) AS CONT_NO_SCAN, 
		COUNT(scd.ContainerId) AS TOTAL_CONT
	FROM dbo.ShippingContainerDatasets scd
	LEFT JOIN (SELECT ShippingContainerDatasetId, EventDate, EventCode, SiteName, ROW_NUMBER() 
		OVER (PARTITION BY ShippingContainerDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets  WHERE EventDate >= @beginDate) t 
				ON scd.Id = t.ShippingContainerDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t .EventCode
	LEFT JOIN dbo.BinDatasets bd ON scd.BinCode = bd.BinCode AND scd.BinActiveGroupId = bd.ActiveGroupId 
	--LEFT JOIN dbo.TrackPackageDatasets tpd ON scd.Id = tpd.ShippingContainerDatasetId
	LEFT JOIN dbo.PackageDatasets pd ON scd.ContainerId = pd.ContainerId

	WHERE scd.Status = 'CLOSED'
		AND scd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
			AND (@location IS NULL OR scd.SiteName IN (SELECT * FROM [dbo].[SplitString](@location, '|')))
			AND (@carrier IS NULL OR scd.ShippingCarrier NOT IN ('USPS', 'USPS PMOD'))
			AND (@product IS NULL OR @product = scd.ContainerType)
			AND (@product IS NULL OR scd.ContainerType IN (SELECT * FROM [dbo].[SplitString](@product, '|')))
			AND (@manifestDateString IS NULL OR scd.CosmosCreateDate IN (SELECT * FROM [dbo].[SplitString](@manifestDateString, '|')))
	GROUP BY 
		scd.SiteName, 
		CONVERT(varchar, scd.CosmosCreateDate, 101), 
		bd.DropShipSiteDescriptionPrimary, 
		bd.DropShipSiteCszPrimary

END

GO
CREATE OR ALTER PROCEDURE [dbo].[getRptRegionalCarrierDetail_detail]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE = @beginDate,
	@product AS VARCHAR(MAX) = null
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
			CONVERT(varchar, scd.SiteName) + 
				CONVERT(varchar, scd.CosmosCreateDate, 101) + 
				IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) +
				IIF(bd.DropShipSiteCszPrimary IS NULL, 'null',  bd.DropShipSiteCszPrimary)
			AS ID, 
		scd.SiteName AS LOCATION, 
		scd.CosmosCreateDate AS MANIFEST_DATE, 
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
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
		FROM dbo.TrackPackageDatasets WHERE EventDate >= @beginDate) t 
			ON scd.Id = t.ShippingContainerDatasetId AND t .ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t .EventCode
	LEFT JOIN dbo.BinDatasets bd ON scd.BinCode = bd.BinCode AND scd.BinActiveGroupId = bd.ActiveGroupId 

	WHERE scd.SiteName = @site AND scd.Status = 'CLOSED'
		AND scd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)
		AND scd.ShippingCarrier NOT IN ('USPS', 'USPS PMOD')
		AND (@product IS NULL OR @product = scd.ContainerType)
END