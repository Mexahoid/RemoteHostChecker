IF NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Persons]'))
BEGIN
CREATE TABLE [dbo].[Persons]
(
	[ID] INT PRIMARY KEY IDENTITY,
	[Login] VARCHAR(40) NOT NULL UNIQUE,
	[Password] VARCHAR(500) NOT NULL,
	[Role] SMALLINT NOT NULL,
);

INSERT INTO [dbo].[Persons] ([Login], [Password], [Role]) VALUES ('Admin', 'Admin', 1);
INSERT INTO [dbo].[Persons] ([Login], [Password], [Role]) VALUES ('Test1', 'Pass1', 0);
INSERT INTO [dbo].[Persons] ([Login], [Password], [Role]) VALUES ('Test2', 'Pass2', 0);
INSERT INTO [dbo].[Persons] ([Login], [Password], [Role]) VALUES ('Test3', 'Pass1', 0);

END




IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Persons]'))
BEGIN
CREATE TABLE [dbo].[CheckRequest]
(
	[ID] INT PRIMARY KEY IDENTITY,
	[HostAddress] VARCHAR(200) NOT NULL,
	[Cron] VARCHAR(100) NOT NULL,
	[PersonID] INT NOT NULL,

	CONSTRAINT FK_CheckRequest_To_Persons FOREIGN KEY (PersonID) REFERENCES [dbo].[Persons] (ID) ON DELETE CASCADE ON UPDATE CASCADE
);

INSERT INTO [dbo].[CheckRequest] ([HostAddress], [Cron], [PersonID]) VALUES ('google.com', '1 * * * *', (SELECT ID FROM [dbo].[Persons] WHERE [Login] = 'Test1'));
INSERT INTO [dbo].[CheckRequest] ([HostAddress], [Cron], [PersonID]) VALUES ('vsu.ru', '3 * * * *', (SELECT ID FROM [dbo].[Persons] WHERE [Login] = 'Test1'));
INSERT INTO [dbo].[CheckRequest] ([HostAddress], [Cron], [PersonID]) VALUES ('yandex.ru', '5 * * * *', (SELECT ID FROM [dbo].[Persons] WHERE [Login] = 'Test2'));
INSERT INTO [dbo].[CheckRequest] ([HostAddress], [Cron], [PersonID]) VALUES ('ibm.com', '1 * * * *', (SELECT ID FROM [dbo].[Persons] WHERE [Login] = 'Test3'));
INSERT INTO [dbo].[CheckRequest] ([HostAddress], [Cron], [PersonID]) VALUES ('youtube.com', '1 * * * *', (SELECT ID FROM [dbo].[Persons] WHERE [Login] = 'Test3'));

END

IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Persons]'))
BEGIN
CREATE TABLE [dbo].[CheckHistory]
(
	[ID] INT PRIMARY KEY IDENTITY,
	[CheckID] INT NOT NULL,
	[Moment] DATETIME NOT NULL,
	[Result] INT NOT NULL,

	CONSTRAINT FK_CheckHistory_To_CheckRequest FOREIGN KEY (CheckID) REFERENCES [dbo].[CheckRequest] (ID) ON DELETE NO ACTION ON UPDATE NO ACTION
);
END

