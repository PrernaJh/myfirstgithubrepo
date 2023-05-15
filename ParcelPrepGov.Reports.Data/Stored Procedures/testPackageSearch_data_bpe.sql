/****** Object:  StoredProcedure [dbo].[testPackageSearch_data_bpe]    Script Date: 11/17/2021 2:22:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[testPackageSearch_data_bpe]
(
    @ids AS VARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT
		pd.PackageId AS PACKAGE_ID, 
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER, 
		bd.ShippingCarrierPrimary AS CARRIER, 
		pd.PackageStatus AS PACKAGE_STATUS, 
		s.Description as CUST_LOCATION,
		pd.ShippingMethod AS PRODUCT, 
		pd.Zip as DEST_ZIP,
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		t.EventDescription AS LAST_KNOWN_DESC, 
		t.EventDate AS LAST_KNOWN_DATE,
		t.EventLocation AS LAST_KNOWN_LOCATION, 
		t.EventZip AS LAST_KNOWN_ZIP 

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventCode, EventDescription, EventLocation, EventZip, p.SiteName, ec.IsStopTheClock, ROW_NUMBER() 
			OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC, ec.IsStopTheClock DESC) AS ROW_NUM 
				FROM dbo.TrackPackageDatasets
							LEFT JOIN dbo.EvsCodes ec ON ec.Code = EventCode
							LEFT JOIN dbo.PackageDatasets AS p ON dbo.TrackPackageDatasets.PackageDatasetId = p.Id
						WHERE EventDate >= p.LocalProcessedDate AND (EventDate <= p.StopTheClockEventDate OR p.StopTheClockEventDate IS NULL) and (p.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR p.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR p.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ',')))) t 
					ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	
	WHERE pd.PackageId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
		OR pd.ShippingBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
				OR pd.HumanReadableBarcode IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY pd.LocalProcessedDate DESC, pd.CosmosCreateDate DESC

END
GO
