/* =============================================================================
   GramVetCRM — INSTALACIÓN DE PRODUCCIÓN DESDE CERO
   -----------------------------------------------------------------------------
   Crea la base `GramVetCRM`, las 15 tablas en ORDEN DE DEPENDENCIAS
   (Rol antes que Usuario, etc.; equivale a correr los scripts 01–16 —el 12
   son índices— más los dos de Datos) y siembra los datos mínimos:
     - Canales de mensajería (WhatsApp queda con Id = 1, obligatorio).
     - Usuario administrador inicial (AdminSys).

   Correr UNA sola vez en SSMS (Ejecutar / F5) conectado a localhost\SQLEXPRESS.

   ⚠️ El hash del admin está versionado en git → contraseña "conocida".
      Apenas entres al CRM por primera vez, cámbiala desde
      "Mi cuenta → Cambiar contraseña".
   ============================================================================= */

-- ─────────────────────────────────────────────────────────────────────────────
-- 0) Crear la base de producción
-- ─────────────────────────────────────────────────────────────────────────────
IF DB_ID('GramVetCRM') IS NOT NULL
BEGIN
    RAISERROR('La base GramVetCRM YA EXISTE. Abortando para no pisar datos.', 16, 1);
    RETURN;
END
GO

CREATE DATABASE GramVetCRM;
GO

USE GramVetCRM;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 02) Rol  (va PRIMERO: Usuario tiene FK a Rol)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Rol (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100),
    Descripcion NVARCHAR(300),
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);
GO

INSERT INTO Rol (Nombre, Descripcion, usercr, fechacr, active)
VALUES
('Admin', 'Acceso total', 'system', GETDATE(), 1),
('Secretario', 'Gestiona conversaciones', 'system', GETDATE(), 1),
('Veterinario', 'Atiende clientes', 'system', GETDATE(), 1);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 01) Usuario
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Usuario (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150),
    Apellido NVARCHAR(150),
    Email NVARCHAR(150),
    Username NVARCHAR(150),
    PasswordHash NVARCHAR(500),
    RolId INT NOT NULL,
    FotoUrl NVARCHAR(500) NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (RolId) REFERENCES Rol(Id)
);
GO

CREATE UNIQUE INDEX UX_Usuario_Username ON Usuario (Username);
CREATE UNIQUE INDEX UX_Usuario_Email ON Usuario (Email);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 03) Contacto
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Contacto (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150),
    Apellido NVARCHAR(150) NULL,
    Telefono NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150),
    Direccion NVARCHAR(300) NULL,
    ReferenciaDireccion NVARCHAR(500) NULL,
    EsNuevo BIT DEFAULT 0 NOT NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 04) Canal
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Canal (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(50),
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 05) Conversacion  (FK a Contacto, Canal, Usuario)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Conversacion (
    Id INT IDENTITY PRIMARY KEY,
    ContactoId INT NOT NULL,
    CanalId INT NOT NULL,
    UsuarioAsignadoId INT NULL,
    UltimoMensaje NVARCHAR(MAX),
    FechaUltimoMensaje DATETIME,
    CantidadNoLeidos INT DEFAULT 0,
    Estado NVARCHAR(50),
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    FOREIGN KEY (ContactoId) REFERENCES Contacto(Id),
    FOREIGN KEY (CanalId) REFERENCES Canal(Id),
    FOREIGN KEY (UsuarioAsignadoId) REFERENCES Usuario(Id)
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 06) Mensaje  (FK a Conversacion)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Mensaje (
    Id INT IDENTITY PRIMARY KEY,
    ConversacionId INT NOT NULL,
    Contenido NVARCHAR(MAX),
    TipoMensaje NVARCHAR(50),
    Direccion NVARCHAR(20),
    ExternalId NVARCHAR(100),          -- ID de WhatsApp (CRÍTICO)
    UsuarioId INT NULL,                -- quién envía el msj (solo outbound)
    MediaUrl NVARCHAR(500) NULL,
    Reaccion NVARCHAR(20) NULL,        -- emoji de reacción (WhatsApp/Meta)
    EstadoEntrega NVARCHAR(20) NULL,   -- sent / delivered / read / failed
    FechaEnvio DATETIME NOT NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    FOREIGN KEY (ConversacionId) REFERENCES Conversacion(Id)
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 07) EstadoMensaje  (FK a Mensaje)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE EstadoMensaje (
    Id INT IDENTITY PRIMARY KEY,
    MensajeId INT NOT NULL,
    Estado NVARCHAR(50),
    FechaEstado DATETIME,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    FOREIGN KEY (MensajeId) REFERENCES Mensaje(Id)
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 08) Etiqueta
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Etiqueta (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100),
    Descripcion NVARCHAR(300),
    Color NVARCHAR(50),
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 09) ContactoEtiqueta  (FK a Contacto, Etiqueta)
-- ─────────────────────────────────────────────────────────────────────────────
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
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 10) RespuestaRapida
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE RespuestaRapida (
    Id INT IDENTITY PRIMARY KEY,
    Comando NVARCHAR(50),
    Texto NVARCHAR(MAX),
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 11) Mascota  (FK a Contacto)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Mascota (
    Id INT IDENTITY PRIMARY KEY,
    ContactoId INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Especie NVARCHAR(100),
    Raza NVARCHAR(150),
    FechaNacimiento DATE NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    FOREIGN KEY (ContactoId) REFERENCES Contacto(Id)
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 12) Índices de rendimiento
-- ─────────────────────────────────────────────────────────────────────────────
CREATE INDEX IX_Mensaje_ConversacionId_FechaEnvio
ON Mensaje (ConversacionId, FechaEnvio DESC);

