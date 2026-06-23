# Traspaso GramVet CRM — Documento de continuación (para otra instancia de Claude)

> Lee este documento completo antes de actuar. Con esto puedes retomar el trabajo sin pedirle a Sebastian que repita nada. También existe una memoria en `C:\Users\svive\.claude\projects\E--Workspace-GRAMVET-GramVet-CRM\memory\` (MEMORY.md + archivos) que complementa esto.

---

## 1. Qué es el proyecto y objetivo general

CRM veterinario **multicanal tipo respond.io** (~70-80% hecho) para la clínica **GramVet** (veterinario a domicilio, V Región, Chile). Sebastian Viveros es el project lead (arquitectura + implementación); trabaja con un frontend dev aparte pero él dirige.

**Objetivo central del proyecto:** un buzón único donde se atienden conversaciones de **WhatsApp, Facebook Messenger e Instagram** mezcladas, con etiquetas, respuestas rápidas, gestión de usuarios por rol, panel de contacto con mascotas, y (futuro) agendamiento de citas con IA a Google Calendar.

**Restricción crítica de negocio:** NUNCA interrumpir el WhatsApp Business real del cliente. Las pruebas de WhatsApp van con el número de prueba. (Messenger/IG sí se pueden correr en paralelo con respond.io — ver sección 8.)

---

## 2. Cómo trabaja Sebastian (preferencias — respétalas)

- **NUNCA modificar código ni hacer commits sin autorización explícita.** Primero proponer el plan/código, preguntar "¿lo aplico?", esperar el "sí", y recién entonces editar. Sin excepciones.
- **Leer el archivo completo antes de editarlo** — nunca asumir su contenido.
- Trabaja iterativo: prueba cada feature antes de seguir.
- Al final de cada sesión pide un **mensaje de commit** (conventional commits, y la descripción **poco técnica**, entendible por no programadores).
- Reporta resultados concretos de qué se hizo y por qué.
- Está en Chile (precios/ejemplos en CLP cuando aplica).

---

## 3. Stack y arquitectura

- **Frontend:** Angular 21, CoreUI, standalone components, Angular Signals, SCSS. Usa **SweetAlert2 (`Swal`)** para modales.
- **Backend:** ASP.NET Core .NET 8, C#, EF Core, SQL Server. Capas: **Api (Controller) → Service → Repository → Model**. Interfaces de Repository y Service registradas en `Program.cs` como **Scoped**.
- **Real-time:** SignalR vía `ChatHub` (en `GramVetCRM.Service/Hubs`).
- **Media storage:** Cloudflare R2 (S3-compatible). Bucket `gramvetcrm-media`, URL pública `https://pub-fa94e83d8318492d87b9d41e72b93de0.r2.dev`.
- **Patrón soft delete:** marcar `Active = false` + `Fechaup`/`Userup` en vez de borrar.

### Rutas/comandos para correr
- Backend: `https://localhost:7101` (`dotnet run` o F5 en Visual Studio). Entorno Development.
- Frontend: `http://localhost:4200` (`ng serve`).
- ngrok (solo para webhooks entrantes de Meta): en `E:\Workspace\Ngrok\ngrok-v3-stable-windows-amd64`, comando `.\ngrok.exe http https://localhost:7101 --host-header="localhost:7101"`.
- **El backend suele quedar corriendo durante el trabajo**: al compilar con `dotnet build` da errores MSB3021/MSB3027 de DLL bloqueada (NO son errores de código). Para verificar compilación sin tocar el Api en uso: `dotnet build GramVetCRM.Service\GramVetCRM.Service.csproj`. El `ng serve` recompila solo (los warnings NG8113/NG8102 son preexistentes e inofensivos).

---

## 4. Credenciales y configuración (todo en `appsettings.json`, TRACKEADO en git)

⚠️ **`appsettings.json` está trackeado en git y contiene secretos** (App Password Gmail, token WhatsApp, llaves R2, JWT, token Meta). **Sebastian se encarga manualmente de NO subirlo.** Pendiente futuro opcional: mover secretos a `appsettings.Development.json` + `.gitignore`.

