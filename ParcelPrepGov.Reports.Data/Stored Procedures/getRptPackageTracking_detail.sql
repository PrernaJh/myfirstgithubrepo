/****** Object:  StoredProcedure [dbo].[getRptPackageTracking_detail]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptPackageTracking_detail]
(
    -- Add the parameters for the stored procedure here
	@Id as INT,  -- package dataset id
    @LocalProcessedDate as DATE,
	@CosmosId as VARCHAR(MAX)
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT 
		'1' as rptindex
			,[PackageId]
			,[ShippingCarrier]
			,[TrackingNumber]
			,[EventDate]
			,[EventDescription]
			,[EventLocation]
			,[ShippingContainerId]
			,[ShippingContainerDatasetId]
			,[EventCode]
			,[EventZip]
			,[Id]
			,[CosmosId]
			,[SiteName]
			,[DatasetCreateDate]
			,[DatasetModifiedDate]
			,[CosmosCreateDate]
			,[PackageDatasetId]
	 FROM [dbo].[TrackPackageDatasets]
		WHERE PackageDatasetId = @Id AND EventDate >= @LocalProcessedDate
			AND NOT EventDescription LIKE '%DUPLICATE%'
	 UNION 
		SELECT
			'0' as rptindex
				,[PackageId]
				,'FSC INTERNAL' as ShippingCarrier
				,'' as TrackingNumber
				,[LocalEventDate] as EventDate
				,[EventStatus]+': '+[Description]	as EventDescription
				,[SiteName]+': '+[MachineId] as EventLocation
				,'' as ShippingContainerId
				,'' as ShippingContainerDatasetId
				,[EventType] as "EventCode"
				,'' as EventZip
				,[Id]
				,[CosmosId]
				,[SiteName]
				,[DatasetCreateDate]
				,[DatasetModifiedDate]
				,[CosmosCreateDate]
				,[PackageDatasetId]
		  FROM [dbo].[PackageEventDatasets]
			  WHERE CosmosId = @CosmosId AND NOT EventType IN ( 'EODPROCESSED', 'RATEASSIGNED' )
		  
	ORDER BY rptindex DESC, EventDate DESC

END
GO
