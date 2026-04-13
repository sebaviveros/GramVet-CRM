CREATE TABLE Conversacion (
    Id INT IDENTITY PRIMARY KEY,
    ContactoId INT NOT NULL,
    CanalId INT NOT NULL,

    UsuarioAsignadoId INT NULL,

    UltimoMensaje NVARCHAR(MAX),
    FechaUltimoMensaje DATETIME,
    CantidadNoLeidos INT DEFAULT 0,

    Estado NVARCHAR(50), -- Abierta, Cerrada, Pendiente

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (ContactoId) REFERENCES Contacto(Id),
    FOREIGN KEY (CanalId) REFERENCES Canal(Id),
    FOREIGN KEY (UsuarioAsignadoId) REFERENCES Usuario(Id)
);