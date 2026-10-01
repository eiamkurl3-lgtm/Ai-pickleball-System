-- ======================================
-- Players + PlayerTournament tables + stored procedures
-- Target: SQL Server 2012 (PickleballDB)
-- Re-runnable: procs are dropped defensively
-- because SQL 2012 has no "DROP ... IF EXISTS".
-- Run after Identity_StoredProcedures.sql (needs Users) and
-- Tournament_StoredProcedures.sql (needs Tournaments).
-- ======================================
USE PickleballDB;
GO

IF NOT EXISTS (SELECT * FROM sys.types WHERE name='IntArray' AND is_table_type=1)
CREATE TYPE dbo.IntArray AS TABLE (Id INT);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Players' AND xtype='U')
CREATE TABLE Players (
    -- SQL 2012 requires single-line inline PK constraint (multi-line parens rejected)
    Id INT PRIMARY KEY IDENTITY(1,1),
    -- Identity user this profile belongs to (Users.Id)
    UserId NVARCHAR(450) NOT NULL,
    -- Mirrors Users.UserName (NVARCHAR(256)) so a profile carries a display name
    UserName NVARCHAR(256) NOT NULL,
    DateOfBirth DATETIME NULL,
    -- Self-rating on the usual 1.0 - 7.0 scale
    SkillLevel DECIMAL(4,1) NULL,
    Status NVARCHAR(50) NULL
);
GO

-- ======================================
-- Migration: databases created before UserName replaced
-- FirstName / LastName / Email. Re-runnable, and a no-op once
-- the table already matches the current shape.
-- SQL 2012 has no ADD/DROP COLUMN IF EXISTS, hence sys.columns guards.
-- ======================================
-- Adding the column must be its own batch: SQL Server binds every statement in a
-- batch before running any of them, so a later reference to the new column in the
-- same batch fails with "Invalid column name 'UserName'".
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'UserName')
    ALTER TABLE Players ADD UserName NVARCHAR(256) NULL;
GO

-- Seed the display name from the owning account so existing rows survive NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'UserName')
BEGIN
    UPDATE p SET p.UserName = u.UserName
    FROM Players p
    INNER JOIN Users u ON u.Id = p.UserId
    WHERE p.UserName IS NULL;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'UserName' AND is_nullable = 1)
        ALTER TABLE Players ALTER COLUMN UserName NVARCHAR(256) NOT NULL;
END;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'FirstName')
    ALTER TABLE Players DROP COLUMN FirstName;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'LastName')
    ALTER TABLE Players DROP COLUMN LastName;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Players') AND name = 'Email')
    ALTER TABLE Players DROP COLUMN Email;
GO

-- One profile per user
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UQ_Players_UserId')
ALTER TABLE Players WITH CHECK ADD CONSTRAINT UQ_Players_UserId
    UNIQUE (UserId);
GO

-- CASCADE: a profile is dependent data, it should die with the account
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Players_Users' AND xtype='F')
ALTER TABLE Players WITH CHECK ADD CONSTRAINT FK_Players_Users
    FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PlayerTournament' AND xtype='U')
CREATE TABLE PlayerTournament (
    PlayerId INT NOT NULL,
    TournamentId INT NOT NULL,
    RegisteredAt DATETIME NOT NULL CONSTRAINT DF_PlayerTournament_RegisteredAt DEFAULT GETUTCDATE(),
    PRIMARY KEY (PlayerId, TournamentId)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_PlayerTournament_Players' AND xtype='F')
ALTER TABLE PlayerTournament WITH CHECK ADD CONSTRAINT FK_PlayerTournament_Players
    FOREIGN KEY (PlayerId) REFERENCES Players (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_PlayerTournament_Tournaments' AND xtype='F')
ALTER TABLE PlayerTournament WITH CHECK ADD CONSTRAINT FK_PlayerTournament_Tournaments
    FOREIGN KEY (TournamentId) REFERENCES Tournaments (Id) ON DELETE CASCADE;
GO

-- ======================================
-- Stored procedures
-- ======================================

IF OBJECT_ID('dbo.Create_Player', 'P') IS NOT NULL DROP PROCEDURE dbo.Create_Player;
GO
CREATE PROCEDURE Create_Player
    @UserId NVARCHAR(450),
    @UserName NVARCHAR(256),
    @DateOfBirth DATETIME,
    @SkillLevel DECIMAL(4,1),
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Players (UserId, UserName, DateOfBirth, SkillLevel, Status)
    VALUES (@UserId, @UserName, @DateOfBirth, @SkillLevel, @Status);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
GO

IF OBJECT_ID('dbo.Player_IfExists', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_IfExists;
GO
CREATE PROCEDURE Player_IfExists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Players WHERE Id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

IF OBJECT_ID('dbo.Player_GetAll', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_GetAll;
GO
CREATE PROCEDURE Player_GetAll
    @Page INT,
    @RecordsPerPage INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM Players
    ORDER BY UserName
    OFFSET (@Page - 1) * @RecordsPerPage ROWS
    FETCH NEXT @RecordsPerPage ROWS ONLY;
END;
GO

IF OBJECT_ID('dbo.Player_Count', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_Count;
GO
CREATE PROCEDURE Player_Count
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) FROM Players;
END;
GO

IF OBJECT_ID('dbo.Player_GetByID', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_GetByID;
GO
CREATE PROCEDURE Player_GetByID
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM Players WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Player_GetByUserId', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_GetByUserId;
GO
CREATE PROCEDURE Player_GetByUserId
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM Players WHERE UserId = @UserId;
END;
GO

IF OBJECT_ID('dbo.Update_Player', 'P') IS NOT NULL DROP PROCEDURE dbo.Update_Player;
GO
-- The API only exposes SkillLevel and Status on update; @UserId, @UserName and
-- @DateOfBirth are passed back as already stored so this statement stays a no-op
-- for them and can never collide with UQ_Players_UserId.
CREATE PROCEDURE Update_Player
    @Id INT,
    @UserId NVARCHAR(450),
    @UserName NVARCHAR(256),
    @DateOfBirth DATETIME,
    @SkillLevel DECIMAL(4,1),
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Players
    SET
        UserId = @UserId,
        UserName = @UserName,
        DateOfBirth = @DateOfBirth,
        SkillLevel = @SkillLevel,
        Status = @Status
    WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Player_Delete', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_Delete;
GO
CREATE PROCEDURE Player_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Players WHERE Id = @Id;
END;
GO

-- Replaces the whole registration set of the player
IF OBJECT_ID('dbo.Player_AssignTournaments', 'P') IS NOT NULL DROP PROCEDURE dbo.Player_AssignTournaments;
GO
CREATE PROCEDURE Player_AssignTournaments
    @PlayerId INT,
    @TournamentIds dbo.IntArray READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM PlayerTournament WHERE PlayerId = @PlayerId;

    INSERT INTO PlayerTournament (PlayerId, TournamentId)
    SELECT @PlayerId, Id FROM @TournamentIds;
END;
GO
