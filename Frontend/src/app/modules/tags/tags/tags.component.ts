import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule, ButtonModule, FormModule } from '@coreui/angular';
import { EtiquetaService, EtiquetaDto } from '../../../services/etiqueta/etiqueta.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, FormModule],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.scss'
})
export class TagsComponent implements OnInit {

  etiquetaService = inject(EtiquetaService);

  etiquetas = signal<EtiquetaDto[]>([]);
  searchTerm = signal('');
  cargando = signal(false);

  // Formulario crear
  nuevoNombre = signal('');
  nuevoColor = signal('#235347');
  nuevoDesc = signal('');

  // Edición inline
  editandoId = signal<number | null>(null);
  editNombre = signal('');
  editColor = signal('');
  editDesc = signal('');

  filtradas = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.etiquetas();
    return this.etiquetas().filter(e =>
      e.nombre.toLowerCase().includes(term) ||
      (e.descripcion ?? '').toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.etiquetaService.getAll().subscribe({
      next: (data) => { this.etiquetas.set(data); this.cargando.set(false); },
      error: () => this.cargando.set(false)
    });
  }

  crear() {
    const nombre = this.nuevoNombre().trim();
    if (!nombre) return;

    this.etiquetaService.crear({
      nombre,
      color: this.nuevoColor(),
      descripcion: this.nuevoDesc().trim() || undefined
    }).subscribe(nueva => {
      this.etiquetas.update(list => [...list, nueva]);
      this.nuevoNombre.set('');
      this.nuevoColor.set('#235347');
      this.nuevoDesc.set('');
    });
  }

  iniciarEdicion(e: EtiquetaDto) {
    this.editandoId.set(e.id);
    this.editNombre.set(e.nombre);
    this.editColor.set(e.color ?? '#235347');
    this.editDesc.set(e.descripcion ?? '');
  }

  cancelarEdicion() {
    this.editandoId.set(null);
  }

  guardarEdicion(id: number) {
    const nombre = this.editNombre().trim();
    if (!nombre) return;

    this.etiquetaService.editar(id, {
      nombre,
      color: this.editColor(),
      descripcion: this.editDesc().trim() || undefined
    }).subscribe(actualizada => {
      this.etiquetas.update(list => list.map(e => e.id === id ? actualizada : e));
      this.editandoId.set(null);
    });
  }

  eliminar(id: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar etiqueta?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.etiquetaService.eliminar(id).subscribe(() => {
        this.etiquetas.update(list => list.filter(e => e.id !== id));
      });
    });
  }
}