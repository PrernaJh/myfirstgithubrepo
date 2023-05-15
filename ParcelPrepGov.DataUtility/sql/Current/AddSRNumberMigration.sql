ALTER TABLE [PackageInquiries] ADD [ServiceRequestNumber] nvarchar(50) NULL; 

GO 

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220510141119_AddServiceRequestNumberToPackageInquiries', N'3.1.6'); 

GO