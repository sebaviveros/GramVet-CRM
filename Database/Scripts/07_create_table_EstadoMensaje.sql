CREATE TABLE EstadoMensaje (
    Id INT IDENTITY PRIMARY KEY,
    MensajeId INT NOT NULL,

    Estado NVARCHAR(50), -- enviado, entregado, leido
    FechaEstado DATETIME,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (MensajeId) REFERENCES Mensaje(Id)
);