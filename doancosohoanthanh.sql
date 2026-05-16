-- Reconstructed SQL Server schema from backup: doanhcosohoanthanh1(1).bak
-- Database name found in backup: doancosohoanthanh
-- Note: this script creates schema, keys, indexes and common defaults. INSERT data is not included.

IF DB_ID(N'doancosohoanthanh') IS NULL
    CREATE DATABASE [doancosohoanthanh];
GO
USE [doancosohoanthanh];
GO

-- Drop existing tables if needed

IF OBJECT_ID(N'[dbo].[UserProfileExtras]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserProfileExtras_AspNetUsers_UserId')
    ALTER TABLE [dbo].[UserProfileExtras] DROP CONSTRAINT [FK_UserProfileExtras_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[BusRoutes]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusRoutes_Stops_StartStopId')
    ALTER TABLE [dbo].[BusRoutes] DROP CONSTRAINT [FK_BusRoutes_Stops_StartStopId];
GO
IF OBJECT_ID(N'[dbo].[BusRoutes]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusRoutes_Stops_EndStopId')
    ALTER TABLE [dbo].[BusRoutes] DROP CONSTRAINT [FK_BusRoutes_Stops_EndStopId];
GO
IF OBJECT_ID(N'[dbo].[TripReports]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TripReports_BusTrips_BusTripId')
    ALTER TABLE [dbo].[TripReports] DROP CONSTRAINT [FK_TripReports_BusTrips_BusTripId];
GO
IF OBJECT_ID(N'[dbo].[TripReports]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TripReports_AspNetUsers_DriverId')
    ALTER TABLE [dbo].[TripReports] DROP CONSTRAINT [FK_TripReports_AspNetUsers_DriverId];
GO
IF OBJECT_ID(N'[dbo].[Stops]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Stops_BusRoutes_BusRouteId')
    ALTER TABLE [dbo].[Stops] DROP CONSTRAINT [FK_Stops_BusRoutes_BusRouteId];
GO
IF OBJECT_ID(N'[dbo].[Seats]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Seats_BusTrips_BusTripId')
    ALTER TABLE [dbo].[Seats] DROP CONSTRAINT [FK_Seats_BusTrips_BusTripId];
GO
IF OBJECT_ID(N'[dbo].[RouteImage]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RouteImage_BusRoutes_BusRouteId')
    ALTER TABLE [dbo].[RouteImage] DROP CONSTRAINT [FK_RouteImage_BusRoutes_BusRouteId];
GO
IF OBJECT_ID(N'[dbo].[GPLXImage]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_GPLXImage_Driverregis_DriverregisId')
    ALTER TABLE [dbo].[GPLXImage] DROP CONSTRAINT [FK_GPLXImage_Driverregis_DriverregisId];
GO
IF OBJECT_ID(N'[dbo].[GPLX2Image]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_GPLX2Image_Driverregis_DriverregisId')
    ALTER TABLE [dbo].[GPLX2Image] DROP CONSTRAINT [FK_GPLX2Image_Driverregis_DriverregisId];
GO
IF OBJECT_ID(N'[dbo].[Driverregis]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Driverregis_AspNetUsers_DriverId')
    ALTER TABLE [dbo].[Driverregis] DROP CONSTRAINT [FK_Driverregis_AspNetUsers_DriverId];
GO
IF OBJECT_ID(N'[dbo].[CCCD2Image]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CCCD2Image_Driverregis_DriverregisId')
    ALTER TABLE [dbo].[CCCD2Image] DROP CONSTRAINT [FK_CCCD2Image_Driverregis_DriverregisId];
GO
IF OBJECT_ID(N'[dbo].[CCCD1Image]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CCCD1Image_Driverregis_DriverregisId')
    ALTER TABLE [dbo].[CCCD1Image] DROP CONSTRAINT [FK_CCCD1Image_Driverregis_DriverregisId];
GO
IF OBJECT_ID(N'[dbo].[BusTrips]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusTrips_BusRoutes_BusRouteId')
    ALTER TABLE [dbo].[BusTrips] DROP CONSTRAINT [FK_BusTrips_BusRoutes_BusRouteId];
GO
IF OBJECT_ID(N'[dbo].[BusTrips]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusTrips_Buses_BusId')
    ALTER TABLE [dbo].[BusTrips] DROP CONSTRAINT [FK_BusTrips_Buses_BusId];
GO
IF OBJECT_ID(N'[dbo].[BusTrips]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusTrips_AspNetUsers_DriverId')
    ALTER TABLE [dbo].[BusTrips] DROP CONSTRAINT [FK_BusTrips_AspNetUsers_DriverId];
GO
IF OBJECT_ID(N'[dbo].[BusTrips]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusTrips_AspNetUsers_AdminId')
    ALTER TABLE [dbo].[BusTrips] DROP CONSTRAINT [FK_BusTrips_AspNetUsers_AdminId];
GO
IF OBJECT_ID(N'[dbo].[BusTripImages]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusTripImages_BusTrips_BusTripId')
    ALTER TABLE [dbo].[BusTripImages] DROP CONSTRAINT [FK_BusTripImages_BusTrips_BusTripId];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_Stops_PickupStopId')
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_Stops_PickupStopId];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_Stops_DropOffStopId')
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_Stops_DropOffStopId];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_Seats_SeatId')
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_Seats_SeatId];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_BusTrips_BusTripId')
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_BusTrips_BusTripId];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_AspNetUsers_UserId')
    ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetUserTokens_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserTokens] DROP CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetUserRoles_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetUserRoles_AspNetRoles_RoleId')
    ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetUserLogins_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetUserClaims_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AspNetRoleClaims_AspNetRoles_RoleId')
    ALTER TABLE [dbo].[AspNetRoleClaims] DROP CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];
