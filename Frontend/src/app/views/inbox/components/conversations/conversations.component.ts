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
    this.state.selectConversation(id);
    this.state.setMobileView('chat');
    this.cargarMensajes(id);
  }

  cargarMensajes(conversacionId: number) {
    // si ya tiene mensajes cargados no vuelve a pedir
    const existing = this.state.selectedMessages();
    if (existing.length > 0) return;

    this.state.setLoadingMessages(true);

    this.conversacionService.getMensajes(conversacionId).subscribe({
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