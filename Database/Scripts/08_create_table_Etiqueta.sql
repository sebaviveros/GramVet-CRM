CREATE TABLE Etiqueta (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100),
    Descripcion NVARCHAR(300),
    Color NVARCHAR(50),

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);