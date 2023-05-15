/****** Object:  StoredProcedure [dbo].[deletePackageInquiry]    Script Date: 11/17/2021 2:22:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deletePackageInquiry]
(
    -- Add the parameters for the stored procedure here
	@inquiryId INT
)
AS
BEGIN

	DELETE PackageInquiries 
	WHERE InquiryId = @inquiryId

END

GO
