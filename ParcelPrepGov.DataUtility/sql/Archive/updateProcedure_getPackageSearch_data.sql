/****** Object:  StoredProcedure [dbo].[getPackageSearch_data]    Script Date: 9/27/2021 9:44:21 AM ******/
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
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED,
		pd.RecallDate as DATE_RECALLED,
		pd.ReleaseDate as DATE_RELEASED,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, 'SHIPPED'), 
			IIF(e.Id IS NULL, t.EventDescription, e.Description)) AS LAST_KNOWN_DESC,
		IIF(t.EventDate IS NULL, pd.ShippedDate, t.EventDate) AS LAST_KNOWN_DATE,
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteCity + ', ' + pd.SiteState), t.EventLocation) AS LAST_KNOWN_LOCATION, 
		IIF(t.EventDate IS NULL, IIF(pd.ShippedDate IS NULL, null, pd.SiteZip), t.EventZip) AS LAST_KNOWN_ZIP,
		i.[InquiryId] AS INQUIRY_ID

	FROM dbo.PackageDatasets pd
	LEFT JOIN [dbo].[PackageInquiries] i ON i.[PackageDatasetId] = pd.Id
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
