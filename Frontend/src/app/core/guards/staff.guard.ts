import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

// Permite el acceso solo a Admin y Secretario.
// Los Veterinarios quedan confinados al módulo de buzón.
export const staffGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.getToken()) {
    router.navigate(['/login']);
    return false;
  }

  if (authService.isStaff()) {
    return true;
  }

  // Veterinario u otro rol → al inbox
  router.navigate(['/inbox']);
  return false;
};
