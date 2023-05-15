
IF ( NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'UserLookups'))
BEGIN
    CREATE TABLE [UserLookups] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(256) NULL,
    [FirstName] nvarchar(200) NULL,
    [LastName] nvarchar(200) NULL,
    CONSTRAINT [PK_UserLookups] PRIMARY KEY ([UserId])
	);
END

