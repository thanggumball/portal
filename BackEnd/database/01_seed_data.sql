-- ============================================================
--  01_seed_data.sql - initial data, hand-written
--  Run after dotnet ef database update
--  Content: 2 roles (Admin, Student) + 1 admin account
--  Stage 2, step 9 in IMPLEMENTATION_PLAN.md
-- ============================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- required because Users has a filtered index (section 4b)

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Admin')
    INSERT INTO [Roles] ([Name], [Description], [CreatedAt])
    VALUES ('Admin', N'System administrator, full rights to manage users and announcements', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Student')
    INSERT INTO [Roles] ([Name], [Description], [CreatedAt])
    VALUES ('Student', N'Student, can only view and edit their own profile', SYSUTCDATETIME());

-- Password: Admin@123 (BCrypt hash, work factor 11) - change it right after first login
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'admin@studentportal.local')
    INSERT INTO [Users] ([Email], [UserName], [PasswordHash], [FullName], [RoleId], [Status], [IsDeleted], [UpdatedAt], [CreatedAt])
    SELECT 'admin@studentportal.local', 'admin', '$2a$11$GcYwUCSG6/M8rR5huJlC2e9/6kBT/zlA0AfSUMvxUYDsbGi4vNNyi',
           N'System Administrator', [Id], 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME()
    FROM [Roles]
    WHERE [Name] = 'Admin';
