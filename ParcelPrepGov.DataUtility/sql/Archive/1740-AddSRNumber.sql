IF NOT EXISTS(SELECT object_id FROM sys.columns 
          WHERE Name = N'ServiceRequestNumber'
          AND Object_ID = Object_ID(N'dbo.PackageInquiries'))
BEGIN
    -- Column Exists
    ALTER TABLE [PackageInquiries] ADD [ServiceRequestNumber] nvarchar(50) NULL;
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220510141119_AddServiceRequestNumberToPackageInquiries', N'3.1.6');
END

GO

CREATE OR ALTER PROCEDURE [dbo].[upsertPackageInquiry]
(
    -- Add the parameters for the stored procedure here
	@inquiryId INT,
    @packageId VARCHAR(50),
	@siteName VARCHAR(24),
	@packageDatasetId INT,
	@serviceRequestNumber varchar(50)
)
AS
BEGIN

	MERGE PackageInquiries tgt
	USING (SELECT @inquiryId, 
		@packageDatasetId,
		@packageId,
		@siteName,
		@serviceRequestNumber) AS src 
		(InquiryId,
		PackageDatasetId,
		PackageId,
		SiteName,
		ServiceRequestNumber)
	ON (tgt.InquiryId = src.inquiryId)
	WHEN MATCHED THEN
		UPDATE SET InquiryId = src.inquiryId,
			PackageDatasetId = @packageDatasetId,
			PackageId = @packageId,
			SiteName = @SiteName,
			ServiceRequestNumber = @serviceRequestNumber
	WHEN NOT MATCHED THEN
		INSERT (InquiryId, PackageDatasetId, PackageId, SiteName, ServiceRequestNumber)
		VALUES (src.inquiryId, src.packageDatasetId, src.packageId, src.siteName, @serviceRequestNumber);
END

GO 

/****** Object:  StoredProcedure [dbo].[getPackageSearch_data]    Script Date: 5/16/2022 2:33:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[getPackageSearch_data]
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
		IIF(pd.ShippingCarrier = 'USPS',
			[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary),
			pd.ShippingCarrier)
			AS CARRIER, 
		pd.ShippingCarrier AS SHIPPING_CARRIER,
		pd.PackageStatus AS PACKAGE_STATUS, 
		pd.RecallStatus AS RECALL_STATUS, 
		s.Description as CUST_LOCATION,
		s.SiteName,
		pd.ShippingMethod AS PRODUCT, 
		pd.Zip as DEST_ZIP,

		FORMAT (pd.LocalProcessedDate, 'MM/dd/yyyy') AS DATE_SHIPPED,
		pd.RecallDate as DATE_RECALLED,
		pd.ReleaseDate as DATE_RELEASED,
		IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteDescriptionPrimary,  bd.DropShipSiteDescriptionSecondary) AS ENTRY_UNIT_NAME, 
        IIF(pd.IsSecondaryContainerCarrier = 0, bd.DropShipSiteCszPrimary,  bd.DropShipSiteCszSecondary) AS ENTRY_UNIT_CSZ, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
		IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP,
		i.[InquiryId] AS INQUIRY_ID,
		i.ServiceRequestNumber AS SERVICE_REQUEST_NUMBER			
	FROM dbo.PackageDatasets pd
	LEFT JOIN [dbo].[PackageInquiries] i ON i.[PackageDatasetId] = pd.Id
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC

END





GO

/****** Object:  StoredProcedure [dbo].[getPackageSearch_master]    Script Date: 5/9/2022 4:48:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[getPackageSearch_master]
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