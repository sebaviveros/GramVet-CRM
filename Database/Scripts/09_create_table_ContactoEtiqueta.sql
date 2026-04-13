CREATE TABLE ContactoEtiqueta (
    Id INT IDENTITY PRIMARY KEY,
    ContactoId INT,
    EtiquetaId INT,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (ContactoId) REFERENCES Contacto(Id),
    FOREIGN KEY (EtiquetaId) REFERENCES Etiqueta(Id)
);