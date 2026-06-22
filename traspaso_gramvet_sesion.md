# Traspaso GramVet CRM — Continuación de sesión (para Claude Code)

## Qué es esto

CRM veterinario tipo respond.io (~60-70% completado) para una clínica veterinaria cliente.
Sebastian es el project lead, trabaja con un frontend developer separado pero él dirige arquitectura e implementación.

**Restricción crítica de negocio: nunca interrumpir el WhatsApp Business real del cliente durante desarrollo.**

**Esta es la primera vez que Sebastian usa Claude Code** — hasta ahora todo el trabajo se hizo vía chat conversacional con Claude, leyendo y escribiendo archivos completos. Su forma de trabajar:
- Implementa los cambios entre mensajes y reporta resultados con observaciones concretas (a veces en español)
- Prefiere archivos completos listos para reemplazar, no diffs parciales (esto puede ser distinto con Claude Code al tener acceso directo al filesystem)
- Trabaja de forma iterativa — prueba cada feature antes de seguir con la próxima
- Al final de cada sesión pide un git commit message consolidando los cambios
- Antes de editar cualquier archivo, hay que leerlo completo primero — nunca asumir contenido

## Stack

- **Frontend:** Angular 21, CoreUI Angular, standalone components, Angular Signals, SCSS
- **Backend:** ASP.NET Core .NET 8, C#, Entity Framework Core, SQL Server, arquitectura en capas (Api → Service → Repository → Model)
- **Real-time:** SignalR vía `ChatHub` (en `GramVetCRM.Service/Hubs`)
- **Mensajería:** WhatsApp Cloud API (Meta)
- **Media storage:** Cloudflare R2 (S3-compatible vía AWSSDK.S3) — bucket `gramvetcrm-media`, URL pública `https://pub-fa94e83d8318492d87b9d41e72b93de0.r2.dev`, CORS permite GET desde cualquier origen
- **Dev tunneling:** ngrok en `E:\Workspace\Ngrok\ngrok-v3-stable-windows-amd64`

## Configuración importante

- Backend corre en `https://localhost:7101`
- ngrok: `.\ngrok.exe http https://localhost:7101 --host-header="localhost:7101"` (la URL pública cambia al reiniciar ngrok, hay que actualizarla en Meta for Developers cuando cambie)
- Frontend: `ng serve`, corre en `http://localhost:4200`

**Meta / WhatsApp:**
- App: VetCRM (ID: 167917039309284) — el nombre "GramVetCRM" fue rechazado por Meta por contener "Gram" (trademark de Instagram)
- Número de prueba: +56 9 2172 4181, PhoneNumberId: 1158710177318339
- System User: `gramvet-crm` (token permanente)
- Webhook verify token: `gramvet_gonzalord`
- Webhook suscrito a evento `messages`

**Cloudflare R2:**
- AccountId: 9fbd50ca0b9f525768d3dcc352ef2cb6
- Bucket: gramvetcrm-media

## Trabajo resuelto en sesiones anteriores (antes de esta sesión)

Esto ya estaba hecho al iniciar esta sesión, documentado solo como contexto:

- Logout funcional (frontend) con limpieza del dropdown de demo items
- SignalR al enviar mensaje outbound — `ConversacionService.cs` emite `NuevoMensaje` y `ConversacionActualizada` vía `IHubContext<ChatHub>`
- Fix imagen outbound no visible para el agente — upload paralelo a WhatsApp Media API y Cloudflare R2 usando dos `MemoryStream` independientes desde un único `byte[]` (decisión importante: `OpenReadStream()` sobre el mismo `IFormFile` no es seguro para lecturas paralelas)
- Scroll infinito (carga de mensajes antiguos al hacer scroll hacia arriba) — funcionaba bien
- CRUD Etiquetas — modelo `Etiqueta` y tabla `ContactoEtiqueta` ya existían en DB desde antes

## Trabajo resuelto EN ESTA SESIÓN

### 1. Fix scroll-to-bottom al abrir conversación

**Problema:** al hacer click en una conversación, el guard triple en `ngAfterViewChecked` (`conv.id === pendingId && msgs.length > 0 && scrollHeight > 100`) se cumplía con el DOM todavía mostrando mensajes de la conversación *anterior*, porque Angular no había re-renderizado aún. Resultado: el scroll al fondo solo ocurría cuando el usuario hacía scroll manual.

