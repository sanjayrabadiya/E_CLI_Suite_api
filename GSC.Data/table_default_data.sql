/*
SET IDENTITY_INSERT Country ON;
INSERT INTO Country
    (Id, CountryCode, CountryName, CountryCallingCode, IsProject, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'IND', N'INDIA', N'+91', 0, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT Country OFF;

SET IDENTITY_INSERT [State] ON;
INSERT INTO [State]
    (Id, StateName, CountryId, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'Gujarat', 1, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT [State] OFF;

SET IDENTITY_INSERT City ON;
INSERT INTO City
    (Id, CityName, StateId, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'Ahemdabad', 1, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT City OFF;

*/

SET IDENTITY_INSERT [Location] ON;
INSERT INTO [Location]
    (Id, [Address], CountryId, StateId, CityId, Zip, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'System Address', 1, 1, 1, N'380015', 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT [Location] OFF;

SET IDENTITY_INSERT Department ON;
INSERT INTO Department
    (Id, DepartmentCode, DepartmentName, Notes, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'SD', 'Sys Department', NULL, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT Department OFF;

SET IDENTITY_INSERT ScopeName ON;
INSERT INTO ScopeName
    (Id, ScopeName, Notes, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, 'Sys Name', NULL, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT ScopeName OFF;

SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users]
    ( Id, FirstName, MiddleName, LastName, GenderId, UserName, Email, DateOfBirth, LocationId, ScopeNameId, Phone, DepartmentId, ValidFrom, ValidTo, FailedLoginAttempts, IsLocked, LastLoginDate, LastIpAddress, LastSystemName, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, N'Super', N'', N'Admin', 0, N'admin', N'admin@admin.com', NULL, 1, 1, N'', 1, NULL, NULL, 0, 0, NULL, N'', N'', 1, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT [Users] OFF;

SET IDENTITY_INSERT UserPassword ON;
INSERT INTO UserPassword
    (Id, UserId, [Password], PasswordSalt, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, 1, 'AF6C3A44BC7C23BBCC38FC90E744C15DA29CF5EE2FBE03DBA970697C2067211D20B0989C6E998858E9B2000F0216F681761D219C99A3A2D90E43AF16925B5A03', '8hjYm2msoIIDovmobCAVlw==', 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT UserPassword OFF;

SET IDENTITY_INSERT [SecurityRole] ON;
INSERT INTO [SecurityRole]
    (Id, RoleShortName, RoleName, IsSystemRole, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, 'Sys Role', 'SysRole', 1, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT [SecurityRole] OFF;

SET IDENTITY_INSERT [UserRole] ON;
INSERT INTO [UserRole]
    (Id, UserId, UserRoleId, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate)
VALUES
    (1, 1, 1, 0, GETUTCDATE(), 0, NULL, 0, NULL);
SET IDENTITY_INSERT [UserRole] OFF;