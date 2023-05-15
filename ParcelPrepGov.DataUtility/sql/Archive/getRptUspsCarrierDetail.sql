/****** Object:  StoredProcedure [dbo].[getRptUspsCarrierDetail]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Jeff Johnson
-- Create Date: 7-20-2021
-- Description: PBI - 967 Update stored proc to only return containers from carrier = USPS
-- Added added a check in the where condition to check if dbo.ShippingContainerDatasets.ShippingCarrier = 'USPS'
-- =============================================

CREATE PROCEDURE [dbo].[getRptUspsCarrierDetail]
(
    -- Add the parameters for the stored procedure here
@localProcessDate date
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
    SELECT dbo.ShippingContainerDatasets.SiteName AS LOCATION, 
    dbo.ShippingContainerDatasets.CosmosCreateDate AS MANIFEST_DATE, 
    dbo.BinDatasets.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
    dbo.BinDatasets.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
    IIF(LEFT(dbo.ShippingContainerDatasets.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
    dbo.ShippingContainerDatasets.ContainerType AS PRODUCT, 
    dbo.BinDatasets.ShippingCarrierPrimary AS CARRIER,
    dbo.ShippingContainerDatasets.ContainerId AS TRACKING_NUMBER, 
    dbo.ShippingContainerDatasets.UpdatedBarcode AS CONTAINER_LABEL, 
    dbo.ShippingContainerDatasets.Weight AS WEIGHT, 
    t.EventDate AS LAST_KNOWN_DATE, 
           t.EventDescription AS LAST_KNOW_DESC, 
           t.EventLocation AS LAST_KNOW_LOCATION, 
           t.EventZip as LAST_KNOWN_ZIP,
           CASE WHEN e.IsStopTheClock = 1 THEN CAST(DATEDIFF(DAY, dbo.ShippingContainerDatasets.CosmosCreateDate, t.EventDate) AS varchar) ELSE '' END AS NUM_DAYS

	       FROM  dbo.ShippingContainerDatasets 
           LEFT JOIN dbo.BinDatasets ON dbo.ShippingContainerDatasets.BinCode = dbo.BinDatasets.BinCode AND dbo.ShippingContainerDatasets.BinActiveGroupId = dbo.BinDatasets.ActiveGroupId 
           LEFT JOIN (	 SELECT ShippingContainerId,EventDate,EventDescription,EventLocation,EventCode, EventZip, ROW_NUMBER() OVER(PARTITION BY ShippingContainerId ORDER BY EventDate DESC) AS ROW_NUM
                         FROM dbo.TrackPackageDatasets) t ON dbo.ShippingContainerDatasets.ContainerId = t.ShippingContainerId AND t.ROW_NUM=1
	       LEFT JOIN dbo.EvsCodes e on e.Code = t.EventCode	
	       WHERE CAST(dbo.ShippingContainerDatasets.CosmosCreateDate AS DATE) = @localProcessDate
	       AND dbo.ShippingContainerDatasets.ShippingCarrier = 'USPS'
	       ORDER BY dbo.ShippingContainerDatasets.ContainerId,t.EventDate
END
GO
