SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'PackageDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'ShippingContainerDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'TrackPackageDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'UndeliverableEventDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'PackageEventDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

SELECT a.index_id, name, avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(N'mms-reports-staging'),
OBJECT_ID(N'ShippingContainerEventDatasets'), NULL, NULL, NULL) AS a
JOIN sys.indexes AS b
ON a.object_id = b.object_id AND a.index_id = b.index_id;
GO

ALTER INDEX ALL ON [dbo].[PackageDatasets]
REBUILD
GO

ALTER INDEX ALL ON [dbo].[ShippingContainerDatasets]
REBUILD
GO

ALTER INDEX ALL ON [dbo].[TrackPackageDatasets]
REBUILD
GO

ALTER INDEX ALL ON [dbo].[UndeliverableEventDatasets]
REBUILD
GO

ALTER INDEX ALL ON [dbo].[PackageEventDatasets]
REBUILD
GO

ALTER INDEX ALL ON [dbo].[ShippingContainerEventDatasets]
REBUILD
GO
