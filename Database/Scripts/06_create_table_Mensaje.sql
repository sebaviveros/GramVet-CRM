CREATE TABLE Mensaje (
    Id INT IDENTITY PRIMARY KEY,
    ConversacionId INT NOT NULL,

    Contenido NVARCHAR(MAX),
    TipoMensaje NVARCHAR(50), -- texto, imagen, audio

    Direccion NVARCHAR(20), -- inbound / outbound

    ExternalId NVARCHAR(100), -- ID de WhatsApp (CRÍTICO)
	UsuarioId INT NULL, -- quien envia el msj (solo outbound)
    FechaEnvio DATETIME NOT NULL,
	
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (ConversacionId) REFERENCES Conversacion(Id)
);