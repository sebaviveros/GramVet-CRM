CREATE TABLE Usuario (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150),
    Apellido NVARCHAR(150),
    Email NVARCHAR(150),
    Username NVARCHAR(150),
    PasswordHash NVARCHAR(500),
    RolId INT,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);