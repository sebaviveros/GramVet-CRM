CREATE TABLE RespuestaRapida (
    Id INT IDENTITY PRIMARY KEY,
    Comando NVARCHAR(50), -- /hola
    Texto NVARCHAR(MAX),  -- respuesta completa

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);