ALTER TABLE [PackageDatasets] ADD [ShippedDate] datetime2 NULL;

GO

CREATE INDEX [IX_PackageDatasets_ShippedDate] ON [PackageDatasets] ([ShippedDate]);

GO

                DECLARE @Id VARCHAR(MAX)
 
                DECLARE MY_data CURSOR FOR SELECT CosmosId from PackageDatasets WHERE ShippedDate IS NULL 
  	                AND PackageStatus = 'PROCESSED' AND ShippingCarrier = 'USPS' 
                OPEN MY_data  

                FETCH NEXT FROM MY_data INTO @Id
                WHILE @@FETCH_STATUS = 0  
                    BEGIN  
		                DECLARE @date DATETIME;
		                SET @date = (SELECT TOP 1 e.EventDate
			                FROM dbo.PackageEventDatasets AS e
				                WHERE e.CosmosId = @Id AND e.EventId = 999
		                )
		                UPDATE PackageDatasets set ShippedDate = @date
			                WHERE CosmosId = @Id
   
                        FETCH NEXT FROM MY_data INTO  @Id
                    END  
                CLOSE MY_data  
                DEALLOCATE MY_data
                
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210511144337_AddShippedDate', N'3.1.6');

GO