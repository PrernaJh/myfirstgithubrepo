ALTER TABLE [PackageDatasets] ADD [IsStopTheClock] int NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220706165944_AddIsStopTheClock', N'3.1.6');

GO

/****** Object:  StoredProcedure [dbo].[getPackageDataForArchive]    Script Date: 7/6/2022 11:54:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     PROCEDURE [dbo].[getPackageDataForArchive]
(
    @subClient AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT p.PackageId
	  ,p.SiteName
      ,p.ClientName
      ,p.ClientFacilityName
      ,p.SubClientName

      ,p.MailCode
      ,p.PackageStatus
      ,p.LocalProcessedDate
      ,p.ShippingBarcode 
	  ,p.HumanReadableBarcode
      ,p.ShippingCarrier
      ,p.ShippingMethod
      ,p.ServiceLevel

      ,p.Zone
      ,p.Weight
      ,p.Length
      ,p.Width
      ,p.Depth
      ,p.TotalDimensions
      ,p.MailerId
      ,p.Cost
      ,p.Charge
      ,p.ExtraCost
      ,p.BillingWeight
      ,p.IsPoBox
      ,p.IsUpsDas
      ,p.IsOutside48States
      ,p.IsOrmd

      ,p.RecipientName
      ,p.AddressLine1
      ,p.AddressLine2
      ,p.AddressLine3
      ,p.City
      ,p.State
      ,p.Zip
      ,p.FullZip
      ,p.Phone

      ,p.CosmosCreateDate
      ,p.StopTheClockEventDate
      ,p.ShippedDate
      ,p.RecallStatus
      ,p.RecallDate
      ,p.ReleaseDate
      ,p.LastKnownEventDate
      ,p.LastKnownEventDescription
      ,p.LastKnownEventLocation
      ,p.LastKnownEventZip
      ,p.CalendarDays
      ,p.PostalDays
	  ,p.IsStopTheClock
      ,p.IsUndeliverable

      ,p.BinCode
      ,p.ContainerId
      ,c.ContainerType
      ,p.IsSecondaryContainerCarrier
      ,c.UpdatedBarcode as ContainerBarcode
      ,c.ShippingCarrier as ContainerShippingCarrier
      ,c.ShippingMethod as ContainerShippingMethod
      ,c.LastKnownEventDate as ContainerLastKnownEventDate
      ,c.LastKnownEventDescription as ContainerLastKnownEventDescription
      ,c.LastKnownEventLocation as ContainerLastKnownEventLocation
      ,c.LastKnownEventZip as ContainerLastKnownEventZip

	FROM [dbo].[PackageDatasets] p
		LEFT JOIN ShippingContainerDatasets c on c.ContainerId = p.ContainerId
		WHERE p.PackageStatus = 'PROCESSED'
			AND p.SubClientName = @subClient
			AND p.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
	ORDER BY p.PackageId

END
GO

/****** Object:  StoredProcedure [dbo].[getPackageSearch_master]    Script Date: 7/8/2022 10:34:36 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[getPackageSearch_master]
(
    @ids AS VARCHAR(MAX)
)
AS
BEGIN
        -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT 
	pd.[Id]
      ,[CosmosId]
      ,[SiteName]
      ,[PackageId]
      ,[ClientName]
      ,[SubClientName]
      ,[MailCode]
      ,[PackageStatus]
      ,[ProcessedDate]
      ,[LocalProcessedDate]
      ,[SiteId]
      ,[SiteZip]
      ,[SiteAddressLineOne]
      ,[SiteCity]
      ,[SiteState]
      ,[JobId]
      ,[ContainerId]
      ,[BinCode]
      ,IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS ShippingBarcode
      ,[ShippingCarrier]
      ,[ShippingMethod]
      ,[ServiceLevel]
      ,[Zone]
      ,[Weight]
      ,[Length]
      ,[Width]
      ,[Depth]
      ,[TotalDimensions]
      ,[Shape]
      ,[RequestCode]
      ,[DropSiteKeyValue]
      ,[MailerId]
      ,[Cost]
      ,[Charge]
      ,[BillingWeight]
      ,[ExtraCost]
      ,[IsPoBox]
      ,[IsUpsDas]
      ,[IsOutside48States]
      ,[IsOrmd]
      ,[IsDuplicate]
      ,[IsSaturday]
      ,[IsDduScfBin]
      ,[IsSecondaryContainerCarrier]
      ,[IsQCRequired]
	  ,[IsStopTheClock]
	  ,[IsUndeliverable]
      ,[AsnImportWebJobId]
      ,[BinGroupId]
      ,[BinMapGroupId]
      ,[RateId]
      ,[ServiceRuleId]
      ,[ServiceRuleGroupId]
      ,[ZoneMapGroupId]
      ,[FortyEightStatesGroupId]
      ,[UpsGeoDescriptorGroupId]
      ,[RecipientName]
      ,[AddressLine1]
      ,[AddressLine2]
      ,[AddressLine3]
      ,[City]
      ,[State]
      ,[Zip]
      ,[FullZip]
      ,[Phone]
      ,[ReturnName]
      ,[ReturnAddressLine1]
      ,[ReturnAddressLine2]
      ,[ReturnCity]
      ,[ReturnState]
      ,[ReturnZip]
      ,[ReturnPhone]
      ,[ZipOverrides]
      ,[ZipOverrideGroupIds]
      ,[DuplicatePackageIds]
      ,[DatasetCreateDate]
      ,[CosmosCreateDate]
      ,[DatasetModifiedDate]
      ,[HumanReadableBarcode]
      ,[StopTheClockEventDate]	
	  ,[RecallDate]
	  ,[LastKnownEventDate]
	  ,[LastKnownEventDescription]
	  ,[LastKnownEventLocation]
	  ,[LastKnownEventZip]
	FROM [dbo].[PackageDatasets] pd
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))

	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC
END
GO