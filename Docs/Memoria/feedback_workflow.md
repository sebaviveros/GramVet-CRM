---
name: feedback-workflow
description: Guías de proceso y decisiones de diseño confirmadas por Sebastian
metadata: 
  node_type: memory
  type: feedback
  originSessionId: cc531dcf-7d56-497f-bed8-d0db644bf307
---

**Nunca modificar código ni hacer commits sin autorización explícita de Sebastian.** Siempre proponer primero, esperar confirmación, y solo entonces ejecutar.

**Why:** Sebastian necesita revisar y aprobar cada cambio antes de que se aplique.
**How to apply:** presentar el plan o el código propuesto, preguntar "¿lo aplico?", y esperar un sí antes de usar Edit/Write/Bash.

---

Leer el archivo completo antes de editarlo — nunca asumir contenido a partir de descripciones previas.

**Why:** el código puede haber cambiado entre sesiones; asumir genera errores difíciles de detectar.
**How to apply:** siempre Read antes de Edit/Write en cualquier archivo del proyecto.

---

Separación de responsabilidades en módulos de gestión vs. uso inline:

**Why:** Sebastian corrigió esto durante la sesión de etiquetas — el formulario CRUD de creación/edición/eliminación debe vivir solo en el módulo de gestión; el punto de uso (chat, panel de contacto) solo permite seleccionar/asignar items ya existentes.
**How to apply:** en cualquier feature nueva (ej. respuestas rápidas), aplicar el mismo patrón: vista de gestión separada del lugar donde se usa en el chat.

---

Al final de cada sesión, generar un mensaje de commit git que consolide todos los cambios.

**Why:** Sebastian lo pide explícitamente como cierre de cada sesión de trabajo.
**How to apply:** esperar a que Sebastian indique que terminó la sesión para proponer el commit message.

---

**Sebastian aplica él mismo los scripts de BD / migraciones (sqlcmd contra `GramVetCRM_DEV`); Claude SOLO entrega el script o el comando — no ejecutar `sqlcmd` ni tocar la BD.** Interrumpió un intento de correr una migración por sqlcmd.

**Why:** controla personalmente qué se ejecuta contra su base, igual que con los commits.
**How to apply:** dar el `.sql` o el comando listo para copiar y que él lo corra; no lanzar la ejecución.

---

**Sebastian hace los commits y los push él mismo; Claude SOLO entrega título y descripción — nunca ejecutar `git add/commit/push`.** Intentó stagear archivos y lo interrumpió.

**Why:** Sebastian quiere controlar personalmente qué entra a cada commit (entre otras cosas para no subir `appsettings.json`, que está trackeado y lleva secretos).
**How to apply:** dar el título (estilo `SVN: <resumen>`) y la descripción y parar ahí. La descripción debe ser **resumida y poco técnica** (qué cambió en términos de producto/usuario, no detalles de implementación).

⚠️ **NO agregar el trailer `Co-Authored-By: Claude ...` a los mensajes de commit.** Sebastian lo pidió explícitamente (2026-07-09). El mensaje termina en la descripción, sin firma ni atribución.

---

**Sebastian verifica visualmente él mismo: no levantar el navegador (preview / ng serve propio) solo para "ver" un cambio de CSS.** Interrumpió una verificación en navegador diciendo "no es necesario que tú lo veas, lo estoy viendo yo".

**Why:** él tiene la app corriendo y mira el resultado al toque; levantar un servidor de preview, proxy y login es lento y redundante.
**How to apply:** aplicar el cambio de estilos y describir qué esperar; que él confirme. Reservar la inspección en navegador para bugs que NO se puedan diagnosticar leyendo el código (y avisarle antes).

---

**Escribir en español de CHILE (tuteo), nunca en voseo argentino.** Aplica a los mensajes de Claude en el chat Y a todos los textos de la app (UI, Swal, mensajes de error del backend que llegan al usuario).

**Why:** Sebastian es chileno, el cliente es chileno y la app va dirigida a un público chileno. Corrigió "Podés guardar solo imágenes" → "Puedes".
**How to apply:** usar "puedes / tienes / escribe / elige / deja / revisa / marcas", nunca "podés / tenés / escribí / elegí / dejá / revisá / marcás". Al agregar texto nuevo a la UI o mensajes de error, revisar el voseo antes de cerrar.

---

Dashboard explícitamente fuera del alcance de esta entrega — no incluir en roadmap ni desarrollar salvo que Sebastian lo reincorpore.

**Why:** Sebastian lo sacó explícitamente del scope.
**How to apply:** no mencionar el dashboard como próximo paso ni proponer trabajo en esa área.
