CREATE TABLE Contacto (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150),
    Telefono NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150),

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);