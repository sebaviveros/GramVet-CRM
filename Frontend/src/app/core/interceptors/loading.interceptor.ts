import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { timer, Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoaderService } from '../../services/loader/loader.service';

// Las acciones del buzón en tiempo real ya tienen sus propios indicadores
// ("Enviando...", "Cargando conversaciones..."), así que no muestran el loader global.
const EXCLUDED_URLS = ['/api/Conversacion'];

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loader = inject(LoaderService);

  if (EXCLUDED_URLS.some(u => req.url.includes(u))) {
    return next(req);
  }

  // Solo mostrar el loader si la petición tarda más de 300ms (evita parpadeos).
  let shown = false;
  const sub: Subscription = timer(300).subscribe(() => {
    shown = true;
    loader.show();
  });

  return next(req).pipe(
    finalize(() => {
      sub.unsubscribe();
      if (shown) loader.hide();
    })
  );
};
