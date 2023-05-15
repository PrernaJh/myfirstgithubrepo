CREATE INDEX [NCL_IDX_PackageDatasets_SubClientName_LocalProcesedDate] ON [PackageDatasets] ([SubClientName], [LocalProcesedDate]);

GO

CREATE INDEX [NCL_IDX_PackageDatasets_SubClientName_RecallDate] ON [PackageDatasets] ([SubClientName], [RecallDate]);

GO

CREATE INDEX [NCL_IDX_PackageDatasets_SubClientName_ReleaseDate] ON [PackageDatasets] ([SubClientName], [ReleaseDate]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210916161235_AddReleaseDateIndex', N'3.1.6');

GO



/****** Object:  StoredProcedure [dbo].[getPackageSearch_data]    Script Date: 9/14/2021 11:33:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[getPackageSearch_data]
(
    @ids AS VARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		pd.Id as ID,
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageStatus AS PACKAGE_STATUS, 
		pd.RecallStatus AS RECALL_STATUS, 
		s.Description as CUST_LOCATION,
		pd.ShippingMethod AS PRODUCT, 
		pd.Zip as DEST_ZIP,
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED,
		pd.RecallDate as DATE_RECALLED,
		pd.ReleaseDate as DATE_RELEASED,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
			OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM 
				FROM dbo.TrackPackageDatasets
							LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
							LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
						WHERE EventDate >= p.LocalProcessedDate AND
							(p.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
								OR p.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
								OR p.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ',')))) t 
					ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC

END

/****** Object:  StoredProcedure [dbo].[getPackageSearch_master]    Script Date: 9/16/2021 8:14:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
ALTER PROCEDURE [dbo].[getPackageSearch_master]
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
	  ,[RecallStatus]
	  ,[ReleaseDate]
--	  ,i.[InquiryId]
	FROM [dbo].[PackageDatasets] pd
--		LEFT JOIN [dbo].[PackageInquiries] i ON i.[FK_PackageDatasetId] = pd.Id
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))

	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC
END