GO
IF OBJECT_ID(N'[dbo].[UserProfileExtras]', N'U') IS NOT NULL DROP TABLE [dbo].[UserProfileExtras];
GO
IF OBJECT_ID(N'[dbo].[TripReports]', N'U') IS NOT NULL DROP TABLE [dbo].[TripReports];
GO
IF OBJECT_ID(N'[dbo].[Stops]', N'U') IS NOT NULL DROP TABLE [dbo].[Stops];
GO
IF OBJECT_ID(N'[dbo].[Seats]', N'U') IS NOT NULL DROP TABLE [dbo].[Seats];
GO
IF OBJECT_ID(N'[dbo].[RouteImage]', N'U') IS NOT NULL DROP TABLE [dbo].[RouteImage];
GO
IF OBJECT_ID(N'[dbo].[GPLXImage]', N'U') IS NOT NULL DROP TABLE [dbo].[GPLXImage];
GO
IF OBJECT_ID(N'[dbo].[GPLX2Image]', N'U') IS NOT NULL DROP TABLE [dbo].[GPLX2Image];
GO
IF OBJECT_ID(N'[dbo].[Driverregis]', N'U') IS NOT NULL DROP TABLE [dbo].[Driverregis];
GO
IF OBJECT_ID(N'[dbo].[CCCD2Image]', N'U') IS NOT NULL DROP TABLE [dbo].[CCCD2Image];
GO
IF OBJECT_ID(N'[dbo].[CCCD1Image]', N'U') IS NOT NULL DROP TABLE [dbo].[CCCD1Image];
GO
IF OBJECT_ID(N'[dbo].[ChatMessages]', N'U') IS NOT NULL DROP TABLE [dbo].[ChatMessages];
GO
IF OBJECT_ID(N'[dbo].[BusTrips]', N'U') IS NOT NULL DROP TABLE [dbo].[BusTrips];
GO
IF OBJECT_ID(N'[dbo].[BusTripImages]', N'U') IS NOT NULL DROP TABLE [dbo].[BusTripImages];
GO
IF OBJECT_ID(N'[dbo].[BusRoutes]', N'U') IS NOT NULL DROP TABLE [dbo].[BusRoutes];
GO
IF OBJECT_ID(N'[dbo].[Buses]', N'U') IS NOT NULL DROP TABLE [dbo].[Buses];
GO
IF OBJECT_ID(N'[dbo].[Bookings]', N'U') IS NOT NULL DROP TABLE [dbo].[Bookings];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetUserTokens];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetUserRoles];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetUserLogins];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetUserClaims];
GO
IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetRoleClaims];
GO
IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetUsers];
GO
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL DROP TABLE [dbo].[AspNetRoles];
GO
IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL DROP TABLE [dbo].[__EFMigrationsHistory];
GO

CREATE TABLE [dbo].[__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
);
GO

CREATE TABLE [dbo].[AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset(7) NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC)
);
GO

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
);
GO

CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC)
);
GO

