USE [DCManagement]
GO
/****** Object:  Table [dbo].[Team]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[TeamID] [int] IDENTITY(1,1) NOT NULL,
	[TeamLead] [int] NULL,
	[PrimaryLocation] [int] NULL,
	[FillIfNoLead] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
	[TeamName] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Team] PRIMARY KEY CLUSTERED 
(
	[TeamID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT DELETE ON [dbo].[Team] TO [ClinicEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Team] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Team] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Team] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Team] TO [PersonnelEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Team] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTeamInfo]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTeamInfo]
(	
	@TeamID int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT TeamID, TeamName, TeamLead, PrimaryLocation, FillIfNoLead, Active
	FROM Team
	WHERE TeamID=@TeamID
)
GO
GRANT SELECT ON [dbo].[GetTeamInfo] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[LocationSlot]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LocationSlot](
	[SlotID] [int] IDENTITY(1,1) NOT NULL,
	[TeamID] [int] NOT NULL,
	[SlotType] [int] NOT NULL,
	[MinQty] [int] NOT NULL,
	[GoalQty] [int] NOT NULL,
 CONSTRAINT [PK_LocationSlot] PRIMARY KEY CLUSTERED 
(
	[SlotID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT DELETE ON [dbo].[LocationSlot] TO [ClinicEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[LocationSlot] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[LocationSlot] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[LocationSlot] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTeamSlots]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTeamSlots] 
(	
	@TeamID int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT SlotID, SlotType, MinQty, GoalQty
	FROM LocationSlot
	WHERE TeamID = @TeamID
)
GO
GRANT SELECT ON [dbo].[GetTeamSlots] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[Person]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person](
	[PersonID] [int] IDENTITY(1,1) NOT NULL,
	[LastName] [varchar](25) NOT NULL,
	[FirstName] [varchar](25) NOT NULL,
	[TeamID] [int] NULL,
	[Active] [bit] NOT NULL,
	[Available] [bit] NOT NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[PersonID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT DELETE ON [dbo].[Person] TO [ClinicEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Person] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Person] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Person] TO [ClinicEditor] AS [dbo]
GO
GRANT DELETE ON [dbo].[Person] TO [PersonnelEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Person] TO [PersonnelEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Person] TO [PersonnelEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Person] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  UserDefinedFunction [dbo].[GetPersonInfo]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetPersonInfo]
(	
	@PersonID int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT PersonID, LastName, FirstName, TeamID, Active, Available
	FROM Person
	WHERE TeamID=@PersonID
)
GO
GRANT SELECT ON [dbo].[GetPersonInfo] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[Assignment]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Assignment](
	[AssignID] [int] IDENTITY(1,1) NOT NULL,
	[Day] [datetime2](7) NOT NULL,
	[DayPart] [tinyint] NOT NULL,
	[PersonID] [int] NOT NULL,
	[TeamID] [int] NOT NULL,
	[UserLocked] [bit] NOT NULL,
 CONSTRAINT [PK_Assignment] PRIMARY KEY CLUSTERED 
(
	[AssignID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT INSERT ON [dbo].[Assignment] TO [AssignmentEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Assignment] TO [AssignmentEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Assignment] TO [AssignmentEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Assignment] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Assignment] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Assignment] TO [ClinicEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Assignment] TO [PersonnelEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Assignment] TO [PersonnelEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Assignment] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[Floorplan]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Floorplan](
	[Image] [varbinary](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GRANT DELETE ON [dbo].[Floorplan] TO [Public Read] AS [dbo]
GO
GRANT INSERT ON [dbo].[Floorplan] TO [Public Read] AS [dbo]
GO
GRANT SELECT ON [dbo].[Floorplan] TO [Public Read] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Floorplan] TO [Public Read] AS [dbo]
GO
/****** Object:  Table [dbo].[Location]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Location](
	[LocID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](20) NOT NULL,
	[LocX] [int] NULL,
	[LocY] [int] NULL,
	[SizeW] [int] NULL,
	[SizeH] [int] NULL,
 CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED 
(
	[LocID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT DELETE ON [dbo].[Location] TO [ClinicEditor] AS [dbo]
GO
GRANT INSERT ON [dbo].[Location] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[Location] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[Location] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[PersonSlot]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonSlot](
	[PersonID] [int] NOT NULL,
	[SlotTypeID] [int] NOT NULL,
 CONSTRAINT [PK_PersonSlot] PRIMARY KEY CLUSTERED 
(
	[PersonID] ASC,
	[SlotTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SlotType]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SlotType](
	[SlotTypeID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](25) NOT NULL,
	[SlotColor] [char](6) NOT NULL,
 CONSTRAINT [PK_SlotType] PRIMARY KEY CLUSTERED 
(
	[SlotTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
GRANT INSERT ON [dbo].[SlotType] TO [ClinicEditor] AS [dbo]
GO
GRANT SELECT ON [dbo].[SlotType] TO [ClinicEditor] AS [dbo]
GO
GRANT UPDATE ON [dbo].[SlotType] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  Table [dbo].[Unavailability]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Unavailability](
	[PersonID] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NULL,
 CONSTRAINT [PK_Unavailability] PRIMARY KEY CLUSTERED 
(
	[PersonID] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Assignment] ADD  CONSTRAINT [DF_Assignment_Locked]  DEFAULT ((0)) FOR [UserLocked]
GO
ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF_Person_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF_Person_Available]  DEFAULT ((1)) FOR [Available]
GO
ALTER TABLE [dbo].[SlotType] ADD  CONSTRAINT [DF_SlotType_SlotColor]  DEFAULT ('000000') FOR [SlotColor]
GO
ALTER TABLE [dbo].[Team] ADD  CONSTRAINT [DF_Team_FillIfNoLead]  DEFAULT ((1)) FOR [FillIfNoLead]
GO
ALTER TABLE [dbo].[Team] ADD  CONSTRAINT [DF_Team_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[Assignment]  WITH CHECK ADD  CONSTRAINT [FK_Assignment_Person] FOREIGN KEY([PersonID])
REFERENCES [dbo].[Person] ([PersonID])
GO
ALTER TABLE [dbo].[Assignment] CHECK CONSTRAINT [FK_Assignment_Person]
GO
ALTER TABLE [dbo].[Assignment]  WITH CHECK ADD  CONSTRAINT [FK_Assignment_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([TeamID])
GO
ALTER TABLE [dbo].[Assignment] CHECK CONSTRAINT [FK_Assignment_Team]
GO
ALTER TABLE [dbo].[LocationSlot]  WITH CHECK ADD  CONSTRAINT [FK_LocationSlot_SlotType] FOREIGN KEY([SlotType])
REFERENCES [dbo].[SlotType] ([SlotTypeID])
GO
ALTER TABLE [dbo].[LocationSlot] CHECK CONSTRAINT [FK_LocationSlot_SlotType]
GO
ALTER TABLE [dbo].[LocationSlot]  WITH CHECK ADD  CONSTRAINT [FK_LocationSlot_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([TeamID])
GO
ALTER TABLE [dbo].[LocationSlot] CHECK CONSTRAINT [FK_LocationSlot_Team]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([TeamID])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_Team]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Location] FOREIGN KEY([PrimaryLocation])
REFERENCES [dbo].[Location] ([LocID])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Location]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Person] FOREIGN KEY([TeamLead])
REFERENCES [dbo].[Person] ([PersonID])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Person]
GO
/****** Object:  StoredProcedure [dbo].[DeleteLocation]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteLocation] 
	@ID int,
	@Name varchar(20) = NULL,
	@LocX int = 0,
	@LocY int = 0,
	@SizeH int = 0,
	@SizeW int = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF (SELECT COUNT(TeamID) FROM Team WHERE PrimaryLocation = @ID) > 0
	BEGIN
		SELECT 'Cascade';
		RETURN
	END
	ELSE BEGIN
		DELETE FROM [Location] WHERE LocID=@ID;
		SELECT '0';
		RETURN
	END
	SELECT '1';
END
GO
GRANT EXECUTE ON [dbo].[DeleteLocation] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[DeleteTeamSlot]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteTeamSlot]
	@SlotID int
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM LocationSlot WHERE SlotID=@SlotID
END
GO
GRANT EXECUTE ON [dbo].[DeleteTeamSlot] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetDefaultSlotAssignments]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetDefaultSlotAssignments]
	@TeamID int,
	@SlotType int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT p.PersonID 
	FROM Person p 
		LEFT JOIN PersonSlot s 
		ON p.PersonID=s.PersonID 
		WHERE p.TeamID=@TeamID 
			AND s.SlotTypeID=@SlotType 
			AND p.Active = 1 
			AND p.Available = 1
END
GO
GRANT EXECUTE ON [dbo].[GetDefaultSlotAssignments] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetPeople]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPeople]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT PersonID, LastName, FirstName, TeamID, Active, Available
	FROM Person
END
GO
GRANT EXECUTE ON [dbo].[GetPeople] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetPersonData]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPersonData] 
	@PersonID int
AS
BEGIN
	SET NOCOUNT ON;
	--Table 1
	SELECT PersonID, LastName, FirstName, TeamID, Active, Available 
	FROM Person 
	WHERE PersonID=@PersonID
	--Table 2
	SELECT ps.SlotTypeID, st.Description, st.SlotColor
	FROM PersonSlot ps
		LEFT JOIN SlotType st
		ON ps.SlotTypeID=st.SlotTypeID
	WHERE PersonID=@PersonID
END
GO
GRANT EXECUTE ON [dbo].[GetPersonData] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetSlotTypes]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetSlotTypes]

AS
BEGIN
	SET NOCOUNT ON;

	SELECT [SlotTypeID]
		  ,[Description]
		  ,[SlotColor]
	FROM [DCManagement].[dbo].[SlotType]
	ORDER BY SlotTypeID
END
GO
GRANT EXECUTE ON [dbo].[GetSlotTypes] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetTeamData]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetTeamData] 
	@TeamID int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [TeamID]
		  ,[TeamLead]
		  ,[PrimaryLocation]
		  ,[FillIfNoLead]
		  ,[Active]
		  ,[TeamName]
	 FROM [DCManagement].[dbo].[Team]
	 WHERE TeamID=@TeamID
END
GO
GRANT EXECUTE ON [dbo].[GetTeamData] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[GetTeams]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetTeams]

AS
BEGIN
	SET NOCOUNT ON;

	SELECT TeamID,
		IIF(Active=1,TeamName,TeamName+'*') AS TeamName
	FROM Team
	ORDER BY TeamName
END
GO
GRANT EXECUTE ON [dbo].[GetTeams] TO [AssignmentEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[InsertLocation]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertLocation]
	@Name varchar(20),
	@LocX int,
	@LocY int,
	@SizeH int,
	@SizeW int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Result TABLE (ID int)

	INSERT INTO Location(Name, LocX, LocY, SizeW, SizeH)
	OUTPUT INSERTED.LocID INTO @Result(ID)
	VALUES (@Name, @LocX, @LocY, @SizeW, @SizeH)
    -- Insert statements for procedure here
	SELECT ID FROM @Result
END
GO
GRANT EXECUTE ON [dbo].[InsertLocation] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[InsertPerson]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[InsertPerson]
	@LastName varchar(25),
	@FirstName varchar(25),
	@TeamID int,
	@Active bit,
	@Available bit
AS BEGIN
	DECLARE @AdjustedTeam int = @TeamID
	DECLARE @Result TABLE (ID int)
	IF @TeamID = -1 SET @AdjustedTeam = NULL
	INSERT INTO Person(LastName, FirstName, TeamID, Active, Available)
	OUTPUT INSERTED.PersonID INTO @Result(ID)
	VALUES (@LastName, @FirstName, @AdjustedTeam, @Active, @Available)

	SELECT ID FROM @Result
END
GO
GRANT EXECUTE ON [dbo].[InsertPerson] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[InsertTeam]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertTeam]
	@Name varchar(20),
	@Lead int,
	@LocID int,
	@Fill bit,
	@Active bit
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @AdjustedLead int
	DECLARE @AdjustedLoc int
	DECLARE @Result TABLE (ID int)
	IF @Lead = -1 SET @AdjustedLead = NULL
	IF @LocID = -1 SET @AdjustedLoc = NULL
	INSERT INTO Team(TeamName, TeamLead, PrimaryLocation, FillIfNoLead, Active)
	OUTPUT INSERTED.TeamID INTO @Result(ID)
	VALUES (@Name, @AdjustedLead, @AdjustedLoc, @Fill, @Active)

	SELECT ID FROM @Result
END
GO
GRANT EXECUTE ON [dbo].[InsertTeam] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[InsertTeamSlot]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertTeamSlot]
	@TeamID int,
	@SlotType int,
	@MinQty int,
	@GoalQty int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Result TABLE (ID int)

	INSERT INTO LocationSlot(TeamID, SlotType, MinQty, GoalQty)
	OUTPUT INSERTED.SlotID INTO @Result(ID)
	VALUES (@TeamID, @SlotType, @MinQty, @GoalQty)

	SELECT ID FROM @Result
END
GO
GRANT EXECUTE ON [dbo].[InsertTeamSlot] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[SetPersonSkill]    Script Date: 1/2/2025 3:04:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SetPersonSkill]
	@PersonID int,
	@SkillID int,
	@IsSet bit
AS BEGIN
	DECLARE @IsSetNow bit
	SELECT @IsSetNow=1 FROM PersonSlot WHERE PersonID=@PersonID AND SlotTypeID=@SkillID
	--CHECK IF INPUT MATCHES CURRENT STATE, IF SO, EXIST EARLY
	IF (@IsSet=COALESCE(@IsSetNow,0)) RETURN;
	IF (@IsSet=1) BEGIN
		INSERT INTO PersonSlot (PersonID, SlotTypeID) VALUES (@PersonID, @SkillID)
	END
	ELSE BEGIN
		DELETE FROM PersonSlot WHERE PersonID=@PersonID AND SlotTypeID=@SkillID
	END
END
GO
GRANT EXECUTE ON [dbo].[SetPersonSkill] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[UpdateFloorplan]    Script Date: 1/2/2025 3:04:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateFloorplan]
	@data varbinary(max)
AS
BEGIN
	SET NOCOUNT ON;

    DELETE FROM Floorplan;
	INSERT INTO Floorplan (Image) VALUES (@data)
END
GO
GRANT EXECUTE ON [dbo].[UpdateFloorplan] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[UpdateLocation]    Script Date: 1/2/2025 3:04:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateLocation]
    @ID int,
	@Name varchar(20),
	@LocX int,
	@LocY int,
	@SizeH int,
	@SizeW int
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE Location SET [Name]=@Name, LocX=@LocX, LocY=@LocY, SizeW=@SizeW, SizeH=@SizeH WHERE LocID=@ID
END
GO
GRANT EXECUTE ON [dbo].[UpdateLocation] TO [ClinicEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[UpdatePerson]    Script Date: 1/2/2025 3:04:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[UpdatePerson]
	@PersonID int,
	@LastName varchar(25),
	@FirstName varchar(25),
	@TeamID int,
	@Active bit,
	@Available bit
AS BEGIN
	DECLARE @AdjustedTeam int = @TeamID
	IF @TeamID = -1 SET @AdjustedTeam = NULL
	UPDATE Person
	SET FirstName=@FirstName, LastName=@LastName, TeamID=@AdjustedTeam, Active=@Active, Available=@Available
	WHERE PersonID=@PersonID
END
GO
GRANT EXECUTE ON [dbo].[UpdatePerson] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTeam]    Script Date: 1/2/2025 3:04:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateTeam]
	@TeamID int,
	@Name varchar(20),
	@Lead int,
	@LocID int,
	@Fill bit,
	@Active bit
AS
BEGIN
	DECLARE @AdjustedLead int = @Lead
	DECLARE @AdjustedLoc int = @LocID
	DECLARE @Result TABLE (ID int)
	IF @Lead = -1 SET @AdjustedLead = NULL
	IF @LocID = -1 SET @AdjustedLoc = NULL
	SET NOCOUNT ON;
	UPDATE Team 
	SET TeamName=@Name, TeamLead=@AdjustedLead, PrimaryLocation=@AdjustedLoc, FillIfNoLead=@Fill, Active=@Active 
	WHERE TeamID=@TeamID
END
GO
GRANT EXECUTE ON [dbo].[UpdateTeam] TO [PersonnelEditor] AS [dbo]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTeamSlot]    Script Date: 1/2/2025 3:04:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Nicholas Gibson
-- Create date: Jan 2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateTeamSlot]
	@SlotID int,
	@TeamID int,
	@SlotType int,
	@MinQty int,
	@GoalQty int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE LocationSlot
	SET TeamID=@TeamID, SlotType=@SlotType, MinQty=@MinQty, GoalQty=@GoalQty
	WHERE SlotID=@SlotID

END
GO
