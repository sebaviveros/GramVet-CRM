CREATE TABLE Rol (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100), -- Admin, Secretario, Veterinario
    Descripcion NVARCHAR(300),

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);