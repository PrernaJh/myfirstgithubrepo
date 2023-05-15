DECLARE @ROLEID VARCHAR(450)
DECLARE @SYSTEMADMINROLEID VARCHAR(450) = (select top 1 id from roles where Name = 'SYSTEMADMINISTRATOR' )


IF NOT EXISTS(SELECT * FROM Roles WHERE  NAME = 'CUSTOMERSERVICE')
BEGIN
	INSERT INTO Roles(id, Name, NormalizedName)
	VALUES(NEWID(), 'CUSTOMERSERVICE', 'CUSTOMERSERVICE')
END

SET @ROLEID = (SELECT TOP 1 ID FROM Roles WHERE NAME = 'CUSTOMERSERVICE')

IF NOT EXISTS ( select top 1 * from RoleClaims WHERE RoleId = @SYSTEMADMINROLEID and ClaimType = 'WebPortal.UserManagment.AddCustomerServiceUser')
BEGIN
	INSERT INTO RoleClaims(RoleId, ClaimType, ClaimValue)
	VALUES(@SYSTEMADMINROLEID, 'WebPortal.UserManagment.AddCustomerServiceUser', '')
END

CREATE TABLE dbo.#Claims
(
    claimType varchar(450) NOT NULL    
);

INSERT INTO #Claims(claimType)
VALUES
('WebPortal.PackageManagement.PackageSearch'),
('WebPortal.ContainerManagement.ContainerSearch'),
('WebPortal.Reporting.Admin')
--,('WebPortal.UserManagment.AddCustomerService')

INSERT into RoleClaims(RoleId, ClaimType, ClaimValue)
select @ROLEID, C.claimType, '' 
from #Claims c LEFT OUTER JOIN RoleClaims rc on rc.claimtype = c.claimType and rc.RoleId = @ROLEID
where rc.Id IS NULL


drop table #Claims