- **JWT:** Key `GramVetCRM_gonzalord_XkP9mN3qR7vL2wJ8`, Issuer `GramVet.Api`, Audience `GramVet.Client`, 480 min.
- **WhatsApp:** VerifyToken `gramvet_gonzalord`, PhoneNumberId `1158710177318339`, número de prueba +56 9 2172 4181. App de Meta: **VetCRM**, App ID `167917039309284`.
- **Email (SMTP, FUNCIONANDO):** sección `Email`: Host `smtp.gmail.com`, Port `587`, User **`gramvetadministracion@gmail.com`**, FromName "GramVet CRM", Password = App Password de Gmail de 16 chars (configurada). La contraseña normal de Gmail NO sirve para SMTP — requiere activar verificación en 2 pasos y generar App Password en myaccount.google.com/apppasswords.
- **Meta (Messenger/Instagram):** sección `Meta`: PageId **`101078882762015`**, VerifyToken `gramvet_meta_verify`, PageAccessToken (configurado, ~201 chars).

### Roles en DB (tabla `Rol`)
- RolId 1 = **Admin** (acceso total; único que ve/entra al panel de usuarios)
- RolId 2 = **Secretario**
- RolId 3 = **Veterinario**
- Detección admin en frontend: `rolNombre.toLowerCase().includes('admin')` (funciona porque el rol se llama "Admin"). El JWT incluye `rolNombre` y `rolId` (`JwtHelper`). Frontend decodifica el JWT en `AuthService` (`getRolNombre`, `isAdmin`, `getUserId`).

---

## 5. Lo IMPLEMENTADO y FUNCIONANDO (esta entrega)

Todo lo siguiente está hecho, compila y fue confirmado por Sebastian salvo donde se indique:

1. **Respuestas rápidas** — CRUD full stack (módulo "Respuestas rápidas" con edición inline). En el chat: al escribir `/` se despliega lista filtrada por comando; click inserta el texto en el input. Comando se guarda sin `/` y en minúsculas.
2. **Panel de contacto completo** — editar nombre/apellido/email del contacto (inline) + **CRUD de mascotas** (botón +, editar inline, eliminar). Iconos de especie junto al nombre (gato/perro/genérico, detección por texto de `especie`). Edad detallada: "1 año 5 meses 15 días", "5 meses 15 días" o "15 días" (omite ceros). Secciones de etiquetas y veterinario asignado ya existían.
3. **Gestión de usuarios** — CRUD + roles dinámicos (dropdown desde `GET /api/Rol`, ligado a tabla `Rol`). Al crear usuario → contraseña aleatoria por correo. Admin puede **restablecer contraseña** (nueva al correo). Usuario cambia su contraseña desde dropdown del header (modal Swal: actual + nueva ×2). Validación de unicidad: `Crear` valida username+email; `Editar` valida que el email no sea de otro usuario. (Unicidad solo entre activos: tras soft-delete se puede reutilizar — decisión confirmada por Sebastian.) Username NO editable.
4. **Permisos por rol:**
   - Panel de usuarios: solo Admin lo ve (item de menú oculto vía `default-layout`) y solo Admin entra (`adminGuard`).
   - Buzón: **Veterinario ve solo sus conversaciones asignadas**; Admin/Secretario ven todas (backend filtra por rol leído del JWT en `GET /api/Conversacion`).
   - Asignación de veterinario cableada a DB real (`Conversacion.UsuarioAsignadoId`). Dropdown del contact-panel usa `GET /api/Usuario/veterinarios` (mismos usuarios creados con rol Veterinario; ligados por `Usuario.Id`). Solo Admin/Secretario ven el selector y pueden asignar (`PUT /api/Conversacion/{id}/asignar`, Forbid si no).
   - SignalR dirigido por grupos: `ChatHub` `[Authorize]`, veterinario → grupo `user-{id}`, admin/secretario → grupo `staff`. Emisiones a staff + user-{asignado}. JWT pasa por query string (`?access_token=`) configurado en `Program.cs` (`OnMessageReceived`); frontend usa `accessTokenFactory`. Reasignación en tiempo real: evento `ConversacionDesasignada` al vet anterior → `removeConversacion(id)`.
5. **Filtros del buzón** (botón embudo en header de conversations). Todos: búsqueda (nombre/teléfono), estado, etiqueta, no leídos. Solo Admin/Secretario: por veterinario, sin asignar. Filtrado client-side. `ConversacionDto` ahora incluye `Etiquetas` (cargadas en lote en `GetAll` vía `IEtiquetaRepository.GetByContactos`).
6. **Mejoras de UX:**
   - **Loader global temático GramVet** (overlay con spinner verde + patita + "GramVet CRM"): `LoaderService` + `loading.interceptor` (muestra solo si la petición tarda >300ms; excluye `/api/Conversacion` para no estorbar el buzón) + `LoaderComponent` en `AppComponent`. Reemplaza el loader del login.
   - **Anti-duplicado de mensajes:** signal `enviando` en chat-window; botón deshabilitado + "Enviando..." mientras dura; ignora repeticiones.
   - **Todas las confirmaciones de eliminar** usan modal SweetAlert (etiquetas, respuestas rápidas, mascotas, usuarios) — ya no hay `confirm()` nativo en el proyecto.
   - Fix: etiquetas eliminadas ya no aparecen en contactos (filtro `ce.Etiqueta.Active` en `GetByContacto`/`GetByContactos`).
