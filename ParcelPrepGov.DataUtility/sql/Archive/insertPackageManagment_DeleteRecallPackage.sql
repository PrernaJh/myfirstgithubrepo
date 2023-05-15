-- =============================================
-- Author:      <Author, Chong,>
-- Create Date: <Create Date, 02/23/2022, >
-- Description: <Description, Give SYSTEMADMINISTRATOR and GENERALMANAGER premission to delete CREATED packages in was created in the Recall/Release screen, >
-- =============================================
IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('SYSTEMADMINISTRATOR', 'GENERALMANAGER') and ClaimType = 'WebPortal.PackageManagement.DeleteRecallPackage')
	BEGIN
		print('no insert required')
	END
ELSE
	BEGIN
		print('creating claim for SYSTEMADMINISTRATOR and GENERALMANAGER')
		insert into RoleClaims (RoleID, ClaimType, ClaimValue)
		select Id, 'WebPortal.PackageManagement.DeleteRecallPackage', '' from Roles where name in ('SYSTEMADMINISTRATOR', 'GENERALMANAGER')
		PRINT 'WebPortal.PackageManagement.DeleteRecallPackage claim assigned to SYSTEMADMINISTRATOR and GENERALMANAGER'
	END