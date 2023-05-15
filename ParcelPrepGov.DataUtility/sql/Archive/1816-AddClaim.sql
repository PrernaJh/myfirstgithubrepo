


declare @roleId varchar(450)
select top 1 @roleId = id from Roles where Name = 'ADMINISTRATOR'

--select @roleId


IF NOT EXISTS(select * from RoleClaims where RoleId = @roleId and ClaimType ='WebPortal.UserManagment.AddCustomerServiceUser')
BEGIN
	INSERT INTO RoleClaims(RoleId, ClaimType, ClaimValue)
	select @roleId, 'WebPortal.UserManagment.AddCustomerServiceUser', ''
END