**Decisión tomada:** descartar el guard triple basado en `ngAfterViewChecked` por completo. En su lugar:
- `InboxStateService` — nuevo signal `_scrollToBottomCounter` (numérico, no un ID de conversación) con método `triggerScrollToBottom()` que lo incrementa. Se usa un counter en vez de un conversationId para poder disparar el scroll aunque la conversación activa no cambie (ej: llega un mensaje nuevo en la conversación que ya está abierta).
- `conversations.component.ts` — al seleccionar una conversación: `setMessages(id, [])` para limpiar, luego carga la página 1, y en el callback `next` (cuando los mensajes ya están en el state) llama `state.triggerScrollToBottom()`.
- `signalr.service.ts` — en el handler de `NuevoMensaje`, si el mensaje pertenece a la conversación activa, también llama `triggerScrollToBottom()`.
- `chat-window.component.ts` — eliminada toda la lógica de `#pendingScrollConvId`. Reemplazada por un `effect()` en el constructor que observa `scrollToBottomCounter()` y, cuando cambia, llama `afterNextRender(() => { el.scrollTop = el.scrollHeight })` usando el `Injector` del componente. `afterNextRender` garantiza que el DOM ya está actualizado con los mensajes nuevos antes de ejecutar el scroll — esto es lo que elimina la race condition.

**Estado: resuelto y confirmado funcionando por Sebastian.**

### 2. CRUD Etiquetas — implementación completa full stack

El modelo `Etiqueta` y la tabla `ContactoEtiqueta` ya existían en la DB desde antes, pero no había ninguna capa de implementación. Se construyó todo el stack:

**Backend (archivos nuevos):**
- `GramVetCRM.Model/DTOs/Etiqueta/EtiquetaDto.cs` — `EtiquetaDto`, `CrearEtiquetaDto`, `AsignarEtiquetaDto`
- `GramVetCRM.Repository/Repositories/Etiqueta/Interface/IEtiquetaRepository.cs`
- `GramVetCRM.Repository/Repositories/Etiqueta/EtiquetaRepository.cs` — implementa soft delete (`Active = false`), `GetByContacto`, `GetContactoEtiqueta`, `AddContactoEtiqueta`, `RemoveContactoEtiqueta`
- `GramVetCRM.Service/EtiquetaService/Interface/IEtiquetaService.cs`
- `GramVetCRM.Service/EtiquetaService/EtiquetaService.cs` — `GetAll`, `Crear`, `Editar` (agregado durante la sesión, ver más abajo), `Eliminar`, `GetByContacto`, `AsignarEtiqueta`, `QuitarEtiqueta`
- `GramVetCRM.Api/Controllers/EtiquetaController.cs` — endpoints:
  - `GET /api/Etiqueta` — todas las etiquetas
  - `POST /api/Etiqueta` — crear
  - `PUT /api/Etiqueta/{id}` — editar (agregado durante la sesión)
  - `DELETE /api/Etiqueta/{id}` — eliminar (soft delete)
  - `GET /api/Etiqueta/contacto/{contactoId}` — etiquetas asignadas a un contacto
  - `POST /api/Etiqueta/asignar` — asignar etiqueta a contacto
  - `DELETE /api/Etiqueta/quitar/{contactoId}/{etiquetaId}` — quitar asignación

**Backend (modificado):**
- `Program.cs` — registrados `IEtiquetaRepository → EtiquetaRepository` y `IEtiquetaService → EtiquetaService` en DI (scoped)

**Frontend (archivo nuevo):**
- `src/app/services/etiqueta/etiqueta.service.ts` — interfaces `EtiquetaDto`, `CrearEtiquetaDto`, y todos los métodos HTTP (`getAll`, `crear`, `editar`, `eliminar`, `getByContacto`, `asignar`, `quitar`)

**Decisión importante de separación de responsabilidades (corregida a mitad de sesión):**

La primera implementación puso el formulario de crear etiquetas dentro del `contact-panel` (panel de contacto del chat). Sebastian corrigió esto: **la gestión completa (crear/editar/eliminar) debe vivir únicamente en el módulo "Gestión de Etiquetas"** (`tags.component`, ya existente con UI armada pero con datos mock). El panel de contacto del chat **solo debe permitir asignar/quitar etiquetas ya existentes** a un contacto puntual, nunca crear nuevas.

