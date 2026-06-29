import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, input, signal, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import {
  AvatarComponent,
  BadgeComponent,
  BreadcrumbRouterComponent,
  ColorModeService,
  ContainerComponent,
  DropdownComponent,
  DropdownDividerDirective,
  DropdownHeaderDirective,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  HeaderComponent,
  HeaderNavComponent,
  HeaderTogglerDirective,
  NavItemComponent,
  NavLinkDirective,
  SidebarToggleDirective
} from '@coreui/angular';

import { IconDirective } from '@coreui/icons-angular';
import { AuthService } from '../../../services/auth/auth.service';
import { UsuarioService } from '../../../services/usuario/usuario.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-default-header',
  templateUrl: './default-header.component.html',
  imports: [ContainerComponent, HeaderTogglerDirective, SidebarToggleDirective, IconDirective, HeaderNavComponent, NavItemComponent, NavLinkDirective, RouterLink, RouterLinkActive, NgTemplateOutlet, BreadcrumbRouterComponent, DropdownComponent, DropdownToggleDirective, AvatarComponent, DropdownMenuDirective, DropdownHeaderDirective, DropdownItemDirective, BadgeComponent, DropdownDividerDirective]
})
export class DefaultHeaderComponent extends HeaderComponent implements OnInit {

  readonly #colorModeService = inject(ColorModeService);
  readonly colorMode = this.#colorModeService.colorMode;
  readonly #authService = inject(AuthService);
  readonly #usuarioService = inject(UsuarioService);
  readonly #router = inject(Router);

  // Foto de perfil del usuario logueado (fallback al avatar por defecto)
  fotoPerfil = signal<string>('./assets/images/avatars/8.jpg');

  ngOnInit(): void {
    this.#usuarioService.getMe().subscribe({
      next: (u) => { if (u.fotoUrl) this.fotoPerfil.set(u.fotoUrl); },
      error: () => { /* deja el avatar por defecto */ }
    });
  }

  onFotoSeleccionada(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    input.value = '';

    this.#usuarioService.subirFotoPerfil(file).subscribe({
      next: (r) => {
        this.fotoPerfil.set(r.fotoUrl);
        Swal.fire({ icon: 'success', title: 'Foto actualizada', timer: 1800, showConfirmButton: false });
      },
      error: () => {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo actualizar la foto' });
      }
    });
  }

  readonly colorModes = [
    { name: 'light', text: 'Light', icon: 'cilSun' },
    { name: 'dark', text: 'Dark', icon: 'cilMoon' },
    { name: 'auto', text: 'Auto', icon: 'cilContrast' }
  ];

  readonly icons = computed(() => {
    const currentMode = this.colorMode();
    return this.colorModes.find(mode => mode.name === currentMode)?.icon ?? 'cilSun';
  });

  constructor() {
    super();
  }

  onLogout() {
    this.#authService.logout();
    this.#router.navigate(['/login']);
  }

  async onCambiarPassword() {
    const { value: formValues } = await Swal.fire({
      title: 'Cambiar contraseña',
      html:
        `<input id="swal-actual" type="password" class="swal2-input" placeholder="Contraseña actual">` +
        `<input id="swal-nueva" type="password" class="swal2-input" placeholder="Nueva contraseña">` +
        `<input id="swal-nueva2" type="password" class="swal2-input" placeholder="Repetir nueva contraseña">`,
      focusConfirm: false,
      showCancelButton: true,
      confirmButtonText: 'Guardar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#235347',
      preConfirm: () => {
        const actual = (document.getElementById('swal-actual') as HTMLInputElement).value;
        const nueva = (document.getElementById('swal-nueva') as HTMLInputElement).value;
        const nueva2 = (document.getElementById('swal-nueva2') as HTMLInputElement).value;

        if (!actual || !nueva || !nueva2) {
          Swal.showValidationMessage('Completa todos los campos');
          return false;
        }
        if (nueva.length < 6) {
          Swal.showValidationMessage('La nueva contraseña debe tener al menos 6 caracteres');
          return false;
        }
        if (nueva !== nueva2) {
          Swal.showValidationMessage('Las contraseñas nuevas no coinciden');
          return false;
        }
        return { actual, nueva };
      }
    });

    if (!formValues) return;

    this.#usuarioService.cambiarPassword({
      passwordActual: formValues.actual,
      passwordNueva: formValues.nueva
    }).subscribe({
      next: (r) => {
        Swal.fire({ icon: 'success', title: 'Listo', text: r.mensaje, timer: 2000, showConfirmButton: false });
      },
      error: (err) => {
        Swal.fire({ icon: 'error', title: 'Error', text: err.error?.mensaje ?? 'No se pudo cambiar la contraseña' });
      }
    });
  }

  sidebarId = input('sidebar1');

}