/****** Object:  UserDefinedFunction [dbo].[Fraction]    Script Date: 4/23/2021 2:14:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[Fraction]
(
	@count INT,
	@total INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN	
	RETURN CASE WHEN @total != 0 THEN CONVERT(DECIMAL, @count) /  CONVERT(DECIMAL, @total) ELSE 0 END
END


