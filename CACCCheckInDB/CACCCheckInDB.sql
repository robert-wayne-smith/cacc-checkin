USE [master]
GO

/****** Object:  Database [CACCCheckIn]    Script Date: 03/16/2009 20:02:31 ******/
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'CACCCheckIn')
--DROP DATABASE [CACCCheckIn]
GO

USE [master]
GO

/****** Object:  Database [CACCCheckIn]    Script Date: 03/16/2009 20:02:31 ******/
CREATE DATABASE [CACCCheckIn] ON  PRIMARY 
( NAME = N'cacccheckin', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\CACCCheckIn.mdf' , SIZE = 3904KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'cacccheckin_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\CACCCheckIn_1.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [CACCCheckIn] SET COMPATIBILITY_LEVEL = 90
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CACCCheckIn].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [CACCCheckIn] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [CACCCheckIn] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [CACCCheckIn] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [CACCCheckIn] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [CACCCheckIn] SET ARITHABORT OFF 
GO

ALTER DATABASE [CACCCheckIn] SET AUTO_CLOSE ON 
GO

ALTER DATABASE [CACCCheckIn] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [CACCCheckIn] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [CACCCheckIn] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [CACCCheckIn] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [CACCCheckIn] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [CACCCheckIn] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [CACCCheckIn] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [CACCCheckIn] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [CACCCheckIn] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [CACCCheckIn] SET  DISABLE_BROKER 
GO

ALTER DATABASE [CACCCheckIn] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [CACCCheckIn] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [CACCCheckIn] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [CACCCheckIn] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [CACCCheckIn] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [CACCCheckIn] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [CACCCheckIn] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [CACCCheckIn] SET  READ_WRITE 
GO

ALTER DATABASE [CACCCheckIn] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [CACCCheckIn] SET  MULTI_USER 
GO

ALTER DATABASE [CACCCheckIn] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [CACCCheckIn] SET DB_CHAINING OFF 
GO



