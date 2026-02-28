using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace val12
{
    internal static class Database
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["LfConnection"].ConnectionString;

        private static string GetAuthConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = "LfAuth";
            return builder.ToString();
        }

        public static void Initialize()
        {
            // 1. Создаём базу LfAuth, если её нет (подключаемся к master)
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = "master";
            var masterConnectionString = builder.ToString();

            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                var sql = @"
IF DB_ID('LfAuth') IS NULL
BEGIN
    CREATE DATABASE LfAuth;
END;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // 2. Подключаемся к LfAuth и создаём таблицы, если их нет
            builder.InitialCatalog = "LfAuth";
            var authConnectionString = builder.ToString();

            using (var connection = new SqlConnection(authConnectionString))
            {
                connection.Open();

                var sql = @"
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NULL,
        Email NVARCHAR(100) NULL,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NULL,
        IsAdmin BIT NOT NULL DEFAULT(0),
        IsBlocked BIT NOT NULL DEFAULT(0),
        CreatedAt DATETIME NOT NULL DEFAULT(GETDATE()),
        LastLogin DATETIME NULL
    );
END;

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Price DECIMAL(10,2) NOT NULL,
        IsFrozen BIT NOT NULL DEFAULT(1)
    );
END;

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT(GETDATE())
    );
END;

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(10,2) NOT NULL
    );
