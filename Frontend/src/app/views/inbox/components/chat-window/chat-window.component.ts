import {
  Component,
  inject,
  signal,
  ViewChild,
  ElementRef,
  effect,
  AfterViewInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';

import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.scss']
})
export class ChatWindowComponent implements AfterViewInit {

  state = inject(InboxStateService);
  conversacionService = inject(ConversacionService);

  messageInput = signal('');

  @ViewChild('chatContainer')
  chatContainer?: ElementRef<HTMLDivElement>;

  constructor() {
    effect(() => {
      this.state.selectedMessages();
      setTimeout(() => this.scrollToBottom(), 0);
    });
  }

  ngAfterViewInit() {
    this.scrollToBottom();
  }

  sendMessage() {
    const text = this.messageInput().trim();
    const conversation = this.state.selectedConversation();

    if (!text || !conversation) return;

    this.conversacionService.enviarMensaje({
      conversacionId: conversation.id,
      contenido: text,
      tipoMensaje: 'texto'
    }).subscribe({
      next: (mensaje) => {
        this.state.addMessage(mensaje);
        this.messageInput.set('');
      },
      error: (err) => {
        console.error('Error enviando mensaje', err);
      }
    });
  }

  goBackToConversations(event: MouseEvent) {
    event.stopPropagation();
    this.state.setMobileView('conversations');
  }

  openContactPanel() {
    this.state.openRightPanel('contact');
    if (window.innerWidth <= 992) {
      this.state.setMobileView('contact');
    }
  }

  private scrollToBottom() {
    if (!this.chatContainer?.nativeElement) return;
    const el = this.chatContainer.nativeElement;
    el.scrollTop = el.scrollHeight;
  }
}