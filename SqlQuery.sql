CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,              -- Primary Key
    Email NVARCHAR(100) NOT NULL UNIQUE,               -- Email must be unique
    PasswordHash NVARCHAR(255) NOT NULL,               -- Store hashed password
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',         -- Role: User/Admin
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()      -- Account creation date
);
-------------------------------------------------------------------------------------------------------
CREATE TABLE Projects (
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectName NVARCHAR(200) NOT NULL,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)      -- Foreign Key constraint
);
-------------------------------------------------------------------------------------------------------
INSERT INTO Users (Email, PasswordHash, Role)
VALUES ('demo@example.com', 'password', 'User');