CREATE INDEX IX_Conversacion_ContactoId
ON Conversacion (ContactoId);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 13) MascotaBitacora  (FK a Mascota)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE MascotaBitacora (
    Id INT IDENTITY PRIMARY KEY,
    MascotaId INT NOT NULL,
    Contenido NVARCHAR(MAX) NULL,      -- anotación (opcional: puede ser solo imágenes)
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    FOREIGN KEY (MascotaId) REFERENCES Mascota(Id)
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 14) MascotaFoto  (FK a MascotaBitacora)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE MascotaFoto (
    Id INT IDENTITY PRIMARY KEY,
    BitacoraId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,        -- URL pública en Cloudflare R2
    Descripcion NVARCHAR(300) NULL,
    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1,
    CONSTRAINT FK_MascotaFoto_MascotaBitacora
        FOREIGN KEY (BitacoraId) REFERENCES MascotaBitacora(Id)
);
GO

CREATE INDEX IX_MascotaFoto_BitacoraId ON MascotaFoto(BitacoraId);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 15) EtiquetaUsuario  (FK a Etiqueta, Usuario)
-- ─────────────────────────────────────────────────────────────────────────────
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

-- ─────────────────────────────────────────────────────────────────────────────
-- 16) RespuestaRapidaUsuario  (FK a RespuestaRapida, Usuario)
-- ─────────────────────────────────────────────────────────────────────────────
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

-- ─────────────────────────────────────────────────────────────────────────────
-- DATOS 01) Canales  (WhatsApp DEBE quedar con Id = 1 — hardcodeado en el backend)
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO Canal (Nombre, usercr, fechacr, active) VALUES ('WhatsApp',  'system', GETDATE(), 1);  -- Id = 1
INSERT INTO Canal (Nombre, usercr, fechacr, active) VALUES ('Instagram', 'system', GETDATE(), 1);
INSERT INTO Canal (Nombre, usercr, fechacr, active) VALUES ('Messenger', 'system', GETDATE(), 1);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- DATOS 02) Usuario administrador inicial (RolId 1 = Admin)
--   Hash BCrypt versionado → cambiar la contraseña al primer login.
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO Usuario (
    Nombre, Apellido, Email, Username, PasswordHash, RolId,
    usercr, userup, fechacr, fechaup, active
)
VALUES (
    'Admin', 'Sistema', 'sviverosn1@gmail.com', 'AdminSys',
    '$2a$11$nQQ9fbXwxNenC2Fnb6Kb5uD9.5EemIBsEPcl0X.CbsVvclT6RitIi',
    1, 'system', NULL, GETDATE(), NULL, 1
);
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- VERIFICACIÓN
-- ─────────────────────────────────────────────────────────────────────────────
PRINT '--- Canales (WhatsApp debe ser Id 1) ---';
SELECT Id, Nombre FROM Canal ORDER BY Id;

PRINT '--- Roles ---';
SELECT Id, Nombre FROM Rol ORDER BY Id;

PRINT '--- Usuario admin ---';
SELECT Id, Username, Email, RolId FROM Usuario;

PRINT '--- Tablas creadas ---';
SELECT name FROM sys.tables ORDER BY name;
GO