CREATE TABLE [dbo].[Bookings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [InvoiceCode] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [SDT] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Timebooking] datetime2(7) NOT NULL,
    [TotalPrice] float NOT NULL,
    [Note] nvarchar(max) NULL,
    [SeatId] int NOT NULL,
    [TripId] int NULL,
    [BusTripId] int NULL,
    [StatusBooking] int NULL,
    [PickupStopId] int NULL,
    [DropOffStopId] int NULL,
    [StatusOnBus] int NOT NULL,
    [Goods] int NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Buses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Company] nvarchar(max) NOT NULL,
    [BusNumber] nvarchar(max) NOT NULL,
    [BusType] int NULL,
    [OperatingStatus] int NOT NULL CONSTRAINT [DF__Buses__Operating__3A4CA8FD] DEFAULT ((0)),
    CONSTRAINT [PK_Buses] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[BusRoutes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Distance] nvarchar(max) NULL,
    [Time] nvarchar(max) NULL,
    [Price] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [EndStopId] int NULL,
    [StartStopId] int NULL,
    CONSTRAINT [PK_BusRoutes] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[BusTripImages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [BusTripId] int NOT NULL,
    CONSTRAINT [PK_BusTripImages] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[BusTrips] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [BusId] int NOT NULL,
    [Capacity] int NULL,
    [ImageUrl] nvarchar(max) NULL,
    [DepartureTime] datetime2(7) NOT NULL,
    [DepartureDate] datetime2(7) NOT NULL,
    [TripStatus] int NULL,
    [BusRouteId] int NOT NULL,
    [DriverId] nvarchar(450) NULL,
    [AdminId] nvarchar(450) NULL,
    [PriceAdjustmentPercent] float NOT NULL CONSTRAINT [DF_BusTrips_PriceAdjustmentPercent] DEFAULT ((0)),
    CONSTRAINT [PK_BusTrips] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[ChatMessages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ConversationId] nvarchar(450) NOT NULL,
    [SenderId] nvarchar(450) NOT NULL,
    [SenderName] nvarchar(256) NOT NULL,
    [SenderRole] nvarchar(50) NOT NULL,
    [MessageText] nvarchar(max) NOT NULL,
    [SentAt] datetime2(7) NOT NULL CONSTRAINT [DF_ChatMessages_SentAt] DEFAULT (getdate()),
    [IsRead] bit NOT NULL CONSTRAINT [DF_ChatMessages_IsRead] DEFAULT (CONVERT([bit],(0))),
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[CCCD1Image] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [DriverregisId] int NOT NULL,
    CONSTRAINT [PK_CCCD1Image] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[CCCD2Image] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [DriverregisId] int NOT NULL,
    CONSTRAINT [PK_CCCD2Image] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Driverregis] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [DateOfBirth] datetime2(7) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [DriverId] nvarchar(450) NOT NULL,
    [LicenseNumber] nvarchar(max) NOT NULL,
    [LicenseIssueDate] datetime2(7) NOT NULL,
    [LicenseIssuingAuthority] nvarchar(max) NOT NULL,
    [LicenseExpiryDate] datetime2(7) NOT NULL,
    [GPLX1ImageUrl] nvarchar(max) NOT NULL,
    [GPLX2ImageUrl] nvarchar(max) NOT NULL,
    [CCCDNumber] nvarchar(max) NOT NULL,
    [CCCD1ImageUrl] nvarchar(max) NOT NULL,
    [CCCD2ImageUrl] nvarchar(max) NOT NULL,
    [ApproveStatus] int NOT NULL,
    CONSTRAINT [PK_Driverregis] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[GPLX2Image] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [DriverregisId] int NOT NULL,
    CONSTRAINT [PK_GPLX2Image] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[GPLXImage] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [DriverregisId] int NOT NULL,
    CONSTRAINT [PK_GPLXImage] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[RouteImage] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [BusRouteId] int NOT NULL,
    CONSTRAINT [PK_RouteImage] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Seats] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SeatNumber] nvarchar(max) NULL,
    [Price] float NOT NULL,
    [SeatStatus] int NULL,
    [BusTripId] int NOT NULL,
    CONSTRAINT [PK_Seats] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Stops] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Stt] int NULL,
    [Name] nvarchar(max) NULL,
    [Location] nvarchar(max) NULL,
    [Latitude] nvarchar(max) NULL,
    [Longitude] nvarchar(max) NULL,
    [BusRouteId] int NULL,
    CONSTRAINT [PK_Stops] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[TripReports] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [BusTripId] int NOT NULL,
    [DriverName] nvarchar(max) NULL,
    [CreateTime] datetime2(7) NULL,
    [Gascost] float NULL,
    [Repaircosts] float NULL,
    [Anothercost] float NULL,
    [DriverId] nvarchar(450) NULL,
    [Note] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TripReports] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[UserProfileExtras] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Mobile] nvarchar(50) NULL,
    [Address] nvarchar(500) NULL,
    [WebsiteUrl] nvarchar(500) NULL,
    [GitHubUrl] nvarchar(500) NULL,
    [TwitterUrl] nvarchar(500) NULL,
    [InstagramUrl] nvarchar(500) NULL,
    [FacebookUrl] nvarchar(500) NULL,
    [AvatarUrl] nvarchar(500) NULL,
    [UpdatedAt] datetime NOT NULL CONSTRAINT [DF_UserProfileExtras_UpdatedAt] DEFAULT (getdate()),
    CONSTRAINT [PK_UserProfileExtras] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

