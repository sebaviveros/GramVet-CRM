// Configuración de PRODUCCIÓN. Reemplaza a environment.ts en el build
// de producción (fileReplacements en angular.json).
// TODO: apuntar backendUrl al dominio real cuando exista el hosting.

const backendUrl = 'https://localhost:7101';

export const environment = {
  production: true,

  backendUrl,
  apiUrl: `${backendUrl}/api`,
  hubUrl: `${backendUrl}/hubs/chat`,

  // Recordar agregar el dominio de producción a los hostnames del widget
  // en el dashboard de Cloudflare Turnstile.
  turnstileSiteKey: '0x4AAAAAADyzWZlofvQpjo8j'
};
