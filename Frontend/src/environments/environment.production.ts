// Configuración de PRODUCCIÓN. Reemplaza a environment.ts en el build
// de producción (fileReplacements en angular.json).

const backendUrl = 'https://api.gramvet.cl';

export const environment = {
  production: true,

  backendUrl,
  apiUrl: `${backendUrl}/api`,
  hubUrl: `${backendUrl}/hubs/chat`,

  // Recordar agregar el dominio de producción a los hostnames del widget
  // en el dashboard de Cloudflare Turnstile.
  turnstileSiteKey: '0x4AAAAAADyzWZlofvQpjo8j'
};
