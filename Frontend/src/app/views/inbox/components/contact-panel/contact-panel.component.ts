import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';
import { ContactoService, MascotaDto, ContactoDto, BitacoraEntradaDto, MascotaFotoDto } from '../../../../services/contacto/contacto.service';
import { ConversacionService, CitaExtraidaDto, CrearCitaDto, MascotaCita, SLOTS_AGENDA } from '../../../../services/conversacion/conversacion.service';
import { UsuarioService, UsuarioDto } from '../../../../services/usuario/usuario.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { LoaderService } from '../../../../services/loader/loader.service';
import { colorEtiquetaTexto } from '../../../../shared/color.util';
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
  loader = inject(LoaderService);

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

  // El veterinario NO puede editar datos del contacto ni quitar etiquetas
  // (solo asignar las que tiene habilitadas). Admin/Secretario sí.
  esVeterinario = this.auth.getRolNombre().toLowerCase().includes('veterinario');
  puedeEditarContacto = !this.esVeterinario;
  puedeQuitarEtiqueta = !this.esVeterinario;

  // Agendar cita (IA) — solo staff (admin/secretario)
  puedeAgendarIa = this.puedeAsignar;
  cargandoCita = signal(false);
  cita = signal<CitaExtraidaDto | null>(null);

  // Confirmación de la cita (Fase 2)
  slots = SLOTS_AGENDA;
  movilCita = signal<number>(1);
  fechaCita = signal<string>('');        // yyyy-MM-dd
  slotCita = signal<number | null>(null);
  creandoCita = signal(false);

  // Título y descripción del evento, generados EN VIVO a partir de los campos editables.
  tituloAuto = computed(() => {
    const c = this.cita();
    if (!c) return '';
    const partes: string[] = [];
    const hora = this.slotHoraTitulo();
    if (hora) partes.push(hora);
    if (c.comunaSector?.trim()) partes.push(c.comunaSector.trim() + ':');
    if (c.direccion?.trim()) partes.push(c.direccion.trim());
    if (c.telefono?.trim()) partes.push(this.fmtTel(c.telefono));
    if (c.nombreCliente?.trim()) partes.push(c.nombreCliente.trim());
    return partes.join(' ').trim();
  });

  // Si el secretario edita el título a mano, su texto manda y deja de regenerarse.
  tituloManual = signal<string | null>(null);
  tituloPreview = computed(() => this.tituloManual() ?? this.tituloAuto());
  tituloEditado = computed(() => this.tituloManual() !== null);

  editarTitulo(valor: string) { this.tituloManual.set(valor); }
  restaurarTitulo() { this.tituloManual.set(null); }

  // Una línea en blanco entre cada campo (pedido del cliente).
  // Mantener alineado con ArmarTituloYDescripcion del backend.
  descripcionPreview = computed(() => {
    const c = this.cita();
    if (!c) return '';

    // Las secciones fijas van siempre, aunque estén vacías: es el formato que la
    // veterinaria lee. Las dos opcionales solo aparecen cuando hay algo que decir.
    const lineas = [
      `Paciente(s): ${this.pacientesAuto()}`,
      `Cliente solicitó: ${c.clienteSolicito ?? ''}`,
      `Desglose de lo cobrado: ${c.cobros ?? ''}`,
      `Total mínimo a cobrar: ${c.totalMinimo ?? ''}`,
      `Referencias para encontrar el domicilio: ${c.referenciasDireccion ?? ''}`
    ];

    if (c.indicacionesEstacionamiento?.trim())
      lineas.push(`Indicaciones de estacionamiento: ${c.indicacionesEstacionamiento.trim()}`);

    if (c.seguroMascota) lineas.push('Seguro de mascota: Sí');

    lineas.push(`Observaciones: ${c.observaciones ?? ''}`);
    lineas.push(`Correo: ${c.correo ?? ''}`);

    return lineas.join('\n\n');
  });

  // Hora real para el título: la del slot elegido (sin la aclaración del paréntesis),
  // o el texto sugerido por la IA si todavía no se eligió slot.
  private slotHoraTitulo(): string {
    const i = this.slotCita();
    if (i === null || i === undefined) return this.cita()?.fechaHoraSugerida?.trim() ?? '';
    const label = this.slots.find(s => s.index === i)?.label ?? '';
    return label.split('(')[0].split('—')[0].trim();
  }

  private fmtTel(t: string): string {
    const x = t.trim();
    return x.startsWith('+') ? x : '+' + x;
  }

  // Texto "Pacientes" auto-generado desde la lista estructurada de mascotas.
  // Ej: "1 gato (roberto)", "2 perros (marcelo, luna)", "1 perro de 2 años" (sin nombre).
  // Si no hay lista, usa el texto libre de la IA.
  pacientesAuto(): string {
    // Una mascota puede venir sin nombre ("mi perro de 2 años"): basta con la especie.
    const ms = (this.cita()?.mascotas ?? []).filter(m => m.nombre?.trim() || m.especie?.trim());
    if (ms.length === 0) return this.cita()?.pacientes?.trim() ?? '';

    const grupos = new Map<string, MascotaCita[]>();
    for (const m of ms) {
      const esp = (m.especie?.trim() || 'mascota').toLowerCase();
      if (!grupos.has(esp)) grupos.set(esp, []);
      grupos.get(esp)!.push(m);
    }

    const partes: string[] = [];
    for (const [esp, lista] of grupos) {
      const n = lista.length;
      const label = n === 1 ? esp : esp + 's';
      const hayNombres = lista.some(m => m.nombre?.trim());
      const detalles = lista.map(m => this.detalleMascota(m)).filter(d => d);

      if (detalles.length === 0) partes.push(`${n} ${label}`);
      // Con nombre van entre paréntesis ("1 gato (roberto)"); si solo hay edad,
      // se lee mejor corrido ("1 perro de 2 años").
      else if (hayNombres) partes.push(`${n} ${label} (${detalles.join(', ')})`);
      else partes.push(`${n} ${label} ${detalles.join(', ')}`);
    }
    return partes.join(', ');
  }

  private detalleMascota(m: MascotaCita): string {
    const nombre = m.nombre?.trim() ?? '';
    const anios = Number(m.edadAnios?.trim());
    const edad = anios > 0 ? `de ${anios} ${anios === 1 ? 'año' : 'años'}` : '';
    return [nombre, edad].filter(x => x).join(' ');
  }

  // Estado de una mascota de la cita: 'registrada' (coincide exacto con una del cliente) o 'nueva'.
  mascotaEstado(m: MascotaCita): 'registrada' | 'nueva' {
    const n = m.nombre?.trim().toLowerCase();
    if (!n) return 'nueva';
    const existe = this.mascotas().some(x => x.nombre?.trim().toLowerCase() === n);
    return existe ? 'registrada' : 'nueva';
  }

  // Si el nombre es NUEVO pero se parece mucho a una mascota existente, devuelve ese nombre (aviso).
  mascotaSimilar(m: MascotaCita): string | null {
    if (this.mascotaEstado(m) !== 'nueva') return null;
    const n = this.normalizar(m.nombre);
    if (n.length < 3) return null;
    for (const x of this.mascotas()) {
      const xn = this.normalizar(x.nombre);
      if (xn.length < 3) continue;
      if (this.levenshtein(n, xn) <= 2) return x.nombre ?? null;
    }
    return null;
  }

  // ¿Este dato se va a GUARDAR en el perfil del cliente? (perfil vacío + la cita tiene valor)
  seGuardaraEnPerfil(campo: 'direccion' | 'referenciasDireccion' | 'correo' | 'nombreCliente'): boolean {
    const c = this.cita();
    const ct = this.contacto();
    if (!c) return false;
    switch (campo) {
      case 'direccion':            return !ct?.direccion?.trim() && !!c.direccion?.trim();
      case 'referenciasDireccion': return !ct?.referenciaDireccion?.trim() && !!c.referenciasDireccion?.trim();
      case 'correo':               return !ct?.email?.trim() && !!c.correo?.trim();
      case 'nombreCliente': {
        const nom = ct?.nombre?.trim() ?? '';
        const sinNombre = !nom || nom === ct?.telefono?.trim() || /^\+?[\d\s]+$/.test(nom);
        return sinNombre && !!c.nombreCliente?.trim();
      }
    }
  }

  private normalizar(s?: string): string {
    return (s ?? '').trim().toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '');
  }

  private levenshtein(a: string, b: string): number {
    const m = a.length, n = b.length;
    const d = Array.from({ length: m + 1 }, (_, i) => [i, ...Array(n).fill(0)]);
    for (let j = 0; j <= n; j++) d[0][j] = j;
    for (let i = 1; i <= m; i++)
      for (let j = 1; j <= n; j++)
        d[i][j] = Math.min(
          d[i - 1][j] + 1,
          d[i][j - 1] + 1,
          d[i - 1][j - 1] + (a[i - 1] === b[j - 1] ? 0 : 1)
        );
    return d[m][n];
  }

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

  // Edición del TEXTO de una anotación (las imágenes se manejan aparte)
  editandoBitacoraId = signal<number | null>(null);
  editBitacoraTexto = signal('');
  guardandoBitacora = signal(false);

  // Imágenes: viven dentro de cada anotación de bitácora (ya no por mascota).
  // `nuevasFotos` son las elegidas para la anotación que todavía no se creó.
  nuevasFotos = signal<File[]>([]);
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
        this.nuevasFotos.set([]);
        this.lightboxFotoUrl.set(null);
        this.cita.set(null);
        this.cargandoCita.set(false);
        this.movilCita.set(1);
        this.fechaCita.set('');
        this.slotCita.set(null);
        this.creandoCita.set(false);
        this.tituloManual.set(null);
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

  /** Id del veterinario asignado a la conversación abierta (null = sin asignar). */
  vetAsignadoId = computed(() => this.state.selectedConversation()?.usuarioAsignadoId ?? null);

  /** Un veterinario puede quitarse a sí mismo, pero nunca asignar a otro. */
  puedeDesasignarse = computed(() =>
    !this.puedeAsignar && this.vetAsignadoId() !== null && this.vetAsignadoId() === this.auth.getUserId());

  desasignarme() {
    const conv = this.state.selectedConversation();
    if (!conv) return;

    Swal.fire({
      icon: 'question',
      title: 'Desasignarme',
      text: '¿Quitarte esta conversación? Dejará de aparecer en tu buzón.',
      showCancelButton: true,
      confirmButtonText: 'Desasignarme',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#235347'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.conversacionService.asignarUsuario(conv.id, null).subscribe({
        next: actualizada => {
          this.state.setAssignedVet(conv.id, actualizada.usuarioAsignadoId ?? null, actualizada.usuarioAsignado);
        },
        error: () => {
          Swal.fire({ icon: 'error', title: 'No se pudo desasignar', confirmButtonColor: '#235347' });
        }
      });
    });
  }

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
      this.nuevasFotos.set([]);
      this.cancelEditBitacora();
      return;
    }
    this.bitacoraAbiertaMascotaId.set(mascotaId);
    this.nuevaBitacora.set('');
    this.nuevasFotos.set([]);
    this.cancelEditBitacora();
    this.cargandoBitacora.set(true);
    this.contactoService.getBitacora(mascotaId).subscribe({
      next: entradas => {
        this.bitacoraEntradas.set(entradas);
        this.cargandoBitacora.set(false);
      },
      error: () => this.cargandoBitacora.set(false)
    });
  }

  /** Se puede guardar con solo texto, con solo imágenes, o con ambos. */
  puedeGuardarBitacora(): boolean {
    return !!this.nuevaBitacora().trim() || this.nuevasFotos().length > 0;
  }

  onSeleccionarNuevasFotos(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.nuevasFotos.update(list => [...list, ...Array.from(input.files!)]);
    input.value = ''; // permite volver a elegir el mismo archivo
  }

  quitarNuevaFoto(i: number) {
    this.nuevasFotos.update(list => list.filter((_, idx) => idx !== i));
  }

  agregarBitacora(mascotaId: number) {
    if (!this.puedeGuardarBitacora()) return;

    const contenido = this.nuevaBitacora().trim() || undefined;
    const archivos = this.nuevasFotos();

    this.subiendoFoto.set(true);
    // La anotación se crea primero: las fotos necesitan su id para colgar de ella.
    this.contactoService.crearBitacora({ mascotaId, contenido }).subscribe({
      next: entrada => {
        this.nuevaBitacora.set('');
        this.nuevasFotos.set([]);

        if (archivos.length === 0) {
          this.bitacoraEntradas.update(list => [entrada, ...list]);
          this.subiendoFoto.set(false);
          return;
        }

        this.#subirFotos(entrada.id, archivos, () => {
          // Se recarga la bitácora para traer las fotos ya persistidas
          this.#recargarBitacora(mascotaId);
          this.subiendoFoto.set(false);
        });
      },
      error: () => {
        this.subiendoFoto.set(false);
        Swal.fire({ icon: 'error', title: 'No se pudo guardar la anotación', confirmButtonColor: '#235347' });
      }
    });
  }

  onAgregarFotosAEntrada(event: Event, bitacoraId: number, mascotaId: number) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const archivos = Array.from(input.files);
    input.value = '';

    this.subiendoFoto.set(true);
    this.#subirFotos(bitacoraId, archivos, () => {
      this.#recargarBitacora(mascotaId);
      this.subiendoFoto.set(false);
    });
  }

  /** Sube las imágenes de a una y avisa al terminar (bien o mal). */
  #subirFotos(bitacoraId: number, archivos: File[], alTerminar: () => void) {
    let restantes = archivos.length;
    const listo = () => { if (--restantes === 0) alTerminar(); };

    for (const file of archivos) {
      this.contactoService.subirFotoBitacora(bitacoraId, file).subscribe({
        next: () => listo(),
        error: () => {
          Swal.fire({ icon: 'error', title: `No se pudo subir "${file.name}"`, confirmButtonColor: '#235347' });
          listo();
        }
      });
    }
  }

  #recargarBitacora(mascotaId: number) {
    this.contactoService.getBitacora(mascotaId).subscribe(entradas => this.bitacoraEntradas.set(entradas));
  }

  startEditBitacora(e: BitacoraEntradaDto) {
    this.editandoBitacoraId.set(e.id);
    this.editBitacoraTexto.set(e.contenido ?? '');
  }

  cancelEditBitacora() {
    this.editandoBitacoraId.set(null);
    this.editBitacoraTexto.set('');
  }

  guardarEditBitacora(e: BitacoraEntradaDto) {
    // Puede quedar sin texto si la anotación tiene imágenes; el backend valida
    // que no quede vacía del todo.
    const contenido = this.editBitacoraTexto().trim();
    if (!contenido && e.fotos.length === 0) {
      Swal.fire({
        icon: 'warning',
        title: 'La anotación quedaría vacía',
        text: 'Escribe un texto o deja al menos una imagen.',
        confirmButtonColor: '#235347'
      });
      return;
    }

    this.guardandoBitacora.set(true);
    this.contactoService.editarBitacora(e.id, contenido || undefined).subscribe({
      next: actualizada => {
        this.bitacoraEntradas.update(list => list.map(x => x.id === e.id ? actualizada : x));
        this.cancelEditBitacora();
        this.guardandoBitacora.set(false);
      },
      error: err => {
        this.guardandoBitacora.set(false);
        Swal.fire({
          icon: 'error',
          title: 'No se pudo guardar',
          text: err?.error?.mensaje ?? 'Error inesperado',
          confirmButtonColor: '#235347'
        });
      }
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

  // ── Imágenes de una anotación ─────────────────────────────────────

  eliminarFoto(id: number, mascotaId: number) {
    Swal.fire({
      icon: 'warning',
      title: '¿Eliminar esta imagen?',
      text: 'Esta acción no se puede deshacer',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545'
    }).then(res => {
      if (!res.isConfirmed) return;
      this.contactoService.eliminarFoto(id).subscribe(() => this.#recargarBitacora(mascotaId));
    });
  }

  abrirFoto(url: string) { this.lightboxFotoUrl.set(url); }
  cerrarFoto() { this.lightboxFotoUrl.set(null); }

  /** Color legible del texto del chip (aclara los colores oscuros). */
  colorTexto = (color?: string | null) => colorEtiquetaTexto(color);

  // ── Teléfono principal ────────────────────────────────────────────

  telefonoCopiado = signal(false);

  /**
   * Los contactos de Messenger/Instagram guardan el PSID en `Telefono` con
   * prefijo `FB:`/`IG:`. A esos NO se les antepone `+`, no son teléfonos.
   */
  telefonoMostrado = computed(() => {
    const tel = this.state.selectedConversation()?.telefono ?? '';
    if (!tel) return '';
    const esRedSocial = tel.startsWith('FB:') || tel.startsWith('IG:');
    return esRedSocial ? tel : `+${tel}`;
  });

  copiarTelefono() {
    const tel = this.telefonoMostrado();
    if (!tel) return;
    navigator.clipboard.writeText(tel).then(() => {
      this.telefonoCopiado.set(true);
      setTimeout(() => this.telefonoCopiado.set(false), 1500);
    });
  }

  // ── Agendar cita (IA) ─────────────────────────────────────────────

  extraerCitaIa() {
    const conv = this.state.selectedConversation();
    if (!conv) return;
    this.cargandoCita.set(true);
    // El interceptor excluye /api/Conversacion del loader global, así que este
    // se levanta a mano. La extracción tarda varios segundos (llamada a la IA).
    this.loader.show('Recopilando información (IA)');
    this.conversacionService.extraerCita(conv.id).subscribe({
      next: c => {
        if (!c.mascotas) c.mascotas = [];
        // Se muestran todas las mascotas detectadas por la IA; cada fila se marca
        // como "nueva" o "ya registrada" (las registradas no se duplican al crear).
        this.cita.set(c);
        this.tituloManual.set(null);
        if (c.slotSugerido !== null && c.slotSugerido !== undefined) this.slotCita.set(c.slotSugerido);
        // La IA resuelve "mañana" contra la fecha de hoy: precarga el date picker.
        if (c.fechaSugerida?.trim()) this.fechaCita.set(c.fechaSugerida.trim());
        this.cargandoCita.set(false);
        this.loader.hide();
      },
      error: err => {
        this.cargandoCita.set(false);
        this.loader.hide();
        Swal.fire({
          icon: 'error',
          title: 'No se pudo extraer la cita',
          text: err?.error?.mensaje ?? 'Ocurrió un error inesperado',
          confirmButtonColor: '#235347'
        });
      }
    });
  }

  cerrarCita() {
    this.cita.set(null);
  }

  actualizarCita(campo: keyof CitaExtraidaDto, valor: string | boolean) {
    this.cita.update(c => c ? { ...c, [campo]: valor } : c);
  }

  // ── Mascotas dentro del formulario de cita ──
  agregarMascotaCita() {
    this.cita.update(c => c ? { ...c, mascotas: [...(c.mascotas ?? []), { nombre: '', especie: '' }] } : c);
  }

  quitarMascotaCita(i: number) {
    this.cita.update(c => c ? { ...c, mascotas: (c.mascotas ?? []).filter((_, idx) => idx !== i) } : c);
  }

  actualizarMascotaCita(i: number, campo: keyof MascotaCita, valor: string) {
    this.cita.update(c => {
      if (!c) return c;
      const mascotas = [...(c.mascotas ?? [])];
      mascotas[i] = { ...mascotas[i], [campo]: valor };
      return { ...c, mascotas };
    });
  }

  // ── Confirmar y crear la cita en el calendar ──
  crearCitaConfirmada() {
    const conv = this.state.selectedConversation();
    const c = this.cita();
    if (!conv || !c) return;
    if (!this.fechaCita()) {
      Swal.fire({ icon: 'warning', title: 'Falta la fecha', text: 'Elige el día de la cita.', confirmButtonColor: '#235347' });
      return;
    }
    if (this.slotCita() === null) {
      Swal.fire({ icon: 'warning', title: 'Falta el horario', text: 'Elige un horario de la lista.', confirmButtonColor: '#235347' });
      return;
    }

    const dto: CrearCitaDto = {
      fecha: this.fechaCita(),
      movil: this.movilCita(),
      slotIndex: this.slotCita()!,
      tituloEvento: this.tituloPreview(),
      descripcionEvento: this.descripcionPreview(),
      ubicacion: c.ubicacionGps?.trim() || c.direccion?.trim() || undefined,
      // Una mascota sin nombre es válida ("mi perro de 2 años"): el backend la
      // guarda como "Sin nombre". Solo se descartan las filas totalmente vacías.
      mascotas: (c.mascotas ?? []).filter(m => m.nombre?.trim() || m.especie?.trim() || m.edadAnios?.trim()),
      nombreCliente: c.nombreCliente?.trim() || undefined,
      direccion: c.direccion?.trim() || undefined,
      referenciasDireccion: c.referenciasDireccion?.trim() || undefined,
      correo: c.correo?.trim() || undefined
    };

    this.creandoCita.set(true);
    this.conversacionService.crearCita(conv.id, dto).subscribe({
      next: r => {
        this.creandoCita.set(false);
        this.cita.set(null);

        // El backend pudo crear mascotas y completar campos vacíos del perfil.
        // Sin esto el panel sigue mostrando los datos viejos hasta cambiar de
        // conversación y volver. El nombre además vive en la lista del buzón.
        this.contactoService.getById(conv.contactoId).subscribe(c => {
          this.contacto.set(c);
          this.state.setNombreContacto(conv.id, c.nombre, c.apellido ?? undefined);
        });
        this.cargarMascotas(conv.contactoId);

        const cuando = new Date(r.inicio).toLocaleString('es-CL');
        Swal.fire({
          icon: 'success',
          title: r.simulada ? 'Cita creada (simulada)' : 'Cita agendada',
          html: `${r.mascotasCreadas > 0 ? `Se crearon ${r.mascotasCreadas} mascota(s).<br>` : ''}` +
                `${r.camposPerfilActualizados > 0 ? `Se completaron ${r.camposPerfilActualizados} dato(s) del perfil del cliente.<br>` : ''}` +
                `Bloque: <b>${cuando}</b>` +
                `${r.eventoLink ? `<br><a href="${r.eventoLink}" target="_blank">Ver en el calendario</a>` : ''}`,
          confirmButtonColor: '#235347'
        });
      },
      error: err => {
        this.creandoCita.set(false);
        Swal.fire({ icon: 'error', title: 'No se pudo agendar', text: err?.error?.mensaje ?? 'Error inesperado', confirmButtonColor: '#235347' });
      }
    });
  }

  goBackToChat() {
    this.state.setMobileView('chat');
  }
}
