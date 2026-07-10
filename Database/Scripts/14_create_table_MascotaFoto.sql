-- Las fotos cuelgan de una ANOTACIÓN de bitácora (no de la mascota):
-- una anotación puede llevar varias imágenes, y ya sabe a qué mascota pertenece.
-- Corre después de 13_create_table_MascotaBitacora.sql (depende de esa tabla).
CREATE TABLE MascotaFoto (
    Id INT IDENTITY PRIMARY KEY,
    BitacoraId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,      -- URL pública en Cloudflare R2
    Descripcion NVARCHAR(300) NULL,  -- ej. "Receta", "Radiografía"

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    CONSTRAINT FK_MascotaFoto_MascotaBitacora
        FOREIGN KEY (BitacoraId) REFERENCES MascotaBitacora(Id)
);

CREATE INDEX IX_MascotaFoto_BitacoraId ON MascotaFoto(BitacoraId);
