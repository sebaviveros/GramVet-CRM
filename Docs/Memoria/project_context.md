---
name: project-context
description: "Qué es GramVet CRM, stack, configuración y restricciones críticas"
metadata: 
  node_type: memory
  type: project
  originSessionId: cc531dcf-7d56-497f-bed8-d0db644bf307
---

CRM veterinario tipo respond.io (~60-70% completado) para una clínica veterinaria cliente.

**Restricción crítica: nunca interrumpir el WhatsApp Business real del cliente. Pruebas siempre con número de prueba (+56 9 2172 4181), nunca tocar configuración productiva.**

## Stack
- **Frontend:** Angular 21, CoreUI Angular, standalone components, Angular Signals, SCSS
- **Backend:** ASP.NET Core .NET 8, C#, Entity Framework Core, SQL Server, capas: Api → Service → Repository → Model
- **Real-time:** SignalR vía `ChatHub` (en `GramVetCRM.Service/Hubs`)
- **Mensajería:** WhatsApp Cloud API (Meta)
- **Media storage:** Cloudflare R2 (S3-compatible), bucket `gramvetcrm-media`, URL pública `https://pub-fa94e83d8318492d87b9d41e72b93de0.r2.dev`
- **Dev tunneling:** ngrok en `E:\Workspace\Ngrok\ngrok-v3-stable-windows-amd64`

## URLs
- Backend: `https://localhost:7101`
- Frontend: `http://localhost:4200` (`ng serve`)
- ngrok: `.\ngrok.exe http https://localhost:7101 --host-header="localhost:7101"` (URL pública cambia al reiniciar — actualizar en Meta for Developers)

## Meta / WhatsApp
- App: VetCRM (ID: <META_APP_ID>) — "GramVetCRM" rechazado por Meta por trademark "Gram"
- Número de prueba: +56 9 2172 4181, PhoneNumberId: <WHATSAPP_PHONE_NUMBER_ID>
- System User: `gramvet-crm` (token permanente)
- Webhook verify token: `<VERIFY_TOKEN_WHATSAPP — ver appsettings>`
- Webhook suscrito a evento `messages`

## Cloudflare R2
- AccountId: <R2_ACCOUNT_ID — ver appsettings>

## Patrones establecidos (respetar siempre)
- Capas: `Api (Controller) → Service → Repository → Model`, interfaces para Repository y Service, registradas en `Program.cs` como Scoped
- Soft delete: marcar `Active = false` y actualizar `Fechaup`/`Userup` (nunca eliminar filas)
- Antes de tocar cualquier archivo, leerlo completo primero — nunca asumir contenido

## Estado al 2026-06-22
| Área | Estado |
|---|---|
| Arquitectura y base | 100% |
| Autenticación (login, JWT, interceptor, guard, logout) | 100% |
| Recepción mensajes WhatsApp (texto, imagen, audio, video) | ~95% |
| Envío texto e imagen desde CRM | 100% |
| SignalR (outbound + dirigido por grupos según rol) | 100% |
| Scroll infinito + scroll-to-bottom | 100% |
| CRUD Etiquetas (gestión + panel contacto + header chat) | 100% |
| Respuestas rápidas (CRUD + trigger `/` en chat) | 100% |
| Panel de contacto completo (editar contacto + CRUD mascotas) | 100% |
| Gestión de usuarios (CRUD + roles + email pass + cambiar/reset pass + gating admin) | 100% |
| Permisos granulares por rol (asignación vet + filtrado buzón + real-time dirigido) | 100% |
| Filtros del buzón (búsqueda, estado, etiqueta, no leídos, sin asignar, por vet) | 100% |
| Validación de email único en edición de usuario | 100% |
| Iconos de especie en lista de mascotas (gato/perro/genérico) | 100% |
| Loader en login + guard anti-duplicado en envío de mensajes | 100% |
| Gestión manual de estado de conversación (cerrar/reabrir/pendiente) | DESCARTADO por el cliente (2026-06-23): se organiza con etiquetas. Filtro por estado REMOVIDO de la UI del buzón (campo `estado` sigue en DTO/backend por si se reactiva). |
| Dashboard | Explícitamente fuera de alcance — no desarrollar |

## Roles en DB (tabla Rol)
- RolId 1 = **Admin** (acceso total; único que ve/entra al panel de usuarios)
- RolId 2 = **Secretario**
- RolId 3 = **Veterinario**
- Detección admin en frontend: `rolNombre.toLowerCase().includes('admin')` (funciona porque el rol se llama "Admin")

## Email / SMTP (CONFIGURADO Y FUNCIONANDO — 2026-06-22)
- Servicio de email SMTP construido (`EmailService`, System.Net.Mail). Config en `appsettings.json` sección `Email`.
- **Correo remitente:** `gramvetadministracion@gmail.com` (cuenta del cliente; reemplazó al de Gonzalo). App Password de Gmail de 16 caracteres configurada (requirió activar 2FA en la cuenta).
- Host `smtp.gmail.com`, Port 587, EnableSsl. Envío real confirmado funcionando por Sebastian.
- Si no hay credenciales SMTP, el `EmailService` hace fallback a consola (`[EMAIL SIMULADO]`).
- ⚠️ `appsettings.json` tiene credenciales sensibles (Gmail App Password, WhatsApp token, R2 keys, JWT) y está trackeado en git. Sebastian se encarga manualmente de NO subirlo. Pendiente futuro opcional: mover secretos a appsettings.Development.json + .gitignore.

