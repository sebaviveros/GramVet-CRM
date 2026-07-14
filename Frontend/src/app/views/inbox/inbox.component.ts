import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ConversationsComponent } from './components/conversations/conversations.component';
import { ChatWindowComponent } from './components/chat-window/chat-window.component';
import { ContactPanelComponent } from './components/contact-panel/contact-panel.component';
import { MobileBottomNavComponent } from './components/mobile-bottom-nav/mobile-bottom-nav.component';

import { InboxStateService } from '../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../services/conversacion/conversacion.service';
import { SignalRService } from '../../services/signalr/signalr.service';

@Component({
  selector: 'app-inbox',
  standalone: true,
  imports: [
    CommonModule,
    ConversationsComponent,
    ChatWindowComponent,
    ContactPanelComponent,
    MobileBottomNavComponent
  ],
  templateUrl: './inbox.component.html',
  styleUrls: ['./inbox.component.scss']
})
export class InboxComponent implements OnInit, OnDestroy {

  state = inject(InboxStateService);
  conversacionService = inject(ConversacionService);
  signalRService = inject(SignalRService);

  isMobile = false;
  isClosingPanel = false;

  ngOnInit(): void {
    // Solo teléfonos usan la vista móvil de una columna; tablets (≥768px) usan el
    // layout de escritorio (2 columnas), igual que ordenador.
    this.isMobile = window.matchMedia('(max-width: 767.98px)').matches;

    if (this.isMobile) {
      this.state.setMobileView('conversations');
    }

    // En tablet/teléfono (<992px) el panel de contacto arranca cerrado: en tablet
    // se superpone sobre el chat, así que abrirlo por defecto taparía el chat.
    // En escritorio (≥992px) se conserva el comportamiento por defecto.
    if (window.innerWidth < 992) {
      this.state.closeRightPanel();
    }

    this.cargarConversaciones();
    this.signalRService.startConnection();
  }

  ngOnDestroy(): void {
    this.signalRService.stopConnection();
  }

  cargarConversaciones() {
    this.state.setLoadingConversations(true);

    this.conversacionService.getAll().subscribe({
      next: (data) => {
        this.state.setConversations(data);
        this.state.setLoadingConversations(false);
      },
      error: (err) => {
        console.error('Error cargando conversaciones', err);
        this.state.setLoadingConversations(false);
      }
    });
  }
}