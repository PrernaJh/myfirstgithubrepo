IF EXISTS(select * from dbo.Roles r
where r.Name = 'TransportationUser')
BEGIN
PRINT 'no insert required'
END
ELSE
INSERT INTO dbo.Roles (Id, Name, NormalizedName) Values(NEWID(), 'TRANSPORTATIONUSER', 'TRANSPORTATIONUSER')
print 'role inserted'


