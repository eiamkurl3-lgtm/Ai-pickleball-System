-- ======================================
-- Tournaments table + stored procedures
-- Target: SQL Server 2012 (PickleballDB)
-- Re-runnable: procs are dropped defensively
-- because SQL 2012 has no "DROP ... IF EXISTS".
-- ======================================
USE PickleballDB;
GO

-- Table type used by Tournament_GetBySeveralIds to validate tournament ids in bulk
IF NOT EXISTS (SELECT * FROM sys.types WHERE name='IntArray' AND is_table_type=1)
CREATE TYPE dbo.IntArray AS TABLE (Id INT);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tournaments' AND xtype='U')
CREATE TABLE Tournaments (
    -- SQL 2012 requires single-line inline PK constraint (multi-line parens rejected)
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(150) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Location NVARCHAR(250) NULL,
    EntryFee DECIMAL(18, 2) NULL,
    MaxPlayers INT NULL,
    Status NVARCHAR(50) NULL
);
GO

IF OBJECT_ID('dbo.Create_Tournament', 'P') IS NOT NULL DROP PROCEDURE dbo.Create_Tournament;
GO
CREATE PROCEDURE Create_Tournament
    @Name NVARCHAR(150),
    @StartDate DATETIME,
    @EndDate DATETIME,
    @Location NVARCHAR(250),
    @EntryFee DECIMAL(18, 2),
    @MaxPlayers INT,
    @Status NVARCHAR(50)
AS
BEGIN
    INSERT INTO Tournaments (Name, StartDate, EndDate, Location, EntryFee, MaxPlayers, Status)
    VALUES (@Name, @StartDate, @EndDate, @Location, @EntryFee, @MaxPlayers, @Status);

    SELECT SCOPE_IDENTITY();
END;
GO

IF OBJECT_ID('dbo.Tournament_IfExists', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_IfExists;
GO
CREATE PROCEDURE Tournament_IfExists
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Tournaments WHERE Id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

IF OBJECT_ID('dbo.Tournament_GetAll', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_GetAll;
GO
CREATE PROCEDURE Tournament_GetAll
    @Page INT,
    @RecordsPerPage INT
AS
BEGIN
    SELECT * FROM Tournaments
    ORDER BY Name
    OFFSET (@Page - 1) * @RecordsPerPage ROWS
    FETCH NEXT @RecordsPerPage ROWS ONLY;
END;
GO

IF OBJECT_ID('dbo.Tournament_Count', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_Count;
GO
CREATE PROCEDURE Tournament_Count
AS
BEGIN
    SELECT COUNT(*) FROM Tournaments;
END;
GO

IF OBJECT_ID('dbo.Tournament_GetByID', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_GetByID;
GO
CREATE PROCEDURE Tournament_GetByID
    @Id INT
AS
BEGIN
    SELECT * FROM Tournaments WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Update_Tournament', 'P') IS NOT NULL DROP PROCEDURE dbo.Update_Tournament;
GO
CREATE PROCEDURE Update_Tournament
    @Id INT,
    @Name NVARCHAR(150),
    @StartDate DATETIME,
    @EndDate DATETIME,
    @Location NVARCHAR(250),
    @EntryFee DECIMAL(18, 2),
    @MaxPlayers INT,
    @Status NVARCHAR(50)
AS
BEGIN
    UPDATE Tournaments
    SET
        Name = @Name,
        StartDate = @StartDate,
        EndDate = @EndDate,
        Location = @Location,
        EntryFee = @EntryFee,
        MaxPlayers = @MaxPlayers,
        Status = @Status
    WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Tournament_Delete', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_Delete;
GO
CREATE PROCEDURE Tournament_Delete
    @Id INT
AS
BEGIN
    DELETE FROM Tournaments WHERE Id = @Id;
END;
GO

-- Returns only the ids that exist, so the caller can compare counts.
IF OBJECT_ID('dbo.Tournament_GetBySeveralIds', 'P') IS NOT NULL DROP PROCEDURE dbo.Tournament_GetBySeveralIds;
GO
CREATE PROCEDURE Tournament_GetBySeveralIds
    @TournamentIds dbo.IntArray READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id FROM Tournaments
    WHERE Id IN (SELECT Id FROM @TournamentIds);
END;
GO