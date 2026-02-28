-- Запусти в SSMS (база LfAuth на сервере LF).
-- Делает пароли обычным текстом: admin = admin, остальные = 123, очищает хеши.

USE LfAuth;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Password' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD Password NVARCHAR(100) NULL;
END;
GO

UPDATE dbo.Users SET Password = N'admin' WHERE Username = N'admin';
UPDATE dbo.Users SET Password = N'123' WHERE Username <> N'admin' AND (Password IS NULL OR LEN(ISNULL(Password,'')) = 64);
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'PasswordHash' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN PasswordHash;
END;
GO

SELECT Id, Username, Password AS [Пароль], FullName, Email, Role FROM dbo.Users;
