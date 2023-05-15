/****** Object:  StoredProcedure [dbo].[getRptRecallReleaseSummary_master]    Script Date: 9/8/2021 2:30:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[PackageDatasets] ADD [RecallStatus] varchar(24) NULL;
GO
CREATE TABLE  [dbo].[RecallStatuses] (
    [Id] int NOT NULL IDENTITY,
    [CreateDate] datetime2 NOT NULL,
    [Status] varchar(24) NULL,
    [Description] varchar(80) NULL,
    CONSTRAINT [PK_RecallStatuses] PRIMARY KEY ([Id])
);
GO
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','CREATED','ENTERED – NO ASN RECORD');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','IMPORTED','ENTERED - PACKAGE NOT PROCESSED');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','PROCESSED','ENTERED - PACKAGE PROCESSED');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','SCANNED','RECALL FOUND');
INSERT INTO RecallStatuses(CreateDate,Status,Description) VALUES ('2021-09-03 11:32:00.0000000','RELEASED','RECALL RELEASE');
GO
INSERT INTO  [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210903154138_AddRecallStatus', N'3.1.6');
GO
    




CREATE PROCEDURE [dbo].[getRptRecallReleaseSummary_master]
(
    -- Add the parameters for the stored procedure here
	@subClientName AS VARCHAR(MAX),
	@status AS VARCHAR(15),
	@startDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    select R.Description as 'PackageStatus',
	COUNT(R.Description) -1 as Num_Packages,
	R.Status
from RecallStatuses R
OUTER APPLY
(
select   *
from PackageDatasets p 
where p.SubClientName = @subClientName
 and p.DatasetCreateDate BETWEEN @startDate AND @endDate 
  and p.RecallStatus = @status 
  ) A
  GROUP BY R.Description, R.Status
	--ORDER BY COUNT([pd].[PackageId])

END

 
GO

CREATE PROCEDURE [dbo].[getRptRecallReleaseSummary_details]
(
    -- Add the parameters for the stored procedure here
	@subClientName AS VARCHAR(20),
	@status AS VARCHAR(15),
	@startDate AS DATE,
	@endDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    SELECT
		pd.PackageId, 
		pd.SiteName, 
		pd.ClientName, 
		pd.SubClientName,
		pd.PackageStatus,
		RecallStatus,
		pd.LocalProcessedDate,
		pd.ContainerId,
		pd.BinCode,
		pd.ShippingCarrier,
		pd.[AddressLine1],
		pd.[AddressLine2],
		pd.[AddressLine3],
		pd.City,
		pd.[State],
		pd.Zip,
		pd.DatasetCreateDate AS CreateDate
			FROM [dbo].[PackageDatasets] pd
				WHERE DatasetCreateDate BETWEEN @startDate AND @endDate
					AND pd.SubClientName = @subClientName
					AND pd.RecallStatus = @status

END
    
  
  
  -- fin