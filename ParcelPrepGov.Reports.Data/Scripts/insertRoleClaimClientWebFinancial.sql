IF exists(select rc.*, r.name from roles r
left join RoleClaims rc on r.Id = rc.RoleId
where r.name = 'ClientWebFinancialUser' and ClaimType = 'WebPortal.Reporting.ClientSpecific')
BEGIN
print('no insert required')
END
ELSE
BEGIN
declare @id uniqueidentifier;
set @id = (select Id from Roles where name = 'CLIENTWEBFINANCIALUSER');
print('creating claim for CLIENTWEBFINANCIALUSER')
insert into RoleClaims (RoleID, ClaimType, ClaimValue) values (@id,'WebPortal.Reporting.ClientSpecific', '')
END

