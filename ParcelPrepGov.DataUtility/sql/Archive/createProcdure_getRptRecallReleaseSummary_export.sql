/****** Object:  StoredProcedure [dbo].[getRptRecallReleaseSummary_master]    Script Date: 9/10/2021 12:34:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[getRptRecallReleaseSummary_export]
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

    select R.Status as 'PackageStatus',
	--COUNT(A.PackageId) as Num_Packages,
	A.PackageId,
	R.Description as 'RecallStatus', 
	A.RecallDate,
	A.LocalProcessedDate,
	A.AddressLine1,
	A.AddressLine2,
	A.AddressLine3,
	A.City,
	A.State,
	A.Zip,
	A.SiteName,
	A.ClientName,
	A.SubClientName
from RecallStatuses R
OUTER APPLY
(
select   *
from PackageDatasets p 
where p.SubClientName = @subClientName
 and p.RecallDate BETWEEN @startDate AND @endDate 
  and p.RecallStatus = R.Status 
  ) A
  GROUP BY R.Description, R.Status, A.PackageId, A.RecallDate, A.LocalProcessedDate,
  A.City,
	A.State,
	A.Zip,
	A.SiteName,
	A.ClientName,
	A.SubClientName,
	A.AddressLine1,
	A.AddressLine2,
	A.AddressLine3
  HAVING COUNT(a.PackageId) > 0
	--ORDER BY COUNT([pd].[PackageId])

END

 
