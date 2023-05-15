/****** Object:  StoredProcedure [dbo].[getRptRecallReleaseSummary_export]    Script Date: 12/21/2021 9:01:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[getRptRecallReleaseSummary_export]
(
    -- Add the parameters for the stored procedure here
	@subClientName AS VARCHAR(MAX), 
	@startDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

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
	AND (p.RecallStatus = 'RELEASED' OR RecallDate IS NOT NULL)
	AND p.PackageStatus <> 'BLOCKED' 
	AND p.RecallDate BETWEEN @startDate AND @endDate    

END

 
