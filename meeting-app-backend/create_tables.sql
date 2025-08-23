USE MeetingAppDb;

-- AppUsers tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AppUsers] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(256) NOT NULL UNIQUE,
        [Phone] NVARCHAR(20) NULL,
        [PasswordHash] VARBINARY(MAX) NOT NULL,
        [PasswordSalt] VARBINARY(MAX) NOT NULL,
        [ProfileImagePath] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'AppUsers tablosu oluşturuldu.';
END
ELSE
    PRINT 'AppUsers tablosu zaten var.';

-- Meetings tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meetings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Meetings] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [StartAt] DATETIME2 NOT NULL,
        [EndAt] DATETIME2 NOT NULL,
        [IsCancelled] BIT NOT NULL DEFAULT 0,
        [CancelledAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Meetings tablosu oluşturuldu.';
END
ELSE
    PRINT 'Meetings tablosu zaten var.';

-- MeetingDocuments tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeetingDocuments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MeetingDocuments] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [MeetingId] UNIQUEIDENTIFIER NOT NULL,
        [FileName] NVARCHAR(256) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [UploadedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_MeetingDocuments_Meetings] FOREIGN KEY ([MeetingId]) REFERENCES [Meetings]([Id]) ON DELETE CASCADE
    );
    PRINT 'MeetingDocuments tablosu oluşturuldu.';
END
ELSE
    PRINT 'MeetingDocuments tablosu zaten var.';

-- DeletedMeetingLogs tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeletedMeetingLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DeletedMeetingLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [MeetingId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [StartAt] DATETIME2 NOT NULL,
        [EndAt] DATETIME2 NOT NULL,
        [WasCancelled] BIT NOT NULL,
        [CancelledAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [DeletedBy] NVARCHAR(128) NULL
    );
    PRINT 'DeletedMeetingLogs tablosu oluşturuldu.';
END
ELSE
    PRINT 'DeletedMeetingLogs tablosu zaten var.';

-- Delete trigger oluşturma
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Meetings_Delete')
BEGIN
    EXEC('
    CREATE TRIGGER TR_Meetings_Delete
    ON Meetings
    AFTER DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        
        INSERT INTO DeletedMeetingLogs (
            MeetingId, 
            Title, 
            StartAt, 
            EndAt, 
            Description, 
            WasCancelled, 
            CancelledAt, 
            DeletedAt,
            DeletedBy
        )
        SELECT 
            d.Id,
            d.Title,
            d.StartAt,
            d.EndAt,
            d.Description,
            d.IsCancelled,
            d.CancelledAt,
            GETUTCDATE(),
            SYSTEM_USER
        FROM deleted d;
        
        PRINT ''Silinen toplantı log tablosuna kaydedildi'';
    END
    ');
    PRINT 'TR_Meetings_Delete trigger oluşturuldu.';
END
ELSE
    PRINT 'TR_Meetings_Delete trigger zaten var.';

SELECT 'Tüm tablolar ve trigger başarıyla oluşturuldu.' AS Result;
