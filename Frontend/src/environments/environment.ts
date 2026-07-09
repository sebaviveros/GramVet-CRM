// Configuración de DESARROLLO (la que usa `ng serve`).
// En build de producción se reemplaza por environment.production.ts
// (ver fileReplacements en angular.json).

const backendUrl = 'https://localhost:7101';

export const environment = {
  production: false,

  backendUrl,
  apiUrl: `${backendUrl}/api`,
  hubUrl: `${backendUrl}/hubs/chat`,

  // Site key pública del captcha de Cloudflare Turnstile.
  // El hostname debe estar autorizado en el dashboard de Cloudflare.
  turnstileSiteKey: '0x4AAAAAADyzWZlofvQpjo8j'
};
