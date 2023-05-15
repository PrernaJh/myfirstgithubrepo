/****** Object:  UserDefinedFunction [dbo].[DropShippingCarrier]    Script Date: 5/26/2021 1:52:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
ALTER FUNCTION [dbo].[DropShippingCarrier]
(
	@product varchar(50),
	@carrier varchar(50)
)
RETURNS varchar(50)
AS
BEGIN
   
   RETURN CASE WHEN @product ='PSLW' and @carrier = 'USPS'
		THEN 'USPS PMOD'
		ELSE CASE WHEN @product ='PSLW' and @carrier <> 'USPS'
		THEN 'REGIONAL CARRIER'
		ELSE 'USPS'
		END
		END
END
