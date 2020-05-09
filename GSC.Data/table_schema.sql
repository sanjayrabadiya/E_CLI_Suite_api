CREATE TABLE [dbo].[Country]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Code] [nvarchar](10) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [CallingCode] [nvarchar](10) NOT NULL,
    [IsProject] [bit] NOT NULL DEFAULT(0),
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[State]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [CountryId] [int] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_State] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[State]  WITH CHECK ADD  CONSTRAINT [FK_State_Country_CountryId_Id] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Country] ([Id]);

ALTER TABLE [dbo].[State] CHECK CONSTRAINT [FK_State_Country_CountryId_Id];

CREATE TABLE [dbo].[City]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](300) NOT NULL,
    [StateId] [int] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[City]  WITH CHECK ADD  CONSTRAINT [FK_City_State_StateId] FOREIGN KEY([StateId])
REFERENCES [dbo].[State] ([Id]);

ALTER TABLE [dbo].[City] CHECK CONSTRAINT [FK_City_State_StateId];

CREATE TABLE [dbo].[Location]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Address] [nvarchar](500) NOT NULL,
    [CountryId] [int] NULL,
    [StateId] [int] NULL,
    [CityId] [int] NULL,
    [Zip] [nvarchar](10) NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_Location_City_CityId_Id] FOREIGN KEY([CityId])
REFERENCES [dbo].[City] ([Id]);

ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_Location_City_CityId_Id];

ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_Location_Country_CountryId] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Country] ([Id]);

ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_Location_Country_CountryId];

ALTER TABLE [dbo].[Location]  WITH CHECK ADD  CONSTRAINT [FK_Location_State_StateId] FOREIGN KEY([StateId])
REFERENCES [dbo].[State] ([Id]);

ALTER TABLE [dbo].[Location] CHECK CONSTRAINT [FK_Location_State_StateId];

CREATE TABLE [dbo].[Department]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Code] [nvarchar](20) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [Notes] [nvarchar](max) NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];


CREATE TABLE [dbo].[ScopeName]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [Notes] [nvarchar](max) NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_ScopeName] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[User]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [FirstName] [nvarchar](50) NOT NULL,
    [MiddleName] [nvarchar](50) NULL,
    [LastName] [nvarchar](50) NOT NULL,
    [GenderId] [int] NOT NULL DEFAULT(0),
    [UserName] [nvarchar](40) NOT NULL,
    [Email] [nvarchar](255) NULL,
    [BirthDate] [date] NULL,
    [LocationId] [int] NULL,
    [ScopeNameId] [int] NULL,
    [Phone] [nvarchar](30) NULL,
    [DepartmentId] [int] NULL,
    [ValidFrom] [date] NULL,
    [ValidTo] [date] NULL,
    [FailedLoginAttempts] [int] NOT NULL DEFAULT(0),
    [IsLocked] [bit] NOT NULL DEFAULT(0),
    [LastLoginDate] [datetime] NULL,
    [LastIpAddress] [nvarchar](25) NULL,
    [LastSystemName] [nvarchar](50) NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Department_DepartmentId_Id] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Department] ([Id]);

ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Department_DepartmentId_Id];

ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Location_LocationId_Id] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id]);

ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Location_LocationId_Id];

ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_ScopeName_ScopeNameId_Id] FOREIGN KEY([ScopeNameId])
REFERENCES [dbo].[ScopeName] ([Id]);

ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_ScopeName_ScopeNameId_Id];

CREATE TABLE [dbo].[UserPassword]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [Password] [nvarchar](150) NOT NULL,
    [Salt] [nvarchar](40) NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_UserPassword] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[UserPassword]  WITH CHECK ADD  CONSTRAINT [FK_UserPassword_User_Id] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id]);

ALTER TABLE [dbo].[UserPassword] CHECK CONSTRAINT [FK_UserPassword_User_Id];

CREATE TABLE [dbo].[SecurityRole]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [ShortName] [nvarchar](50) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [IsSystemRole] [bit] NOT NULL DEFAULT(0),
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_SecurityRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[UserRole]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [RoleId] [int] NOT NULL,
    [CreatedBy] [int] NULL,
    [CreatedDate] [datetime] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedDate] [datetime] NULL,
    [DeletedBy] [int] NULL,
    [DeletedDate] [datetime] NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_SecurityRole_RoleId_Id] FOREIGN KEY([RoleId])
REFERENCES [dbo].[SecurityRole] ([Id])
GO

ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_SecurityRole_RoleId_Id]
GO

ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK_UserRole_User_UserId_Id] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO

ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK_UserRole_User_UserId_Id]
GO

