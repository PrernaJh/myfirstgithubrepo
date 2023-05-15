/*
	exec [getContainerSearchPackages] '99M901958890000101105', 'DALLAS'
*/
CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchPackages]
    @containerId varchar(50),
	@siteName varchar(50)
AS    
  SELECT 
  pd.Id,
  pd.PackageId AS [PACKAGE_ID],
  IIF(pd.HumanReadableBarcode IS NULL, pd.ShippingBarcode, pd.HumanReadableBarcode) AS TRACKING_NUMBER,
  pd.ShippingCarrier AS CARRIER, 
  pd.ShippingMethod AS SHIPPING_METHOD,    
  pd.PackageStatus AS [PACKAGE_STATUS],
  pd.RecallDate AS [RECALL_DATE],
  pd.RecallStatus AS [RECALL_STATUS],
  IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) AS LAST_KNOWN_DATE,
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, 'SHIPPED'), pd.LastKnownEventDescription) AS LAST_KNOWN_DESC,
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, pd.SiteCity + ', ' + pd.SiteState), pd.LastKnownEventLocation) AS LAST_KNOWN_LOCATION, 
  IIF(pd.LastKnownEventDate IS NULL, IIF(pd.ShippedDate IS NULL, NULL, pd.SiteZip), pd.LastKnownEventZip) AS LAST_KNOWN_ZIP
  FROM PackageDatasets pd 
  WHERE pd.SiteName = @siteName	
	AND pd.ContainerId = @containerId
	AND pd.PackageStatus = 'PROCESSED'
	ORDER BY   IIF(pd.LastKnownEventDate IS NULL, pd.ShippedDate, pd.LastKnownEventDate) DESC	