/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_gtr6]    Script Date: 2/17/2022 10:39:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[getRptPostalPerformanceSummary_gtr6]
(
    -- Add the parameters for the stored procedure here
    @site AS VARCHAR(50),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT        
		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary) AS ID, 

		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary)+
			CONVERT(VARCHAR, left(pd.Zip,3)) AS ID3, 

		CONVERT(VARCHAR, pd.SubClientName) + 
			CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + 
			CONVERT(VARCHAR, p.PostalArea) + 
			CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary)+
			CONVERT(VARCHAR, left(pd.Zip,5)) AS ID5, 


		CAST(CONVERT(VARCHAR, pd.LocalProcessedDate, 101) AS DATE) AS DATE_SHIPPED, 
		s.Description AS CUST_LOCATION,
		pd.PackageId AS PACKAGE_ID, 
		pd.ShippingBarcode AS TRACKING_NUMBER, 
		pd.City AS DEST_CITY,
		pd.State AS DEST_STATE,
		pd.Zip AS DEST_ZIP,
		pd.ShippingMethod AS PRODUCT, 
		bd.ShippingCarrierPrimary AS CARRIER, 
		p.PostalArea AS USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		bd.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ,
		t.EventDescription AS LAST_KNOWN_DESC, 
		t.EventDate AS LAST_KNOWN_DATE,
		t.EventLocation AS LAST_KNOWN_LOCATION, 
		t.EventZip AS LAST_KNOWN_ZIP, 
		v.Visn AS VISN,
		v.SiteNumber AS PHARM_DIV_NO,
		v.SiteName AS PHARM_DIV

	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT PackageId, PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.TrackPackageDatasets
				JOIN dbo.EvsCodes e ON e.Code = EventCode
					WHERE e.IsStopTheClock = 1) t ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
	LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName

	WHERE CAST(DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate) AS DECIMAL) >= 6 AND pd.PackageStatus = 'PROCESSED' AND pd.SiteName = @site 
		AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

END