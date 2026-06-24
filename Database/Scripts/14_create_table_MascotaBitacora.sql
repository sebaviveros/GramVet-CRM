CREATE TABLE MascotaBitacora (
    Id INT IDENTITY PRIMARY KEY,
    MascotaId INT NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,  -- una anotación de la bitácora

    usercr NVARCHAR(100),              -- quién escribió la anotación
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),-- fecha de la sección/anotación
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (MascotaId) REFERENCES Mascota(Id)
);
