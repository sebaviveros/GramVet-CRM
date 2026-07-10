/**
 * Utilidades de color para los chips de etiqueta.
 *
 * Los chips pintan el TEXTO con el color propio de la etiqueta sobre un fondo
 * de ese mismo color al 13% de opacidad. Como los paneles del buzón son
 * oscuros, una etiqueta de color oscuro (violeta, azul marino) queda ilegible.
 * `colorEtiquetaTexto` aclara el color lo justo para que se lea, conservando
 * su tono para que la etiqueta se siga reconociendo.
 */

/** Luminancia relativa (WCAG), 0 = negro, 1 = blanco. */
function luminancia(r: number, g: number, b: number): number {
  const canal = (c: number) => {
    const s = c / 255;
    return s <= 0.03928 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
  };
  return 0.2126 * canal(r) + 0.7152 * canal(g) + 0.0722 * canal(b);
}

function parseHex(hex: string): [number, number, number] | null {
  let h = hex.trim().replace('#', '');
  if (h.length === 3) h = h.split('').map(c => c + c).join('');
  if (h.length !== 6 || !/^[0-9a-f]{6}$/i.test(h)) return null;
  return [
    parseInt(h.slice(0, 2), 16),
    parseInt(h.slice(2, 4), 16),
    parseInt(h.slice(4, 6), 16)
  ];
}

const aHex = (n: number) => Math.round(n).toString(16).padStart(2, '0');

/** Luminancia mínima para que el texto se lea sobre los paneles oscuros del buzón. */
const LUMINANCIA_MINIMA = 0.35;

/**
 * Devuelve el color de la etiqueta, aclarado hacia el blanco solo si es
 * demasiado oscuro para leerse sobre un fondo oscuro. Si el hex es inválido
 * devuelve el original (no rompe nada).
 */
export function colorEtiquetaTexto(color: string | null | undefined): string {
  const base = color?.trim() || '#235347';
  const rgb = parseHex(base);
  if (!rgb) return base;

  let [r, g, b] = rgb;
  if (luminancia(r, g, b) >= LUMINANCIA_MINIMA) return base;

  // Se mezcla con blanco en pasos chicos: conserva el tono, sube el brillo.
  for (let paso = 0; paso < 20; paso++) {
    r += (255 - r) * 0.12;
    g += (255 - g) * 0.12;
    b += (255 - b) * 0.12;
    if (luminancia(r, g, b) >= LUMINANCIA_MINIMA) break;
  }
  return `#${aHex(r)}${aHex(g)}${aHex(b)}`;
}
