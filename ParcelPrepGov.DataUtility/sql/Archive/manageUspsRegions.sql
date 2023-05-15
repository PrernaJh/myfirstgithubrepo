
  select u.UserName,u.Email, r.Name, rc.ClaimType from Users u
  left join UserRoles ur on u.Id = ur.UserId
  left join Roles r on ur.RoleId = r.Id
  left join RoleClaims rc on r.Id = rc.RoleId
  where rc.ClaimType = 'WebPortal.ServiceManagement.ManageUspsRegions'
  order by UserName
