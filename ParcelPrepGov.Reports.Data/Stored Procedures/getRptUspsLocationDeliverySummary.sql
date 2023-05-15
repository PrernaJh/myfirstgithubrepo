/****** Object:  StoredProcedure [dbo].[getRptUspsLocationDeliverySummary]    Script Date: 5/10/2021 11:13:58 AM ******/

CREATE OR ALTER PROCEDURE [dbo].[getRptUspsLocationDeliverySummary]
(
    -- Add the parameters for the stored procedure here
    @subClients AS VARCHAR(MAX),
       @beginDate AS DATE,
       @endDate AS DATE
)
AS
BEGIN
    SET NOCOUNT ON

       SELECT      
              s.Description AS LOCATION,
              SUM(CASE WHEN ISNULL(pd.PostalDays,0) <= 3 AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END) AS DAY3_PCS,
              [dbo].[Percent](SUM(CASE WHEN ISNULL(pd.PostalDays,0) <=3  AND NOT pd.StopTheClockEventDate IS NULL THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY3_PCT,
              SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END) AS DAY4_PCS,
              [dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 4 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY4_PCT,
              SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END) AS DAY5_PCS,
              [dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 5 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY5_PCT,
              SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END) AS DAY6_PCS,
              [dbo].[Percent](SUM(CASE WHEN pd.PostalDays = 6 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DAY6_PCT,
              SUM(CASE WHEN pd.PostalDays >=7 THEN 1 ELSE 0 END) AS DELAYED_PCS,
              [dbo].[Percent](SUM(CASE WHEN pd.PostalDays >= 7 THEN 1 ELSE 0 END), COUNT(pd.PackageId)) AS DELAYED_PCT,             
              SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END) AS DELIVERED_PCS,
              [dbo].[Percent](SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END), COUNT(pd.PackageId)) AS DELIVERED_PCT,
              [dbo].[Fraction](SUM(pd.PostalDays), SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END)) AS AVG_POSTAL_DAYS,              
              [dbo].[Fraction](SUM(pd.CalendarDays), SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END)) AS AVG_CAL_DAYS,
              COUNT(pd.PackageId) - SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END) AS NO_STC_PCS,
              [dbo].[Percent](COUNT(pd.PackageId) - SUM(CASE WHEN pd.StopTheClockEventDate IS NULL THEN 0 ELSE 1 END), COUNT(pd.PackageId)) AS NO_STC_PCT,
              COUNT(pd.PackageId) AS TOTAL_PCS 
       FROM dbo.PackageDatasets pd WITH (NOLOCK, FORCESEEK) 
       LEFT JOIN dbo.SubClientDatasets s ON s.[Name] = pd.SubClientName
       
       WHERE pd.PackageStatus = 'PROCESSED' AND pd.ShippingCarrier = 'USPS'
              AND pd.SubClientName IN (SELECT * FROM [dbo].[SplitString](@subClients, ','))
              AND LocalProcessedDate BETWEEN @beginDate AND DATEADD(day, 1, @endDate)       
       GROUP BY s.Description
END

