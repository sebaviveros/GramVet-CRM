-- Crear Rol
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

-- Insertar roles
INSERT INTO Rol (Nombre, Descripcion, usercr, fechacr, active)
VALUES
('Admin', 'Acceso total', 'system', GETDATE(), 1),
('Secretario', 'Gestiona conversaciones', 'system', GETDATE(), 1),
('Veterinario', 'Atiende clientes', 'system', GETDATE(), 1);

GO