/****** Object:  UserDefinedFunction [dbo].[SiteLocalTime]    Script Date: 5/18/2021 2:06:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[SiteLocalTime]
(
	@refUtcDateTime DATETIME,
	@refLocalDateTime DATETIME
)
RETURNS DATETIME
AS
BEGIN
    DECLARE @offset INT

	SELECT @offset = DATEDIFF(MINUTE, @refUtcDateTime, @refLocalDateTime)
	
	RETURN  DATEADD(MINUTE, @offset, GETDATE())
END