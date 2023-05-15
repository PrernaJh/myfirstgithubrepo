-- Set ShippedDate = 8PM on date processed if NULL
UPDATE  [dbo].[PackageDatasets] 
	SET ShippedDate = TRY_CONVERT(DATETIME, 
		(CONVERT(VARCHAR, CONVERT(DATE, LocalProcessedDate)) + ' 20:00:00.000'), 121)
	WHERE ShippedDate IS NULL AND LocalProcessedDate IS NOT NULL AND PackageStatus = 'PROCESSED' 

GO

-- Add Missing shipped events
DECLARE @Id VARCHAR(MAX)
 
DECLARE MY_data CURSOR FOR SELECT CosmosId from PackageDatasets WHERE ShippedDate IS NOT NULL 
  	      
OPEN MY_data  

FETCH NEXT FROM MY_data INTO @Id
WHILE @@FETCH_STATUS = 0  
    BEGIN  
		INSERT INTO [dbo].[PackageEventDatasets] (CosmosId, SiteName, PackageDatasetId, PackageId,
				EventId, EventType, EventStatus, 
				Description, EventDate,LocalEventDate,
				Username, MachineId, 
				DatasetCreateDate, DatasetModifiedDate)
			SELECT TOP (1) pd.CosmosId, pd.SiteName, pd.Id AS PackageDatesetId, pd.PackageId, 
					999 as EventId, 'SHIPPED' AS EventType, pd.PackageStatus AS EventStatus,
					'shipped' AS Description, pd.ShippedDate as EventDate, pd.ShippedDate as LocalEventDate,
					'System' AS UserName, 'System' AS MachineId,
					GETDATE() AS DateSetCreateDate, GETDATE() AS DateSetModifiedDate 
				FROM [dbo].[PackageDatasets] pd
					LEFT JOIN [dbo].[PackageEventDatasets] e ON e.CosmosId = @Id AND e.EventId = 999
				WHERE pd.CosmosId = @Id AND e.Id IS NULL
   
        FETCH NEXT FROM MY_data INTO  @Id
    END  
CLOSE MY_data  
DEALLOCATE MY_data

GO


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210525162801_AddShippedEvents', N'3.1.6');

GO

