CREATE TABLE MascotaFoto (
    Id INT IDENTITY PRIMARY KEY,
    MascotaId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,      -- URL pública en Cloudflare R2
    Descripcion NVARCHAR(300) NULL,  -- ej. "Receta", "Radiografía"

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (MascotaId) REFERENCES Mascota(Id)
);
