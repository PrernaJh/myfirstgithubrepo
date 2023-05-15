/****** Object:  StoredProcedure [dbo].[getRptUspsCarrierDetail_detail]    Script Date: 4/22/2022 8:20:28 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[getRptUspsCarrierDetail_detail]
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
			CONVERT(varchar, scd.SiteName) + 
			CONVERT(varchar, scd.LocalProcessedDate, 101) + 
				IIF(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) IS NULL, 'null',  IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary)) +
                IIF(IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) IS NULL, 'null',  IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary))  
			AS ID, 
		scd.SiteName AS LOCATION,
		CONVERT(varchar, scd.LocalProcessedDate, 101) AS MANIFEST_DATE, 
        IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
		IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteKeyPrimary,  bd.DropShipSiteKeySecondary) as ENTRY_UNIT_KEY,
        IIF(scd.IsSecondaryCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ,  
		IIF(LEFT(scd.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
		scd.ContainerType AS PRODUCT, 
		IIF(scd.IsSecondaryCarrier = 0, bd.ShippingCarrierPrimary,  bd.ShippingCarrierSecondary) AS CARRIER, 
		scd.UpdatedBarcode AS TRACKING_NUMBER, 
		scd.ContainerId AS CONTAINER_LABEL, 
		scd.Weight AS WEIGHT, 
		scd.LastKnownEventDate AS LAST_KNOWN_DATE,
		scd.LastKnownEventDescription AS LAST_KNOWN_DESC, 
		scd.LastKnownEventLocation AS LAST_KNOWN_LOCATION, 
		scd.LastKnownEventZip AS LAST_KNOWN_ZIP, 
		case when scd.StopTheClockEventDate is null THEN '' ELSE CAST(DATEDIFF(DAY, scd.LocalProcessedDate, scd.StopTheClockEventDate) AS varchar) END AS NUM_DAYS
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