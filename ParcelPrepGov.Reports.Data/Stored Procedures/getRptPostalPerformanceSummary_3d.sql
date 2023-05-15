/****** Object:  StoredProcedure [dbo].[getRptPostalPerformanceSummary_3d]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[getRptPostalPerformanceSummary_3d]
(
    -- Add the parameters for the stored procedure here
    @ID AS VARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON

	SELECT        
		CONVERT(varchar, pd.SubClientName) + 
			IIF(LEFT(bd.BinCode, 1) IS NULL, ' ',  LEFT(bd.BinCode, 1)) + 
			IIF(p.PostalArea IS NULL, 'null',  p.PostalArea) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) 
			AS ID, 

		CONVERT(varchar, pd.SubClientName) + 
			IIF(LEFT(bd.BinCode, 1) IS NULL, ' ',  LEFT(bd.BinCode, 1)) + 
			IIF(p.PostalArea IS NULL, 'null',  p.PostalArea) + 
			IIF(bd.DropShipSiteDescriptionPrimary IS NULL, 'null',  bd.DropShipSiteDescriptionPrimary) + 
			CONVERT(VARCHAR, left(pd.Zip,3)) 
			AS ID3, 

		pd.SubClientName as CMOP,
		MAX(IIF(LEFT(bd.BinCode, 1) = 'D', 'DDU', 'SCF')) AS ENTRY_UNIT_TYPE, 
		p.PostalArea as USPS_AREA,
		bd.DropShipSiteDescriptionPrimary AS ENTRY_UNIT_NAME, 
		left(pd.Zip,3) as ZIP3,
		COUNT(pd.PackageId) AS TOTAL_PCS, 
		COUNT(t.PackageId) AS TOTAL_PCS_STC,
		COUNT(pd.PackageId)-COUNT(t.PackageId) AS TOTAL_PCS_NO_STC,
		CONVERT(DECIMAL,COUNT(t.PackageId))/CONVERT(DECIMAL, COUNT(pd.PackageId)) AS STC_SCAN_PCT,
		AVG(CAST(DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate) AS DECIMAL)) AS AVG_DEL_DAYS,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=0 THEN 1 Else 0 END) AS DAY0_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=0 THEN 1 Else 0 END)/COUNT(t.PackageId) END AS DAY0_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=1 THEN 1 Else 0 END) AS DAY1_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=1 THEN 1 Else 0 END) /COUNT(t.PackageId) END AS DAY1_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=2 THEN 1 Else 0 END) AS DAY2_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=2 THEN 1 Else 0 END) /COUNT(t.PackageId) END AS DAY2_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=3 THEN 1 Else 0 END) AS DAY3_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=3 THEN 1 Else 0 END) /COUNT(t.PackageId) END AS DAY3_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=4 THEN 1 Else 0 END) AS DAY4_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=4 THEN 1 Else 0 END) /COUNT(t.PackageId) END AS DAY4_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=5 THEN 1 Else 0 END) AS DAY5_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=5 THEN 1 Else 0 END)/COUNT(t.PackageId) END AS DAY5_PCT,
		SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=6 THEN 1 Else 0 END) AS DAY6_PCS,
		CASE WHEN COUNT(t.PackageId)=0 THEN 0 ELSE SUM(CASE WHEN DATEDIFF(DAY, pd.LocalProcessedDate, t.EventDate)=6 THEN 1 Else 0 END) /COUNT(t.PackageId) END AS DAY6_PCT

		FROM dbo.PackageDatasets pd
		LEFT JOIN (SELECT PackageId, PackageDatasetId, EventDate, EventDescription, EventLocation, EventCode, EventZip, ROW_NUMBER() 
			OVER (PARTITION BY PackageDatasetId ORDER BY EventDate DESC) AS ROW_NUM
				FROM dbo.TrackPackageDatasets
					JOIN dbo.EvsCodes e ON e.Code = EventCode
						WHERE e.IsStopTheClock = 1) t ON pd.Id = t.PackageDatasetId AND t.ROW_NUM = 1 
		LEFT JOIN dbo.EvsCodes e ON e.Code = t.EventCode
		LEFT JOIN dbo.PostalAreasAndDistricts p ON LEFT(pd.Zip,3) = p.ZipCode3Zip
		LEFT JOIN dbo.BinDatasets bd ON pd.BinGroupId = bd.ActiveGroupId AND pd.BinCode = bd.BinCode 

		WHERE CONVERT(VARCHAR, pd.SubClientName) + CONVERT(VARCHAR, LEFT(bd.BinCode, 1)) + CONVERT(VARCHAR, p.PostalArea) + CONVERT(VARCHAR, bd.DropShipSiteDescriptionPrimary) = @ID

		GROUP BY 
			pd.SubClientName,
			LEFT(bd.BinCode, 1), 
			p.PostalArea, 
			bd.DropShipSiteDescriptionPrimary,
			LEFT(pd.Zip,3)

END
GO
