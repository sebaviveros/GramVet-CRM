import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoaderService {
  // Contador de peticiones activas (soporta llamadas concurrentes)
  #count = signal(0);

  // Texto opcional bajo el logo. Solo lo usan las acciones que lo piden
  // explícitamente (ej. "Agendar cita (IA)"); el loader global va sin texto.
  #mensaje = signal<string | null>(null);

  isLoading = computed(() => this.#count() > 0);
  mensaje = this.#mensaje.asReadonly();

  show(mensaje?: string) {
    if (mensaje) this.#mensaje.set(mensaje);
    this.#count.update(n => n + 1);
  }

  hide() {
    this.#count.update(n => Math.max(0, n - 1));
    // El mensaje se limpia recién cuando no queda ninguna petición en vuelo,
    // para que una llamada de fondo no borre el texto de la que lo pidió.
    if (this.#count() === 0) this.#mensaje.set(null);
  }
}
