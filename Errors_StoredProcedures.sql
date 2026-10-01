-- ======================================
-- Errors table + Errors_Create stored proc
-- Target: SQL Server 2012 (PickleballDB)
-- ======================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Errors' AND xtype='U')
CREATE TABLE Errors (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ErrorMessage NVARCHAR(MAX) NOT NULL,
    StackTrace NVARCHAR(MAX) NULL,
    Date DATETIME NOT NULL
);
GO

CREATE PROCEDURE Errors_Create
    @Id UNIQUEIDENTIFIER,
    @ErrorMessage NVARCHAR(MAX),
    @StackTrace NVARCHAR(MAX),
    @Date DATETIME
AS
BEGIN
    INSERT INTO Errors (Id, ErrorMessage, StackTrace, Date)
    VALUES (@Id, @ErrorMessage, @StackTrace, @Date);
END;
GO