Cambios resultantes:
- `tags.component.ts/html/scss` (en `src/app/modules/tags/tags/`) — conectado a datos reales vía `EtiquetaService` en lugar de mock. Tiene: tabla con listado, búsqueda por nombre/descripción, formulario de creación con color picker, edición inline por fila (con inputs que aparecen reemplazando la fila normal, Enter guarda, Escape cancela), eliminar con `confirm()`. Esta vista es la única con capacidad de crear/editar/eliminar etiquetas del catálogo global.
- `contact-panel.component.ts/html/scss` — simplificado para que **solo** muestre las etiquetas asignadas al contacto activo (todas, sin límite) con botón ✕ para quitar cada una, y un botón "+ Agregar etiqueta" que abre un selector con las etiquetas del catálogo que aún no están asignadas a ese contacto. No tiene formulario de creación.

**Estado: resuelto y confirmado funcionando por Sebastian, tanto el módulo de gestión como el panel de contacto.**

### 3. Etiquetas visibles en el header del chat (buzón / inbox)

Sebastian pidió que el header de la conversación (donde antes había un badge mockup de estado "Abierta"/"Cerrada") mostrara en su lugar las etiquetas reales del contacto, máximo 2 visibles + contador "+N" si hay más.

**Implementación:**
- `chat-window.component.ts` — se inyectó `EtiquetaService`, se agregó signal `etiquetasContacto`, y un `effect()` en el constructor que llama `etiquetaService.getByContacto(conv.contactoId)` cada vez que cambia `state.selectedConversation()`.
- El header del chat ahora muestra etiquetas en lugar del badge de estado mockup.

**Iteración de diseño del layout (varias vueltas, esto es lo importante a preservar para no repetir la discusión):**

1. *Primer intento:* etiquetas en la misma fila (`name-row`) junto al nombre del contacto → **descartado**, porque con 2+ etiquetas el contenido se expandía horizontalmente y empujaba todo el layout, perdiéndose el panel de contacto y la barra de iconos de la derecha (no hay scroll horizontal en ese contenedor).

2. *Segundo intento:* aplicar `min-width: 0`, `overflow: hidden`, `flex-shrink`, `max-width` + `text-overflow: ellipsis` en cascada por todo el árbol (`chat-header-container` → `chat-user` → `user-info` → `name-row`) para forzar compresión → mejoró parcialmente pero con 3 etiquetas el problema volvía a aparecer.

3. *Tercer intento (decisión intermedia):* mover las etiquetas a una segunda línea debajo del nombre (`tags-canal-row`, etiquetas + canal "WhatsApp" en la misma fila) → resolvió el desborde horizontal pero el canal "WhatsApp" terminaba pegado después de las etiquetas en lugar de en su propia línea, lo cual no era el diseño que Sebastian quería.

4. **Layout final (correcto, confirmado por Sebastian):** estructura de **2 columnas** dentro de `.user-info`:
   - **Columna izquierda** (`.user-info-left`): nombre del contacto arriba, canal ("WhatsApp") abajo — apilados verticalmente.
   - **Columna derecha** (`.user-info-tags`): primera etiqueta arriba (fila 1), segunda etiqueta + contador "+N" abajo (fila 2, en `.tags-second-row`) — solo se renderiza si hay al menos 1 etiqueta.

   En el HTML esto se implementó accediendo directamente a `etiquetasContacto()[0]` y `etiquetasContacto()[1]` (en lugar de un `@for` con `slice(0,2)`) porque cada índice va en una fila distinta del layout de 2 columnas.

5. **Ajuste final de espaciado:** el chip `.header-etiqueta` tenía `max-width: 130px` sin `width: fit-content`, lo que generaba que el chip mostrara más aire/padding del necesario para textos cortos como "Nuevo Usuario". Se corrigió agregando `width: fit-content` antes del `max-width: 130px`, dejando el max-width solo como tope de seguridad para nombres muy largos (se truncan con `text-overflow: ellipsis` y tienen `[title]="etiqueta.nombre"` como tooltip nativo del navegador).

**CSS final de `.header-etiqueta`:**
```scss
.header-etiqueta {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 500;
  border: 1px solid;
  white-space: nowrap;
  line-height: 1.5;
  width: fit-content;
  max-width: 130px;
  overflow: hidden;
  text-overflow: ellipsis;
}
```

