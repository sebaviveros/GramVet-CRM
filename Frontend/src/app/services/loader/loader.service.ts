import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoaderService {
  // Contador de peticiones activas (soporta llamadas concurrentes)
  #count = signal(0);

  isLoading = computed(() => this.#count() > 0);

  show() {
    this.#count.update(n => n + 1);
  }

  hide() {
    this.#count.update(n => Math.max(0, n - 1));
  }
}
