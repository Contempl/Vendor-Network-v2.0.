IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Businesses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Address] nvarchar(50) NOT NULL,
    [Email] nvarchar(50) NOT NULL,
    [BusinessType] nvarchar(max) NOT NULL,
    [LogoUrl] nvarchar(max) NULL,
    [Occupation] nvarchar(50) NULL,
    CONSTRAINT [PK_Businesses] PRIMARY KEY ([Id])
);

CREATE TABLE [User] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NULL,
    [Password] varbinary(max) NULL,
    [FirstName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NULL,
    [Email] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id])
);

CREATE TABLE [Industries] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(40) NOT NULL,
    [Address] nvarchar(50) NOT NULL,
    [Latitude] float(10) NOT NULL,
    [Longitude] float(11) NOT NULL,
    [OperatorId] int NOT NULL,
    CONSTRAINT [PK_Industries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Industries_Businesses_OperatorId] FOREIGN KEY ([OperatorId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VendorsFacilities] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Location] nvarchar(100) NOT NULL,
    [Longitude] float(11) NOT NULL,
    [Latitude] float(10) NOT NULL,
    [VendorId] int NOT NULL,
    [RadiusOfWork] float NOT NULL,
    CONSTRAINT [PK_VendorsFacilities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VendorsFacilities_Businesses_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Administrators] (
    [Id] int NOT NULL,
    CONSTRAINT [PK_Administrators] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Administrators_User_Id] FOREIGN KEY ([Id]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [OperatorUsers] (
    [Id] int NOT NULL,
    [OperatorId] int NULL,
    CONSTRAINT [PK_OperatorUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OperatorUsers_Businesses_OperatorId] FOREIGN KEY ([OperatorId]) REFERENCES [Businesses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OperatorUsers_User_Id] FOREIGN KEY ([Id]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VendorUsers] (
    [Id] int NOT NULL,
    [VendorId] int NULL,
    CONSTRAINT [PK_VendorUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VendorUsers_Businesses_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Businesses] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_VendorUsers_User_Id] FOREIGN KEY ([Id]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VendorsFacilityServices] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(40) NOT NULL,
    [VendorFacilityId] int NOT NULL,
    CONSTRAINT [PK_VendorsFacilityServices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VendorsFacilityServices_VendorsFacilities_VendorFacilityId] FOREIGN KEY ([VendorFacilityId]) REFERENCES [VendorsFacilities] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Invites] (
    [Id] int NOT NULL IDENTITY,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    [UserId] int NOT NULL,
    [AdminId] int NOT NULL,
    CONSTRAINT [PK_Invites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invites_Administrators_AdminId] FOREIGN KEY ([AdminId]) REFERENCES [Administrators] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Invites_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Industries_OperatorId] ON [Industries] ([OperatorId]);

CREATE INDEX [IX_Invites_AdminId] ON [Invites] ([AdminId]);

CREATE INDEX [IX_Invites_UserId] ON [Invites] ([UserId]);

CREATE INDEX [IX_OperatorUsers_OperatorId] ON [OperatorUsers] ([OperatorId]);

CREATE INDEX [IX_VendorsFacilities_VendorId] ON [VendorsFacilities] ([VendorId]);

CREATE INDEX [IX_VendorsFacilityServices_VendorFacilityId] ON [VendorsFacilityServices] ([VendorFacilityId]);

CREATE INDEX [IX_VendorUsers_VendorId] ON [VendorUsers] ([VendorId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241030115749_Administrator inherited from User', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Invites] DROP CONSTRAINT [FK_Invites_Administrators_AdminId];

ALTER TABLE [Invites] DROP CONSTRAINT [FK_Invites_User_UserId];

EXEC sp_rename N'[Invites].[AdminId]', N'SenderId', 'COLUMN';

EXEC sp_rename N'[Invites].[IX_Invites_AdminId]', N'IX_Invites_SenderId', 'INDEX';

ALTER TABLE [Invites] ADD CONSTRAINT [FK_Invites_User_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [User] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Invites] ADD CONSTRAINT [FK_Invites_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250111144455_Entities refactored', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [OperatorUsers] DROP CONSTRAINT [FK_OperatorUsers_Businesses_OperatorId];

ALTER TABLE [VendorUsers] DROP CONSTRAINT [FK_VendorUsers_Businesses_VendorId];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[VendorsFacilityServices]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [VendorsFacilityServices] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [VendorsFacilityServices] ALTER COLUMN [Name] nvarchar(250) NOT NULL;

ALTER TABLE [OperatorUsers] ADD CONSTRAINT [FK_OperatorUsers_Businesses_OperatorId] FOREIGN KEY ([OperatorId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE;

ALTER TABLE [VendorUsers] ADD CONSTRAINT [FK_VendorUsers_Businesses_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250318143931_Business OnDeleteBehaviour of users changed to cascade', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Invites] DROP CONSTRAINT [FK_Invites_User_SenderId];

ALTER TABLE [Invites] DROP CONSTRAINT [FK_Invites_User_UserId];

DROP INDEX [IX_Invites_UserId] ON [Invites];

ALTER TABLE [User] ADD [UserType] int NOT NULL DEFAULT 0;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invites]') AND [c].[name] = N'UserId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Invites] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Invites] ALTER COLUMN [UserId] int NULL;

CREATE UNIQUE INDEX [IX_Invites_UserId] ON [Invites] ([UserId]) WHERE [UserId] IS NOT NULL;

ALTER TABLE [Invites] ADD CONSTRAINT [FK_Invites_User_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Invites] ADD CONSTRAINT [FK_Invites_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250621162823_Invite-user relationship refactored', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Invites] DROP CONSTRAINT [FK_Invites_User_UserId];

DROP INDEX [IX_Invites_UserId] ON [Invites];

EXEC sp_rename N'[Invites].[UserId]', N'UpdatedBy', 'COLUMN';

ALTER TABLE [VendorsFacilityServices] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [VendorsFacilityServices] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [VendorsFacilityServices] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [VendorsFacilityServices] ADD [UpdatedBy] int NULL;

ALTER TABLE [VendorsFacilities] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [VendorsFacilities] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [VendorsFacilities] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [VendorsFacilities] ADD [UpdatedBy] int NULL;

ALTER TABLE [User] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [User] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [User] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [User] ADD [UpdatedBy] int NULL;

ALTER TABLE [Invites] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [Invites] ADD [InvitedUserId] int NULL;

ALTER TABLE [Invites] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [Industries] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Industries] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [Industries] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [Industries] ADD [UpdatedBy] int NULL;

ALTER TABLE [Businesses] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Businesses] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [Businesses] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [Businesses] ADD [UpdatedBy] int NULL;
GO

CREATE UNIQUE INDEX [IX_Invites_InvitedUserId] ON [Invites] ([InvitedUserId]) WHERE [InvitedUserId] IS NOT NULL;

ALTER TABLE [Invites] ADD CONSTRAINT [FK_Invites_User_InvitedUserId] FOREIGN KEY ([InvitedUserId]) REFERENCES [User] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250808184239_Embedded interceptors', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'UserType');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [User] ALTER COLUMN [UserType] nvarchar(20) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250808200034_Roles configuration added', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'UserType');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [User] ALTER COLUMN [UserType] nvarchar(30) NOT NULL;

ALTER TABLE [User] ADD [BusinessId] int NULL;

CREATE INDEX [IX_User_BusinessId] ON [User] ([BusinessId]);

ALTER TABLE [User] ADD CONSTRAINT [FK_User_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250809111203_User Type fixed as string', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [RefreshTokens] (
    [Id] bigint NOT NULL IDENTITY,
    [Token] nvarchar(200) NOT NULL,
    [UserId] int NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Revoked] bit NOT NULL,
    [UserId1] int NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RefreshTokens_User_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [User] ([Id])
);

CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_RefreshTokens_UserId1] ON [RefreshTokens] ([UserId1]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251009092610_Added Refresh Token Entity', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_User_UserId1];

DROP INDEX [IX_RefreshTokens_UserId1] ON [RefreshTokens];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RefreshTokens]') AND [c].[name] = N'UserId1');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [RefreshTokens] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [RefreshTokens] DROP COLUMN [UserId1];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invites]') AND [c].[name] = N'Status');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Invites] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Invites] ALTER COLUMN [Status] nvarchar(max) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260323105724_Invite status stirng value instead of enum int', N'10.0.12');

COMMIT;
GO
