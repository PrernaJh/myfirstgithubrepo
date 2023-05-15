

CREATE OR ALTER PROCEDURE dbo.[RoleAddExistingClaim] 
    @roleName VARCHAR(50),
    @claimName VARCHAR(50)
AS
BEGIN
	declare @roleId varchar(450)
	select top 1 @roleId = id from Roles where Name = @roleName

	IF EXISTS( SELECT DISTINCT ClaimType from RoleClaims WHERE ClaimType = @claimName )
	BEGIN		
		IF NOT EXISTS(select * from RoleClaims where RoleId = @roleId and ClaimType = @claimName)
		BEGIN
			INSERT INTO RoleClaims(RoleId, ClaimType, ClaimValue)
			select @roleId, @claimName, ''
		END
	END
END
GO


EXEC [RoleAddExistingClaim] 'SUPERVISOR', 'WebPortal.ContainerManagement.ContainerSearch'
EXEC [RoleAddExistingClaim] 'GENERALMANAGER', 'WebPortal.ContainerManagement.ContainerSearch'
EXEC [RoleAddExistingClaim] 'OPERATOR', 'WebPortal.ContainerManagement.ContainerSearch'
EXEC [RoleAddExistingClaim] 'QUALITYASSURANCE', 'WebPortal.ContainerManagement.ContainerSearch'
EXEC [RoleAddExistingClaim] 'CUSTOMERSERVICE', 'WebPortal.ContainerManagement.ContainerSearch'