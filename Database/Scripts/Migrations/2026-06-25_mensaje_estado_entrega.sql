/* ============================================================================
   Migración 2026-06-25 — "Visto" de mensajes (estado de entrega)
   Agrega Mensaje.EstadoEntrega (sent / delivered / read / failed).
   Se actualiza con los webhooks de estado de WhatsApp (value.statuses).
   Idempotente. Aplica sobre GramVetCRM_DEV ya creada.
   ============================================================================ */

USE GramVetCRM_DEV;
GO

IF COL_LENGTH('Mensaje', 'EstadoEntrega') IS NULL
    ALTER TABLE Mensaje ADD EstadoEntrega NVARCHAR(20) NULL;
GO
