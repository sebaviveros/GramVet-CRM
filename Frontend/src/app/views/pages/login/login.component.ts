import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  ButtonDirective,
  FormControlDirective,
  FormDirective,
} from '@coreui/angular';
import { AuthService } from '../../../services/auth/auth.service';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { environment } from '../../../../environments/environment';

declare global {
  interface Window {
    turnstile?: {
      render(el: HTMLElement, opts: Record<string, unknown>): string;
      reset(widgetId?: string): void;
      remove(widgetId?: string): void;
    };
  }
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [
    FormDirective,
    FormControlDirective,
    ButtonDirective,
    FormsModule
  ],
})
export class LoginComponent implements AfterViewInit, OnDestroy {

  username: string = '';
  password: string = '';

  @ViewChild('captcha') captchaRef!: ElementRef<HTMLDivElement>;

  captchaToken = signal<string | null>(null);
  enviando = signal(false);
  verPassword = signal(false);

  #widgetId: string | null = null;
  #esperandoScript: number | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngAfterViewInit(): void {
    this.#montarCaptcha();
  }

  ngOnDestroy(): void {
    if (this.#esperandoScript !== null) clearInterval(this.#esperandoScript);
    if (this.#widgetId) window.turnstile?.remove(this.#widgetId);
  }

  /**
   * El script de Turnstile se carga async desde index.html, así que puede no
   * estar listo cuando Angular monta la vista. Se espera a que aparezca.
   */
  #montarCaptcha(): void {
    if (window.turnstile) {
      this.#render();
      return;
    }

    this.#esperandoScript = window.setInterval(() => {
      if (!window.turnstile) return;
      clearInterval(this.#esperandoScript!);
      this.#esperandoScript = null;
      this.#render();
    }, 150);
  }

  #render(): void {
    this.#widgetId = window.turnstile!.render(this.captchaRef.nativeElement, {
      sitekey: environment.turnstileSiteKey,
      theme: 'dark',
      callback: (token: string) => this.captchaToken.set(token),
      'expired-callback': () => this.captchaToken.set(null),
      'error-callback': () => this.captchaToken.set(null)
    });
  }

  /** El token es de un solo uso: tras cada intento hay que pedir uno nuevo. */
  #resetCaptcha(): void {
    this.captchaToken.set(null);
    if (this.#widgetId) window.turnstile?.reset(this.#widgetId);
  }

  onSubmit() {

    const token = this.captchaToken();
    if (!token || this.enviando()) return;

    this.enviando.set(true);

    this.authService.login(this.username, this.password, token)
      .subscribe({
        next: (res) => {

          const token = res.token;
          this.authService.saveToken(token);

          Swal.fire({
            icon: 'success',
            title: 'Autenticado',
            text: 'Inicio de sesión exitoso',
            background: '#0d0d0d',
            color: '#e0e0e0',
            confirmButtonColor: '#5aad5a',
            iconColor: '#5aad5a',
            showConfirmButton: false,
            timer: 1500,
            customClass: {
              popup: 'swal-dark'
            }
          });

          setTimeout(() => {
            this.router.navigate(['/inbox']);
          }, 1500);
        },

        error: (err) => {

          console.error('Error login', err);

          this.enviando.set(false);
          this.#resetCaptcha();

          // 400 = el captcha no pasó la verificación; 401 = credenciales malas
          const captchaFallido = err.status === 400;

          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: captchaFallido
              ? 'Verificación de seguridad fallida. Intente de nuevo.'
              : 'Usuario o contraseña incorrectos',
            background: '#0d0d0d',
            color: '#e0e0e0',
            confirmButtonColor: '#d33',
            iconColor: '#ff4d4f',
            customClass: {
              popup: 'swal-dark'
            }
          });
        }
      });
  }
}
