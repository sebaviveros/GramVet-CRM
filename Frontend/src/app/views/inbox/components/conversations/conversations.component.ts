import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { UsuarioService, UsuarioDto } from '../../../../services/usuario/usuario.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';

@Component({
  selector: 'app-conversations',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './conversations.component.html',
  styleUrl: './conversations.component.scss'
})
export class ConversationsComponent {

  state = inject(InboxStateService);
  conversacionService = inject(ConversacionService);
  auth = inject(AuthService);
  usuarioService = inject(UsuarioService);
  etiquetaService = inject(EtiquetaService);

  // Admin y Secretario tienen filtros extra (sin asignar / por veterinario)
  esStaff = (() => {
    const rol = this.auth.getRolNombre().toLowerCase();
    return rol.includes('admin') || rol.includes('secretario');
  })();

  // Estado de los filtros
  mostrarFiltros = signal(false);
  searchText = signal('');
  estadoFiltro = signal('');
  soloNoLeidos = signal(false);
  sinAsignar = signal(false);
  veterinarioFiltro = signal<number | null>(null);
  etiquetaFiltro = signal<number | null>(null);

  // Datos para los dropdowns
  veterinarios = signal<UsuarioDto[]>([]);
  etiquetas = signal<EtiquetaDto[]>([]);

  filtroActivo = computed(() =>
    !!this.searchText().trim() ||
    !!this.estadoFiltro() ||
    this.soloNoLeidos() ||
    this.sinAsignar() ||
    this.veterinarioFiltro() !== null ||
    this.etiquetaFiltro() !== null
  );

  conversacionesFiltradas = computed(() => {
    let list = this.state.conversations();

    const txt = this.searchText().trim().toLowerCase();
    if (txt) {
      list = list.filter(c =>
        `${c.nombreContacto} ${c.apellidoContacto ?? ''}`.toLowerCase().includes(txt) ||
        c.telefono.toLowerCase().includes(txt)
      );
    }

    const est = this.estadoFiltro();
    if (est) list = list.filter(c => c.estado === est);

    if (this.soloNoLeidos()) list = list.filter(c => c.cantidadNoLeidos > 0);

    if (this.esStaff) {
      if (this.sinAsignar()) list = list.filter(c => !c.usuarioAsignadoId);
      const vet = this.veterinarioFiltro();
      if (vet !== null) list = list.filter(c => c.usuarioAsignadoId === vet);
    }

    const et = this.etiquetaFiltro();
    if (et !== null) list = list.filter(c => (c.etiquetas ?? []).some(e => e.id === et));

    return list;
  });

  ngOnInit() {
    this.etiquetaService.getAll().subscribe(e => this.etiquetas.set(e));
    if (this.esStaff) {
      this.usuarioService.getVeterinarios().subscribe(v => this.veterinarios.set(v));
    }
  }

  limpiarFiltros() {
    this.searchText.set('');
    this.estadoFiltro.set('');
    this.soloNoLeidos.set(false);
    this.sinAsignar.set(false);
    this.veterinarioFiltro.set(null);
    this.etiquetaFiltro.set(null);
  }

  onSelectConversation(id: number) {
    if (this.state.selectedConversation()?.id === id) return;

    this.state.selectConversation(id);
    this.state.setMobileView('chat');

    this.state.setMessages(id, []);
    this.cargarMensajes(id, 1);

    this.conversacionService.marcarLeida(id).subscribe();
    this.state.resetearNoLeidos(id);
  }

  cargarMensajes(conversacionId: number, page: number) {
    this.state.setLoadingMessages(true);

    this.conversacionService.getMensajes(conversacionId, page).subscribe({
      next: (data) => {
        this.state.setMessages(conversacionId, data);
        this.state.setLoadingMessages(false);
        this.state.triggerScrollToBottom();
      },
      error: (err) => {
        console.error('Error cargando mensajes', err);
        this.state.setLoadingMessages(false);
      }
    });
  }
}
