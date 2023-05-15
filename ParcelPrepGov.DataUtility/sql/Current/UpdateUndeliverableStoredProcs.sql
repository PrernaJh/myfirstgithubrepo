/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_master]    Script Date: 4/18/2022 10:54:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[getRptUspsUndeliverables_master]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
SELECT
	CONVERT(varchar, s.Description) + 
			IIF(v.Visn IS NULL, 'null',  v.Visn) +
			IIF(v.SiteNumber IS NULL, 'null', v.SiteNumber) +
			IIF(v.SiteName IS NULL, 'null',  v.SiteName)  +
			IIF(u.EventDescription IS NULL, pd.LastKnownEventDescription, u.EventDescription)
		AS ID, 
	s.Description AS CUST_LOCATION, 
	v.Visn AS VISN,
	v.SiteNumber AS MEDICAL_CENTER_NO,
	v.SiteName AS MEDICAL_CENTER_NAME, 
	IIF(u.EventDescription IS NULL, pd.LastKnownEventDescription, u.EventDescription) AS EVENT_DESC,
	COUNT(pd.PackageId) AS TOTAL_PCS 
	FROM dbo.PackageDatasets pd
	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventDescription, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.UndeliverableEventDatasets WHERE EventDate >= @beginDate) u 
				ON u.PackageDatasetId = pd.Id AND u.ROW_NUM = 1 

	WHERE (pd.IsUndeliverable = 1 OR u.EventDate IS NOT NULL) 
		AND PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS'	
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

	GROUP BY 
		s.Description,
		v.Visn,
		v.SiteNumber,
		v.SiteName,
		IIF(u.EventDescription IS NULL, pd.LastKnownEventDescription, u.EventDescription)

END

GO

/****** Object:  StoredProcedure [dbo].[getRptUspsUndeliverables_detail]    Script Date: 4/18/2022 11:32:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[getRptUspsUndeliverables_detail]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
	@beginDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
	SELECT
		CONVERT(varchar, s.Description) + 
				IIF(v.Visn IS NULL, 'null',  v.Visn) +
				IIF(v.SiteNumber IS NULL, 'null', v.SiteNumber) +
				IIF(v.SiteName IS NULL, 'null',  v.SiteName)  +
				IIF(u.EventDescription IS NULL, 
					IIF(pd.LastKnownEventDescription IS NULL, 'null', pd.LastKnownEventDescription), 
					u.EventDescription)
			AS ID, 
		s.Description AS CUST_LOCATION, 
		v.Visn AS VISN,
		v.SiteNumber AS MEDICAL_CENTER_NO,
		v.SiteName AS MEDICAL_CENTER_NAME,
		pd.PackageId AS PACKAGE_ID,
		pd.Zip AS DESTINATION_ZIP,
		IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
		[dbo].[DropShippingCarrier](pd.ShippingMethod,bd.ShippingCarrierPrimary) AS CARRIER, 
		pd.ShippingCarrier AS PACKAGE_CARRIER,
		CAST(CONVERT(varchar, pd.LocalProcessedDate, 101) AS DATE) AS MANIFEST_DATE, 
		IIF(u.EventDescription IS NULL, 
			IIF(pd.LastKnownEventDescription IS NULL, 'null', pd.LastKnownEventDescription),
			u.EventDescription) AS UNDELIVERABLE_DESC,
		IIF(u.EventDescription IS NULL, 
			CAST(CONVERT(varchar, pd.LastKnownEventDate, 101) AS DATE),
			CAST(CONVERT(varchar, u.EventDate, 101) AS DATE)) AS UNDELIVERABLE_DATE_TIME,
		pd.LastKnownEventDescription AS LAST_KNOWN_EVENT_DESC,
		CAST(CONVERT(varchar, pd.LastKnownEventDate, 101) AS DATE) AS LAST_KNOWN_EVENT_DATE_TIME
	FROM dbo.PackageDatasets pd

	LEFT JOIN (SELECT siteparent, MIN(visn) AS visn, MIN(sitenumber) AS sitenumber, MIN(sitename) 
		AS sitename FROM dbo.VisnSites GROUP BY siteparent) v ON v.SiteParent = pd.VisnSiteParent
	LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 
	LEFT JOIN dbo.SubClientDatasets s ON pd.SubClientName = s.[Name]
	LEFT JOIN (SELECT PackageDatasetId, EventDate, EventDescription, ROW_NUMBER() 
		OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
			FROM dbo.UndeliverableEventDatasets WHERE EventDate >= @beginDate) u 
				ON u.PackageDatasetId = pd.Id AND u.ROW_NUM = 1 

	WHERE (pd.IsUndeliverable = 1 OR u.EventDescription IS NOT NULL)
		AND pd.PackageStatus = 'PROCESSED' AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
		AND pd.ShippingCarrier = 'USPS' 
		AND pd.LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)

END

GO