END;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем колонки IsAdmin/IsBlocked, если таблица уже была
                var alterSql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'IsAdmin' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD IsAdmin BIT NOT NULL DEFAULT(0);
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'IsBlocked' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD IsBlocked BIT NOT NULL DEFAULT(0);
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'FullName' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD FullName NVARCHAR(100) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Email' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD Email NVARCHAR(100) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Role' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD Role NVARCHAR(50) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'LastLogin' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD LastLogin DATETIME NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Password' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users ADD Password NVARCHAR(100) NULL;
END;
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'PasswordHash' AND Object_ID = Object_ID('dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users DROP COLUMN PasswordHash;
END;";

                using (var alterCmd = new SqlCommand(alterSql, connection))
                {
                    alterCmd.ExecuteNonQuery();
                }

                // Обновляем пароли обычным текстом — отдельной командой, чтобы новая колонка была видна
                var passwordSql = @"
UPDATE dbo.Users SET Password = N'admin' WHERE Username = N'admin';
UPDATE dbo.Users SET Password = N'123' WHERE Username <> N'admin' AND (Password IS NULL OR LEN(ISNULL(Password,'')) = 64);";
                try
                {
                    using (var pwdCmd = new SqlCommand(passwordSql, connection))
                    {
                        pwdCmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    // Колонка Password ещё не создана (старая база без миграции) — игнорируем
                }

                // Заполняем новые поля для существующих строк
                var migrateSql = @"
UPDATE dbo.Users SET FullName = Username WHERE FullName IS NULL;
UPDATE dbo.Users SET Email = Username + N'@local' WHERE Email IS NULL;
UPDATE dbo.Users SET Role = CASE WHEN IsAdmin = 1 THEN N'Администратор' ELSE N'Пользователь' END WHERE Role IS NULL;";

                using (var migrateCmd = new SqlCommand(migrateSql, connection))
                {
                    migrateCmd.ExecuteNonQuery();
                }

                // Админ и демо-товары (пароль — обычный текст, не шифруется)
                var seedSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users (Username, Password, IsAdmin, IsBlocked, FullName, Email, Role)
    VALUES ('admin', @adminPassword, 1, 0, N'Иванов А.С.', 'admin@makeevka.met', N'Администратор');
END
ELSE
BEGIN
    UPDATE dbo.Users SET FullName = N'Иванов А.С.', Email = 'admin@makeevka.met', Role = N'Администратор', Password = @adminPassword WHERE Username = 'admin';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (Name, Price) VALUES
        (N'Пельмени замороженные 1 кг', 350.00),
        (N'Пицца замороженная 450 г', 420.00),
        (N'Овощи замороженные микс 400 г', 210.00),
        (N'Мороженое пломбир 500 г', 180.00),
        (N'Наггетсы куриные замороженные 300 г', 260.00);
END;";

                using (var seedCmd = new SqlCommand(seedSql, connection))
                {
                    seedCmd.Parameters.AddWithValue("@adminPassword", "admin");
                    seedCmd.ExecuteNonQuery();
                }
            }
        }

        public static bool RegisterUser(string fullName, string email, string username, string password, out string errorMessage)
        {
            errorMessage = null;

            // Пароль сохраняется как есть (обычный текст), как при регистрации
            try
            {
                using (var connection = new SqlConnection(GetAuthConnectionString()))
                {
                    connection.Open();

                    const string sql =
                        @"INSERT INTO dbo.Users (FullName, Email, Username, Password, IsAdmin, IsBlocked, Role)
                          VALUES (@fullName, @email, @username, @password, 0, 0, N'Пользователь');";

                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@fullName", (object)fullName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // unique constraint
                {
                    errorMessage = "Пользователь с таким логином уже существует.";
                }
                else
                {
                    errorMessage = "Ошибка БД: " + ex.Message;
                }

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Не удалось сохранить пользователя: " + ex.Message;
                return false;
            }
        }

        public static bool ValidateUser(string username, string password, out bool isAdmin, out bool isBlocked)
        {
            isAdmin = false;
            isBlocked = false;

            // Сравнение с паролем как с обычным текстом (как при регистрации)
            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();

                const string sql =
                    @"SELECT Password, IsAdmin, IsBlocked
                      FROM dbo.Users
                      WHERE Username = @username;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        var storedPassword = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        isAdmin = !reader.IsDBNull(1) && reader.GetBoolean(1);
                        isBlocked = !reader.IsDBNull(2) && reader.GetBoolean(2);
                        if (string.IsNullOrEmpty(storedPassword)) storedPassword = "";
                        if (storedPassword != password)
                            return false;
                    }
                }

                // Обновляем LastLogin при успешном входе
                using (var conn2 = new SqlConnection(GetAuthConnectionString()))
                {
                    conn2.Open();
                    using (var cmd2 = new SqlCommand("UPDATE dbo.Users SET LastLogin = GETDATE() WHERE Username = @u", conn2))
                    {
                        cmd2.Parameters.AddWithValue("@u", username);
                        cmd2.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }

        public static DataTable GetProducts()
        {
            var table = new DataTable();

            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();

                const string sql = @"SELECT Id, Name, Price FROM dbo.Products ORDER BY Name;";

                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        public static void AddProduct(string name, decimal price)
        {
            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();
                const string sql = @"INSERT INTO dbo.Products (Name, Price) VALUES (@name, @price);";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteProduct(int productId)
        {
            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    const string sqlOrderItems = @"DELETE FROM dbo.OrderItems WHERE ProductId = @id;";
                    using (var cmd = new SqlCommand(sqlOrderItems, connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.ExecuteNonQuery();
                    }
                    const string sqlProduct = @"DELETE FROM dbo.Products WHERE Id = @id;";
                    using (var cmd = new SqlCommand(sqlProduct, connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        private static int GetUserIdByUsername(SqlConnection connection, SqlTransaction tx, string username)
        {
            const string sql = @"SELECT Id FROM dbo.Users WHERE Username = @username;";
            using (var cmd = new SqlCommand(sql, connection, tx))
            {
                cmd.Parameters.AddWithValue("@username", username);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Пользователь не найден.");
                return Convert.ToInt32(result);
            }
        }

        public static void PlaceOrder(string username, System.Collections.Generic.Dictionary<int, int> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Корзина пуста.", nameof(items));

            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    var userId = GetUserIdByUsername(connection, tx, username);

                    int orderId;
                    const string insertOrderSql =
                        @"INSERT INTO dbo.Orders (UserId) OUTPUT INSERTED.Id VALUES (@userId);";
                    using (var cmd = new SqlCommand(insertOrderSql, connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        orderId = (int)cmd.ExecuteScalar();
                    }

                    const string insertItemSql =
                        @"INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, Price)
                          SELECT @orderId, p.Id, @qty, p.Price
                          FROM dbo.Products p
                          WHERE p.Id = @productId;";

                    foreach (var kvp in items)
                    {
                        using (var cmd = new SqlCommand(insertItemSql, connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.Parameters.AddWithValue("@productId", kvp.Key);
                            cmd.Parameters.AddWithValue("@qty", kvp.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        public static DataTable GetUsers()
        {
            var table = new DataTable();

            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();

                const string sql = @"
SELECT Id AS UserID, FullName, Email, Username AS Login,
       Password,
       Role,
       CASE WHEN IsBlocked = 1 THEN 0 ELSE 1 END AS IsActive,
       CreatedAt, LastLogin
FROM dbo.Users
ORDER BY Username;";

                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        public static void SetUserBlocked(int userId, bool blocked)
        {
            using (var connection = new SqlConnection(GetAuthConnectionString()))
            {
                connection.Open();

                const string sql = @"UPDATE dbo.Users SET IsBlocked = @blocked WHERE Id = @id;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@blocked", blocked);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string ComputeHash(string value)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.AppendFormat("{0:x2}", b);
                }

                return sb.ToString();
            }
        }
    }
}

