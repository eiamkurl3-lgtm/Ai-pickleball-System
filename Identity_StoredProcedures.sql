-- =============================================
-- ASP.NET Core Identity Tables and Stored Procedures
-- Target database: PickleballDB
--
-- NOTE: every table-level PRIMARY KEY constraint is written on a single line.
-- The multi-line "( \n [Id] ASC \n )" form used by the stock Identity+Tables.sql
-- script fails to parse on SQL Server 2012 with "Incorrect syntax near ')'".
-- The single-line form is semantically identical.
--
-- This script is idempotent: every object is guarded by an existence check,
-- so it can safely be re-run against an existing PickleballDB.
-- =============================================

-- Create the database if it does not exist yet
IF DB_ID('PickleballDB') IS NULL
BEGIN
    CREATE DATABASE PickleballDB;
END;
GO

USE PickleballDB;
GO

/****** Object:  Table [dbo].[Roles]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Roles' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[Roles](
        [Id] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](256) NULL,
        [NormalizedName] [nvarchar](256) NULL,
        [ConcurrencyStamp] [nvarchar](max) NULL,
     CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
    )
END
GO

/****** Object:  Table [dbo].[RolesClaims]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'RolesClaims' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[RolesClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RoleId] [nvarchar](450) NOT NULL,
        [ClaimType] [nvarchar](max) NULL,
        [ClaimValue] [nvarchar](max) NULL,
     CONSTRAINT [PK_RolesClaims] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
    )
END
GO

/****** Object:  Table [dbo].[Users]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'Users' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [nvarchar](450) NOT NULL,
        [UserName] [nvarchar](256) NULL,
        [NormalizedUserName] [nvarchar](256) NULL,
        [Email] [nvarchar](256) NULL,
        [NormalizedEmail] [nvarchar](256) NULL,
        [EmailConfirmed] [bit] NOT NULL,
        [PasswordHash] [nvarchar](max) NULL,
        [SecurityStamp] [nvarchar](max) NULL,
        [ConcurrencyStamp] [nvarchar](max) NULL,
        [PhoneNumber] [nvarchar](max) NULL,
        [PhoneNumberConfirmed] [bit] NOT NULL,
        [TwoFactorEnabled] [bit] NOT NULL,
        [LockoutEnd] [datetimeoffset](7) NULL,
        [LockoutEnabled] [bit] NOT NULL,
        [AccessFailedCount] [int] NOT NULL,
     CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
    )
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DF_Users_EmailConfirmed' AND xtype = 'D')
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_EmailConfirmed] DEFAULT ('false') FOR [EmailConfirmed]
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DF_Users_PhoneNumberConfirmed' AND xtype = 'D')
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_PhoneNumberConfirmed] DEFAULT ('false') FOR [PhoneNumberConfirmed]
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DF_Users_TwoFactorEnabled' AND xtype = 'D')
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_TwoFactorEnabled] DEFAULT ('false') FOR [TwoFactorEnabled]
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DF_Users_LockoutEnabled' AND xtype = 'D')
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_LockoutEnabled] DEFAULT ('false') FOR [LockoutEnabled]
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'DF_Users_AccessFailedCount' AND xtype = 'D')
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_AccessFailedCount] DEFAULT ((0)) FOR [AccessFailedCount]
GO

/****** Object:  Table [dbo].[UsersClaims]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'UsersClaims' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[UsersClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ClaimType] [nvarchar](max) NULL,
        [ClaimValue] [nvarchar](max) NULL,
     CONSTRAINT [PK_UsersClaims] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
    )
END
GO

/****** Object:  Table [dbo].[UsersLogins]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'UsersLogins' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[UsersLogins](
        [LoginProvider] [nvarchar](450) NOT NULL,
        [ProviderKey] [nvarchar](450) NOT NULL,
        [ProviderDisplayName] [nvarchar](max) NULL,
        [UserId] [nvarchar](450) NOT NULL,
     CONSTRAINT [PK_UsersLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC) ON [PRIMARY]
    )
END
GO

/****** Object:  Table [dbo].[UsersRoles]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'UsersRoles' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[UsersRoles](
        [UserId] [nvarchar](450) NOT NULL,
        [RoleId] [nvarchar](450) NOT NULL,
     CONSTRAINT [PK_UsersRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC) ON [PRIMARY]
    )
END
GO

/****** Object:  Table [dbo].[UsersTokens]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'UsersTokens' AND xtype = 'U')
BEGIN
    CREATE TABLE [dbo].[UsersTokens](
        [UserId] [nvarchar](450) NOT NULL,
        [LoginProvider] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](450) NOT NULL,
        [Value] [nvarchar](max) NULL,
     CONSTRAINT [PK_UsersTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC) ON [PRIMARY]
    )
END
GO

-- Foreign Key Constraints
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_RolesClaims_Roles_RoleId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[RolesClaims] WITH CHECK ADD CONSTRAINT [FK_RolesClaims_Roles_RoleId] FOREIGN KEY([RoleId])
    REFERENCES [dbo].[Roles] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[RolesClaims] CHECK CONSTRAINT [FK_RolesClaims_Roles_RoleId]
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_UsersClaims_Users_UserId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[UsersClaims] WITH CHECK ADD CONSTRAINT [FK_UsersClaims_Users_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[UsersClaims] CHECK CONSTRAINT [FK_UsersClaims_Users_UserId]
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_UsersLogins_Users_UserId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[UsersLogins] WITH CHECK ADD CONSTRAINT [FK_UsersLogins_Users_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[UsersLogins] CHECK CONSTRAINT [FK_UsersLogins_Users_UserId]
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_UsersRoles_Roles_RoleId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[UsersRoles] WITH CHECK ADD CONSTRAINT [FK_UsersRoles_Roles_RoleId] FOREIGN KEY([RoleId])
    REFERENCES [dbo].[Roles] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[UsersRoles] CHECK CONSTRAINT [FK_UsersRoles_Roles_RoleId]
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_UsersRoles_Users_UserId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[UsersRoles] WITH CHECK ADD CONSTRAINT [FK_UsersRoles_Users_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[UsersRoles] CHECK CONSTRAINT [FK_UsersRoles_Users_UserId]
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = N'FK_UsersTokens_Users_UserId' AND xtype = 'F')
BEGIN
    ALTER TABLE [dbo].[UsersTokens] WITH CHECK ADD CONSTRAINT [FK_UsersTokens_Users_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE CASCADE
    ALTER TABLE [dbo].[UsersTokens] CHECK CONSTRAINT [FK_UsersTokens_Users_UserId]
END
GO

-- Unique indexes enforce username/email uniqueness at the data layer.
-- These are FILTERED indexes: users register with a username and no email, and a
-- plain unique index would treat every NULL email as the same value (SQL Server
-- permits only one NULL per unique index), so the second email-less registration
-- would fail with a duplicate-key error.
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_Users_NormalizedUserName' AND object_id = OBJECT_ID(N'Users'))
DROP INDEX UX_Users_NormalizedUserName ON [dbo].[Users];
GO
CREATE UNIQUE INDEX UX_Users_NormalizedUserName ON [dbo].[Users]([NormalizedUserName])
WHERE [NormalizedUserName] IS NOT NULL;
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_Users_NormalizedEmail' AND object_id = OBJECT_ID(N'Users'))
DROP INDEX UX_Users_NormalizedEmail ON [dbo].[Users];
GO
CREATE UNIQUE INDEX UX_Users_NormalizedEmail ON [dbo].[Users]([NormalizedEmail])
WHERE [NormalizedEmail] IS NOT NULL;
GO

-- =============================================
-- Stored Procedures
-- =============================================

-- Users_Create
IF OBJECT_ID('dbo.Users_Create', 'P') IS NOT NULL DROP PROCEDURE dbo.Users_Create;
GO
CREATE PROCEDURE Users_Create
	@Id NVARCHAR(450),
	@Email NVARCHAR(256),
	@NormalizedEmail NVARCHAR(256),
	@UserName NVARCHAR(256),
	@NormalizedUserName NVARCHAR(256),
	@PasswordHash NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Users(Id, Email, NormalizedEmail, UserName, NormalizedUserName, PasswordHash)
	VALUES (@Id, @Email, @NormalizedEmail, @UserName, @NormalizedUserName, @PasswordHash);
END;
GO

-- Users_GetByEmail
IF OBJECT_ID('dbo.Users_GetByEmail', 'P') IS NOT NULL DROP PROCEDURE dbo.Users_GetByEmail;
GO
CREATE PROCEDURE Users_GetByEmail
	@NormalizedEmail NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM Users WHERE NormalizedEmail = @NormalizedEmail;
END;
GO

-- Users_GetByUserName
IF OBJECT_ID('dbo.Users_GetByUserName', 'P') IS NOT NULL DROP PROCEDURE dbo.Users_GetByUserName;
GO
CREATE PROCEDURE Users_GetByUserName
	@NormalizedUserName NVARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM Users WHERE NormalizedUserName = @NormalizedUserName;
END;
GO

-- Users_GetClaims
IF OBJECT_ID('dbo.Users_GetClaims', 'P') IS NOT NULL DROP PROCEDURE dbo.Users_GetClaims;
GO
CREATE PROCEDURE Users_GetClaims
	@Id NVARCHAR(450)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT ClaimType AS [Type], ClaimValue AS [Value]
	FROM UsersClaims
	WHERE UserId = @Id;
END;
GO

-- Users_GetById
IF OBJECT_ID('dbo.Users_GetById', 'P') IS NOT NULL DROP PROCEDURE dbo.Users_GetById;
GO
CREATE PROCEDURE Users_GetById
	@Id NVARCHAR(450)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM Users WHERE Id = @Id;
END;
GO