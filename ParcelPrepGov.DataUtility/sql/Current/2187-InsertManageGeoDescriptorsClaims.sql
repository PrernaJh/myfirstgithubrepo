IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'FSCWEBFINANCIALUSER') and ClaimType = 'WebPortal.ServiceManagement.ManageGeoDescriptors')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ADMINISTRATOR, SYSTEMADMINISTRATOR, and FSCWEBFINANCIALUSER')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.ServiceManagement.ManageGeoDescriptors', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'FSCWEBFINANCIALUSER')
		PRINT 'WebPortal.ServiceManagement.ManageGeoDescriptors claim assigned to ADMINISTRATOR | SYSTEMADMINISTRATOR | FSCWEBFINANCIALUSER'
	END

IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'FSCWEBFINANCIALUSER') and ClaimType = 'WebPortal.ServiceManagement.ManageGeoDescriptors')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ADMINISTRATOR, SYSTEMADMINISTRATOR, and FSCWEBFINANCIALUSER')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.ServiceManagement.ManageGeoDescriptors', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'FSCWEBFINANCIALUSER')
		PRINT 'WebPortal.ServiceManagement.ManageGeoDescriptors claim assigned to ADMINISTRATOR, SYSTEMADMINISTRATOR, and FSCWEBFINANCIALUSER'
	END