7. **Email SMTP real** funcionando (`EmailService`, System.Net.Mail; fallback a consola `[EMAIL SIMULADO]` si no hay credenciales).
8. **Badge de canal** sobre el avatar en lista del buzón y header del chat (`channel-badge.component`): WhatsApp verde, Instagram rosa, Messenger azul. Buzón unificado multicanal.

### Decisiones de diseño tomadas (NO volver a discutir)
- **Gestión (CRUD) separada del punto de uso.** El CRUD de catálogos (etiquetas, respuestas rápidas) vive en su módulo de gestión; en el chat/panel solo se selecciona/asigna lo existente. Confirmado por Sebastian en etiquetas; aplicar igual a features nuevas.
- **Trigger de respuestas rápidas:** `/` + filtro por comando (no botón). Confirmado.
- **Edad de mascotas:** formato granular omitiendo ceros (arriba). Confirmado.
- **Loader:** interceptor automático con umbral 300ms, excluyendo el buzón en tiempo real. Confirmado.
- **Anti-duplicado:** deshabilitar botón mientras el envío está en vuelo (NO delay fijo, que es frágil). Confirmado.
- **Contactos Messenger/IG** se identifican con prefijo en `Contacto.Telefono` (`FB:` / `IG:`) reusando `Mensaje.ExternalId` — **sin cambios de esquema** en tablas existentes (agregar columnas a Contacto rompería EF en el flujo WhatsApp en uso). Decisión deliberada.
- **Dashboard:** explícitamente FUERA de alcance. No desarrollar salvo que Sebastian lo reincorpore.

---

## 6. CANAL MESSENGER — implementado y probado en modo Desarrollo (lo más reciente)

Arquitectura backend (toda aditiva, WhatsApp intacto):
- `MetaMessagingService` (en `GramVetCRM.Service/MetaService/`): recibe webhooks de Messenger e Instagram (mismo formato; `object` = "page" → Messenger, "instagram" → Instagram), crea contacto+conversación+mensaje en el canal correcto, emite por SignalR a los grupos, ignora echoes, y envía vía Graph API v21.0 `POST /me/messages`. Tiene `ObtenerPerfil` que consulta el nombre real vía Graph.
- `MetaController` (`/api/Meta/webhook`): GET verifica (verify token `gramvet_meta_verify`), POST procesa. Separado del webhook de WhatsApp.
- `CanalRepository.GetOrCreate(nombre)`: siembra los canales Instagram/Messenger en la tabla `Canal` automáticamente.
- `ConversacionService.EnviarMensaje` enruta por nombre de canal: si contiene "messenger"/"instagram" → `MetaMessagingService` (recipient = Telefono sin el prefijo); si no → WhatsApp (comportamiento existente intacto).
- Registrados en `Program.cs`: `ICanalRepository`, `IMetaMessagingService`.

Configuración hecha en Meta (app VetCRM):
- Agregados casos de uso "Interactuar con clientes en Messenger" e "Instagram" (en Meta nuevo se llaman "Casos de uso", no "Agregar producto").
- Webhook de Messenger verificado + página GramVet suscrita a campos `messages` y `messaging_postbacks`.
- Page Access Token generado (activar "solo páginas actuales" → GramVet) y puesto en `appsettings`.
- Cuenta personal de Sebastian de Facebook (**usuario `Bufonsillo`**, facebook.com/Bufonsillo) agregada como **Evaluador** en Roles de la app y aceptada (developers.facebook.com/settings/developer/requests/), para poder probar en dev.

**RESULTADO:** mensajes de Messenger entran al CRM en tiempo real, con badge azul, en el buzón unificado junto a WhatsApp. Envío de respuestas operativo.

