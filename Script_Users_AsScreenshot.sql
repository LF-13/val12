-- Скрипт воспроизводит таблицу пользователей как на скриншоте.
-- Выполни на сервере LF в базе LfAuth (или измени USE на свою базу).
-- Внимание: если есть таблицы Orders/OrderItems (ссылаются на Users), сначала удали их или выполни скрипт в пустой базе.

USE LfAuth;
GO

-- Таблица как на скриншоте: Id, Username, PasswordHash, CreatedAt, IsAdmin, IsBlocked, FullName, Email, Role, LastLogin
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64)   NOT NULL,
    CreatedAt    DATETIME       NOT NULL DEFAULT(GETDATE()),
    IsAdmin      BIT            NOT NULL DEFAULT(0),
    IsBlocked    BIT            NOT NULL DEFAULT(0),
    FullName     NVARCHAR(100)  NULL,
    Email        NVARCHAR(100)  NULL,
    Role         NVARCHAR(50)   NULL,
    LastLogin    DATETIME       NULL
);
GO

-- Данные как на скриншоте (4 пользователя)
SET IDENTITY_INSERT dbo.Users ON;

INSERT INTO dbo.Users (Id, Username, PasswordHash, CreatedAt, IsAdmin, IsBlocked, FullName, Email, Role, LastLogin)
VALUES
    (1, N'LF',   N'03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', '2026-02-27 22:26:26.957', 0, 0, N'LF',           N'LF@local',          N'Пользователь', NULL),
    (2, N'admin', N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', '2026-02-27 22:34:18.750', 1, 0, N'Иванов А.С.',  N'admin@makeevka.met', N'Администратор', NULL),
    (3, N'n1',   N'03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', '2026-02-27 22:34:49.267', 0, 0, N'n1',           N'n1@local',           N'Пользователь', NULL),
    (4, N'user1', N'03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', '2026-02-28 14:03:43.997', 0, 0, N'Man',          N'belov@gmail.com',    N'Пользователь', NULL);

SET IDENTITY_INSERT dbo.Users OFF;
GO

-- Результат как на скриншоте:
SELECT Id, Username, PasswordHash, CreatedAt, IsAdmin, IsBlocked, FullName, Email, Role, LastLogin
FROM dbo.Users
ORDER BY Id;
