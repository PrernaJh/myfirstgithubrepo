IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ClientWebFinancialUser', 'CLIENTWEBUSER') and ClaimType = 'WebPortal.FileManagement.AsnImports')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for ClientWebFinancialUser and CLIENTWEBUSER')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.FileManagement.AsnImports', '' from Roles where name in ('ClientWebFinancialUser', 'CLIENTWEBUSER')
		PRINT 'WebPortal.FileManagement.AsnImportsclaim assigned to ClientWebFinancialUser | CLIENTWEBUSER'
	END