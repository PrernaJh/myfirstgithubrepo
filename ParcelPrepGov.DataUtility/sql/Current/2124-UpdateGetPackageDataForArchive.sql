/****** Object:  StoredProcedure [dbo].[getPackageDataForArchive]    Script Date: 11/09/2022 11:54:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER     PROCEDURE [dbo].[getPackageDataForArchive]
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
      ,p.MarkupType
      ,p.IsPoBox
      ,p.IsRural
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
	  ,p.IsStopTheClock
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