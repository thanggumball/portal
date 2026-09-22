-- ============================================================
--  01_seed_data.sql - initial data, hand-written
--  Run after dotnet ef database update
--  Content: 2 roles (Staff, Student) + 1 admin account
-- ============================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Staff')
    INSERT INTO [Roles] ([Name], [Description], [CreatedAt])
    VALUES (
        'Staff',
        N'Staff, full rights to manage the system',
        SYSUTCDATETIME()
    );

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Student')
    INSERT INTO [Roles] ([Name], [Description], [CreatedAt])
    VALUES (
        'Student',
        N'Student, can only access student functions',
        SYSUTCDATETIME()
    );

-- Default admin account
-- Password: Admin@123
-- Change it right after first login
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'admin@staff.avepoint.com')
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