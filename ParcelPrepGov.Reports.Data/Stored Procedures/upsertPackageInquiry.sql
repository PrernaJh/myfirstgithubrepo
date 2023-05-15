/****** Object:  StoredProcedure [dbo].[upsertPackageInquiry]    Script Date: 5/9/2022 10:27:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[upsertPackageInquiry]
(
    -- Add the parameters for the stored procedure here
	@inquiryId INT,
    @packageId VARCHAR(50),
	@siteName VARCHAR(24),
	@packageDatasetId INT,
	@serviceRequestNumber varchar(50)
)
AS
BEGIN

	MERGE PackageInquiries tgt
	USING (SELECT @inquiryId, 
		@packageDatasetId,
		@packageId,
		@siteName,
		@serviceRequestNumber) AS src 
		(InquiryId,
		PackageDatasetId,
		PackageId,
		SiteName,
		ServiceRequestNumber)
	ON (tgt.InquiryId = src.inquiryId)
	WHEN MATCHED THEN
		UPDATE SET InquiryId = src.inquiryId,
			PackageDatasetId = @packageDatasetId,
			PackageId = @packageId,
			SiteName = @SiteName,
			ServiceRequestNumber = @serviceRequestNumber
	WHEN NOT MATCHED THEN
		INSERT (InquiryId, PackageDatasetId, PackageId, SiteName, ServiceRequestNumber)
		VALUES (src.inquiryId, src.packageDatasetId, src.packageId, src.siteName, @serviceRequestNumber);
END