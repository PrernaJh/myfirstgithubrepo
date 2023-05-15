 
      IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR') and ClaimType = 'WebPortal.ServiceManagement.ManageSiteAlerts')
BEGIN
print('no insert required')
END
ELSE
BEGIN
print('creating claim for ADMINISTRATOR and SYSTEMADMINISTRATOR')
insert into RoleClaims (RoleID, ClaimType, ClaimValue)
select Id, 'WebPortal.ServiceManagement.ManageSiteAlerts', '' from Roles where name in ('ADMINISTRATOR', 'SYSTEMADMINISTRATOR')

END

/*
Id	RoleId	ClaimType	ClaimValue	name
xx	80004cea-5bfa-4087-8247-44f907c28100	WebPortal.ServiceManagement.ManageSiteAlerts		SYSTEMADMINISTRATOR
xxx	56fc9e85-5625-46f7-a414-8f4512acb48d	WebPortal.ServiceManagement.ManageSiteAlerts		ADMINISTRATOR
*/