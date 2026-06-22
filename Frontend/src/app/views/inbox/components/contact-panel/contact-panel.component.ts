import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';
import { ContactoService, MascotaDto, ContactoDto } from '../../../../services/contacto/contacto.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';
import { UsuarioService, UsuarioDto } from '../../../../services/usuario/usuario.service';
import { AuthService } from '../../../../services/auth/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-contact-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule],
  templateUrl: './contact-panel.component.html',
  styleUrls: ['./contact-panel.component.scss']
})
export class ContactPanelComponent {

  state = inject(InboxStateService);
  etiquetaService = inject(EtiquetaService);
  contactoService = inject(ContactoService);
  conversacionService = inject(ConversacionService);
  usuarioService = inject(UsuarioService);
  auth = inject(AuthService);

  todasEtiquetas = signal<EtiquetaDto[]>([]);
  etiquetasContacto = signal<EtiquetaDto[]>([]);
  mostrarSelector = signal(false);

  // Asignación de veterinario
  veterinarios = signal<UsuarioDto[]>([]);
  // Solo Admin y Secretario pueden asignar
  puedeAsignar = (() => {
    const rol = this.auth.getRolNombre().toLowerCase();
    return rol.includes('admin') || rol.includes('secretario');
  })();

  // Datos del contacto
  contacto = signal<ContactoDto | null>(null);
  editandoContacto = signal(false);
  editNombre = signal('');
  editApellido = signal('');
  editEmail = signal('');

  // Mascotas
  mascotas = signal<MascotaDto[]>([]);
  mostrarFormMascota = signal(false);
  editandoMascotaId = signal<number | null>(null);

  newMascotaNombre = signal('');
  newMascotaEspecie = signal('');
  newMascotaRaza = signal('');
  newMascotaFecha = signal('');

  editMascotaNombre = signal('');
  editMascotaEspecie = signal('');
  editMascotaRaza = signal('');
  editMascotaFecha = signal('');

  constructor() {
    this.etiquetaService.getAll().subscribe(e => this.todasEtiquetas.set(e));

    if (this.puedeAsignar) {
      this.usuarioService.getVeterinarios().subscribe(v => this.veterinarios.set(v));
    }

    effect(() => {
      const conv = this.state.selectedConversation();
      if (conv) {
        this.cargarEtiquetasContacto(conv.contactoId);
        this.cargarContacto(conv.contactoId);
        this.cargarMascotas(conv.contactoId);
        this.mostrarSelector.set(false);
        this.editandoContacto.set(false);
        this.mostrarFormMascota.set(false);
        this.editandoMascotaId.set(null);
      } else {
        this.etiquetasContacto.set([]);
        this.contacto.set(null);
        this.mascotas.set([]);
      }
    });
  }

  // ── Contacto ─────────────────────────────────────────────────────

  cargarContacto(contactoId: number) {
    this.contactoService.getById(contactoId).subscribe(c => this.contacto.set(c));
  }

  startEditContacto() {
    const c = this.contacto();
    if (!c) return;
    this.editNombre.set(c.nombre);
    this.editApellido.set(c.apellido ?? '');
    this.editEmail.set(c.email ?? '');
    this.editandoContacto.set(true);
  }

  cancelEditContacto() {
    this.editandoContacto.set(false);
  }

  guardarContacto() {
    const conv = this.state.selectedConversation();
    if (!conv) return;
    this.contactoService.editar(conv.contactoId, {
      nombre: this.editNombre().trim(),
      apellido: this.editApellido().trim() || undefined,
      email: this.editEmail().trim() || undefined
    }).subscribe(actualizado => {
      this.contacto.set(actualizado);
      this.editandoContacto.set(false);
    });
  }

  // ── Etiquetas ─────────────────────────────────────────────────────

  cargarEtiquetasContacto(contactoId: number) {
    this.etiquetaService.getByContacto(contactoId).subscribe(e => this.etiquetasContacto.set(e));
  }

  etiquetasDisponibles(): EtiquetaDto[] {
    const asignadasIds = new Set(this.etiquetasContacto().map(e => e.id));
    return this.todasEtiquetas().filter(e => !asignadasIds.has(e.id));
  }

