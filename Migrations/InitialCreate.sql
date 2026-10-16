/* SQL script for InitialCreate migration */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

CREATE TABLE [dbo].[Users]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Email] NVARCHAR(256) NOT NULL,
	[PasswordHash] NVARCHAR(MAX) NOT NULL,
	[FirstName] NVARCHAR(100) NOT NULL,
	[LastName] NVARCHAR(100) NOT NULL,
	[PhoneNumber] NVARCHAR(30) NULL,
	[Role] NVARCHAR(50) NOT NULL,
	[DateOfBirth] DATETIME2 NULL,
	[Gender] NVARCHAR(50) NULL,
	[IdNumber] NVARCHAR(50) NULL,
	[EmergencyContactName] NVARCHAR(200) NULL,
	[EmergencyContactNumber] NVARCHAR(30) NULL,
	[MedicalConditions] NVARCHAR(MAX) NULL,
	[ClubAffiliation] NVARCHAR(200) NULL,
	[CreatedAt] DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Users_Email ON [dbo].[Users]([Email]);

CREATE TABLE [dbo].[Organisers]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserId] INT NOT NULL,
	[OrganisationName] NVARCHAR(200) NULL
);

ALTER TABLE [dbo].[Organisers] ADD CONSTRAINT FK_Organisers_Users_UserId FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
CREATE UNIQUE INDEX IX_Organisers_UserId ON [dbo].[Organisers]([UserId]);

CREATE TABLE [dbo].[Participants]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserId] INT NOT NULL
);

ALTER TABLE [dbo].[Participants] ADD CONSTRAINT FK_Participants_Users_UserId FOREIGN KEY([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
CREATE UNIQUE INDEX IX_Participants_UserId ON [dbo].[Participants]([UserId]);

CREATE TABLE [dbo].[Events]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Title] NVARCHAR(200) NOT NULL,
	[Description] NVARCHAR(MAX) NULL,
	[EventDate] DATETIME2 NOT NULL,
	[EventTime] TIME NULL,
	[Venue] NVARCHAR(200) NULL,
	[City] NVARCHAR(100) NULL,
	[Province] NVARCHAR(100) NULL,
	[EventType] NVARCHAR(100) NULL,
	[Distance] DECIMAL(8,2) NOT NULL,
	[DistanceUnit] NVARCHAR(10) NOT NULL,
	[MaxParticipants] INT NULL,
	[RegistrationOpenDate] DATETIME2 NULL,
	[RegistrationCloseDate] DATETIME2 NULL,
	[Metadata] NVARCHAR(MAX) NULL,
	[Status] NVARCHAR(50) NULL,
	[OrganiserId] INT NOT NULL
);

ALTER TABLE [dbo].[Events] ADD CONSTRAINT FK_Events_Organisers_OrganiserId FOREIGN KEY([OrganiserId]) REFERENCES [dbo].[Organisers]([Id]) ON DELETE CASCADE;
CREATE INDEX IX_Events_OrganiserId ON [dbo].[Events]([OrganiserId]);

CREATE TABLE [dbo].[Categories]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[EventId] INT NOT NULL,
	[CategoryName] NVARCHAR(200) NOT NULL,
	[Description] NVARCHAR(MAX) NULL,
	[Distance] DECIMAL(8,2) NOT NULL,
	[DistanceUnit] NVARCHAR(10) NULL,
	[AgeGroupMin] INT NULL,
	[AgeGroupMax] INT NULL,
	[GenderRestriction] NVARCHAR(20) NULL,
	[EntryFee] DECIMAL(10,2) NOT NULL,
	[LateEntryFee] DECIMAL(10,2) NULL,
	[MaxParticipants] INT NULL,
	[StartTime] TIME NULL,
	[CutOffTime] TIME NULL
);

ALTER TABLE [dbo].[Categories] ADD CONSTRAINT FK_Categories_Events_EventId FOREIGN KEY([EventId]) REFERENCES [dbo].[Events]([Id]) ON DELETE CASCADE;
CREATE INDEX IX_Categories_EventId ON [dbo].[Categories]([EventId]);

CREATE TABLE [dbo].[Enrolments]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ParticipantId] INT NOT NULL,
	[EventId] INT NOT NULL,
	[CategoryId] INT NOT NULL,
	[Status] NVARCHAR(50) NULL,
	[EnrolmentDate] DATETIME2 NOT NULL
);

ALTER TABLE [dbo].[Enrolments] ADD CONSTRAINT FK_Enrolments_Participants_ParticipantId FOREIGN KEY([ParticipantId]) REFERENCES [dbo].[Participants]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Enrolments] ADD CONSTRAINT FK_Enrolments_Events_EventId FOREIGN KEY([EventId]) REFERENCES [dbo].[Events]([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Enrolments] ADD CONSTRAINT FK_Enrolments_Categories_CategoryId FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE CASCADE;
CREATE INDEX IX_Enrolments_ParticipantId ON [dbo].[Enrolments]([ParticipantId]);
CREATE INDEX IX_Enrolments_EventId ON [dbo].[Enrolments]([EventId]);
CREATE INDEX IX_Enrolments_CategoryId ON [dbo].[Enrolments]([CategoryId]);

CREATE TABLE [dbo].[Results]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[EnrolmentId] INT NOT NULL,
	[FinishTime] TIME NULL,
	[GunTime] TIME NULL,
	[ChipTime] TIME NULL,
	[OverallPosition] INT NULL,
	[GenderPosition] INT NULL,
	[CategoryPosition] INT NULL,
	[Pace] NVARCHAR(50) NULL,
	[IsDisqualified] BIT NOT NULL,
	[DisqualificationReason] NVARCHAR(MAX) NULL,
	[ResultNotes] NVARCHAR(MAX) NULL,
	[RecordedAt] DATETIME2 NOT NULL
);

ALTER TABLE [dbo].[Results] ADD CONSTRAINT FK_Results_Enrolments_EnrolmentId FOREIGN KEY([EnrolmentId]) REFERENCES [dbo].[Enrolments]([Id]) ON DELETE CASCADE;
CREATE UNIQUE INDEX IX_Results_EnrolmentId ON [dbo].[Results]([EnrolmentId]);

/* End of InitialCreate SQL */
