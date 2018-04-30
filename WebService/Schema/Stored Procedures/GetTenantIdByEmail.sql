USE [SZ_Encryptics]
GO

/****** Object:  StoredProcedure [dbo].[GetTenantIdByEmail]    Script Date: 03-29-2018 15:00:24 ******/
IF OBJECT_ID('GetTenantIdByEmail', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[GetTenantIdByEmail]
GO

/****** Object:  StoredProcedure [dbo].[GetTenantIdByEmail]    Script Date: 03-29-2018 15:00:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE
 [dbo].[GetTenantIdByEmail] 
	@Email		VARCHAR(100),
	@TenantId nvarchar(120) OUTPUT
AS 
BEGIN
Declare @EntityId int;
	SELECT	@EntityId=EntityID
				FROM	dbo.Users
				WHERE	Email = RTRIM(@Email)
	SElECT @TenantId=LTRIM(RTRIM(Tenant ))
	FROM dbo.Entity_Authentication
	Where EntityId=@EntityId

END



GO


