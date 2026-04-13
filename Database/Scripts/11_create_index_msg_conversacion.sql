CREATE INDEX IX_Mensaje_ConversacionId_FechaEnvio
ON Mensaje (ConversacionId, FechaEnvio DESC);

CREATE INDEX IX_Conversacion_ContactoId
ON Conversacion (ContactoId);