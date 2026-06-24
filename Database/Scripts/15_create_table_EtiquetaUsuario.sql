-- Visibilidad de etiquetas por veterinario.
-- Admin y Secretario ven TODAS las etiquetas (sin filtro).
-- Un Veterinario solo ve una etiqueta si existe una fila que lo vincule aquí.
CREATE TABLE EtiquetaUsuario (
    Id INT IDENTITY PRIMARY KEY,
    EtiquetaId INT NOT NULL,
    UsuarioId INT NOT NULL,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (EtiquetaId) REFERENCES Etiqueta(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
);
