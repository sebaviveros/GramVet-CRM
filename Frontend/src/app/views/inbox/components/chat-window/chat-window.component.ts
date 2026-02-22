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

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.scss']
})
export class ChatWindowComponent implements AfterViewInit {

  state = inject(InboxStateService);

  messageInput = signal('');

  @ViewChild('chatContainer')
  chatContainer!: ElementRef<HTMLDivElement>;

  constructor() {

    // Detecta cambios en mensajes
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

    this.state.addMessage({
      id: Date.now(),
      conversationId: conversation.id,
      text: text,
      timestamp: new Date(),
      sender: 'agent'
    });

    this.messageInput.set('');

  }

  private scrollToBottom() {

    if (!this.chatContainer) return;

    const element = this.chatContainer.nativeElement;

    element.scrollTop = element.scrollHeight;

  }

}