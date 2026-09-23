-- ============================================================
--  01_seed_data.sql - initial data, hand-written
--  Run after dotnet ef database update
--
--  Roles:
--    - Staff   : full system access
--    - Student : student access
--
--  Default admin account:
--    UserCode : admin
--    Email    : admin@staff.avepoint.com
--    Role     : Staff
--
--  Default password:
--    Admin@123
--    Change it after first login.
-- ============================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

-- ============================================================
-- 1. ROLES
-- ============================================================

IF NOT EXISTS (
    SELECT 1
    FROM [Roles]
    WHERE [Name] = 'Staff'
)
BEGIN
    INSERT INTO [Roles]
    (
        [Name],
        [Description],
        [CreatedAt]
    )
    VALUES
    (
        'Staff',
        N'Staff, full rights to manage the system',
        SYSUTCDATETIME()
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [Roles]
    WHERE [Name] = 'Student'
)
BEGIN
    INSERT INTO [Roles]
    (
        [Name],
        [Description],
        [CreatedAt]
    )
    VALUES
    (
        'Student',
        N'Student, can only access student functions',
        SYSUTCDATETIME()
    );
END;


-- ============================================================
-- 2. DEFAULT ADMIN ACCOUNT
-- ============================================================
--
-- Admin is NOT a separate Role.
-- Admin uses the Staff role and therefore has full rights.
--
-- UserCode:
--   admin
--
-- Email:
--   admin@staff.avepoint.com
--
-- Password:
--   Admin@123
--
-- BCrypt work factor: 11
-- ============================================================

IF NOT EXISTS (
    SELECT 1
    FROM [Users]
    WHERE [Email] = 'admin@staff.avepoint.com'
)
BEGIN
    INSERT INTO [Users]
    (
        [Email],
        [UserName],
        [PasswordHash],
        [FullName],
        [UserCode],
        [RoleId],
        [Status],
        [IsDeleted],
        [UpdatedAt],
        [CreatedAt]
    )
    SELECT
        'admin@staff.avepoint.com',
        'admin',
        '$2a$11$GcYwUCSG6/M8rR5huJlC2e9/6kBT/zlA0AfSUMvxUYDsbGi4vNNyi',
        N'System Administrator',
        'admin',
        [Id],
        1,
        0,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    FROM [Roles]
    WHERE [Name] = 'Staff';
END;


-- ============================================================
-- 3. ACCOUNT SEQUENCES
-- ============================================================
--
-- Staff:
--   26000001@staff.avepoint.com
--   26000002@staff.avepoint.com
--   ...
--
-- Student:
--   26000001@student.avepoint.com
--   26000002@student.avepoint.com
--   ...
--
-- Staff and Student have independent sequences.
-- ============================================================

IF NOT EXISTS (
    SELECT 1
    FROM [AccountSequences]
    WHERE [AccountType] = 'Staff'
)
BEGIN
    INSERT INTO [AccountSequences]
    (
        [AccountType],
        [NextNumber],
        [CreatedAt]
    )
    VALUES
    (
        'Staff',
        26000001,
        SYSUTCDATETIME()
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AccountSequences]
    WHERE [AccountType] = 'Student'
)
BEGIN
    INSERT INTO [AccountSequences]
    (
        [AccountType],
        [NextNumber],
        [CreatedAt]
    )
    VALUES
    (
        'Student',
        26000001,
        SYSUTCDATETIME()
    );
END;

SELECT *
FROM [AccountSequences];