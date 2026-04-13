CREATE TABLE Canal (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(50), -- WhatsApp, Instagram, Messenger

    usercr NVARCHAR(100),
    userup NVARCHAR(100),
    fechacr DATETIME DEFAULT GETDATE(),
    fechaup DATETIME NULL,
    active BIT DEFAULT 1
);