ALTER TABLE [dbo].[AspNetRoleClaims] WITH CHECK ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];
GO

ALTER TABLE [dbo].[AspNetUserClaims] WITH CHECK ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];
GO

ALTER TABLE [dbo].[AspNetUserLogins] WITH CHECK ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];
GO

ALTER TABLE [dbo].[AspNetUserRoles] WITH CHECK ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];
GO

ALTER TABLE [dbo].[AspNetUserRoles] WITH CHECK ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];
GO

ALTER TABLE [dbo].[AspNetUserTokens] WITH CHECK ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];
GO

ALTER TABLE [dbo].[Bookings] WITH CHECK ADD CONSTRAINT [FK_Bookings_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_AspNetUsers_UserId];
GO

ALTER TABLE [dbo].[Bookings] WITH CHECK ADD CONSTRAINT [FK_Bookings_BusTrips_BusTripId] FOREIGN KEY ([BusTripId])
REFERENCES [dbo].[BusTrips] ([Id]);
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_BusTrips_BusTripId];
GO

ALTER TABLE [dbo].[Bookings] WITH CHECK ADD CONSTRAINT [FK_Bookings_Seats_SeatId] FOREIGN KEY ([SeatId])
REFERENCES [dbo].[Seats] ([Id]);
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Seats_SeatId];
GO

ALTER TABLE [dbo].[Bookings] WITH CHECK ADD CONSTRAINT [FK_Bookings_Stops_DropOffStopId] FOREIGN KEY ([DropOffStopId])
REFERENCES [dbo].[Stops] ([Id]);
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Stops_DropOffStopId];
GO

ALTER TABLE [dbo].[Bookings] WITH CHECK ADD CONSTRAINT [FK_Bookings_Stops_PickupStopId] FOREIGN KEY ([PickupStopId])
REFERENCES [dbo].[Stops] ([Id]);
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Stops_PickupStopId];
GO

ALTER TABLE [dbo].[BusTripImages] WITH CHECK ADD CONSTRAINT [FK_BusTripImages_BusTrips_BusTripId] FOREIGN KEY ([BusTripId])
REFERENCES [dbo].[BusTrips] ([Id]);
GO
ALTER TABLE [dbo].[BusTripImages] CHECK CONSTRAINT [FK_BusTripImages_BusTrips_BusTripId];
GO

