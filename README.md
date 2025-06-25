# MiniAccountsSystem

## please follow the below instructions:


run the below scripts
#1.update-database from package console [Visual Studio]


#2. run the below for sql server

CREATE PROCEDURE [dbo].[sp_ManageChartOfAccounts]
    @Action NVARCHAR(10),
    @AccountId INT = NULL,
    @AccountName NVARCHAR(100) = NULL,
    @ParentAccountId INT = NULL,
    @AccountType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'Create'
    BEGIN
        INSERT INTO dbo.ChartOfAccounts (AccountName, ParentAccountId, AccountType)
        VALUES (@AccountName, @ParentAccountId, @AccountType)
    END
    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE ChartOfAccounts
        SET AccountName = @AccountName,
            ParentAccountId = @ParentAccountId,
            AccountType = @AccountType
        WHERE AccountId = @AccountId
    END
    ELSE IF @Action = 'Delete'
    BEGIN
        DELETE FROM ChartOfAccounts WHERE AccountId = @AccountId
    END
    ELSE IF @Action = 'Select'
    BEGIN
        SELECT * FROM ChartOfAccounts
    END;
END;
GO

Create proc [dbo].[sp_MiniAccount_AssignRight] 
@userId nvarchar(450), 
@roleId nvarchar(450)
as
begin
insert into AspNetUserRoles(UserId,RoleId) values(@userId,@roleId);
end;
GO


#3: 

  your admin id = "admin@yourapp.com"; // Choose a default admin email
 admin Password = "AdminPassword123!"; // *IMPORTANT: Use a strong default password and consider changing it later*


 

