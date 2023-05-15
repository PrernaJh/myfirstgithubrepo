/****** Object:  UserDefinedFunction [dbo].[Percent]    Script Date: 5/26/2021 2:57:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[Percent]
(
	@count INT,
	@total INT
)
RETURNS DECIMAL(18,7)
AS
BEGIN	
	RETURN CASE WHEN @total != 0 THEN CONVERT(DECIMAL, @count) /  CONVERT(DECIMAL, @total) ELSE 0 END
END

