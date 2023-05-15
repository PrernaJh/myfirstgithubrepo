/****** Object:  StoredProcedure [dbo].[getRptDailyAdvanceWarning]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptDailyAdvanceWarning]
(
    -- Add the parameters for the stored procedure here
@localProcessDateBegin date,
@localProcessDateEnd date
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Insert statements for procedure here
	SELECT dbo.PackageDatasets.SiteName as LOCATION,
	CAST(dbo.PackageDatasets.LocalProcessedDate as date) AS MANIFEST_DATE,
	dbo.PackageDatasets.PackageId AS PACKAGE_ID,
       dbo.PackageDatasets.ShippingBarcode AS TRACKING_NUMBER, 
	   dbo.PackageDatasets.ShippingMethod AS PRODUCT,
	   dbo.PackageDatasets.Zip,
		IIF(LEFT(dbo.BinDatasets.BinCode, 1) = 'D', 'DDU', 'SCF') AS ENTRY_UNIT_TYPE, 
       dbo.BinDatasets.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME,
       dbo.BinDatasets.DropShipSiteCszPrimary AS ENTRY_UNIT_CSZ, 
	   t.EventDescription AS LAST_KNOW_DESC,
       t.EventDate AS LAST_KNOWN_DATE, 
       t.EventLocation AS LAST_KNOW_LOCATION, 
       t.EventZip as LAST_KNOWN_ZIP
FROM  dbo.PackageDatasets 
       LEFT JOIN dbo.BinDatasets ON dbo.PackageDatasets.BinGroupId = dbo.BinDatasets.ActiveGroupId AND dbo.PackageDatasets.BinCode = dbo.BinDatasets.BinCode 
       LEFT JOIN (	 Select PackageId,EventDate,EventDescription,EventLocation,EventCode, EventZip, ROW_NUMBER() OVER(PARTITION BY PackageId ORDER BY EventDate DESC) as ROW_NUM
                     FROM dbo.TrackPackageDatasets WHERE EventDate >= @localProcessDateBegin) t ON dbo.PackageDatasets.PackageId = t.PackageId AND t.ROW_NUM=1
	   LEFT JOIN dbo.EvsCodes e on e.Code = t.EventCode	
	   WHERE dbo.PackageDatasets.PackageStatus = 'PROCESSED'
			AND dbo.PackageDatasets.LocalProcessedDate BETWEEN @localProcessDateBegin AND DATEADD(day, 1, @localProcessDateEnd)
	   ORDER BY dbo.PackageDatasets.PackageId,t.EventDate
END
GO
