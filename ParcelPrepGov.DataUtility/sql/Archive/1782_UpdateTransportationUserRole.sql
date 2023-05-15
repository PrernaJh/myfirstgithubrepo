/****** Script for SelectTopNRows command from SSMS  ******/
-- Add TRANSPORTATIONUSER RoleClaims, Portal.PackageManagement.PackageSearch, ContainerSearch
IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'TRANSPORTATIONUSER'
AND ClaimType = 'WebPortal.PackageManagement.PackageSearch') <= 0
INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
VALUES ((SELECT ID FROM Roles WHERE Name = 'TRANSPORTATIONUSER'), 'WebPortal.PackageManagement.PackageSearch', '')

IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'TRANSPORTATIONUSER'
AND ClaimType = 'WebPortal.ContainerManagement.ContainerSearch') <= 0
INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
VALUES ((SELECT ID FROM Roles WHERE Name = 'TRANSPORTATIONUSER'), 'WebPortal.ContainerManagement.ContainerSearch', '')

IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'TRANSPORTATIONUSER'
AND ClaimType = 'WebPortal.Reporting.SiteSpecific') <= 0
INSERT INTO [dbo].[RoleClaims] (RoleId, ClaimType, ClaimValue)
VALUES ((SELECT ID FROM Roles WHERE Name = 'TRANSPORTATIONUSER'), 'WebPortal.Reporting.SiteSpecific', '')

--Delete RoleClaims that should not be assigned
IF(SELECT COUNT(*) FROM RoleClaims rc
JOIN Roles r ON rc.RoleId = r.Id
WHERE r.Name = 'TRANSPORTATIONUSER'
AND ClaimType = 'WebPortal.ServiceManagement.ManageBinRules') > 0
DELETE FROM [dbo].[RoleClaims] 
WHERE RoleId = (SELECT RoleId FROM RoleClaims rc JOIN Roles r ON rc.RoleId = r.Id WHERE r.Name = 'TRANSPORTATIONUSER' AND rc.ClaimType = 'WebPortal.ServiceManagement.ManageBinRules')