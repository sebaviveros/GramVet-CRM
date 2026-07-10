/* =============================================================================
   Usuario administrador inicial.

   Sin esta fila no se puede entrar al CRM, y como el login exige un usuario
   válido, tampoco se podría crear el primero desde la app.

   `PasswordHash` es un hash BCrypt: `AuthService` valida con `BCrypt.Verify`
   (ver `PasswordHelper`). No sirve poner la contraseña en texto plano.

   RolId = 1 → Admin (los roles se siembran en 02_create_table_Rol.sql).

   ⚠️ EN PRODUCCIÓN: este hash está versionado en git, así que la contraseña se
      considera conocida. Apenas entres por primera vez, cambiala desde
      "Mi cuenta → Cambiar contraseña". A partir de ahí el hash de este script
      deja de servir.

   Correr después de los scripts de estructura (01 al 16).
   ============================================================================= */

INSERT INTO Usuario (
    Nombre,
    Apellido,
    Email,
    Username,
    PasswordHash,
    RolId,
    usercr,
    userup,
    fechacr,
    fechaup,
    active
)
VALUES (
    'Admin',
    'Sistema',
    'admin@gramvet.cl',
    'AdminSys',
    '$2a$11$nQQ9fbXwxNenC2Fnb6Kb5uD9.5EemIBsEPcl0X.CbsVvclT6RitIi',
    1,
    'system',
    NULL,
    GETDATE(),
    NULL,
    1
);
GO
