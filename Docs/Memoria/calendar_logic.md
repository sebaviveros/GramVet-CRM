---
name: calendar-logic
description: Lógica EXACTA de posicionamiento de citas en el Google Calendar madre (Fase 2 IA) — confirmada por el cliente
metadata: 
  node_type: memory
  type: project
  originSessionId: e8228c74-aed6-4e22-be35-f7d2ebf5651d
---

Lógica del Google Calendar "gramvet" (el **calendario madre**) para agendar citas — **confirmada por el cliente (Gonzalo) el 2026-06-29**. Es la base de la Fase 2 de la automatización IA. Ver también [[project-context]] y `Docs/TRASPASO_2026-06-28.md`.

**Idea central:** la **hora real** de la cita va en el **TÍTULO**; el evento se **posiciona** en un bloque horario CODIFICADO según el móvil. Así separan visualmente Móvil 1 (franja AM) de Móvil 2 (franja PM) en el mismo calendario.

**Móvil 1 y 2 = el AUTO que usa el veterinario** (rotativo, cualquier vet). Lo elige el secretario al agendar; es interno (el cliente NO lo ve). Define el posicionamiento.

## Tabla de slots (lista fija; el secretario elige uno)
Índice `i` → hora real (título) → posición física Móvil 1 (AM) / Móvil 2 (PM). Bloque físico = **1 hora**.

| i | Hora real (título) | Móvil 1 (AM) | Móvil 2 (PM) |
|---|---|---|---|
| 0 | 10 – 11:30 | 1–2 AM | 1–2 PM |
| 1 | 11 – 1 | 2–3 AM | 2–3 PM |
| 2 | 12 – 2 | 3–4 AM | 3–4 PM |
| 3 | 1 – 3 | 4–5 AM | 4–5 PM |
| 4 | 2 – 4 | 5–6 AM | 5–6 PM |
| 5 | 3 – 5 | 6–7 AM | 6–7 PM |
| 6 | 4 – 6 | 7–8 AM | 7–8 PM |
| 7 | 5 – 7 | 8–9 AM | 8–9 PM |
| 8 | 6 – 7:30 | 9–10 AM | 9–10 PM |
| 9 | 6 – 7:30 (2º) | 10–11 AM | — |

**Algoritmo:** posición física = `01:00 + i horas` (Móvil 1) o `13:00 + i horas` (Móvil 2). Evento dura 1 hora física.

## Reglas / detalles
- **Slot i=9 (segundo 6–7:30):** SOLO Móvil 1 y SOLO de **miércoles a sábado** (la gente siempre pide ese horario tarde por trabajo). Móvil 2 NO lo tiene.
- **Slot "1–2 (no siempre)":** opcional, a criterio del secretario (él decide si agenda ahí o no). Caso de borde — probablemente manual.
- Los bloques **naranjos "Instrucciones para agendar día X"** (12 PM) son separadores visuales entre la franja AM (móvil 1) y PM (móvil 2). No son citas.
- El cliente **abandona los placeholders manuales**: la automatización crea eventos NUEVOS en la posición codificada.
- **Un solo calendario madre.** El secretario reparte a cada vet manualmente; nosotros solo escribimos en el madre.
- El cliente pasó un **Google Calendar nuevo VACÍO** para hacer la integración y pruebas antes de conectarlo al real.

## Formato del evento (ver detalle en project-context / traspaso)
- **Título:** `[hora real] [comuna/sector]: [dirección] +[teléfono] [nombre cliente]`
- **Descripción:** Paciente(s) / Cliente solicitó / Se cobró / Total mínimo a cobrar / Referencias del domicilio / Observaciones / Correo.

## Datos que pide el secretario al agendar (alimentan form/IA)
Nombre completo · Dirección (calle y N°) · Referencias · Email · **Ubicación GPS** del domicilio · **Seguro de mascota** (sí/no; docs impresos para firmar al momento, no se gestionan después) · **Condominio + estacionamiento de visita** (sí/no) · **Mascota(s): especie (perro/gato) + nombre** (al agendar se CREAN como mascotas del cliente; edad opcional) · **Móvil (1/2)**.
