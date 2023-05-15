-- Add CLIENTWEBFINANCIALUSER to the ClaimType of WebPortal.FileManagement.ProgramManagementBlob
IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'CLIENTWEBFINANCIALUSER'
AND ClaimType = 'WebPortal.FileManagement.ProgramManagementBlob') <= 0
	INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
	VALUES ((SELECT ID FROM Roles WHERE Name = 'CLIENTWEBFINANCIALUSER'), 'WebPortal.FileManagement.ProgramManagementBlob', '')

-- Add CLIENTWEBUSER to the ClaimType of WebPortal.FileManagement.ProgramManagementBlob
GO
IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'CLIENTWEBUSER'
AND ClaimType = 'WebPortal.FileManagement.ProgramManagementBlob') <= 0
	INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
	VALUES ((SELECT ID FROM Roles WHERE Name = 'CLIENTWEBUSER'), 'WebPortal.FileManagement.ProgramManagementBlob', '')

-- Add SUBCLIENTWEBUSER to the ClaimType of WebPortal.FileManagement.ProgramManagementBlob
GO
IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'SUBCLIENTWEBUSER'
AND ClaimType = 'WebPortal.FileManagement.ProgramManagementBlob') <= 0
	INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
	VALUES ((SELECT ID FROM Roles WHERE Name = 'SUBCLIENTWEBUSER'), 'WebPortal.FileManagement.ProgramManagementBlob', '')

GO