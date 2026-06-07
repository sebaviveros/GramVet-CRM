import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';

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

  onSelectConversation(id: number) {
    // Si ya estaba seleccionada, no hacer nada
    if (this.state.selectedConversation()?.id === id) return;

    this.state.selectConversation(id);
    this.state.setMobileView('chat');

    // Limpiar mensajes anteriores y cargar página 1 fresca
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
      },
      error: (err) => {
        console.error('Error cargando mensajes', err);
        this.state.setLoadingMessages(false);
      }
    });
  }
}