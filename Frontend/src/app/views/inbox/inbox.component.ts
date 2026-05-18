import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

import { InboxSidebarComponent } from './components/inbox-sidebar/inbox-sidebar.component';
import { ConversationsComponent } from './components/conversations/conversations.component';
import { ChatWindowComponent } from './components/chat-window/chat-window.component';
import { ContactPanelComponent } from './components/contact-panel/contact-panel.component';
import { RightToolbarComponent } from './components/right-toolbar/right-toolbar.component';
import { MobileBottomNavComponent } from './components/mobile-bottom-nav/mobile-bottom-nav.component';

import { InboxStateService } from '../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../services/conversacion/conversacion.service';
import { SignalRService } from '../../services/signalr/signalr.service';

@Component({
  selector: 'app-inbox',
  standalone: true,
  imports: [
    CommonModule,
    InboxSidebarComponent,
    ConversationsComponent,
    ChatWindowComponent,
    ContactPanelComponent,
    RightToolbarComponent,
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
    this.isMobile = window.matchMedia('(max-width: 992px)').matches;

    if (this.isMobile) {
      this.state.setMobileView('conversations');
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