## Permisos granulares por rol (IMPLEMENTADO — sesión 2026-06-22)
- Asignación de veterinario a conversación cableada a DB real (`Conversacion.UsuarioAsignadoId`). El dropdown del contact-panel usa `GET /api/Usuario/veterinarios` (usuarios con RolId 3). Son los MISMOS usuarios creados en el panel de Usuarios con rol Veterinario — ligados por `Usuario.Id` de punta a punta.
- `GET /api/Conversacion` filtra por rol leído del JWT: Veterinario solo ve sus asignadas; Admin/Secretario ven todas.
- `PUT /api/Conversacion/{id}/asignar` — solo Admin/Secretario (Forbid si no).
- SignalR dirigido por grupos: `ChatHub` `[Authorize]`, veterinario → grupo `user-{id}`, admin/secretario → grupo `staff`. Emisiones van a staff + user-{asignado}. JWT pasa por query string (`?access_token=`) configurado en Program.cs (`OnMessageReceived`). Frontend usa `accessTokenFactory`.
- Reasignación en tiempo real: evento `ConversacionDesasignada` al vet anterior → `removeConversacion(id)` en el state lo quita de su lista al instante.

## Filtros del buzón (IMPLEMENTADO — sesión 2026-06-22)
- `ConversacionDto` ahora incluye `Etiquetas` (lista). `ConversacionService.GetAll` las carga en lote vía `IEtiquetaRepository.GetByContactos` (sin N+1).
- UI: botón embudo en el header de `conversations.component` despliega panel de filtros. Filtrado 100% client-side (computed `conversacionesFiltradas`).
- Filtros para todos: búsqueda (nombre/teléfono), estado (Abierta/Pendiente/Cerrada), etiqueta, no leídos.
- Filtros solo admin/secretario (oculto a vet): por veterinario, sin asignar. Gating con `esStaff` (rol incluye admin o secretario).
- Safeguard en `upsertConversacion`: preserva etiquetas existentes si el evento SignalR llega sin ellas (las emisiones real-time no incluyen etiquetas).

## Detalles de UX y validaciones (sesión 2026-06-22)
- **Unicidad de usuarios:** `Crear` valida username + email; `Editar` valida que el email no sea de OTRO usuario (`existeEmail.Id != id`). Validación solo entre activos (soft delete permite reutilizar username/email de usuarios eliminados — decisión confirmada por Sebastian).
- **Confirmaciones:** eliminar usuario y restablecer contraseña tienen modal SweetAlert de confirmación (no son inmediatos).
- **Datos editables de usuario:** Nombre, Apellido, Email, Rol. Username NO editable (read-only). Contraseña por flujos separados (reset admin / cambiar propia).
- **Iconos de especie** en `contact-panel` lista de mascotas: helper `especieTipo()` (gat→gato, perr/can→perro, otro→huella), SVGs inline.
- **Login:** Swal de carga ("Ingresando...") inmediato al submit, se reemplaza por éxito/error. Evita sensación de congelado.
- **Anti-duplicado de mensajes:** signal `enviando` en `chat-window`; botón deshabilitado + "Enviando..." mientras dura; `sendMessage` ignora repeticiones (`if (enviando()) return`); se libera en next/error (texto e imagen).

## Próximos pasos (pendientes)
1. ✅ ~~Configurar App Password de Gmail~~ — HECHO (2026-06-22, envío real funcionando con gramvetadministracion@gmail.com).
2. ✅ ~~Gestión manual de estado de conversación~~ — DESCARTADO por el cliente (usa etiquetas). Filtro por estado removido de la UI (2026-06-23).
3. **Integración Instagram** — siguiente en la fila (backend ya lo soporta, falta webhook IG + prueba).

