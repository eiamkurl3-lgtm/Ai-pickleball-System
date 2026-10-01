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
    -- Side A = Player1 (+ Player2 when doubles), Side B = Player3 (+ Player4 when doubles)
    Player1Id NVARCHAR(450) NOT NULL,
    Player2Id NVARCHAR(450) NOT NULL,
    Player3Id NVARCHAR(450) NULL,
    Player4Id NVARCHAR(450) NULL,
    ScheduledStart DATETIME NOT NULL,
    Court NVARCHAR(100) NULL,
    Status NVARCHAR(50) NULL,
    -- Aggregate points won by each side across the whole match
    SideAScore INT NULL,
    SideBScore INT NULL,
    WinnerSide NVARCHAR(1) NULL
);
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

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Matches_Player3' AND xtype='F')
ALTER TABLE Matches WITH CHECK ADD CONSTRAINT FK_Matches_Player3
    FOREIGN KEY (Player3Id) REFERENCES Users (Id);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FK_Matches_Player4' AND xtype='F')
ALTER TABLE Matches WITH CHECK ADD CONSTRAINT FK_Matches_Player4
    FOREIGN KEY (Player4Id) REFERENCES Users (Id);
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
    @Player3Id NVARCHAR(450),
    @Player4Id NVARCHAR(450),
    @ScheduledStart DATETIME,
    @Court NVARCHAR(100),
    @Status NVARCHAR(50),
    @SideAScore INT,
    @SideBScore INT,
    @WinnerSide NVARCHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Matches (TournamentId, Player1Id, Player2Id, Player3Id, Player4Id,
                         ScheduledStart, Court, Status, SideAScore, SideBScore, WinnerSide)
    VALUES (@TournamentId, @Player1Id, @Player2Id, @Player3Id, @Player4Id,
            @ScheduledStart, @Court, @Status, @SideAScore, @SideBScore, @WinnerSide);

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
           m.Player1Id, p1.Email AS Player1Email,
           m.Player2Id, p2.Email AS Player2Email,
           m.Player3Id, p3.Email AS Player3Email,
           m.Player4Id, p4.Email AS Player4Email,
           m.ScheduledStart, m.Court, m.Status,
           m.SideAScore, m.SideBScore, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    LEFT JOIN Users p3 ON p3.Id = m.Player3Id
    LEFT JOIN Users p4 ON p4.Id = m.Player4Id
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
           m.Player1Id, p1.Email AS Player1Email,
           m.Player2Id, p2.Email AS Player2Email,
           m.Player3Id, p3.Email AS Player3Email,
           m.Player4Id, p4.Email AS Player4Email,
           m.ScheduledStart, m.Court, m.Status,
           m.SideAScore, m.SideBScore, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    LEFT JOIN Users p3 ON p3.Id = m.Player3Id
    LEFT JOIN Users p4 ON p4.Id = m.Player4Id
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
           m.Player1Id, p1.Email AS Player1Email,
           m.Player2Id, p2.Email AS Player2Email,
           m.Player3Id, p3.Email AS Player3Email,
           m.Player4Id, p4.Email AS Player4Email,
           m.ScheduledStart, m.Court, m.Status,
           m.SideAScore, m.SideBScore, m.WinnerSide
    FROM Matches m
    LEFT JOIN Users p1 ON p1.Id = m.Player1Id
    LEFT JOIN Users p2 ON p2.Id = m.Player2Id
    LEFT JOIN Users p3 ON p3.Id = m.Player3Id
    LEFT JOIN Users p4 ON p4.Id = m.Player4Id
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
    @Player3Id NVARCHAR(450),
    @Player4Id NVARCHAR(450),
    @ScheduledStart DATETIME,
    @Court NVARCHAR(100),
    @Status NVARCHAR(50),
    @SideAScore INT,
    @SideBScore INT,
    @WinnerSide NVARCHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Matches
    SET
        TournamentId = @TournamentId,
        Player1Id = @Player1Id,
        Player2Id = @Player2Id,
        Player3Id = @Player3Id,
        Player4Id = @Player4Id,
        ScheduledStart = @ScheduledStart,
        Court = @Court,
        Status = @Status,
        SideAScore = @SideAScore,
        SideBScore = @SideBScore,
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
