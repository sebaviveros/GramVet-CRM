import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';
import { ContactoService, MascotaDto, ContactoDto, BitacoraEntradaDto, MascotaFotoDto } from '../../../../services/contacto/contacto.service';
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
  editDireccion = signal('');
  editReferencia = signal('');

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

  // Bitácora de mascota
  bitacoraAbiertaMascotaId = signal<number | null>(null);
  bitacoraEntradas = signal<BitacoraEntradaDto[]>([]);
  cargandoBitacora = signal(false);
  nuevaBitacora = signal('');

  // Fotos de mascota
  fotosAbiertaMascotaId = signal<number | null>(null);
  fotos = signal<MascotaFotoDto[]>([]);
  cargandoFotos = signal(false);
  subiendoFoto = signal(false);
  lightboxFotoUrl = signal<string | null>(null);

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
        this.bitacoraAbiertaMascotaId.set(null);
        this.bitacoraEntradas.set([]);
        this.nuevaBitacora.set('');
        this.fotosAbiertaMascotaId.set(null);
        this.fotos.set([]);
        this.lightboxFotoUrl.set(null);
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
    this.editDireccion.set(c.direccion ?? '');
    this.editReferencia.set(c.referenciaDireccion ?? '');
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
      email: this.editEmail().trim() || undefined,
      direccion: this.editDireccion().trim() || undefined,
      referenciaDireccion: this.editReferencia().trim() || undefined
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
    if (isNaN(nac.getTime()) || nac > hoy) return '';

    let años = hoy.getFullYear() - nac.getFullYear();
    let meses = hoy.getMonth() - nac.getMonth();
    let dias = hoy.getDate() - nac.getDate();

    // Ajustar días negativos tomando los días del mes anterior
    if (dias < 0) {
      meses--;
      dias += new Date(hoy.getFullYear(), hoy.getMonth(), 0).getDate();
    }
    if (meses < 0) {
      años--;
      meses += 12;
    }

    // Armar el texto omitiendo las unidades en cero
    const partes: string[] = [];
    if (años > 0) partes.push(`${años} ${años === 1 ? 'año' : 'años'}`);
    if (meses > 0) partes.push(`${meses} ${meses === 1 ? 'mes' : 'meses'}`);
    if (dias > 0) partes.push(`${dias} ${dias === 1 ? 'día' : 'días'}`);

    if (partes.length === 0) return 'Recién nacido';
    return partes.join(' ');
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

  asignarVeterinario(event: Event) {
    const conv = this.state.selectedConversation();
    if (!conv) return;

    const select = event.target as HTMLSelectElement;
    const valor = select.value;
    const id = valor ? Number(valor) : null;
    const actualId = conv.usuarioAsignadoId ?? null;
    if (id === actualId) return;

    const vet = id ? this.veterinarios().find(v => v.id === id) : null;
    const mensaje = id
      ? `¿Asignar la conversación a ${vet ? vet.nombre + ' ' + vet.apellido : 'este veterinario'}?`
      : '¿Quitar la asignación del veterinario?';

    Swal.fire({
      icon: 'question',
      title: 'Confirmar asignación',
      text: mensaje,
      showCancelButton: true,
      confirmButtonText: id ? 'Asignar' : 'Desasignar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#235347'
    }).then(res => {
      if (!res.isConfirmed) {
        // Revertir el select a su valor anterior
        select.value = actualId !== null ? String(actualId) : '';
        return;
      }
      this.conversacionService.asignarUsuario(conv.id, id).subscribe(actualizada => {
        this.state.setAssignedVet(conv.id, actualizada.usuarioAsignadoId ?? null, actualizada.usuarioAsignado);
      });
    });
  }

  // ── Bitácora de mascota ───────────────────────────────────────────

  toggleBitacora(mascotaId: number) {
    if (this.bitacoraAbiertaMascotaId() === mascotaId) {
      this.bitacoraAbiertaMascotaId.set(null);
      this.bitacoraEntradas.set([]);
      this.nuevaBitacora.set('');
      return;
    }
    this.bitacoraAbiertaMascotaId.set(mascotaId);
    this.nuevaBitacora.set('');
    this.cargandoBitacora.set(true);
    this.contactoService.getBitacora(mascotaId).subscribe({
      next: entradas => {
        this.bitacoraEntradas.set(entradas);
        this.cargandoBitacora.set(false);
      },
      error: () => this.cargandoBitacora.set(false)
    });
  }

  agregarBitacora(mascotaId: number) {
    const contenido = this.nuevaBitacora().trim();
    if (!contenido) return;
    this.contactoService.crearBitacora({ mascotaId, contenido }).subscribe(entrada => {
      this.bitacoraEntradas.update(list => [entrada, ...list]);
      this.nuevaBitacora.set('');
    });
  }

  eliminarBitacora(id: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar esta anotación?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.contactoService.eliminarBitacora(id).subscribe(() => {
        this.bitacoraEntradas.update(list => list.filter(e => e.id !== id));
      });
    });
  }

  // ── Fotos de mascota ──────────────────────────────────────────────

  toggleFotos(mascotaId: number) {
    if (this.fotosAbiertaMascotaId() === mascotaId) {
      this.fotosAbiertaMascotaId.set(null);
      this.fotos.set([]);
      return;
    }
    this.fotosAbiertaMascotaId.set(mascotaId);
    this.cargandoFotos.set(true);
    this.contactoService.getFotos(mascotaId).subscribe({
      next: fotos => {
        this.fotos.set(fotos);
        this.cargandoFotos.set(false);
      },
      error: () => this.cargandoFotos.set(false)
    });
  }

  onSeleccionarFoto(event: Event, mascotaId: number) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    input.value = ''; // permite re-subir el mismo archivo

    this.subiendoFoto.set(true);
    this.contactoService.subirFoto(mascotaId, file).subscribe({
      next: foto => {
        this.fotos.update(list => [foto, ...list]);
        this.subiendoFoto.set(false);
      },
      error: () => this.subiendoFoto.set(false)
    });
  }

  eliminarFoto(id: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar esta foto?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.contactoService.eliminarFoto(id).subscribe(() => {
        this.fotos.update(list => list.filter(f => f.id !== id));
      });
    });
  }

  abrirFoto(url: string) { this.lightboxFotoUrl.set(url); }
  cerrarFoto() { this.lightboxFotoUrl.set(null); }

  goBackToChat() {
    this.state.setMobileView('chat');
  }
}
