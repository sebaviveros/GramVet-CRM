/* =============================================================================
   Canales de mensajería.

   ⚠️ EL ORDEN IMPORTA: `WhatsAppService` crea las conversaciones entrantes con
      `CanalId = 1` HARDCODEADO. WhatsApp tiene que quedar con Id = 1, así que
      va primero y este script NO debe correrse dos veces.

   Los nombres deben escribirse EXACTAMENTE así: `MetaMessagingService` busca
   los canales con `GetOrCreate("Instagram")` / `GetOrCreate("Messenger")` y,
   si no los encuentra por nombre, crea filas duplicadas.

   Correr después de los scripts de estructura (01 al 16).
   ============================================================================= */

INSERT INTO Canal (Nombre, Usercr, Fechacr, Active) VALUES ('WhatsApp',  'system', GETDATE(), 1);  -- debe quedar con Id = 1
INSERT INTO Canal (Nombre, Usercr, Fechacr, Active) VALUES ('Instagram', 'system', GETDATE(), 1);
INSERT INTO Canal (Nombre, Usercr, Fechacr, Active) VALUES ('Messenger', 'system', GETDATE(), 1);
GO

-- Verificación: WhatsApp tiene que devolver Id = 1
SELECT Id, Nombre FROM Canal ORDER BY Id;
GO
