-- Crear Usuario
CREATE TABLE Usuario (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150),
    Apellido NVARCHAR(150),
    Email NVARCHAR(150),
    Username NVARCHAR(150),
    PasswordHash NVARCHAR(500),
    RolId INT NOT NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (RolId) REFERENCES Rol(Id)
);

GO

-- Índices
CREATE UNIQUE INDEX UX_Usuario_Username ON Usuario (Username);
CREATE UNIQUE INDEX UX_Usuario_Email ON Usuario (Email);

GO