  asignar(etiquetaId: number) {
    const conv = this.state.selectedConversation();
    if (!conv) return;
    this.etiquetaService.asignar(conv.contactoId, etiquetaId).subscribe(() => {
      this.cargarEtiquetasContacto(conv.contactoId);
      this.mostrarSelector.set(false);
    });
  }

  quitar(etiquetaId: number) {
    const conv = this.state.selectedConversation();
    if (!conv) return;
    this.etiquetaService.quitar(conv.contactoId, etiquetaId).subscribe(() => {
      this.cargarEtiquetasContacto(conv.contactoId);
    });
  }

  // ── Mascotas ──────────────────────────────────────────────────────

  cargarMascotas(contactoId: number) {
    this.contactoService.getMascotas(contactoId).subscribe(m => this.mascotas.set(m));
  }

  especieTipo(especie?: string): 'gato' | 'perro' | 'otro' {
    const e = (especie ?? '').toLowerCase();
    if (e.includes('gat')) return 'gato';
    if (e.includes('perr') || e.includes('can')) return 'perro';
    return 'otro';
  }

  calcularEdad(fechaNacimiento?: string): string {
    if (!fechaNacimiento) return '';
    const hoy = new Date();
    const nac = new Date(fechaNacimiento);
    const años = hoy.getFullYear() - nac.getFullYear();
    const meses = hoy.getMonth() - nac.getMonth();
    const total = años * 12 + meses;
    if (total < 12) return `${total} mes${total !== 1 ? 'es' : ''}`;
    const a = Math.floor(total / 12);
    return `${a} año${a !== 1 ? 's' : ''}`;
  }

  crearMascota() {
    const conv = this.state.selectedConversation();
    if (!conv || !this.newMascotaNombre().trim()) return;
    this.contactoService.crearMascota({
      contactoId: conv.contactoId,
      nombre: this.newMascotaNombre().trim(),
      especie: this.newMascotaEspecie().trim() || undefined,
      raza: this.newMascotaRaza().trim() || undefined,
      fechaNacimiento: this.newMascotaFecha() || undefined
    }).subscribe(nueva => {
      this.mascotas.update(list => [...list, nueva]);
      this.mostrarFormMascota.set(false);
      this.newMascotaNombre.set('');
      this.newMascotaEspecie.set('');
      this.newMascotaRaza.set('');
      this.newMascotaFecha.set('');
    });
  }

  startEditMascota(m: MascotaDto) {
    this.editandoMascotaId.set(m.id);
    this.editMascotaNombre.set(m.nombre);
    this.editMascotaEspecie.set(m.especie ?? '');
    this.editMascotaRaza.set(m.raza ?? '');
    this.editMascotaFecha.set(m.fechaNacimiento ? m.fechaNacimiento.substring(0, 10) : '');
  }

  cancelEditMascota() {
    this.editandoMascotaId.set(null);
  }

  guardarMascota(id: number) {
    if (!this.editMascotaNombre().trim()) return;
    this.contactoService.editarMascota(id, {
      nombre: this.editMascotaNombre().trim(),
      especie: this.editMascotaEspecie().trim() || undefined,
      raza: this.editMascotaRaza().trim() || undefined,
      fechaNacimiento: this.editMascotaFecha() || undefined
    }).subscribe(actualizada => {
      this.mascotas.update(list => list.map(m => m.id === id ? actualizada : m));
      this.editandoMascotaId.set(null);
    });
  }

  eliminarMascota(id: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar esta mascota?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.contactoService.eliminarMascota(id).subscribe(() => {
        this.mascotas.update(list => list.filter(m => m.id !== id));
      });
    });
  }

  // ── Asignación de veterinario ─────────────────────────────────────

  asignarVeterinario(valor: string) {
    const conv = this.state.selectedConversation();
    if (!conv) return;

    const id = valor ? Number(valor) : null;
    this.conversacionService.asignarUsuario(conv.id, id).subscribe(actualizada => {
      this.state.setAssignedVet(conv.id, actualizada.usuarioAsignadoId ?? null, actualizada.usuarioAsignado);
    });
  }

  goBackToChat() {
    this.state.setMobileView('chat');
  }
}
