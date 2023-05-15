/****** Object:  StoredProcedure [dbo].[getPackageSearch_master]    Script Date: 11/10/2022 10:34:36 AM ******/
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
      ,[MarkupType]
      ,[IsPoBox]
      ,[IsRural]
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