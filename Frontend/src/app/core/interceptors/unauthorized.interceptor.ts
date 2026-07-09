import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';

/**
 * Si el token vence con la app abierta, el authGuard ya no vuelve a correr y
 * las llamadas empiezan a fallar en silencio. Acá se cierra la sesión y se
 * manda al login.
 *
 * Ojo: el 401 del propio login significa "credenciales incorrectas", no sesión
 * vencida, así que se deja pasar para que el LoginComponent muestre su mensaje.
 */
export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      const esLogin = req.url.includes('/Auth/login');

      if (error instanceof HttpErrorResponse && error.status === 401 && !esLogin) {
        authService.logout();
        router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
