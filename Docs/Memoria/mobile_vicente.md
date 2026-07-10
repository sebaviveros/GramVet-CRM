---
name: mobile-vicente
description: Vicente Fernandez se encarga SOLO de la versión móvil; no debe tomar otros pendientes del TODO
metadata: 
  node_type: memory
  type: project
  originSessionId: 3a79a579-a220-432d-981a-4e8fed79bc11
---

**La versión móvil (responsividad) es la ÚNICA tarea de Vicente Fernandez.** A partir de ~2026-07-09 él baja el proyecto y trabaja en que lo construido se vea y funcione bien en celular. Hasta hoy **no se hizo nada de responsividad móvil**: todo el refinamiento fue para escritorio.

**Reparto de responsabilidades:**
- **Sebastian Viveros** (project lead): terminó y refinó el sistema antes de producción. Es quien decide el alcance.
- **Vicente Fernandez**: adaptar el CRM a móvil. **Nada más.**

**Regla para Claude:** si la sesión la abre Vicente (o alguien que no es Sebastian) y esta memoria aparece en contexto, **NO tomar ningún otro pendiente del TODO** (`Docs/TODO_2026-07-09.md`): ni el respaldo de la base, ni el refresh token, ni la validación de rol en el backend, ni Instagram, ni nada. Solo trabajo de móvil. Si pide otra cosa, avisarle que ese trabajo es de Sebastian y confirmarlo con él antes de tocar código.

**Contexto útil para el trabajo móvil:**
- El buzón ya tiene un layout móvil separado en `inbox.component.html` (`.mobile-views` con las vistas `conversations` / `chat` / `contact` que se deslizan) y un `app-mobile-bottom-nav`. Es el punto de partida, no está terminado.
- El breakpoint que usa el proyecto es `992px` (`d-none d-lg-flex` / `d-lg-none` de CoreUI).
- La sesión ya sobrevive a que el navegador del celular descarte la pestaña (token en `localStorage`, ver [[project-context]]).
- Los textos van en español de Chile, tuteo (ver [[feedback-workflow]]).