USE [CACCCheckIn]
GO
/****** Object:  ForeignKey [FK_Attendance_Class]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] DROP CONSTRAINT [FK_Attendance_Class]
GO
/****** Object:  ForeignKey [FK_Attendance_People]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] DROP CONSTRAINT [FK_Attendance_People]
GO
/****** Object:  ForeignKey [FK_Class_Department]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Class_Department]') AND parent_object_id = OBJECT_ID(N'[dbo].[Class]'))
ALTER TABLE [dbo].[Class] DROP CONSTRAINT [FK_Class_Department]
GO
/****** Object:  ForeignKey [FK_ClassMember_Class]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_Class]
GO
/****** Object:  ForeignKey [FK_ClassMember_ClassRole]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_ClassRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_ClassRole]
GO
/****** Object:  ForeignKey [FK_ClassMember_People]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_People]
GO
/****** Object:  ForeignKey [FK_People_FamilyRole]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_FamilyRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] DROP CONSTRAINT [FK_People_FamilyRole]
GO
/****** Object:  View [dbo].[AttendanceWithDetail]    Script Date: 03/16/2009 20:01:56 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceWithDetail]'))
DROP VIEW [dbo].[AttendanceWithDetail]
GO
/****** Object:  View [dbo].[PeopleWithDepartmentAndClassView]    Script Date: 03/16/2009 20:01:56 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PeopleWithDepartmentAndClassView]'))
DROP VIEW [dbo].[PeopleWithDepartmentAndClassView]
GO
/****** Object:  Table [dbo].[Attendance]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] DROP CONSTRAINT [FK_Attendance_Class]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] DROP CONSTRAINT [FK_Attendance_People]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND type in (N'U'))
DROP TABLE [dbo].[Attendance]
GO
/****** Object:  Table [dbo].[ClassMember]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_Class]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_ClassRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_ClassRole]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [FK_ClassMember_People]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClassMembers_ClassId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [DF_ClassMembers_ClassId]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClassMembers_PersonId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClassMember] DROP CONSTRAINT [DF_ClassMembers_PersonId]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClassMember]') AND type in (N'U'))
DROP TABLE [dbo].[ClassMember]
GO
/****** Object:  Table [dbo].[Class]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Class_Department]') AND parent_object_id = OBJECT_ID(N'[dbo].[Class]'))
ALTER TABLE [dbo].[Class] DROP CONSTRAINT [FK_Class_Department]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Class_Id]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Class] DROP CONSTRAINT [DF_Class_Id]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Class]') AND type in (N'U'))
DROP TABLE [dbo].[Class]
GO
/****** Object:  Table [dbo].[People]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_FamilyRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] DROP CONSTRAINT [FK_People_FamilyRole]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_People_Id]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[People] DROP CONSTRAINT [DF_People_Id]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND type in (N'U'))
DROP TABLE [dbo].[People]
GO
/****** Object:  Table [dbo].[ClassRole]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClassRole]') AND type in (N'U'))
DROP TABLE [dbo].[ClassRole]
GO
/****** Object:  Table [dbo].[Department]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Department_Id]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Department] DROP CONSTRAINT [DF_Department_Id]
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Department]') AND type in (N'U'))
DROP TABLE [dbo].[Department]
GO
/****** Object:  Table [dbo].[FamilyRole]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FamilyRole]') AND type in (N'U'))
DROP TABLE [dbo].[FamilyRole]
GO
/****** Object:  StoredProcedure [dbo].[ResetSecurityCode]    Script Date: 03/16/2009 20:01:51 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetSecurityCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetSecurityCode]
GO
/****** Object:  Table [dbo].[SecurityCode]    Script Date: 03/16/2009 20:01:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SecurityCode]') AND type in (N'U'))
DROP TABLE [dbo].[SecurityCode]
GO
/****** Object:  Table [dbo].[SecurityCode]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SecurityCode]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SecurityCode](
	[GenerationDate] [smalldatetime] NOT NULL,
	[SecurityCode] [int] IDENTITY(700,1) NOT NULL,
 CONSTRAINT [PK_SecurityCode] PRIMARY KEY CLUSTERED 
(
	[GenerationDate] ASC,
	[SecurityCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  StoredProcedure [dbo].[ResetSecurityCode]    Script Date: 03/16/2009 20:01:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetSecurityCode]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ResetSecurityCode] 
AS
BEGIN
	DBCC CHECKIDENT ( ''SecurityCode'', RESEED, 700) WITH NO_INFOMSGS
END
' 
END
GO
/****** Object:  Table [dbo].[FamilyRole]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FamilyRole]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[FamilyRole](
	[Role] [varchar](50) NOT NULL,
 CONSTRAINT [PK_FamilyRole] PRIMARY KEY CLUSTERED 
(
	[Role] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Department]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Department]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Department](
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_Department_Id]  DEFAULT (newid()),
	[Name] [varchar](50) NOT NULL,
	[Description] [varchar](250) NULL,
	[RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ClassRole]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClassRole]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClassRole](
	[Role] [varchar](50) NOT NULL,
 CONSTRAINT [PK_ClassRole] PRIMARY KEY CLUSTERED 
(
	[Role] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[People]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[People](
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_People_Id]  DEFAULT (newid()),
	[FirstName] [varchar](25) NOT NULL,
	[LastName] [varchar](25) NOT NULL,
	[PhoneNumber] [varchar](10) NULL,
	[FamilyId] [uniqueidentifier] NULL,
	[SpecialConditions] [varchar](50) NULL,
	[FamilyRole] [varchar](50) NULL,
	[RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Class]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Class]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Class](
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_Class_Id]  DEFAULT (newid()),
	[Name] [varchar](50) NOT NULL,
	[Description] [varchar](250) NULL,
	[DeptId] [uniqueidentifier] NULL,
	[RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Class] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Class]') AND name = N'IX_Class')
CREATE UNIQUE NONCLUSTERED INDEX [IX_Class] ON [dbo].[Class] 
(
	[DeptId] ASC,
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClassMember]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClassMember]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClassMember](
	[ClassId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_ClassMembers_ClassId]  DEFAULT (newid()),
	[PersonId] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_ClassMembers_PersonId]  DEFAULT (newid()),
	[ClassRole] [varchar](50) NOT NULL,
	[RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_ClassMembers] PRIMARY KEY CLUSTERED 
(
	[ClassId] ASC,
	[PersonId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Attendance]    Script Date: 03/16/2009 20:01:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Attendance](
	[Date] [smalldatetime] NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[ClassId] [uniqueidentifier] NOT NULL,
	[SecurityCode] [int] NOT NULL,
	[RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_Attendance] PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[PersonId] ASC,
	[ClassId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[PeopleWithDepartmentAndClassView]    Script Date: 03/16/2009 20:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PeopleWithDepartmentAndClassView]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[PeopleWithDepartmentAndClassView]
AS
SELECT dbo.Department.Id AS ''DepartmentId'', dbo.Department.Name AS ''DepartmentName'', dbo.Class.Id AS ''ClassId'', dbo.Class.Name AS ''ClassName'', 
               dbo.ClassMember.ClassRole, dbo.People.Id AS ''PersonId'', dbo.People.FirstName, dbo.People.LastName, dbo.People.PhoneNumber, 
               dbo.People.SpecialConditions, dbo.People.FamilyId, dbo.People.FamilyRole
FROM  dbo.Department INNER JOIN
               dbo.Class ON dbo.Department.Id = dbo.Class.DeptId INNER JOIN
               dbo.ClassMember ON dbo.Class.Id = dbo.ClassMember.ClassId INNER JOIN
               dbo.People ON dbo.ClassMember.PersonId = dbo.People.Id
'
GO
/****** Object:  View [dbo].[AttendanceWithDetail]    Script Date: 03/16/2009 20:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceWithDetail]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[AttendanceWithDetail]
AS
SELECT dbo.Attendance.Date, dbo.Attendance.PersonId, dbo.People.FirstName, dbo.People.LastName, dbo.People.PhoneNumber, 
               dbo.People.SpecialConditions, dbo.People.FamilyId, dbo.Attendance.ClassId, dbo.Class.Name AS ClassName, dbo.Department.Id AS DeptId, 
               dbo.Department.Name AS DeptName, dbo.Attendance.SecurityCode, dbo.ClassMember.ClassRole
FROM  dbo.Attendance INNER JOIN
               dbo.Class ON dbo.Attendance.ClassId = dbo.Class.Id INNER JOIN
               dbo.Department ON dbo.Class.DeptId = dbo.Department.Id INNER JOIN
               dbo.People ON dbo.Attendance.PersonId = dbo.People.Id LEFT OUTER JOIN
               dbo.ClassMember ON dbo.Class.Id = dbo.ClassMember.ClassId AND dbo.People.Id = dbo.ClassMember.PersonId
'
GO
/****** Object:  ForeignKey [FK_Attendance_Class]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_Attendance_Class] FOREIGN KEY([ClassId])
REFERENCES [dbo].[Class] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_Attendance_Class]
GO
/****** Object:  ForeignKey [FK_Attendance_People]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_Attendance_People] FOREIGN KEY([PersonId])
REFERENCES [dbo].[People] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Attendance_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[Attendance]'))
ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_Attendance_People]
GO
/****** Object:  ForeignKey [FK_Class_Department]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Class_Department]') AND parent_object_id = OBJECT_ID(N'[dbo].[Class]'))
ALTER TABLE [dbo].[Class]  WITH CHECK ADD  CONSTRAINT [FK_Class_Department] FOREIGN KEY([DeptId])
REFERENCES [dbo].[Department] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Class_Department]') AND parent_object_id = OBJECT_ID(N'[dbo].[Class]'))
ALTER TABLE [dbo].[Class] CHECK CONSTRAINT [FK_Class_Department]
GO
/****** Object:  ForeignKey [FK_ClassMember_Class]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember]  WITH CHECK ADD  CONSTRAINT [FK_ClassMember_Class] FOREIGN KEY([ClassId])
REFERENCES [dbo].[Class] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_Class]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] CHECK CONSTRAINT [FK_ClassMember_Class]
GO
/****** Object:  ForeignKey [FK_ClassMember_ClassRole]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_ClassRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember]  WITH CHECK ADD  CONSTRAINT [FK_ClassMember_ClassRole] FOREIGN KEY([ClassRole])
REFERENCES [dbo].[ClassRole] ([Role])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_ClassRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] CHECK CONSTRAINT [FK_ClassMember_ClassRole]
GO
/****** Object:  ForeignKey [FK_ClassMember_People]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember]  WITH CHECK ADD  CONSTRAINT [FK_ClassMember_People] FOREIGN KEY([PersonId])
REFERENCES [dbo].[People] ([Id])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassMember_People]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassMember]'))
ALTER TABLE [dbo].[ClassMember] CHECK CONSTRAINT [FK_ClassMember_People]
GO
/****** Object:  ForeignKey [FK_People_FamilyRole]    Script Date: 03/16/2009 20:01:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_FamilyRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People]  WITH CHECK ADD  CONSTRAINT [FK_People_FamilyRole] FOREIGN KEY([FamilyRole])
REFERENCES [dbo].[FamilyRole] ([Role])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_People_FamilyRole]') AND parent_object_id = OBJECT_ID(N'[dbo].[People]'))
ALTER TABLE [dbo].[People] CHECK CONSTRAINT [FK_People_FamilyRole]
GO