ALTER TABLE [dbo].[BusTrips] WITH CHECK ADD CONSTRAINT [FK_BusTrips_AspNetUsers_AdminId] FOREIGN KEY ([AdminId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[BusTrips] CHECK CONSTRAINT [FK_BusTrips_AspNetUsers_AdminId];
GO

ALTER TABLE [dbo].[BusTrips] WITH CHECK ADD CONSTRAINT [FK_BusTrips_AspNetUsers_DriverId] FOREIGN KEY ([DriverId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[BusTrips] CHECK CONSTRAINT [FK_BusTrips_AspNetUsers_DriverId];
GO

ALTER TABLE [dbo].[BusTrips] WITH CHECK ADD CONSTRAINT [FK_BusTrips_Buses_BusId] FOREIGN KEY ([BusId])
REFERENCES [dbo].[Buses] ([Id]);
GO
ALTER TABLE [dbo].[BusTrips] CHECK CONSTRAINT [FK_BusTrips_Buses_BusId];
GO

ALTER TABLE [dbo].[BusTrips] WITH CHECK ADD CONSTRAINT [FK_BusTrips_BusRoutes_BusRouteId] FOREIGN KEY ([BusRouteId])
REFERENCES [dbo].[BusRoutes] ([Id]);
GO
ALTER TABLE [dbo].[BusTrips] CHECK CONSTRAINT [FK_BusTrips_BusRoutes_BusRouteId];
GO

ALTER TABLE [dbo].[CCCD1Image] WITH CHECK ADD CONSTRAINT [FK_CCCD1Image_Driverregis_DriverregisId] FOREIGN KEY ([DriverregisId])
REFERENCES [dbo].[Driverregis] ([Id]);
GO
ALTER TABLE [dbo].[CCCD1Image] CHECK CONSTRAINT [FK_CCCD1Image_Driverregis_DriverregisId];
GO

ALTER TABLE [dbo].[CCCD2Image] WITH CHECK ADD CONSTRAINT [FK_CCCD2Image_Driverregis_DriverregisId] FOREIGN KEY ([DriverregisId])
REFERENCES [dbo].[Driverregis] ([Id]);
GO
ALTER TABLE [dbo].[CCCD2Image] CHECK CONSTRAINT [FK_CCCD2Image_Driverregis_DriverregisId];
GO

ALTER TABLE [dbo].[Driverregis] WITH CHECK ADD CONSTRAINT [FK_Driverregis_AspNetUsers_DriverId] FOREIGN KEY ([DriverId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[Driverregis] CHECK CONSTRAINT [FK_Driverregis_AspNetUsers_DriverId];
GO

ALTER TABLE [dbo].[GPLX2Image] WITH CHECK ADD CONSTRAINT [FK_GPLX2Image_Driverregis_DriverregisId] FOREIGN KEY ([DriverregisId])
REFERENCES [dbo].[Driverregis] ([Id]);
GO
ALTER TABLE [dbo].[GPLX2Image] CHECK CONSTRAINT [FK_GPLX2Image_Driverregis_DriverregisId];
GO

ALTER TABLE [dbo].[GPLXImage] WITH CHECK ADD CONSTRAINT [FK_GPLXImage_Driverregis_DriverregisId] FOREIGN KEY ([DriverregisId])
REFERENCES [dbo].[Driverregis] ([Id]);
GO
ALTER TABLE [dbo].[GPLXImage] CHECK CONSTRAINT [FK_GPLXImage_Driverregis_DriverregisId];
GO

ALTER TABLE [dbo].[RouteImage] WITH CHECK ADD CONSTRAINT [FK_RouteImage_BusRoutes_BusRouteId] FOREIGN KEY ([BusRouteId])
REFERENCES [dbo].[BusRoutes] ([Id]);
GO
ALTER TABLE [dbo].[RouteImage] CHECK CONSTRAINT [FK_RouteImage_BusRoutes_BusRouteId];
GO

ALTER TABLE [dbo].[Seats] WITH CHECK ADD CONSTRAINT [FK_Seats_BusTrips_BusTripId] FOREIGN KEY ([BusTripId])
REFERENCES [dbo].[BusTrips] ([Id]);
GO
ALTER TABLE [dbo].[Seats] CHECK CONSTRAINT [FK_Seats_BusTrips_BusTripId];
GO

ALTER TABLE [dbo].[Stops] WITH CHECK ADD CONSTRAINT [FK_Stops_BusRoutes_BusRouteId] FOREIGN KEY ([BusRouteId])
REFERENCES [dbo].[BusRoutes] ([Id]);
GO
ALTER TABLE [dbo].[Stops] CHECK CONSTRAINT [FK_Stops_BusRoutes_BusRouteId];
GO

ALTER TABLE [dbo].[TripReports] WITH CHECK ADD CONSTRAINT [FK_TripReports_AspNetUsers_DriverId] FOREIGN KEY ([DriverId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[TripReports] CHECK CONSTRAINT [FK_TripReports_AspNetUsers_DriverId];
GO

ALTER TABLE [dbo].[TripReports] WITH CHECK ADD CONSTRAINT [FK_TripReports_BusTrips_BusTripId] FOREIGN KEY ([BusTripId])
REFERENCES [dbo].[BusTrips] ([Id]);
GO
ALTER TABLE [dbo].[TripReports] CHECK CONSTRAINT [FK_TripReports_BusTrips_BusTripId];
GO

ALTER TABLE [dbo].[BusRoutes] WITH CHECK ADD CONSTRAINT [FK_BusRoutes_Stops_EndStopId] FOREIGN KEY ([EndStopId])
REFERENCES [dbo].[Stops] ([Id]);
GO
ALTER TABLE [dbo].[BusRoutes] CHECK CONSTRAINT [FK_BusRoutes_Stops_EndStopId];
GO

ALTER TABLE [dbo].[BusRoutes] WITH CHECK ADD CONSTRAINT [FK_BusRoutes_Stops_StartStopId] FOREIGN KEY ([StartStopId])
REFERENCES [dbo].[Stops] ([Id]);
GO
ALTER TABLE [dbo].[BusRoutes] CHECK CONSTRAINT [FK_BusRoutes_Stops_StartStopId];
GO

ALTER TABLE [dbo].[UserProfileExtras] WITH CHECK ADD CONSTRAINT [FK_UserProfileExtras_AspNetUsers_UserId] FOREIGN KEY ([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id]);
GO
ALTER TABLE [dbo].[UserProfileExtras] CHECK CONSTRAINT [FK_UserProfileExtras_AspNetUsers_UserId];
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId] ASC);
GO
CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName] ASC) WHERE [NormalizedName] IS NOT NULL;
GO
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId] ASC);
GO
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId] ASC);
GO
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId] ASC);
GO
CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail] ASC);
GO
CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName] ASC) WHERE [NormalizedUserName] IS NOT NULL;
GO
CREATE INDEX [IX_Bookings_BusTripId] ON [dbo].[Bookings] ([BusTripId] ASC);
GO
CREATE INDEX [IX_Bookings_DropOffStopId] ON [dbo].[Bookings] ([DropOffStopId] ASC);
GO
CREATE INDEX [IX_Bookings_PickupStopId] ON [dbo].[Bookings] ([PickupStopId] ASC);
GO
CREATE INDEX [IX_Bookings_SeatId] ON [dbo].[Bookings] ([SeatId] ASC);
GO
CREATE INDEX [IX_Bookings_UserId] ON [dbo].[Bookings] ([UserId] ASC);
GO
CREATE INDEX [IX_BusRoutes_EndStopId] ON [dbo].[BusRoutes] ([EndStopId] ASC);
GO
CREATE INDEX [IX_BusRoutes_StartStopId] ON [dbo].[BusRoutes] ([StartStopId] ASC);
GO
CREATE INDEX [IX_BusTripImages_BusTripId] ON [dbo].[BusTripImages] ([BusTripId] ASC);
GO
CREATE INDEX [IX_BusTrips_BusId] ON [dbo].[BusTrips] ([BusId] ASC);
GO
CREATE INDEX [IX_BusTrips_BusRouteId] ON [dbo].[BusTrips] ([BusRouteId] ASC);
GO
CREATE INDEX [IX_BusTrips_DriverId] ON [dbo].[BusTrips] ([DriverId] ASC);
GO
CREATE INDEX [IX_BusTrips_AdminId] ON [dbo].[BusTrips] ([AdminId] ASC);
GO
CREATE INDEX [IX_CCCD1Image_DriverregisId] ON [dbo].[CCCD1Image] ([DriverregisId] ASC);
GO
CREATE INDEX [IX_CCCD2Image_DriverregisId] ON [dbo].[CCCD2Image] ([DriverregisId] ASC);
GO
CREATE INDEX [IX_Driverregis_DriverId] ON [dbo].[Driverregis] ([DriverId] ASC);
GO
CREATE INDEX [IX_GPLX2Image_DriverregisId] ON [dbo].[GPLX2Image] ([DriverregisId] ASC);
GO
CREATE INDEX [IX_GPLXImage_DriverregisId] ON [dbo].[GPLXImage] ([DriverregisId] ASC);
GO
CREATE INDEX [IX_RouteImage_BusRouteId] ON [dbo].[RouteImage] ([BusRouteId] ASC);
GO
CREATE INDEX [IX_Seats_BusTripId] ON [dbo].[Seats] ([BusTripId] ASC);
GO
CREATE INDEX [IX_Stops_BusRouteId] ON [dbo].[Stops] ([BusRouteId] ASC);
GO
CREATE INDEX [IX_TripReports_BusTripId] ON [dbo].[TripReports] ([BusTripId] ASC);
GO
CREATE INDEX [IX_TripReports_DriverId] ON [dbo].[TripReports] ([DriverId] ASC);
GO
CREATE INDEX [IX_UserProfileExtras_UserId] ON [dbo].[UserProfileExtras] ([UserId] ASC);
GO
CREATE INDEX [IX_ChatMessages_ConversationId] ON [dbo].[ChatMessages] ([ConversationId] ASC);
GO