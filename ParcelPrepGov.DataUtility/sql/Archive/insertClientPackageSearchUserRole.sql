IF EXISTS(select * from dbo.Roles r
where r.Name = 'CLIENTWEBPACKAGESEARCHUSER')
	BEGIN
		PRINT 'no insert required'
	END
ELSE
	BEGIN
		INSERT INTO dbo.Roles (Id, Name, NormalizedName) Values(NEWID(), 'CLIENTWEBPACKAGESEARCHUSER', 'CLIENTWEBPACKAGESEARCHUSER')
		PRINT 'CLIENTWEBPACKAGESEARCHUSER role inserted'
	END


IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR') and ClaimType = 'WebPortal.UserManagement.AddClientPackageSearchUser')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.UserManagement.AddClientPackageSearchUser', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR')
		PRINT 'WebPortal.UserManagement.AddClientPackageSearchUser claim assigned to ADMINISTRATOR | SYSTEMADMINISTRATOR'
	END

IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('CLIENTWEBPACKAGESEARCHUSER') and ClaimType = 'WebPortal.PackageManagement.PackageSearch')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for CLIENTWEBPACKAGESEARCHUSER')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.PackageManagement.PackageSearch', '' from Roles where name in ('CLIENTWEBPACKAGESEARCHUSER')
		PRINT 'WebPortal.PackageManagement.PackageSearch claim assigned to CLIENTWEBPACKAGESEARCHUSER'
	END