-- Visibilidad de respuestas rápidas por veterinario.
-- Admin y Secretario ven TODAS las respuestas rápidas (sin filtro).
-- Un Veterinario solo ve una respuesta si existe una fila que lo vincule aquí.
CREATE TABLE RespuestaRapidaUsuario (
    Id INT IDENTITY PRIMARY KEY,
    RespuestaRapidaId INT NOT NULL,
    UsuarioId INT NOT NULL,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (RespuestaRapidaId) REFERENCES RespuestaRapida(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
);
