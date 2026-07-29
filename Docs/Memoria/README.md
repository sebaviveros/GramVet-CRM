# Memoria del proyecto — GramVet CRM

Copia versionada de la memoria persistente que usa Claude Code. Sirve como
contexto para cualquier desarrollador (o instancia de Claude) que baje el
proyecto sin haber visto las sesiones anteriores.

## Índice

| Archivo | Qué contiene |
|---|---|
| [mobile_vicente.md](mobile_vicente.md) | ⚠️ **Leer primero si eres Vicente.** La versión móvil es su única tarea. |
| [project_context.md](project_context.md) | Stack, arquitectura, estado de cada área, decisiones y causas raíz de bugs difíciles. |
| [feedback_workflow.md](feedback_workflow.md) | Cómo se trabaja: commits, migraciones, tuteo chileno, qué NO hacer. |
| [calendar_logic.md](calendar_logic.md) | Lógica exacta de posicionamiento de citas en Google Calendar. Confirmada por el cliente. |
| [user_profile.md](user_profile.md) | Quién es Sebastian y cómo trabaja. |

## Dos advertencias

**Los secretos están redactados.** Donde el original decía un verify token, un
`AccountId` de Cloudflare o un `PhoneNumberId`, acá dice `<PLACEHOLDER>`. Los
valores reales viven en `appsettings.json`, que **no se sube a git**. No pegues
credenciales en estos archivos.

**Esta copia se desactualiza.** La memoria viva está fuera del repo, en
`~/.claude/projects/<proyecto>/memory/`. Si cambias algo importante, actualiza
las dos. Ante una contradicción, gana el código.

## Pendientes

El TODO vivo es [../TODO_2026-07-09.md](../TODO_2026-07-09.md). El traspaso más
completo es [../TODO_2026-07-09.md](../TODO_2026-07-09.md).
