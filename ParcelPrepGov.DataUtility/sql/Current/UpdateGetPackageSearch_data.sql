/****** Object:  StoredProcedure [dbo].[getPackageSearch_data]    Script Date: 4/27/2022 3:27:47 PM ******/
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
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		pd.PackageStatus AS PACKAGE_STATUS, 
		pd.RecallStatus AS RECALL_STATUS, 
		s.Description as CUST_LOCATION,
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
		i.[InquiryId] AS INQUIRY_ID
		
	

	FROM dbo.PackageDatasets pd
	LEFT JOIN [dbo].[PackageInquiries] i ON i.[PackageDatasetId] = pd.Id
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC

END
