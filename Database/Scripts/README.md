# Scripts de base de datos — GramVet CRM

## Instalación desde cero (producción)

Correr **en este orden**:

1. `create_db_dev.sql` — crea la base. Cambiarle el nombre para producción.
2. `01` … `16` — estructura, **en orden numérico**.
3. `Datos/01_insert_into_Canal.sql` — canales de mensajería.
4. `Datos/02_insert_into_Usuario.sql` — usuario administrador inicial.

### Por qué el orden importa

- **`13` antes que `14`.** Desde el 2026-07-09 `MascotaFoto` tiene una FK contra
  `MascotaBitacora` (la foto cuelga de la anotación, no de la mascota). Si se
  corre el `14` primero, falla por clave foránea. Antes de esa fecha los números
  estaban al revés.
- **WhatsApp debe quedar con `Canal.Id = 1`.** `WhatsAppService` crea las
  conversaciones entrantes con `CanalId = 1` hardcodeado. Por eso WhatsApp va
  primero en el script de canales, y ese script no se corre dos veces.
- **Sin el usuario inicial no se puede entrar.** El login exige un usuario
  válido, así que el primero no puede crearse desde la app.

### Después de instalar

Cambiar la contraseña del admin desde "Mi cuenta → Cambiar contraseña": el hash
del script está versionado en git.

## Migraciones

`Migrations/` tiene los cambios incrementales sobre una base ya creada, con
fecha en el nombre. **No** hace falta correrlos en una instalación desde cero:
los scripts canónicos (`01`…`16`) ya los incluyen.

Al cambiar el esquema hay que tocar **los dos**: la migración y el canónico.
