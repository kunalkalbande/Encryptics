USE [SZ_Encryptics]
GO

/****** Object:  Table [dbo].[Entity_Authentication]    Script Date: 03-29-2018 14:43:28 ******/
IF Exists(SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'Entity_Authentication')
Begin
DROP TABLE [dbo].[Entity_Authentication]
End 
/****** Object:  Table [dbo].[Entity_Authentication]    Script Date: 03-29-2018 14:43:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Entity_Authentication](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NULL,
	[Tenant] [nchar](10) NULL,
 CONSTRAINT [PK_Entity_Authenticatio] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


