import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule, ButtonModule, FormModule } from '@coreui/angular';
import Swal from 'sweetalert2';
import {
  UsuarioService,
  UsuarioDto,
  RolDto
} from '../../../services/usuario/usuario.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, FormModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  #svc = inject(UsuarioService);

  usuarios = signal<UsuarioDto[]>([]);
  roles = signal<RolDto[]>([]);
  searchTerm = signal('');

  // Formulario nuevo
  newNombre = signal('');
  newApellido = signal('');
  newEmail = signal('');
  newUsername = signal('');
  newRolId = signal<number | null>(null);

  // Edición inline
  editingId = signal<number | null>(null);
  editNombre = signal('');
  editApellido = signal('');
  editEmail = signal('');
  editRolId = signal<number | null>(null);

  filtrados = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.usuarios();
    return this.usuarios().filter(u =>
      u.nombre.toLowerCase().includes(term) ||
      u.apellido.toLowerCase().includes(term) ||
      u.username.toLowerCase().includes(term) ||
      u.email.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.cargar();
    this.#svc.getRoles().subscribe(r => this.roles.set(r));
  }

  cargar() {
    this.#svc.getAll().subscribe(data => this.usuarios.set(data));
  }

  iniciales(u: UsuarioDto): string {
    return ((u.nombre?.charAt(0) ?? '') + (u.apellido?.charAt(0) ?? '')).toUpperCase();
  }

  // ── Crear ─────────────────────────────────────────────────────────

  crear() {
    if (!this.newNombre().trim() || !this.newApellido().trim() ||
        !this.newEmail().trim() || !this.newUsername().trim() || !this.newRolId()) {
      Swal.fire({ icon: 'warning', title: 'Faltan datos', text: 'Completa todos los campos', timer: 2000, showConfirmButton: false });
      return;
    }
    this.#svc.crear({
      nombre: this.newNombre().trim(),
      apellido: this.newApellido().trim(),
      email: this.newEmail().trim(),
      username: this.newUsername().trim(),
      rolId: this.newRolId()!
    }).subscribe({
      next: (nuevo) => {
        this.usuarios.update(list => [...list, nuevo]);
        this.limpiarForm();
        Swal.fire({
          icon: 'success',
          title: 'Usuario creado',
          text: 'Se envió una contraseña al correo del usuario',
          timer: 2500,
          showConfirmButton: false
        });
      },
      error: (err) => {
        Swal.fire({ icon: 'error', title: 'Error', text: err.error?.mensaje ?? 'No se pudo crear el usuario' });
      }
    });
  }

  limpiarForm() {
    this.newNombre.set('');
    this.newApellido.set('');
    this.newEmail.set('');
    this.newUsername.set('');
    this.newRolId.set(null);
  }

  // ── Editar inline ─────────────────────────────────────────────────

  startEdit(u: UsuarioDto) {
    this.editingId.set(u.id);
    this.editNombre.set(u.nombre);
    this.editApellido.set(u.apellido);
    this.editEmail.set(u.email);
    this.editRolId.set(u.rolId);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  guardarEdit(id: number) {
    if (!this.editNombre().trim() || !this.editApellido().trim() ||
        !this.editEmail().trim() || !this.editRolId()) return;
    this.#svc.editar(id, {
      nombre: this.editNombre().trim(),
      apellido: this.editApellido().trim(),
      email: this.editEmail().trim(),
      rolId: this.editRolId()!
    }).subscribe({
      next: (actualizado) => {
        this.usuarios.update(list => list.map(u => u.id === id ? actualizado : u));
        this.editingId.set(null);
      },
      error: (err) => {
        Swal.fire({ icon: 'error', title: 'Error', text: err.error?.mensaje ?? 'No se pudo actualizar' });
      }
    });
  }

  // ── Eliminar ──────────────────────────────────────────────────────

  eliminar(u: UsuarioDto) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar usuario?',
      text: `Se eliminará a ${u.nombre} ${u.apellido}`,
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.#svc.eliminar(u.id).subscribe(() => {
        this.usuarios.update(list => list.filter(x => x.id !== u.id));
      });
    });
  }

  // ── Reset password ────────────────────────────────────────────────

  resetPassword(u: UsuarioDto) {
    Swal.fire({
      icon: 'question',
      title: '¿Restablecer contraseña?',
      text: `Se enviará una nueva contraseña a ${u.email}`,
      showCancelButton: true,
      confirmButtonText: 'Restablecer',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#235347'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.#svc.resetPassword(u.id).subscribe({
        next: (r) => {
          Swal.fire({ icon: 'success', title: 'Listo', text: r.mensaje, timer: 2500, showConfirmButton: false });
        },
        error: () => {
          Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo restablecer la contraseña' });
        }
      });
    });
  }
}
