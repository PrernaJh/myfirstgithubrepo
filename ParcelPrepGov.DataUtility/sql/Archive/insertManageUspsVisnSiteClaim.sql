  IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR') and ClaimType = 'WebPortal.ServiceManagement.ManageUspsVisnSite')
BEGIN
print('no insert required')
END
ELSE
BEGIN
print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR')
insert into RoleClaims (RoleID, ClaimType, ClaimValue) 
select Id, 'WebPortal.ServiceManagement.ManageUspsVisnSite', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR')
 
END

