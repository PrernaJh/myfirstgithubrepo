DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[PackageDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		ProcessedDate = IIF(ProcessedDate IS NULL, NULL, DATEADD(month, 6, ProcessedDate)),
		LocalProcessedDate = IIF(LocalProcessedDate IS NULL, NULL, DATEADD(month, 6, localProcessedDate)),
		StopTheClockEventDate = IIF(StopTheClockEventDate IS NULL, NULL, DATEADD(month, 6, StopTheClockEventDate)),
		ShippedDate = IIF(ShippedDate IS NULL, NULL, DATEADD(month, 6, ShippedDate)),
		RecallDate = IIF(RecallDate IS NULL, NULL, DATEADD(month, 6, RecallDate)),
		ReleaseDate = IIF(ReleaseDate IS NULL, NULL, DATEADD(month, 6, ReleaseDate)),
		LastKnownEventDate = IIF(LastKnownEventDate IS NULL, NULL, DATEADD(month, 6, LastKnownEventDate)),
		ClientShipDate = IIF(ClientShipDate IS NULL, NULL, DATEADD(month, 6, ClientShipDate))

  WHERE DatasetModifiedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO

DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[ShippingContainerDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		ProcessedDate = IIF(ProcessedDate IS NULL, NULL, DATEADD(month, 6, ProcessedDate)),
		LocalProcessedDate = IIF(LocalProcessedDate IS NULL, NULL, DATEADD(month, 6, localProcessedDate)),
		StopTheClockEventDate = IIF(StopTheClockEventDate IS NULL, NULL, DATEADD(month, 6, StopTheClockEventDate)),
		LastKnownEventDate = IIF(LastKnownEventDate IS NULL, NULL, DATEADD(month, 6, LastKnownEventDate)),
		LocalCreateDate = IIF(LocalCreateDate IS NULL, NULL, DATEADD(month, 6, LocalCreateDate))

  WHERE DatasetModifiedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO

DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[TrackPackageDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		EventDate = DATEADD(month, 6, EventDate)

  WHERE EventDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO

DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[UndeliverableEventDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		EventDate = DATEADD(month, 6, EventDate)

  WHERE EventDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO

DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[PackageEventDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		LocalEventDate = DATEADD(month, 6, LocalEventDate),
		EventDate = DATEADD(month, 6, EventDate)

  WHERE LocalEventDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO

DECLARE @beginDate DATETIME2;
SET @beginDate = '12/01/2021';
DECLARE @endDate DATETIME2;
SET @endDate = '03/01/2022';

UPDATE [dbo].[ShippingContainerEventDatasets]
  SET 
		DatasetCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetCreateDate)),
		DatasetModifiedDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, DatasetModifiedDate)),
		CosmosCreateDate = IIF(DatasetCreateDate IS NULL, NULL, DATEADD(month, 6, CosmosCreateDate)),

		LocalEventDate = DATEADD(month, 6, LocalEventDate),
		EventDate = DATEADD(month, 6, EventDate)

  WHERE LocalEventDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate) 
GO