 
 -- create claims for these roles to access program management
      IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'GENERALMANAGER', 'CLIENTWEBUSER', 
'QUALITYASSURANCE', 'CLIENTWEBADMINISTRATOR') and ClaimType = 'WebPortal.FileManagement.ProgramManagementBlob')
BEGIN
print('no insert required')
END
ELSE
BEGIN
print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR and GENERALMANAGER and CLIENTWEBUSER and CLIENTWEBADMINISTRATOR')
insert into RoleClaims (RoleID, ClaimType, ClaimValue)
select Id, 'WebPortal.FileManagement.ProgramManagementBlob', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR', 'GENERALMANAGER', 'CLIENTWEBUSER', 
'QUALITYASSURANCE', 'CLIENTWEBADMINISTRATOR')

END
 

 -- create azure blob claim for administrator
  
      IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR') and ClaimType = 'WebPortal.FileManagement.AzureBlob')
BEGIN
print('no insert required')
END
ELSE
BEGIN
print('creating claim for ADMINISTRATOR ')
insert into RoleClaims (RoleID, ClaimType, ClaimValue)
select Id, 'WebPortal.FileManagement.AzureBlob', '' from Roles where name in ('ADMINISTRATOR')

END
 




