UPDATE [dbo].[Users]
SET [PasswordHash] = NULL
WHERE [Email] LIKE '%@fedex.com';

GO