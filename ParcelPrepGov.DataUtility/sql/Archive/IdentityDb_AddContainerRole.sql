
IF NOT EXISTS (  select top 1 *
  from RoleClaims
  where RoleId = 'f559516a-59c6-45db-907c-9541fa4a105a'
  and ClaimType = 'WebPortal.ContainerManagement.ContainerSearch'
  ) 
BEGIN
  INSERT INTO RoleClaims([RoleId], [ClaimType], [ClaimValue])
  values(  'f559516a-59c6-45db-907c-9541fa4a105a', 'WebPortal.ContainerManagement.ContainerSearch', '')
END

IF NOT EXISTS(  select top 1 *
  from RoleClaims
  where RoleId = '1a437106-2805-4da8-b138-401b274228b2'
  and ClaimType = 'WebPortal.ContainerManagement.ContainerSearch'
  )
BEGIN
  INSERT INTO RoleClaims([RoleId], [ClaimType], [ClaimValue])
  values(  '1a437106-2805-4da8-b138-401b274228b2', 'WebPortal.ContainerManagement.ContainerSearch', '')
END