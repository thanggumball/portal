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
<<<<<<< HEAD

SELECT *
FROM [AccountSequences];
=======
IF NOT EXISTS (
SELECT 1
FROM [Announcements]
)
BEGIN
INSERT INTO Announcements
(
    Id,
    Title,
    Summary,
    Content,
    Status,
    RoleReceived,
    PublishedAt,
    CreatedAt,
    UpdatedAt,
    CreatedBy,
    IsDeleted
)
VALUES
(
    '10000000-0000-0000-0000-000000000004',
    N'Thông báo chung',
    N'Dành cho tất cả người dùng',
    N'Nội dung thông báo chung',
    2,
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    GETUTCDATE(),
    '4B3CB2B7-1AB7-F111-8C1A-00155D3C1650',
    0
),
(
    '10000000-0000-0000-0000-000000000002',
    N'Thông báo sinh viên',
    N'Dành cho sinh viên',
    N'Nội dung dành cho sinh viên',
    2,
    2,
    GETUTCDATE(),
    GETUTCDATE(),
    GETUTCDATE(),
    '4B3CB2B7-1AB7-F111-8C1A-00155D3C1650',
    0
),
(
    '10000000-0000-0000-0000-000000000003',
    N'Thông báo nhân viên',
    N'Dành cho nhân viên',
    N'Nội dung dành cho nhân viên',
    2,
    3,
    GETUTCDATE(),
    GETUTCDATE(),
    GETUTCDATE(),
    '4B3CB2B7-1AB7-F111-8C1A-00155D3C1650',
    0
);
END;


IF NOT EXISTS (
    SELECT 1
    FROM [Categories]
)
BEGIN
INSERT INTO Categories (Id, Name)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'General'),
    ('22222222-2222-2222-2222-222222222222', 'Academic'),
    ('33333333-3333-3333-3333-333333333333', 'Exam'),
    ('44444444-4444-4444-4444-444444444444', 'Registration'),
    ('55555555-5555-5555-5555-555555555555', 'Scholarship'),
    ('66666666-6666-6666-6666-666666666666', 'Event'),
    ('77777777-7777-7777-7777-777777777777', 'Internship'),
    ('88888888-8888-8888-8888-888888888888', 'Financial');
END;
IF NOT EXISTS (
    SELECT 1
    FROM [AnnouncementCategory]
    )
BEGIN

INSERT INTO AnnouncementCategory
(
    Id,
    AnnouncementId,
    CategoryId
)
VALUES
(
    '90000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    '66666666-6666-6666-6666-666666666666'
),
(
    '90000000-0000-0000-0000-000000000002',
    '10000000-0000-0000-0000-000000000002',
    '33333333-3333-3333-3333-333333333333'
),
(
    '90000000-0000-0000-0000-000000000003',
    '10000000-0000-0000-0000-000000000003',
    '88888888-8888-8888-8888-888888888888'
);
END;

>>>>>>> 23cd1cbad7abd9bfead1818773e5770271fa8e83
