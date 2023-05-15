/****** Object:  StoredProcedure [dbo].[getOldestPackageForArchive]    Script Date: 12/27/2021 10:21:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER     PROCEDURE [dbo].[getOldestPackageForArchive]
(
    @subClient AS VARCHAR(MAX),
	@startDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT TOP 1 *
	FROM [dbo].[PackageDatasets] p
		WHERE p.PackageStatus = 'PROCESSED'
			AND p.SubClientName = @subClient
			AND p.LocalProcessedDate >= @startDate
	ORDER BY p.LocalProcessedDate

END

GO

/****** Object:  StoredProcedure [dbo].[getPackageDataForArchive]    Script Date: 12/27/2021 9:59:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[getPackageDataForArchive]
(
    @subClient AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT p.PackageId
	  ,p.SiteName
      ,p.ClientName
      ,p.ClientFacilityName
      ,p.SubClientName

      ,p.MailCode
      ,p.PackageStatus
      ,p.LocalProcessedDate
      ,p.ShippingBarcode 
	  ,p.HumanReadableBarcode
      ,p.ShippingCarrier
      ,p.ShippingMethod
      ,p.ServiceLevel

      ,p.Zone
      ,p.Weight
      ,p.Length
      ,p.Width
      ,p.Depth
      ,p.TotalDimensions
      ,p.MailerId
      ,p.Cost
      ,p.Charge
      ,p.ExtraCost
      ,p.BillingWeight
      ,p.IsPoBox
      ,p.IsUpsDas
      ,p.IsOutside48States
      ,p.IsOrmd

      ,p.RecipientName
      ,p.AddressLine1
      ,p.AddressLine2
      ,p.AddressLine3
      ,p.City
      ,p.State
      ,p.Zip
      ,p.FullZip
      ,p.Phone

      ,p.CosmosCreateDate
      ,p.StopTheClockEventDate
      ,p.ShippedDate
      ,p.RecallStatus
      ,p.RecallDate
      ,p.ReleaseDate
      ,p.LastKnownEventDate
      ,p.LastKnownEventDescription
      ,p.LastKnownEventLocation
      ,p.LastKnownEventZip
      ,p.CalendarDays
      ,p.PostalDays
      ,p.IsUndeliverable

      ,p.BinCode
      ,p.ContainerId
      ,c.ContainerType
      ,p.IsSecondaryContainerCarrier
      ,c.UpdatedBarcode as ContainerBarcode
      ,c.ShippingCarrier as ContainerShippingCarrier
      ,c.ShippingMethod as ContainerShippingMethod
      ,c.LastKnownEventDate as ContainerLastKnownEventDate
      ,c.LastKnownEventDescription as ContainerLastKnownEventDescription
      ,c.LastKnownEventLocation as ContainerLastKnownEventLocation
      ,c.LastKnownEventZip as ContainerLastKnownEventZip

	FROM [dbo].[PackageDatasets] p
		LEFT JOIN ShippingContainerDatasets c on c.ContainerId = p.ContainerId
		WHERE p.PackageStatus = 'PROCESSED'
			AND p.SubClientName = @subClient
			AND p.LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
	ORDER BY p.PackageId

END

GO

/****** Object:  StoredProcedure [dbo].[deleteArchivedPackages]    Script Date: 12/27/2021 10:55:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[deleteArchivedPackages]    Script Date: 1/4/2022 9:36:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[deleteArchivedPackages]
(
    @subClient AS VARCHAR(MAX),
	@manifestDate AS DATE
)
AS
BEGIN
	DECLARE @count INT
	SET @count = 
		(SELECT COUNT(*)
			FROM [dbo].[PackageDatasets] 
				WHERE PackageStatus = 'PROCESSED'
					AND SubClientName = @subClient
					AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
		)
	SELECT @count					
	SET @count = @count / 10000
	WHILE @count >= 0
	BEGIN
		BEGIN TRANSACTION
		SET @count = @count -1
		DELETE TOP (10000)			
			FROM [dbo].[PackageDatasets] 
				WHERE PackageStatus = 'PROCESSED'
					AND SubClientName = @subClient
					AND LocalProcessedDate BETWEEN @manifestDate AND DATEADD(day, 1, @manifestDate)
		COMMIT TRANSACTION 
		CHECKPOINT 
	END
END

GO

/****** Object:  StoredProcedure [dbo].[deleteOlderPackages]    Script Date: 1/4/2022 9:36:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[deleteOlderPackages]
(
    @subClient AS VARCHAR(MAX),
	@processed AS BIT,
	@createDate AS DATE
)
AS
BEGIN
	DECLARE @count INT
	SET @count = 
		(SELECT COUNT(*)
			FROM [dbo].[PackageDatasets] 
				WHERE SubClientName = @subClient
					AND ((@processed = 1 AND PackageStatus = 'PROCESSED')
						OR (@processed = 0 AND PackageStatus != 'PROCESSED'))
					AND CosmosCreateDate < @createDate
		)
	SELECT @count					
	SET @count = @count / 10000
	WHILE @count >= 0
	BEGIN
		BEGIN TRANSACTION
		SET @count = @count -1
		DELETE TOP (10000)			
			FROM [dbo].[PackageDatasets] 
				WHERE SubClientName = @subClient
					AND ((@processed = 1 AND PackageStatus = 'PROCESSED')
						OR (@processed = 0 AND PackageStatus != 'PROCESSED'))
					AND CosmosCreateDate < @createDate
		COMMIT TRANSACTION 
		CHECKPOINT 
	END
END

GO