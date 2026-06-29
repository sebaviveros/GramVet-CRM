/* ============================================================================
   Migración 2026-06-25 — Foto de perfil de usuarios
   Agrega la columna FotoUrl a Usuario (URL pública en Cloudflare R2).
   Idempotente. Aplica sobre GramVetCRM_DEV ya creada.
   ============================================================================ */

USE GramVetCRM_DEV;
GO

IF COL_LENGTH('Usuario', 'FotoUrl') IS NULL
    ALTER TABLE Usuario ADD FotoUrl NVARCHAR(500) NULL;
GO
