-- ============================================================================
-- MyWallet Database Schema - Enhanced Version
-- ============================================================================

-- ============================================================================
-- 1. REFRESH TOKENS TABLE
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_RefreshTokens')
BEGIN
    CREATE TABLE tbl_RefreshTokens
    (
        TokenId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsRevoked BIT NOT NULL DEFAULT 0,
        RevokedAt DATETIME2 NULL,
        
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) 
            REFERENCES tbl_Users(UserId) ON DELETE CASCADE,
            
        INDEX IX_RefreshTokens_UserId (UserId),
        INDEX IX_RefreshTokens_Token (Token),
        INDEX IX_RefreshTokens_ExpiresAt (ExpiresAt)
    );
    
    PRINT 'tbl_RefreshTokens table created successfully';
END
ELSE
BEGIN
    PRINT 'tbl_RefreshTokens table already exists';
END
GO

-- ============================================================================
-- 2. API KEYS TABLE (Optional - for managing multiple API keys)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_ApiKeys')
BEGIN
    CREATE TABLE tbl_ApiKeys
    (
        ApiKeyId INT IDENTITY(1,1) PRIMARY KEY,
        KeyName NVARCHAR(100) NOT NULL,
        KeyValue NVARCHAR(500) NOT NULL UNIQUE,
        CompanyId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL,
        LastUsedAt DATETIME2 NULL,
        UsageCount INT NOT NULL DEFAULT 0,
        
        CONSTRAINT FK_ApiKeys_Company FOREIGN KEY (CompanyId) 
            REFERENCES tbl_Company(CompanyId) ON DELETE SET NULL,
            
        INDEX IX_ApiKeys_KeyValue (KeyValue),
        INDEX IX_ApiKeys_IsActive (IsActive)
    );
    
    PRINT 'tbl_ApiKeys table created successfully';
END
ELSE
BEGIN
    PRINT 'tbl_ApiKeys table already exists';
END
GO

-- ============================================================================
-- 3. AUDIT LOG TABLE
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_AuditLog')
BEGIN
    CREATE TABLE tbl_AuditLog
    (
        LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        Action NVARCHAR(100) NOT NULL,
        Entity NVARCHAR(100) NULL,
        EntityId INT NULL,
        OldValue NVARCHAR(MAX) NULL,
        NewValue NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        TraceId NVARCHAR(50) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        INDEX IX_AuditLog_UserId (UserId),
        INDEX IX_AuditLog_Timestamp (Timestamp),
        INDEX IX_AuditLog_Action (Action)
    );
    
    PRINT 'tbl_AuditLog table created successfully';
END
ELSE
BEGIN
    PRINT 'tbl_AuditLog table already exists';
END
GO

-- ============================================================================
-- 4. LOGIN ATTEMPTS TABLE (for rate limiting/security)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_LoginAttempts')
BEGIN
    CREATE TABLE tbl_LoginAttempts
    (
        AttemptId BIGINT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        IpAddress NVARCHAR(50) NOT NULL,
        IsSuccessful BIT NOT NULL,
        FailureReason NVARCHAR(200) NULL,
        AttemptedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        INDEX IX_LoginAttempts_Username (Username),
        INDEX IX_LoginAttempts_IpAddress (IpAddress),
        INDEX IX_LoginAttempts_AttemptedAt (AttemptedAt)
    );
    
    PRINT 'tbl_LoginAttempts table created successfully';
END
ELSE
BEGIN
    PRINT 'tbl_LoginAttempts table already exists';
END
GO

-- ============================================================================
-- 5. SAMPLE DATA - Insert test user (if not exists)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM tbl_Users WHERE Username = 'admin')
BEGIN
    -- Note: In production, use proper password hashing (bcrypt, PBKDF2, etc.)
    -- This is just for demonstration
    INSERT INTO tbl_Users (FullName, Username, PasswordHash, RoleId, CompanyId, DOB, Gender, Email, MobileNo, IssuedDate_From, IsActive, CreatedAt)
    VALUES (
        'Admin User',
        'admin',
        'admin123', -- In production: use hashed password
        1, -- Assuming RoleId 1 is Admin
        1, -- Assuming CompanyId 1 exists
        '1990-01-01',
        'M',
        'admin@mywallet.com',
        '1234567890',
        GETUTCDATE(),
        1,
        GETUTCDATE()
    );
    
    PRINT 'Sample admin user created: Username=admin, Password=admin123';
END
GO

-- ============================================================================
-- 6. STORED PROCEDURES
-- ============================================================================

-- Procedure to check user account status
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CheckUserAccountStatus')
    DROP PROCEDURE sp_CheckUserAccountStatus;
GO

CREATE PROCEDURE sp_CheckUserAccountStatus
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Username,
        u.IsActive,
        u.IssuedDate_From,
        u.IssuedDate_To,
        CASE 
            WHEN u.IsActive = 0 THEN 'Inactive'
            WHEN u.IssuedDate_To IS NOT NULL AND u.IssuedDate_To < GETUTCDATE() THEN 'Expired'
            ELSE 'Active'
        END AS AccountStatus
    FROM tbl_Users u
    WHERE u.UserId = @UserId;
END
GO

-- Procedure to get user login statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserLoginStats')
    DROP PROCEDURE sp_GetUserLoginStats;
GO

CREATE PROCEDURE sp_GetUserLoginStats
    @UserId INT,
    @Days INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Username NVARCHAR(100);
    SELECT @Username = Username FROM tbl_Users WHERE UserId = @UserId;
    
    SELECT 
        COUNT(*) AS TotalAttempts,
        SUM(CASE WHEN IsSuccessful = 1 THEN 1 ELSE 0 END) AS SuccessfulLogins,
        SUM(CASE WHEN IsSuccessful = 0 THEN 1 ELSE 0 END) AS FailedAttempts,
        MAX(AttemptedAt) AS LastLoginAttempt
    FROM tbl_LoginAttempts
    WHERE Username = @Username
        AND AttemptedAt >= DATEADD(DAY, -@Days, GETUTCDATE());
END
GO

-- ============================================================================
-- 7. VIEWS FOR REPORTING
-- ============================================================================

-- View for active refresh tokens
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ActiveRefreshTokens')
    DROP VIEW vw_ActiveRefreshTokens;
GO

CREATE VIEW vw_ActiveRefreshTokens
AS
SELECT 
    t.TokenId,
    t.UserId,
    u.Username,
    u.FullName,
    u.Email,
    t.CreatedAt,
    t.ExpiresAt,
    DATEDIFF(HOUR, GETUTCDATE(), t.ExpiresAt) AS HoursUntilExpiry
FROM tbl_RefreshTokens t
INNER JOIN tbl_Users u ON t.UserId = u.UserId
WHERE t.IsRevoked = 0 
    AND t.ExpiresAt > GETUTCDATE();
GO

PRINT 'Database schema creation completed successfully!';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Update connection string in appsettings.json';
PRINT '2. Create proper user roles and permissions';
PRINT '3. Implement password hashing in production';
PRINT '4. Configure backup and maintenance jobs';
GO
