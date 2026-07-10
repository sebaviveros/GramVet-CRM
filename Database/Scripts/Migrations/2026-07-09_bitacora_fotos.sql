/* =============================================================================
   Las fotos de mascota pasan a colgar de una ANOTACIÓN de bitácora, no de la
   mascota. Una anotación puede llevar varias imágenes (receta, boleta, etc.)
   y puede ser SOLO imágenes, sin texto.

   ⚠️  Las fotos existentes se DESCARTAN (son de prueba — decisión 2026-07-09).
       Se borran las filas; los objetos en Cloudflare R2 quedan huérfanos, lo
       cual es irrelevante en desarrollo.

   Correr contra GramVetCRM_DEV.
   ============================================================================= */

USE GramVetCRM_DEV;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 1) MascotaBitacora.Contenido pasa a ser opcional (anotación de solo imágenes)
-- ─────────────────────────────────────────────────────────────────────────────
ALTER TABLE MascotaBitacora ALTER COLUMN Contenido NVARCHAR(MAX) NULL;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 2) MascotaFoto: de MascotaId a BitacoraId
-- ─────────────────────────────────────────────────────────────────────────────

-- Fotos de prueba: se descartan (no hay a qué anotación reasignarlas)
DELETE FROM MascotaFoto;
GO

-- La FK tiene nombre autogenerado: se busca antes de borrarla
DECLARE @fk NVARCHAR(200) = (
    SELECT TOP 1 name FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('MascotaFoto')
      AND referenced_object_id = OBJECT_ID('Mascota')
);
IF @fk IS NOT NULL EXEC('ALTER TABLE MascotaFoto DROP CONSTRAINT [' + @fk + ']');
GO

-- Se elimina MascotaId: la anotación ya sabe de qué mascota es. Dejarla sería
-- una segunda fuente de verdad que puede desincronizarse.
IF COL_LENGTH('MascotaFoto', 'MascotaId') IS NOT NULL
    ALTER TABLE MascotaFoto DROP COLUMN MascotaId;
GO

ALTER TABLE MascotaFoto ADD BitacoraId INT NOT NULL;
GO

ALTER TABLE MascotaFoto
    ADD CONSTRAINT FK_MascotaFoto_MascotaBitacora
    FOREIGN KEY (BitacoraId) REFERENCES MascotaBitacora(Id);
GO

CREATE INDEX IX_MascotaFoto_BitacoraId ON MascotaFoto(BitacoraId);
GO
