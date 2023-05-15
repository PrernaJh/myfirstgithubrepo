/****** Object:  StoredProcedure [dbo].[getRptPackageTracking_hdr]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
CREATE PROCEDURE [dbo].[getRptPackageTracking_hdr]
(
    -- Add the parameters for the stored procedure here
	@IdType int,  -- 0 = packageid, 1= shipping barcode
    @pkginfo nvarchar(50)
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	IF @IdType=0
		BEGIN
			SELECT * FROM [dbo].[PackageDatasets]
				WHERE PackageId = @pkginfo -- match exact
			  ORDER BY LocalProcessedDate DESC, CosmosCreateDate DESC
		END
	ELSE
		BEGIN
			SELECT * FROM [dbo].[PackageDatasets]
			  WHERE ShippingBarcode LIKE '%' + @pkginfo -- match end
			  ORDER BY LocalProcessedDate DESC, CosmosCreateDate DESC
		END
END
GO