## Avance sesión 2026-06-23 (parte 2) — features con BD COMPLETADAS
Las 4 features que requerían BD quedaron implementadas end-to-end (backend C# + frontend Angular). Migración `Database/Scripts/Migrations/2026-06-23_features_mascota_contacto_visibilidad.sql` ya corrida por Sebastian en `GramVetCRM_DEV`. Scripts canónicos 13–16 creados para instalación de prod desde cero.
- **Feature 2 (dirección cliente):** `Contacto.Direccion` + `ReferenciaDireccion`. Editable en contact-panel (rótulos + inputs).
- **Feature 7 (bitácora mascota):** tabla `MascotaBitacora` (1 fila = 1 anotación fechada). Endpoints en `MascotaController` (`{id}/bitacora`). El service resuelve el nombre del autor desde `usercr`. UI: botón libro por mascota → panel con textarea + lista fechada.
- **Feature 6 (fotos mascota):** tabla `MascotaFoto` (URL R2). `MascotaFotoService` sube a R2 y al borrar elimina el objeto (deriva key de la URL). UI: botón imagen → grid de miniaturas + lightbox.
- **Feature 3 (visibilidad por vet):** tablas `EtiquetaUsuario` / `RespuestaRapidaUsuario`. Estricto: admin/secretario ven todo; veterinario ve solo lo vinculado. `GetAll(int? veterinarioId)` filtra por rol (claim `rolNombre`). DTOs llevan `VeterinarioIds`. Gestión (tags/macros) con multiselect de checkboxes de veterinarios en crear/editar.
- **Bug de auditoría arreglado:** el modelo `Usuario` NO tenía las propiedades `Usercr/Userup/Fechacr/Fechaup` (la tabla sí las columnas), por eso EF nunca escribía `usercr` → quedaba NULL. Agregadas al modelo + `UsuarioService` ahora registra el admin actor en crear/editar/eliminar/reset/cambiar pass. Usuarios viejos (filas 2–6) quedaron con usercr NULL (no se backfilleó, Sebastian dijo que no era necesario).

### Geolocalización (sesión 2026-06-23 parte 3) — IMPLEMENTADA
- **Sin cambio de BD:** ubicación = `Mensaje` con `TipoMensaje="location"`, `MediaUrl` = URL Google Maps (`?q=lat,lng`), `Contenido` = nombre/dirección opcional.
- **Recepción:** WhatsApp (`type:"location"`) y Messenger/IG (attachment `location` con `coordinates.lat/long`). Helpers `WhatsAppService.MapsUrl` + `ResumenMensaje` (también mejoran el "último mensaje": 📍/📷/🎵/🎬).
- **Envío:** botón "Enviar mi ubicación" en el footer del chat usa `navigator.geolocation` → WhatsApp envía ubicación NATIVA; Messenger/IG van como link de Maps en texto (no tienen ubicación nativa al enviar).
- ⚠️ Coordenadas con `InvariantCulture` (es-CL usaría coma decimal y rompería la URL).
- ⚠️ Geolocation API requiere contexto seguro (HTTPS o localhost). En dev funciona en localhost; en prod debe ser HTTPS.
- ⚠️ Messenger deprecó el location sharing (~2019) e Instagram no soporta ubicación — el código lo maneja si llega, pero WhatsApp es el canal real para esto.

### Emojis y Reacciones (sesión 2026-06-23 parte 4) — IMPLEMENTADAS
- **Emojis (composer):** botón 😊 en el footer abre panel de emojis que se insertan en el input. Frontend puro, sirve en los 3 canales (texto Unicode).
- **Reacciones:** ⚠️ requiere correr migración `Database/Scripts/Migrations/2026-06-23_mensaje_reaccion.sql` (agrega `Mensaje.Reaccion NVARCHAR(20)`). Canónico `06_create_table_Mensaje.sql` ya actualizado.
  - **Recibir:** webhook WhatsApp `type:"reaction"` → `WhatsAppService.ProcesarReaccion` ubica el mensaje por `ExternalId` y guarda el emoji; emite SignalR `MensajeReaccionado`.
  - **Enviar:** botón 🙂 al hacer hover sobre un mensaje (solo WhatsApp) → picker rápido (👍❤️😂😮😢🙏) → `POST /api/Conversacion/mensaje/{id}/reaccion`. `ConversacionService.ReaccionarMensaje` → `WhatsAppService.EnviarReaccion` (type reaction). Si el canal es Messenger/IG, lanza error (no soportado).
  - **Captura de ExternalId saliente:** los métodos de envío de WhatsApp (`EnviarMensajeTexto/Imagen/Ubicacion`) y `MetaMessagingService.EnviarMensaje` ahora devuelven el wamid/message_id (antes bool); `ConversacionService.EnviarMensaje` lo guarda en `Mensaje.ExternalId` para poder mapear reacciones a mensajes salientes.
  - `MensajeDto`/`Message` (front) llevan `Reaccion`; SignalR `MensajeReaccionado` actualiza el state (`updateReaccion`).
  - **Limitación:** Messenger/IG no permiten ENVIAR reacciones por API de páginas (recibirlas requeriría suscribir `message_reactions`, no hecho). WhatsApp es el canal real.

### Quick wins frontend completados (sesión 2026-06-23 parte 1)
Filtro por estado removido del buzón; "Filtros:" en el header; rótulos en contact-panel; ícono de adjuntos SVG; modal confirmación al asignar/desasignar vet; indicador de vet asignado (badge inferior derecha del chat).

## ⏩ PUNTO ACTUAL (2026-07-09) — Feature IA COMPLETA + tanda visual — ver `Docs/TRASPASO_2026-07-09.md`
**Traspaso más reciente y completo: `Docs/TRASPASO_2026-07-09.md`** (leerlo primero; lo de abajo es historial).

**Estado 2026-07-09:** La feature "Agendar cita (IA)" (Fase 1 + Fase 2) está **terminada y funcionando con datos reales**: Anthropic (`claude-haiku-4-5`) y Google Calendar CONFIGURADOS y andando. Google apunta al **calendario de PRUEBA** `gramvetmovil3@gmail.com` (Service Account `gramvet-agenda@gramvet-crm-calendar-test.iam.gserviceaccount.com`, JSON key en `Backend/GramVetCRM.Api/gramvet-crm-calendar-test-b49a9cfabeab.json`, ya gitignored). Falta pasarlo al calendario real cuando terminen las pruebas (cambiar `CalendarId` + compartir con el SA). ⚠️ La API key de Anthropic se expuso en el chat → recomendado rotarla.
- **Además de Fase 1/2 IA:** dedup de mascotas, título/descripción en vivo (computed), "Pacientes" auto desde la lista de mascotas, badges "se creará"/"ya registrada" + aviso de nombre parecido (Levenshtein), autocompletado de campos VACÍOS del perfil del cliente al confirmar (con badge "se guardará en el perfil").
> **Leer primero:** `Docs/TODO_2026-07-09.md` (pendientes vivos) y `Docs/TRASPASO_2026-07-09.md` (contexto).

## ⏩ Sesión 2026-07-09 parte 3 — TODO grande ejecutado (ver `Docs/TODO_2026-07-09.md`)
Tras probar con una conversación real, Sebastian levantó ~24 pendientes. Se cerraron 21. **El TODO vivo es `Docs/TODO_2026-07-09.md`** (tiene causa raíz de cada bug y las decisiones cerradas).

**Feature IA refinada (B1–B10, COMPLETA):** el prompt separa servicio (`clienteSolicito`) de nombre del cliente; `observaciones` queda vacío si no hay nota especial (hay que darle permiso EXPLÍCITO al modelo o rellena cualquier cosa); el transcript arranca con `HOY ES: <día> <fecha>` para que la IA resuelva "mañana" → campo `fechaSugerida` precarga el date picker; `edadAnios` por mascota → se guarda `FechaNacimiento = hoy − N años`; mascota sin nombre se guarda como `"Sin nombre"` (`Mascota.Nombre` es NOT NULL); estacionamiento pasó de checkbox a campo de texto `IndicacionesEstacionamiento`; `Se cobró` → `Desglose de lo cobrado`; descripción con línea en blanco entre campos; título editable a mano (`tituloManual` pisa a `tituloAuto`); cita para hoy/mañana → sin `colorId` (verde por defecto del calendario), de pasado mañana en adelante → `colorId = "7"` (Peacock/celeste). `Seguro de mascota: Sí` e `Indicaciones de estacionamiento` se agregan a la descripción **solo si tienen valor**.
- ⚠️ **El formato de título/descripción está DUPLICADO** en `AgendaIaService.ArmarTituloYDescripcion` (backend) y `contact-panel.descripcionPreview` (frontend). **Lo que se envía al calendario es el del frontend.** Tocar los dos.

**Permisos:** el veterinario puede DESASIGNARSE a sí mismo (`AsignarUsuario(..., bool esStaff)`; la validación va en el SERVICE, no en el controller, porque hay que leer quién está asignado). El botón "Agendar cita (IA)" ya estaba bien gateado a staff — no era bug.

**Bugs con causa raíz (no re-diagnosticar):**
- `<select [value]>` + `<option>` de un `@for` async → el navegador descarta el value. Solución: `[selected]` por option. (Pasó con el veterinario asignado.)
- Un `<input>` NO admite saltos de línea: los descarta al pegar. Las respuestas rápidas necesitan `<textarea>` + `white-space: pre-wrap` al renderizar.
- Medir `scrollHeight` justo después de `signal.set()` lee el valor VIEJO (el binding `[value]` se aplica en el próximo ciclo de CD). Usar `setTimeout`, no `queueMicrotask`.
- **Esquinas "en punta" de los paneles del buzón:** los `:host` de `app-conversations`/`app-chat-window` tenían `overflow: hidden`; el card ocupa el 100% del host y su `box-shadow` se dibuja FUERA → se la comían. Sin sombra la esquina redondeada queda cortada contra el fondo. El panel de contacto era el único sin `overflow` en su host.

**Pendiente de este TODO:** etiquetas con color oscuro ilegibles (G1); fotos de mascota → bitácora (C4, toca BD: una anotación lleva VARIAS imágenes, `MascotaFoto` se reapunta a la anotación; las fotos actuales se descartan); respaldo diario de la BD (F1, solo producción). **Descartado:** editar/eliminar mensajes (la Cloud API de WhatsApp no lo permite).

---

**Sesión 2026-07-09 parte 2 (ya commiteada):** rediseño gris `#2B2B2B` + verde acento en los 3 paneles del buzón; separadores de fecha Hoy/Ayer/día/fecha en el chat; foto del vet en el chip de asignado; burbuja morada nombre/rol en el header; **captcha Cloudflare Turnstile en el login** (site key `<TURNSTILE_SITE_KEY — ver environment.ts>`, secret en appsettings, modo Managed, hostname `localhost`); **environments centralizados** en `src/environments/` (ya no hay URLs hardcodeadas); **sesión persistente para vets en celular**: token a `localStorage`, `exp` validado en el guard, `unauthorized.interceptor.ts` (401 → logout), `ExpireMinutes` 480→600. Deuda: refresh token. Detalle en `Docs/TRASPASO_2026-07-09.md` sección 2.J.

- **Tanda visual (ya pusheada en `13435d8`):** fotos de usuarios (opcional al crear + editable en la lista, endpoint `POST /api/Usuario/{id}/foto`); scroll en listas de gestión; hover de filas morado sólido `#55456F` + texto blanco (Usuarios/Tags/Macros); colores del chat (burbuja mía `#608439`, del contacto `#EAEAEA`, fondo `#2B2B2B`); menú lateral activo morado `#55456F`; se quitó la columna de filtros del buzón (`app-inbox-sidebar`, NO el ítem del menú); empty-state del chat con wallpaper `assets/images/vet_chat_wallpaper_wide.png` + "Seleccione una conversación" (oculta header y composer sin conversación). Último commit pusheado: `cd2f1dd`.

---
### Historial (2026-06-29)
Documento de traspaso previo en `Docs/TRASPASO_2026-06-28.md`. **Fase 1 de la automatización IA implementada (2026-06-29):** extracción de cita con Claude + panel inline editable.

**Lo construido (Fase 1):**
- DTO `CitaExtraidaDto` (`GramVetCRM.Model/DTOs/Cita/`).
- `IAgendaIaService`/`AgendaIaService` (`GramVetCRM.Service/AgendaIaService/`): carga conversación+contacto+mascotas+últimos 40 msjs de texto, llama `POST https://api.anthropic.com/v1/messages` por **HttpClient** ("Anthropic", registrado en Program.cs) con structured outputs (`output_config.format`, JSON schema, todos los campos required + additionalProperties:false). Arma `tituloEvento`/`descripcionEvento` en el formato exacto del cliente. **Fallback simulado** (`[IA SIMULADA]` + datos del contacto) si `Anthropic:ApiKey` vacío, y también ante cualquier excepción de la API. Modelo `claude-haiku-4-5` configurable. SIN `thinking`/`effort` (Haiku no los soporta).
- Endpoint `POST /api/Conversacion/{id}/extraer-cita`, **gateado a admin/secretario** (Forbid + front).
- Front: `extraerCita(id)` + interfaz `CitaExtraidaDto` en `conversacion.service.ts`; botón "Agendar cita (IA)" + panel inline editable en `contact-panel` (debajo del nombre, solo staff vía `puedeAgendarIa`). Estilos `.agenda-ia-section`/`.cita-*` en el scss.
- **SIN cambios de BD.** Compila (Service 0 errores; front buildea).

**Pendiente del usuario:** agregar a `appsettings.json` (NO commitear) sección `"Anthropic": { "ApiKey": "", "Model": "claude-haiku-4-5" }`. Con ApiKey vacío usa el fallback simulado; al pegar la key real funciona la IA sin más cambios. La key todavía no estaba pegada al cierre de esta sesión.

**Fase 2 (IMPLEMENTADA 2026-06-29 — falta solo configurar Google):** crear el evento en el Google Calendar madre. La lógica de posicionamiento quedó confirmada por el cliente y está en [[calendar-logic]].
- Backend: `AgendaSlots` (catálogo + algoritmo de posición móvil/slot, validación slot i=9 solo móvil 1 + miér-sáb), `GoogleCalendarService` (NuGet `Google.Apis.Calendar.v3`, Service Account, fallback simulado), `AgendaIaService.CrearCita` (crea mascotas del cliente + posiciona + evento), endpoint `POST /api/Conversacion/{id}/crear-cita` (staff). Extracción IA ampliada (mascotas/GPS/seguro/estacionamiento).
- Frontend: panel de cita con GPS/seguro/estacionamiento, mascotas editables, selector Móvil 1/2, día (date picker), horario (dropdown lista fija) y botón "Crear cita". Todo en oscuro.
- **Pendiente del usuario:** sección `"GoogleCalendar": { "ServiceAccountKeyPath", "CalendarId", "TimeZone": "America/Santiago" }` en appsettings (NO commitear) + crear Service Account en Google Cloud, habilitar Calendar API, compartir el calendario de PRUEBA (vacío, ya lo tiene) con el email del SA (permiso "hacer cambios en eventos"). Mientras tanto funciona en modo SIMULADO. ⚠️ Al agregar el NuGet hay que parar la app en VS y reconstruir.
- Decisión confirmada: crear eventos NUEVOS en la posición codificada (no rellenar placeholders); el cliente abandona los placeholders manuales. Un solo calendario madre; el secretario reparte a los vets.

### Tema oscuro del buzón (HECHO 2026-06-29)
El buzón pasó a tema oscuro GramVet (paneles verde profundo `#0F332E`, gap `#0B2B26`, chat plano `#0A2420`, acento `#6ECFAB`): inbox card (`_custom.scss`), conversations, chat-window (incl. fondo plano del chat + burbujas recibidas), contact-panel. Los módulos de gestión (Usuarios/Tags/Macros) quedan CLAROS a propósito. Header: ocultos los íconos de notificaciones/filtros/mensajería + toggle de tema (comentados); `colorMode` fijado en 'light' en `app.component.ts` para que gestión sea legible (la oscuridad viene del shell + buzón). **Grupo A (estandarización visual Usuarios/Tags/Macros) HECHO 2026-06-30:** header de Usuarios sin la banda gris `--cui-card-cap-bg` (igual a tags/macros); inputs `.field-input`/`.search-wrap` de Usuarios en blanco `rgba(255,255,255,0.85)` sin borde radius 12 y focus ring verde; `.table-container` de Usuarios con fondo blanco radius 14 y hover de fila `rgba(35,83,71,0.04)`; botones de crear de tags/macros reemplazados (`btn btn-primary` → `.btn-create` con ícono + SVG stroke #6ECFAB, bg #1A4A3C, texto blanco — igual a "Crear usuario").

## TODO DEFINITIVO (consolidado 2026-06-25) — lo que FALTA
Del TODO viejo ya está casi todo hecho; solo quedan los 2 ítems de IA (autocompletar datos del cliente + botón IA bajo el nombre), que son parte de la feature de Calendar.

**Bugs / ajustes rápidos (de TODO2):**
1. ✅ HECHO — Stickers: WhatsApp recibe/renderiza stickers (descarga `.webp`, render sin burbuja `.sticker-msg`, ~120px; resumen "Sticker").
2. ✅ HECHO — Panel de emojis: no se cierra al elegir; se cierra con click afuera (`@HostListener('document:click')` + stopPropagation en botón y panel).
3. ✅ HECHO — Conversación seleccionada + no leídos: `upsertConversacion` fuerza no leídos=0 si es la abierta; SignalR marca leído al llegar mensaje a la activa; CSS `.unread:not(.selected)`.
4. Geolocalización imprecisa → NO es bug de código: en desktop la Geolocation API usa WiFi/IP (imprecisa); en celular usa GPS (precisa). Probar desde celular. `enableHighAccuracy` ya está.

**"Visto" de mensajes (HECHO 2026-06-25):** ✓ enviado / ✓✓ entregado / ✓✓ azul leído en mensajes SALIENTES de WhatsApp. Columna `Mensaje.EstadoEntrega` (migración `2026-06-25_mensaje_estado_entrega.sql` + canónico 06). Backend: `WhatsAppService.ProcesarEstados` captura `value.statuses` (sent/delivered/read/failed), ubica por `ExternalId`, solo avanza el estado, emite SignalR `MensajeEstado`. `EnviarMensaje` setea "sent" inicial (solo WhatsApp, no Meta). Frontend: `tickSymbol()` + `updateEstadoMensaje` en state + render en texto/imagen/ubicación. Sebastian eligió SOLO "ver si el cliente leyó mis mensajes" (no marcar como leído los del cliente). Messenger/IG no tienen este flujo.

**Features medianas:**
5. ✅ HECHO — Foto de perfil de usuarios: columna `Usuario.FotoUrl` (migración `2026-06-25_usuario_foto.sql` + canónico 01). Backend: `POST /api/Usuario/foto` (sube a R2, borra anterior), `GET /api/Usuario/me`. Frontend: header muestra la foto y "Cambiar foto de perfil" en menú Mi cuenta; lista de Usuarios muestra avatar con foto. Cada usuario edita la SUYA.
6. Integración Instagram → backend ya lo soporta; falta suscribir webhook IG en Meta + probar.

**Estandarización de estilos (HECHO 2026-06-25):** etiquetas (tags) y respuestas rápidas (macros) alineadas al estándar del módulo de Usuarios: header padding 18/24, ícono 36px radius 10 bg #1A4A3C stroke #6ECFAB 18px, título 15px, subtítulo 12px #6c757d, badge pill claro (#E1F5EE/#085041) DM Mono, section-label 11px #6c757d, host DM Sans. Fuentes ya globales en `_custom.scss`.

**Feature grande:**
7. Botón "Agendar cita" con IA → Google Calendar. Incluye autocompletar datos del cliente (TODO viejo #8) y botón IA bajo el nombre (TODO viejo #11). Bloqueada hasta que el dueño explique cómo organiza su calendario.

**Deuda técnica (antes de prod):** validación de rol en backend; reacciones Messenger/IG; secretos de appsettings fuera de git; limpiar rutas demo del template.

## TODO de la reunión con el cliente (2026-06-23) — pendientes confirmados
1. **Geolocalización:** las clientes envían ubicación para indicar dónde viven; los veterinarios envían ubicación para avisar dónde están. Soportar mensajes de location (recepción y envío) en el chat.
2. **Dirección + referencias de dirección** como campos del cliente en el panel de contacto (editables manualmente).
3. **Visibilidad de etiquetas y respuestas rápidas por veterinario:** SOLO el admin crea etiquetas/respuestas rápidas y decide qué veterinarios pueden verlas (dropdown multiselect de veterinarios, uno por cada item). Admin y secretario las ven TODAS por defecto; el veterinario ve solo las que el admin le asignó. → requiere modelar relación etiqueta/respuesta-rápida ↔ veterinarios en BD.
4. **Indicador de asignado en el chat:** foto/ícono en la esquina inferior derecha del chat mostrando qué veterinario está atendiendo, para que el admin lo vea de un vistazo.
5. **Modal de confirmación** al asignar/desasignar un veterinario.
6. **Fotos en mascotas:** poder adjuntar fotos a cada mascota (ej. descargar una receta enviada en el chat y guardarla en la mascota X) y visualizarlas. → modificar BD.
7. **Bitácora de mascota:** nuevo campo tipo texto grande con scroll, con secciones fechadas, para que los usuarios del CRM registren atenciones/tips/historial. → modificar BD.
8. **Nombres de campo en el panel de contacto:** mostrar etiquetas "Nombre:", "Email:", etc. (hoy solo el Canal tiene rótulo).
9. Los datos personales del cliente se editan manual Y se autocompletan vía el botón IA de llenar formulario.
10. **Cambiar ícono del botón de adjuntos** del chat por uno más serio/empresarial.
11. Anteponer texto **"Filtros:"** al ícono de filtros en el panel de conversaciones.
12. Ubicar el **botón para iniciar el flujo IA del formulario** justo debajo del nombre del cliente en el panel de contacto.

### CANALES Instagram y Facebook Messenger (Messenger FUNCIONANDO en dev — 2026-06-23)
El CRM es multicanal tipo respond.io. **WhatsApp** y ahora **Messenger** funcionan. Falta **Instagram**.

**MESSENGER — IMPLEMENTADO Y PROBADO (modo Desarrollo):**
- Backend: `MetaMessagingService` (recibe webhooks Messenger/IG, crea contacto+conversación+mensaje, SignalR por grupos, envío vía Graph API v21.0 `/me/messages`), `MetaController` (`/api/Meta/webhook`, verify token `<VERIFY_TOKEN_META — ver appsettings>`), `CanalRepository.GetOrCreate` (siembra canales). `ConversacionService.EnviarMensaje` enruta por canal (WhatsApp intacto).
- Contactos Messenger/IG: se identifican por `Contacto.Telefono` con prefijo `FB:`/`IG:` (sin cambios de esquema).
- Frontend: badge de canal sobre el avatar (`channel-badge.component`) en lista y header — WhatsApp verde, IG rosa, Messenger azul. Buzón unificado multicanal.
- Config Meta en `appsettings.json` sección `Meta`: PageId `101078882762015`, VerifyToken `<VERIFY_TOKEN_META — ver appsettings>`, PageAccessToken (configurado, ~201 chars).
- App de Meta VetCRM: agregados casos de uso Messenger + Instagram; webhook suscrito a `messages`+`messaging_postbacks`; cuenta personal de Sebastian (Bufonsillo) agregada como Evaluador para probar en dev.
- **Limitación conocida (dev mode):** la Graph API devuelve **400** al pedir el nombre del perfil (first_name/last_name) de cuentas tester → el contacto se guarda con el PSID como nombre. El código de `ObtenerPerfil` es correcto; **en producción (Live + App Review) el nombre real llegará**. Workaround: editar el nombre a mano en el CRM.
- ⚠️ **ngrok free cambia de URL cada vez que se reinicia** → hay que actualizar la callback URL en AMBOS webhooks de Meta (WhatsApp `/api/WhatsApp/webhook` y Meta `/api/Meta/webhook`). Recomendación: usar dominio estático gratuito de ngrok.

**INSTAGRAM — PENDIENTE:** el backend ya lo soporta (mismo `MetaMessagingService`, object "instagram"). Falta suscribir el webhook de IG en Meta + probar. Permisos `instagram_basic`/`instagram_manage_messages` ya agregados.

**Para producción (clientes reales en los 2 canales):** la app debe pasar a **Live** + **App Review** de Meta (`pages_messaging`, `instagram_manage_messages`). En dev solo funcionan cuentas con rol en la app.
- Ambos van por la **Meta Graph API** (misma familia que WhatsApp Cloud API), así que el patrón es similar al de WhatsApp ya construido: webhook entrante + envío + multimedia + SignalR + persistencia.
- La DB ya modela multicanal: `Conversacion.CanalId` + tabla `Canal` (WhatsApp es CanalId 1). Habría que crear los canales IG y Messenger y enrutar por canal.
- **Datos del cliente confirmados (2026-06-22):**
  - Página de Facebook: **"GramVet Médico Veterinario a Domicilio V Región"**, **Page ID `101078882762015`** (~7,3 mil seguidores). Messenger YA recibe mensajes reales.
  - Instagram: vinculado a la página (~12,8 mil seguidores, @gramvet.cl). Cuenta profesional conectada.
  - Sebastian tiene acceso a la misma cuenta/app de Meta donde está WhatsApp (app **VetCRM**).
  - Pendiente conseguir: agregar productos Messenger + Instagram en la app VetCRM, generar Page Access Token, confirmar modo (Desarrollo vs Live / App Review) y suscribir webhooks.
- Pendiente al retomar: tokens y permisos de Meta (cada canal requiere su suscripción de webhook y permisos de la app de Meta), y cómo se mapean los contactos de cada plataforma a la tabla `Contacto`.
- El `WhatsAppService` actual es específico de WhatsApp; conviene generalizar el envío/recepción por canal (interfaz de mensajería por canal) al sumar IG y Messenger.
- **UX del buzón (definido por Sebastian):** el buzón principal muestra los 3 canales MEZCLADOS en una sola lista (no separados por pestañas). Cada conversación debe llevar un indicador visual del canal de origen: un ícono/imagen de WhatsApp, Instagram o Messenger en el avatar/portrait de la conversación (probablemente como badge sobre el avatar). Hay que exponer el canal en el `ConversacionDto` (ya viene el nombre del canal) y renderizar el ícono correspondiente en `conversations.component` y en el header del chat.

### FEATURE GRANDE PENDIENTE — Botón "Agendar cita" en el chat → Google Calendar
Funcionalidad importante para terminar el proyecto. Un botón en el chat que crea una cita en el Google Calendar de la veterinaria usando la información de la conversación.
- **Extracción de info de la conversación:** de un chat largo (ej. el secretario lleva una semana hablando con el cliente) hay que sacar datos como nombre de mascotas, especie, dirección, motivo, fecha/hora deseada. Decisión abierta: usar IA (recomendado — Claude API para extraer/estructurar texto no estructurado) vs parsing manual (frágil). Recomendación: IA con paso de confirmación humana antes de crear el evento.
- **Integración Google Calendar:** crear evento en el calendar de la veterinaria (requiere OAuth / service account de Google + definir credenciales).
- **⚠️ Lógica de calendario NO definida todavía:** el dueño usa Google Calendar "de una manera particular" para hacer calzar en el mismo calendario las citas de TODOS los veterinarios. Falta que el dueño explique exactamente cómo lo organiza (¿un solo calendario compartido? ¿cómo distinguen/asignan por veterinario? ¿bloques horarios?). NO desarrollar la lógica de agendado hasta tener esto claro.
- Esta feature tiene 2 partes técnicas: (a) extracción de datos del chat, (b) integración con Google Calendar.

**Formato real del cliente en Google Calendar (visto en capturas 2026-06-23) — esto es el SCHEMA de salida de la IA:**
- **Título del evento:** `[rango horario] [comuna/sector]: [dirección] [+teléfono] [nombre cliente]`. Ej: `1 - 3 viña (reñaca alto): Lago villarrica 599, Block C depto 42 +56959280537 Ailin F...`
- **Descripción del evento** (texto estructurado con estas secciones fijas):
  - `Paciente(s):` (ej. "1 gato")
  - `Cliente solicitó:`
  - `Se cobró:` (líneas de cobro, ej. "$19.000 triple / $6.000 ida / $6.000 inyectables c/u")
  - `Total mínimo a cobrar:` (ej. "$25.000")
  - `Referencias para encontrar el domicilio:` (ej. "Condominio color salmón y blanco, frente a casa blanco con verde")
  - `Observaciones:`
  - `Correo:`
- La IA debe extraer del chat de WhatsApp/Messenger y producir EXACTAMENTE este título + descripción.

**Sobre los slots vacíos pre-creados del cliente (captura 2 — calendario semanal):** el dueño crea de antemano citas VACÍAS con rangos horarios fijos recurrentes (bloques solapados de 2h: "12-2", "1-3", "2-4", "3-5", "4-6", "5-7", "6-7:30"...) todos los días, como plantilla visual de capacidad. Cuando llega una cita real, rellena un bloque a mano. También hay bloques naranjos "instrucciones para agendar día X". El cliente dijo que si hay que cambiar este sistema, no hay problema.
- **Análisis (Claude):** rellenar esos slots vacíos vía API es FRÁGIL (habría que encontrar por tiempo el evento placeholder correcto y hacer PATCH; el matching es poco confiable, y los bloques se solapan). **Recomendación: crear eventos NUEVOS directamente en la fecha/hora confirmada** (la Calendar API crea/actualiza por ID, no "rellena huecos"). Como el cliente está abierto a cambiar su flujo, lo ideal es que deje de crear placeholders manuales una vez que el CRM agende. Lo que SÍ hay que preservar intacto es su formato de título + descripción (es como toda su operación lee las citas).
- Flujo propuesto v1: botón "Agendar cita" en el chat → IA extrae datos → formulario de confirmación editable (campos + selector fecha/hora) → al confirmar, crear evento en el calendar `gramvet` con el formato exacto. Paso de confirmación humana obligatorio antes de crear.
- **Estimación de costo de la IA (extracción), consultada en precios oficiales de Claude API el 2026-06-22):**
  - Se cobra por uso/tokens vía Claude API (requiere API key de Anthropic con saldo; separado del resto del proyecto). Cada clic del botón = 1 llamada pagada.
  - Costo por clic con chat típico (~6k tokens entrada + ~500 salida): Haiku 4.5 ≈ US$0.009 (~9 CLP); Sonnet 4.6 ≈ US$0.027 (~26 CLP); Opus 4.8 ≈ US$0.045 (~43 CLP).
  - Chat muy largo (~20k tokens): Haiku ≈ US$0.02; Sonnet ≈ US$0.068; Opus ≈ US$0.11.
  - **Recomendación: usar Haiku 4.5 o Sonnet 4.6** para la extracción (suficientes y mucho más baratos que Opus). Con Haiku, ~500 citas/mes ≈ US$5/mes (~5.000 CLP).
  - Precios oficiales (USD por 1M tokens, entrada/salida): Haiku 4.5 $1/$5; Sonnet 4.6 $3/$15; Opus 4.8 $5/$25.
  - Control de gasto: recargar saldo + límites de gasto mensual en el panel de Anthropic. Medir costo real con count_tokens sobre chats reales antes de activar.