**Limitación conocida (NO es bug — confirmado con evidencia):** en **modo Desarrollo**, la Graph API devuelve **HTTP 400** al pedir el nombre del perfil (first_name/last_name) de cuentas tester. El token se lee bien (201 chars). Por eso el contacto se guarda con el **PSID como nombre**. El código de `ObtenerPerfil` es correcto; **en producción (Live + App Review) el nombre real llegará automáticamente**. Workaround temporal: editar el nombre del contacto a mano en el CRM. (Se quitaron los logs temporales de diagnóstico; queda solo el warning "No se pudo obtener perfil".)

⚠️ **ngrok free cambia la URL pública cada vez que se reinicia/cierra.** Cuando pase, hay que actualizar la callback URL en **AMBOS webhooks** de Meta: WhatsApp (`/api/WhatsApp/webhook`) y Meta (`/api/Meta/webhook`). Causó que dejaran de llegar mensajes una vez. **Recomendación pendiente:** usar el dominio estático gratuito de ngrok (panel ngrok → Domains → `--domain=...`) para que la URL no cambie.

---

## 7. PENDIENTES — en orden

1. **Canal Instagram (DM)** — el backend YA lo soporta (mismo `MetaMessagingService`, object "instagram"; permisos `instagram_basic`/`instagram_manage_messages` ya agregados). Falta: en Meta, configurar/suscribir el webhook de **Instagram** a `messages` (similar a lo hecho con Messenger) y probar enviando un DM desde una cuenta con rol. La cuenta de IG (@gramvet.cl, ~12,8k) ya está vinculada a la página de FB.
2. **Pasar la app de Meta a Live + App Review** (`pages_messaging`, `instagram_manage_messages`) para recibir mensajes de **clientes reales** en Messenger/IG y que lleguen los **nombres reales**. En dev solo funcionan cuentas con rol en la app. Se puede correr en paralelo con respond.io durante la prueba (ver sección 8).
3. **Configurar dominio estático de ngrok** (opcional pero muy recomendado) para no reconfigurar Meta cada vez.
4. **Gestión manual del estado de conversación** (cerrar/reabrir/marcar pendiente): el campo `Conversacion.Estado` existe y el webhook reabre auto, pero NO hay UI para que el agente lo cambie. **Sebastian iba a consultar al cliente si lo quiere** antes de desarrollar.
5. **Botón "Agendar cita" en el chat → Google Calendar (con IA)** — feature grande. Extrae info de la conversación (nombre mascotas, dirección, fecha/hora, motivo) con IA y crea la cita en el Google Calendar de la veterinaria. 2 partes: (a) extracción con IA (recomendado: Claude API, modelo Haiku 4.5 o Sonnet 4.6 — baratos; con paso de confirmación humana antes de crear el evento), (b) integración Google Calendar. **BLOQUEANTE:** falta que el dueño explique CÓMO organiza su calendario (un solo calendario para todos los vets, cómo distingue por veterinario, bloques horarios). NO desarrollar la lógica de agendado hasta tenerlo. Estimación de costo IA: con Haiku ~9-20 CLP por agendado; ~500 citas/mes ≈ US$5/mes. Precios oficiales USD/1M tokens (in/out): Haiku 4.5 $1/$5, Sonnet 4.6 $3/$15, Opus 4.8 $5/$25.

---

## 8. Datos clave sobre la integración Meta (no reconstruibles)

- Página de Facebook: **"GramVet Médico Veterinario a Domicilio V Región"**, Page ID `101078882762015` (~7,3k seguidores). Messenger ya recibe mensajes reales del público (lo atiende respond.io hoy).
- Instagram: @gramvet.cl (~12,8k), cuenta profesional vinculada a la página.
- Sebastian tiene acceso a la misma cuenta/app de Meta (VetCRM) donde está WhatsApp.
- **Coexistencia con respond.io:** Messenger e Instagram permiten **múltiples apps suscritas a la misma página** — Meta entrega los webhooks a todas. Por eso nuestra app puede recibir en paralelo SIN romper respond.io, **mientras solo agreguemos** nuestra app y NO toquemos/desconectemos la de respond.io. (WhatsApp NO permite esto: un número solo va a un proveedor; por eso WhatsApp usa número de prueba.) Plan de producción: correr en paralelo unas semanas, y cuando funcione, el cliente desconecta respond.io. Para recibir de clientes reales en paralelo se necesita la app en Live + App Review.

---

## 9. Estado de compilación al cierre
Backend y frontend compilan sin errores (solo warnings preexistentes). El backend estaba corriendo; los últimos cambios de limpieza de logs en `MetaMessagingService` toman efecto al reiniciar. WhatsApp y Messenger funcionando.
