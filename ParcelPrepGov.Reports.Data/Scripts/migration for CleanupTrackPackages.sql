
                WITH cte AS (
                    SELECT 
                        PackageDatasetId, EventCode, EventDate, EventDescription,
                        ROW_NUMBER() OVER (
                            PARTITION BY 
                                PackageDatasetId, EventCode, EventDate, EventDescription
                            ORDER BY 
                                PackageDatasetId, EventCode, EventDate DESC
                        ) AS row_num
                     FROM 
                        TrackPackageDatasets
                )
                DELETE FROM cte
                WHERE row_num > 1 AND NOT PackageDatasetId IS NULL;
           

GO


                WITH cte AS (
                    SELECT 
                        ShippingContainerDatasetId, EventCode, EventDate, EventDescription,
                        ROW_NUMBER() OVER (
                            PARTITION BY 
                                ShippingContainerDatasetId, EventCode, EventDate, EventDescription
                            ORDER BY 
                                ShippingContainerDatasetId, EventCode, EventDate DESC
                        ) AS row_num
                     FROM 
                        TrackPackageDatasets
                )
                DELETE FROM cte
                WHERE row_num > 1 AND NOT ShippingContainerDatasetId IS NULL;           
             

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210426165617_CleanupTrackPackages', N'3.1.6');

GO

