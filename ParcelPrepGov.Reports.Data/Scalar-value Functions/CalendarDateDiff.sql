/****** Object:  UserDefinedFunction [dbo].[CalendarDateDiff]    Script Date: 5/19/2021 11:27:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[CalendarDateDiff]
(
	@beginDate DATE,
	@endDate DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @diff INT

	SELECT @diff = DATEDIFF(DAY, @beginDate, @endDate)
	
	RETURN CASE WHEN @diff > 0 THEN @diff ELSE 0 END
END
