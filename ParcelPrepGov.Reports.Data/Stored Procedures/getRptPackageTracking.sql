/****** Object:  StoredProcedure [dbo].[getRptPackageTracking]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptPackageTracking]
(
    -- Add the parameters for the stored procedure here
	@IdType as int,  -- 0 = packageid, 1= shipping barcode
    @pkginfo nvarchar(50)
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

IF @IdType=0
BEGIN
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
		,[CosmosCreateDate]
,0 AS PackageDatasetId
  FROM [dbo].[TrackPackageDatasets]
  where PackageId =  @pkginfo
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

	  ,Id
,CosmosId
,SiteName
,DatasetCreateDate
,CosmosCreateDate
,0 AS PackageDatasetId
  FROM [dbo].[PackageEventDatasets]
 where PackageId =  @pkginfo
  order by rptindex desc, EventDate desc
END
ELSE
BEGIN

Select 
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
		,[CosmosCreateDate]
		,0 AS PackageDatasetId
  FROM [dbo].[TrackPackageDatasets]
  where TrackingNumber = @pkginfo
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

	  ,Id
		,CosmosId
		,SiteName
		,DatasetCreateDate
		,CosmosCreateDate
		,0 AS PackageDatasetId
  FROM [dbo].[PackageEventDatasets]
  where PackageId = @pkginfo
  order by rptindex desc, EventDate desc
END



END
GO