**También se eliminó** el `<select>` mockup "Asignar veterinario" que estaba en el header del chat (en `.chat-actions`) y **se movió al panel de contacto** (`contact-panel`), dentro de una nueva sección "Veterinario asignado" con su propio `<select>` conectado a `state.setAssignedVet(...)` (este método y el array `state.vets` ya existían previamente en `InboxStateService`, no son nuevos). Se eliminaron los estilos `.chat-actions select` del SCSS del chat-window ya que el elemento dejó de existir ahí.

**Estado: resuelto y confirmado funcionando por Sebastian — esta fue la última tarea cerrada de la sesión.**

## Estado general del proyecto al cierre de esta sesión

| Área | Estado |
|---|---|
| Arquitectura y base | 100% |
| Autenticación (login, JWT, interceptor, guard) | 100% |
| Logout | 100% |
| Recepción mensajes WhatsApp (texto, imagen, audio, video) | ~95% |
| Envío texto desde CRM | 100% |
| Envío imagen desde CRM (visible para el agente) | 100% |
| SignalR outbound | 100% |
| Scroll infinito (carga páginas anteriores) | 100% |
| Scroll-to-bottom al abrir conversación | Resuelto esta sesión |
| CRUD Etiquetas (gestión completa) | Resuelto esta sesión |
| Asignar/quitar etiquetas desde panel de contacto | Resuelto esta sesión |
| Etiquetas visibles en header del chat | Resuelto esta sesión |
| Respuestas rápidas | 0% |
| Panel de contacto completo (mascotas) | 0% (excepto la sección de etiquetas y veterinario, ya hechas) |
| Gestión de usuarios | ~10% |
| Dashboard | Fuera de alcance de esta entrega — explícitamente sacado del scope por Sebastian, no desarrollar. |

## Próximos pasos (en orden, definidos explícitamente por Sebastian)

1. **Respuestas rápidas**
   - Mismo patrón que etiquetas: el modelo ya existe en DB (confirmar nombre exacto del modelo al leer el código), falta toda la implementación backend (repository → service → controller) y frontend.
   - UI propuesta: un botón en el footer del chat (cerca del input de mensaje) que despliega una lista de respuestas rápidas para insertar directamente en el input.
   - Aplicar el mismo criterio de separación de responsabilidades aprendido en etiquetas: probablemente conviene una vista de gestión (CRUD completo) separada del punto de uso en el chat (que solo selecciona/inserta).

2. **Panel de contacto completo**
   - Editar nombre/apellido del contacto.
   - Ver mascotas asociadas (tabla `Mascota` ya existe en DB, confirmar estructura al leer el código).
   - Nota: las secciones de etiquetas y veterinario asignado en el contact-panel ya están implementadas; esto es para sumar lo que falta del panel (datos editables del contacto + mascotas), no para rehacer lo existente.

3. **Gestión de usuarios**
   - CRUD con roles: admin, secretario, veterinario.
   - Hay ~10% ya hecho de sesiones previas a esta — falta confirmar exactamente qué existe al retomar.

**Dashboard explícitamente fuera de la entrega — no incluir en el roadmap ni desarrollar salvo que Sebastian lo reincorpore explícitamente.**

## Notas de proceso para quien retome este trabajo

- Antes de tocar cualquier archivo, leerlo completo primero. No asumir su contenido a partir de descripciones previas — el código puede haber cambiado.
- Sebastian prueba cada feature antes de avanzar a la siguiente; espera reportes concretos de qué se hizo y por qué, no solo el código.
- Al final de cada sesión de trabajo, generar un mensaje de commit de git que consolide todos los cambios de la sesión (estilo conventional commits, como se ve en el ejemplo de esta sesión: `feat: <resumen>` seguido de secciones por área).
- La regla de WhatsApp Business real del cliente nunca debe verse interrumpida — cualquier prueba con la API de WhatsApp debe usar el número de prueba (+56 9 2172 4181), nunca tocar configuración que pueda afectar el número productivo del cliente.
- Patrón de capas establecido y que debe respetarse para cualquier feature nueva: `Api (Controller) → Service → Repository → Model`, con interfaces para Repository y Service registradas en `Program.cs` como `Scoped`.
- Patrón de soft delete establecido: en lugar de eliminar filas, se marca `Active = false` y se actualiza `Fechaup`/`Userup`. Ya usado en `EtiquetaRepository.Delete()` y en el quitar de `ContactoEtiqueta`.
