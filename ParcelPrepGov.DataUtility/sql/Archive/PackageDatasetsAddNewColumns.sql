/*
   Wednesday, November 10, 202112:01:04 PM
   User: tecmailing
   Server: tcp:ppg-test-sql.database.usgovcloudapi.net,1433
   Database: mms-reports-dev
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PackageDatasets ADD
	LastKnownDate datetime2(7) NULL,
	LastKnownLocation varchar(120) NULL,
	LastKnownZip varchar(10) NULL
GO
ALTER TABLE dbo.PackageDatasets SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
