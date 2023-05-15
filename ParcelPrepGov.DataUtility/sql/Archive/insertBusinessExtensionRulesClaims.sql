IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR') and ClaimType = 'WebPortal.ServiceManagement.ManageExtendedBusinessRules')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.ServiceManagement.ManageExtendedBusinessRules', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR')
		PRINT 'WebPortal.ServiceManagement.ManageExtendedBusinessRules claim assigned to ADMINISTRATOR | SYSTEMADMINISTRATOR'
	END

IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR') and ClaimType = 'WebPortal.ServiceManagement.ManageExtendedBusinessRules')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.ServiceManagement.ManageExtendedBusinessRules', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR')
		PRINT 'WebPortal.ServiceManagement.ManageExtendedBusinessRules claim assigned to ADMINISTRATOR and SYSTEMADMINISTRATOR'
	END