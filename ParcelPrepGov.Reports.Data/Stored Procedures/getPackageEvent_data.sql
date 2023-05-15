/****** Object:  StoredProcedure [dbo].[getPackageEvent_data]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[getPackageEvent_data]
(    
	@ids as VARCHAR(MAX),
	@beginDate AS DATE = '2020-06-01'
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	SELECT p.*, ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName,'') as DisplayName 
		FROM [dbo].[PackageEventDatasets] p LEFT JOIN UserLookups u on u.Username = p.Username
			WHERE EventDate >= @beginDate
				AND CosmosId IN (SELECT * FROM [dbo].[SplitString](@ids, ','))
	ORDER BY EventDate DESC

END