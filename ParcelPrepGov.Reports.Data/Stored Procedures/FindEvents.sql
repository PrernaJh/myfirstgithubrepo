/****** Object:  StoredProcedure [dbo].[FindEvents]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[FindEvents]
(
	@eventType AS VARCHAR(MAX),
	@machineId AS VARCHAR(MAX),
	@eventStart AS DATETIME,
	@eventEnd AS DATETIME
)
AS
BEGIN
	SELECT pd.PackageId, pd.LocalProcessedDate, e.EventDate, e.Description, e.EventType
	FROM PackageDatasets pd
	JOIN (SELECT PackageId, CosmosId, EventDate, Description, EventType, MachineId, ROW_NUMBER() 
				OVER (PARTITION BY CosmosId ORDER BY EventDate DESC) AS ROW_NUM
					FROM dbo.PackageEventDatasets
						WHERE EventType = @eventType AND MachineId = @machineId
							AND LocalEventDate BETWEEN @eventStart AND @eventEnd) e 
								ON pd.CosmosId = e.CosmosId AND e.ROW_NUM = 1
	WHERE pd.LocalProcessedDate BETWEEN @eventStart AND @eventEnd
	ORDER BY pd.LocalProcessedDate
END

GO
