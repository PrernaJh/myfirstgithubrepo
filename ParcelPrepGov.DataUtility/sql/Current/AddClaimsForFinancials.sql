declare @roleId varchar(450)
select top 1 @roleId = id from Roles where Name = 'ADMINISTRATOR'

--select @roleId


IF NOT EXISTS(select * from RoleClaims where RoleId = @roleId and ClaimType ='WebPortal.FileManagement.AzureBlob')
BEGIN
	INSERT INTO RoleClaims(RoleId, ClaimType, ClaimValue)
	select @roleId, 'WebPortal.FileManagement.AzureBlob', ''
END

GO