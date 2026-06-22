import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule, ButtonModule, FormModule } from '@coreui/angular';
import { RespuestaRapidaService, RespuestaRapidaDto } from '../../../services/respuesta-rapida/respuesta-rapida.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-macros',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, FormModule],
  templateUrl: './macros.component.html',
  styleUrl: './macros.component.scss'
})
export class MacrosComponent {
  #svc = inject(RespuestaRapidaService);

  respuestas = signal<RespuestaRapidaDto[]>([]);
  searchTerm = signal('');
  newComando = signal('');
  newTexto = signal('');

  editingId = signal<number | null>(null);
  editComando = signal('');
  editTexto = signal('');

  filtradas = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.respuestas();
    return this.respuestas().filter(r =>
      r.comando.toLowerCase().includes(term) ||
      r.texto.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.#svc.getAll().subscribe(data => this.respuestas.set(data));
  }

  crear() {
    const comando = this.newComando().trim();
    const texto = this.newTexto().trim();
    if (!comando || !texto) return;
    this.#svc.crear({ comando, texto }).subscribe(nueva => {
      this.respuestas.update(list =>
        [...list, nueva].sort((a, b) => a.comando.localeCompare(b.comando))
      );
      this.newComando.set('');
      this.newTexto.set('');
    });
  }

  startEdit(r: RespuestaRapidaDto) {
    this.editingId.set(r.id);
    this.editComando.set(r.comando);
    this.editTexto.set(r.texto);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  guardarEdit(id: number) {
    const comando = this.editComando().trim();
    const texto = this.editTexto().trim();
    if (!comando || !texto) return;
    this.#svc.editar(id, { comando, texto }).subscribe(actualizada => {
      this.respuestas.update(list =>
        list.map(r => r.id === id ? actualizada : r)
            .sort((a, b) => a.comando.localeCompare(b.comando))
      );
      this.editingId.set(null);
    });
  }

  eliminar(id: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar respuesta rápida?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.#svc.eliminar(id).subscribe(() => {
        this.respuestas.update(list => list.filter(r => r.id !== id));
      });
    });
  }
}
