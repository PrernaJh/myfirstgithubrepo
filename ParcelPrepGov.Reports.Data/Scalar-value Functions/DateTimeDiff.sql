/****** Object:  UserDefinedFunction [dbo].[DateTimeDiff]    Script Date: 5/18/2021 2:06:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[DateTimeDiff]
(
	@beginDateTime DATETIME,
	@endDateTime DATETIME
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @diff INT

	SELECT @diff = DATEDIFF(MINUTE, @beginDateTime, @endDateTime)
	
	RETURN  CONVERT(DECIMAL, @diff) / (60 * 24)
END