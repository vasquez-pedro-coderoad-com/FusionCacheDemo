-- =============================================
-- FusionCacheDemo Database Setup
-- =============================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FusionCacheDemo')
BEGIN
    CREATE DATABASE FusionCacheDemo;
END
GO

USE FusionCacheDemo;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Accounts Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE Accounts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0
    );
END
GO

-- Drivers Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Drivers]') AND type in (N'U'))
BEGIN
    CREATE TABLE Drivers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        TruckId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- =============================================
-- Seed Data (Optional)
-- =============================================

-- Sample Accounts
IF NOT EXISTS (SELECT 1 FROM Accounts)
BEGIN
    INSERT INTO Accounts (Name, Balance) VALUES
        ('Cuenta Principal', 10000.00),
        ('Cuenta Secundaria', 5000.50),
        ('Cuenta Ahorros', 25000.00);
END
GO

-- Sample Drivers
IF NOT EXISTS (SELECT 1 FROM Drivers)
BEGIN
    INSERT INTO Drivers (Name, TruckId, IsActive) VALUES
        ('Juan Perez', 101, 1),
        ('Maria Garcia', 102, 1),
        ('Carlos Rodriguez', 103, 0),
        ('Ana Martinez', 104, 1);
END
GO

PRINT 'Database FusionCacheDemo created successfully.';
GO
