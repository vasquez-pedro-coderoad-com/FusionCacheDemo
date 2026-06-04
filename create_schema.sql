USE [FusionCacheDemo]
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Accounts')
BEGIN
    CREATE TABLE [dbo].[Accounts] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] TEXT NOT NULL,
        [Balance] DECIMAL NOT NULL
    )
    CREATE INDEX [IX_Accounts] ON [dbo].[Accounts] ([Id])
END
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Drivers')
BEGIN
    CREATE TABLE [dbo].[Drivers] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] TEXT NOT NULL,
		[TruckId] INT NOT NULL,
        [IsActive] BIT NOT NULL
    )
    CREATE INDEX [IX_Drivers] ON [dbo].[Drivers] ([Id])
END
GO
