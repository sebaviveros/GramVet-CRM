/* ============================================================================
   Migración 2026-06-23
   Aplica sobre una BD ya creada (ej. GramVetCRM_DEV) sin perder datos.
   Idempotente: se puede ejecutar varias veces sin error.

   Cubre:
     - Feature 2: Dirección + referencias de dirección del contacto
     - Feature 6: Fotos de mascota (MascotaFoto)
     - Feature 7: Bitácora de mascota (MascotaBitacora)
     - Feature 3: Visibilidad de etiquetas/respuestas por veterinario
                  (EtiquetaUsuario, RespuestaRapidaUsuario)
   ============================================================================ */

USE GramVetCRM_DEV;
GO

/* ---- Feature 2: columnas de dirección en Contacto -------------------------- */
IF COL_LENGTH('Contacto', 'Direccion') IS NULL
    ALTER TABLE Contacto ADD Direccion NVARCHAR(300) NULL;
GO

IF COL_LENGTH('Contacto', 'ReferenciaDireccion') IS NULL
    ALTER TABLE Contacto ADD ReferenciaDireccion NVARCHAR(500) NULL;
GO

/* ---- Feature 6: MascotaFoto ------------------------------------------------ */
IF OBJECT_ID('MascotaFoto', 'U') IS NULL
CREATE TABLE MascotaFoto (
    Id INT IDENTITY PRIMARY KEY,
    MascotaId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    Descripcion NVARCHAR(300) NULL,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (MascotaId) REFERENCES Mascota(Id)
);
GO

/* ---- Feature 7: MascotaBitacora ------------------------------------------- */
IF OBJECT_ID('MascotaBitacora', 'U') IS NULL
CREATE TABLE MascotaBitacora (
    Id INT IDENTITY PRIMARY KEY,
    MascotaId INT NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,

    FOREIGN KEY (MascotaId) REFERENCES Mascota(Id)
);
GO

/* ---- Feature 3: EtiquetaUsuario ------------------------------------------- */
IF OBJECT_ID('EtiquetaUsuario', 'U') IS NULL
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
GO

/* ---- Feature 3: RespuestaRapidaUsuario ------------------------------------ */
IF OBJECT_ID('RespuestaRapidaUsuario', 'U') IS NULL
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
GO
