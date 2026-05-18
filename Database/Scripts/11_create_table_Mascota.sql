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