-- Подключись к серверу LF, выбери базу LfAuth и выполни этот скрипт.
-- Таблица пользователей уже создаётся приложением при запуске.
-- Этот скрипт создаёт представление с колонками как на твоём скриншоте:
-- UserID, FullName, Email, Login, PasswordHash, Role, IsActive, CreatedAt, LastLogin

USE LfAuth;
GO

IF OBJECT_ID('dbo.vw_UsersReport', 'V') IS NOT NULL
    DROP VIEW dbo.vw_UsersReport;
GO

CREATE VIEW dbo.vw_UsersReport AS
SELECT
    Id AS UserID,
    FullName,
    Email,
    Username AS Login,
    PasswordHash,
    Role,
    CASE WHEN IsBlocked = 1 THEN 0 ELSE 1 END AS IsActive,
    CreatedAt,
    LastLogin
FROM dbo.Users;
GO

-- Посмотреть данные как на скриншоте:
-- SELECT * FROM dbo.vw_UsersReport ORDER BY UserID;
