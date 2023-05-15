CREATE PROCEDURE [dbo].[upsertPackageInquiry]
(
    -- Add the parameters for the stored procedure here
	@inquiryId INT,
    @packageId VARCHAR(50),
	@siteName VARCHAR(24),
	@packageDatasetId INT
)
AS
BEGIN

	MERGE PackageInquiries tgt
	USING (SELECT @inquiryId, 
		@packageDatasetId,
		@packageId,
		@siteName) AS src 
		(InquiryId,
		PackageDatasetId,
		PackageId,
		SiteName)
	ON (tgt.InquiryId = src.inquiryId)
	WHEN MATCHED THEN
		UPDATE SET InquiryId = src.inquiryId,
			PackageDatasetId = @packageDatasetId,
			PackageId = @packageId,
			SiteName = @SiteName
	WHEN NOT MATCHED THEN
		INSERT (InquiryId, PackageDatasetId, PackageId, SiteName)
		VALUES (src.inquiryId, src.packageDatasetId, src.packageId, src.siteName);
END

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

