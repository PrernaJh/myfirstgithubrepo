/****** Object:  StoredProcedure [dbo].[getContainerSearchEventsByContainerId]    Script Date: 4/29/2022 10:54:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	exec [getContainerSearchEventsByContainerId] '99M901958890000101105', 'DALLAS'
*/
CREATE OR ALTER PROCEDURE [dbo].[getContainerSearchEventsByContainerId]
    @containerId VARCHAR(50),
	@siteName VARCHAR(50)
AS
BEGIN
	-- get external events first and then append internal events
	DECLARE @id INT = (SELECT TOP 1 id FROM ShippingContainerDatasets WHERE ContainerId = @containerId ORDER BY CosmosCreateDate DESC)

	-- get external events first and then append internal events
	SELECT LocalEventDate AS [LOCAL_EVENT_DATE],
	sc.UpdatedBarcode AS [TRACKING_NUMBER],	
	EventType AS [EVENT_TYPE],
	EventStatus AS [EVENT_STATUS],
	scd.Username AS [USER_NAME],
	ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'') as DISPLAY_NAME,  
	scd.MachineId AS [MACHINE_ID],
	sc.ShippingCarrier AS [SHIPPING_CARRIER]
	FROM ShippingContainerEventDatasets scd 
		INNER JOIN ShippingContainerDatasets sc ON sc.Id = scd.ShippingContainerDatasetId
		LEFT JOIN UserLookups u ON u.username = scd.username
	WHERE sc.Id = @id
		AND sc.sitename = @siteName	
	UNION
		SELECT t.EventDate AS [LOCAL_EVENT_DATE], 
	t.TrackingNumber AS [TRACKING_NUMBER],	
	t.EventCode AS [EVENT_TYPE],
	t.EventDescription AS [EVENT_STATUS],
	'' AS [USER_NAME],
	'' AS [DISPLAY_NAME],
	t.SiteName AS [MACHINE_ID],
	t.ShippingCarrier AS [SHIPPING_CARRIER]
	FROM [dbo].[TrackPackageDatasets] t
		INNER JOIN dbo.ShippingContainerDatasets AS p ON t.ShippingContainerDatasetId = p.Id	
	WHERE p.Id = @id	
		AND p.siteName = @siteName
	ORDER BY [LOCAL_EVENT_DATE] DESC
END