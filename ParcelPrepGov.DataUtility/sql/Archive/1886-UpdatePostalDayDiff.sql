/****** Object:  UserDefinedFunction [dbo].[PostalDateDiff]    Script Date: 7/18/2022 11:13:26 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER FUNCTION [dbo].[PostalDateDiff]
(
	@beginDate DATE,
	@endDate DATE,
	@shippingMethod VARCHAR(60)
)
RETURNS INT
AS
BEGIN
    DECLARE @beginDays INT
    DECLARE @endDays INT
    DECLARE @diff INT
	DECLARE @dropPointDay INT = 0

	SELECT @beginDays =
		(SELECT Ordinal
			FROM [dbo].[PostalDays]
				WHERE @beginDate = PostalDate
		)

	SELECT @endDays =
		(SELECT Ordinal
			FROM [dbo].[PostalDays]
				WHERE @endDate = PostalDate
		)

	SELECT @diff = CASE WHEN @beginDays IS NULL OR @endDays IS NULL
		THEN DATEDIFF(DAY, @beginDate, @endDate)
		ELSE @endDays - @beginDays
	END

	IF @shippingMethod = 'PSLW' OR @shippingMethod = 'PS'
		SET @dropPointDay = 1
	
	RETURN CASE WHEN @diff > 0 THEN @diff - @dropPointDay ELSE 0 END -- Don't count ship date.
END