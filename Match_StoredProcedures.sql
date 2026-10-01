-- ======================================
-- Matches table + stored procedures
-- Target: SQL Server 2012 (PickleballDB)
-- Re-runnable: procs are dropped defensively
-- because SQL 2012 has no "DROP ... IF EXISTS".
-- ======================================
USE PickleballDB;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Matches' AND xtype='U')
CREATE TABLE Matches (
    -- SQL 2012 requires single-line inline PK constraint (multi-line parens rejected)
    Id INT PRIMARY KEY IDENTITY(1,1),
    TournamentId INT NOT NULL,
    -- Side A = Player1, Side B = Player2
    Player1Id NVARCHAR(450) NOT NULL,
    Player2Id NVARCHAR(450) NOT NULL,
    ScheduledStart DATETIME NOT NULL,
    Status NVARCHAR(50) NULL,
    -- Aggregate points won by each side across the whole match
    SideAScore INT NOT NULL,
    SideBScore INT NOT NULL,
    -- Retained for historical rows only: the create payload no longer carries them
    Court NVARCHAR(100) NULL,
    WinnerSide NVARCHAR(1) NULL
);
GO

-- ======================================
-- Migration: singles-only model
-- Drops the third and fourth player slots and tightens the scores
-- so the non-nullable C# entity can always be materialised.
-- ======================================
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Matches_Player3')
    ALTER TABLE Matches DROP CONSTRAINT FK_Matches_Player3;
GO
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Matches_Player4')
    ALTER TABLE Matches DROP CONSTRAINT FK_Matches_Player4;
GO
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Matches') AND name = 'Player3Id')
    ALTER TABLE Matches DROP COLUMN Player3Id;
GO
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Matches') AND name = 'Player4Id')
    ALTER TABLE Matches DROP COLUMN Player4Id;
GO
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Matches') AND name = 'SideAScore' AND is_nullable = 1)
BEGIN
    UPDATE Matches SET SideAScore = 0 WHERE SideAScore IS NULL;
    UPDATE Matches SET SideBScore = 0 WHERE SideBScore IS NULL;

    ALTER TABLE Matches ALTER COLUMN SideAScore INT NOT NULL;
    ALTER TABLE Matches ALTER COLUMN SideBScore INT NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Matches_Tournaments' AND xtype='F')
ALTER TABLE Matches WITH CHECK ADD CONSTRAINT FK_Matches_Tournaments
    FOREIGN KEY (TournamentId) REFERENCES Tournaments (Id) ON DELETE CASCADE;
GO

-- Player references are NO ACTION on purpose: deleting a user who already
-- played must fail loudly rather than silently rewrite match history.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Matches_Player1' AND xtype='F')
ALTER TABLE Matches WITH CHECK ADD CONSTRAINT FK_Matches_Player1
    FOREIGN KEY (Player1Id) REFERENCES Users (Id);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Matches_Player2' AND xtype='F')
ALTER TABLE Matches WITH CHECK ADD CONSTRAINT FK_Matches_Player2
    FOREIGN KEY (Player2Id) REFERENCES Users (Id);
GO

-- ======================================
-- Stored procedures
-- ======================================

IF OBJECT_ID('dbo.Create_Match', 'P') IS NOT NULL DROP PROCEDURE dbo.Create_Match;
GO
CREATE PROCEDURE Create_Match
    @TournamentId INT,
    @Player1Id NVARCHAR(450),
    @Player2Id NVARCHAR(450),
    @ScheduledStart DATETIME,
    @Status NVARCHAR(50),
    @SideAScore INT,
    @SideBScore INT,
    @Court NVARCHAR(100),
    @WinnerSide NVARCHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Matches (TournamentId, Player1Id, Player2Id,
                         ScheduledStart, Status, SideAScore, SideBScore, Court, WinnerSide)
    VALUES (@TournamentId, @Player1Id, @Player2Id,
            @ScheduledStart, @Status, @SideAScore, @SideBScore, @Court, @WinnerSide);

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
GO

IF OBJECT_ID('dbo.Match_IfExists', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_IfExists;
GO
CREATE PROCEDURE Match_IfExists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Matches WHERE Id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

IF OBJECT_ID('dbo.Match_GetAll', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_GetAll;
GO
CREATE PROCEDURE Match_GetAll
    @Page INT,
    @RecordsPerPage INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.Id, m.TournamentId,
           m.Player1Id, p1.UserName AS Player1UserName,
           m.Player2Id, p2.UserName AS Player2UserName,
           m.ScheduledStart, m.Status,
           m.SideAScore, m.SideBScore, m.Court, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    ORDER BY m.ScheduledStart, m.Id
    OFFSET (@Page - 1) * @RecordsPerPage ROWS
    FETCH NEXT @RecordsPerPage ROWS ONLY;
END;
GO

IF OBJECT_ID('dbo.Match_Count', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_Count;
GO
CREATE PROCEDURE Match_Count
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) FROM Matches;
END;
GO

IF OBJECT_ID('dbo.Match_GetByID', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_GetByID;
GO
CREATE PROCEDURE Match_GetByID
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.Id, m.TournamentId,
           m.Player1Id, p1.UserName AS Player1UserName,
           m.Player2Id, p2.UserName AS Player2UserName,
           m.ScheduledStart, m.Status,
           m.SideAScore, m.SideBScore, m.Court, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    WHERE m.Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Match_GetByTournament', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_GetByTournament;
GO
CREATE PROCEDURE Match_GetByTournament
    @TournamentId INT,
    @Page INT,
    @RecordsPerPage INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.Id, m.TournamentId,
           m.Player1Id, p1.UserName AS Player1UserName,
           m.Player2Id, p2.UserName AS Player2UserName,
           m.ScheduledStart, m.Status,
           m.SideAScore, m.SideBScore, m.Court, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    WHERE m.TournamentId = @TournamentId
    ORDER BY m.ScheduledStart, m.Id
    OFFSET (@Page - 1) * @RecordsPerPage ROWS
    FETCH NEXT @RecordsPerPage ROWS ONLY;
END;
GO

IF OBJECT_ID('dbo.Match_CountByTournament', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_CountByTournament;
GO
CREATE PROCEDURE Match_CountByTournament
    @TournamentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) FROM Matches WHERE TournamentId = @TournamentId;
END;
GO

IF OBJECT_ID('dbo.Update_Match', 'P') IS NOT NULL DROP PROCEDURE dbo.Update_Match;
GO
CREATE PROCEDURE Update_Match
    @Id INT,
    @TournamentId INT,
    @Player1Id NVARCHAR(450),
    @Player2Id NVARCHAR(450),
    @ScheduledStart DATETIME,
    @Status NVARCHAR(50),
    @SideAScore INT,
    @SideBScore INT,
    @Court NVARCHAR(100),
    @WinnerSide NVARCHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Matches
    SET
        TournamentId = @TournamentId,
        Player1Id = @Player1Id,
        Player2Id = @Player2Id,
        ScheduledStart = @ScheduledStart,
        Status = @Status,
        SideAScore = @SideAScore,
        SideBScore = @SideBScore,
        Court = @Court,
        WinnerSide = @WinnerSide
    WHERE Id = @Id;
END;
GO

IF OBJECT_ID('dbo.Match_Delete', 'P') IS NOT NULL DROP PROCEDURE dbo.Match_Delete;
GO
CREATE PROCEDURE Match_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Matches WHERE Id = @Id;
END;
GO
