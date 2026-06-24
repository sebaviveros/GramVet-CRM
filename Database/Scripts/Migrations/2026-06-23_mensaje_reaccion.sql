/* ============================================================================
   Migración 2026-06-23 — Reacciones de mensajes
   Agrega la columna Reaccion a Mensaje (emoji de reacción).
   Idempotente. Aplica sobre GramVetCRM_DEV ya creada.
   ============================================================================ */

USE GramVetCRM_DEV;
GO

IF COL_LENGTH('Mensaje', 'Reaccion') IS NULL
    ALTER TABLE Mensaje ADD Reaccion NVARCHAR(20) NULL;
GO
