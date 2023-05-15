/****** Object:  StoredProcedure [dbo].[getRptRecallReleaseSummary_master]    Script Date: 12/21/2021 9:02:25 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[getRptRecallReleaseSummary_master]
(
    -- Add the parameters for the stored procedure here
	@subClientName AS VARCHAR(MAX), 
	@startDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN

SET NOCOUNT ON

SELECT s.Status AS 'PackageStatus', COUNT(a.PackageId) AS Num_Packages, s.Status
FROM RecallStatuses s
OUTER APPLY (
SELECT DISTINCT	
		p.PackageId, 
		p.SiteName, 
		p.ClientName, 
		p.SubClientName,
		p.PackageStatus,
		IIF(p.LastKnownEventDescription IS NULL, p.PackageStatus, p.LastKnownEventDescription) AS 'PackageStatusDescription',
		p.RecallStatus,
		p.DatasetCreateDate AS 'RecordCreateDate',
		(SELECT TOP 1 pd.EventDate FROM dbo.PackageEventDatasets AS pd WHERE p.Id = pd.PackageDatasetId AND pd.EventStatus = 'RECALLED' ORDER BY pd.EventDate ASC) AS 'RecallDate',
		p.ReleaseDate,
		p.LocalProcessedDate,
		p.ContainerId,
		p.BinCode,
		p.ShippingCarrier,
		p.[AddressLine1],
		p.[AddressLine2],
		p.[AddressLine3],
		p.City,
		p.[State],
		p.Zip
  FROM [dbo].[PackageDatasets] p
	WHERE p.SubClientName = @subClientName
	AND p.RecallStatus = s.Status
	AND (p.RecallStatus = 'RELEASED' OR RecallDate IS NOT NULL)
	AND p.PackageStatus <> 'BLOCKED' AND p.PackageStatus<>'REPLACED'
	AND p.DatasetCreateDate BETWEEN @startDate AND @endDate
		) a
GROUP BY s.Description, s.Status

END