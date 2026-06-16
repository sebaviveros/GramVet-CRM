import { Component, inject, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';

@Component({
  selector: 'app-contact-panel',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './contact-panel.component.html',
  styleUrls: ['./contact-panel.component.scss']
})
export class ContactPanelComponent {

  state = inject(InboxStateService);
  etiquetaService = inject(EtiquetaService);

  todasEtiquetas = signal<EtiquetaDto[]>([]);
  etiquetasContacto = signal<EtiquetaDto[]>([]);
  mostrarSelector = signal(false);

  constructor() {
    // Cargar catálogo completo una sola vez
    this.etiquetaService.getAll().subscribe(e => this.todasEtiquetas.set(e));

    // Recargar etiquetas del contacto al cambiar conversación
    effect(() => {
      const conv = this.state.selectedConversation();
      if (conv) {
        this.cargarEtiquetasContacto(conv.contactoId);
        this.mostrarSelector.set(false);
      } else {
        this.etiquetasContacto.set([]);
      }
    });
  }

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

  goBackToChat() {
    this.state.setMobileView('chat');
  }
}