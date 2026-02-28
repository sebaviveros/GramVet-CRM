import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { InboxSidebarComponent } from './components/inbox-sidebar/inbox-sidebar.component';
import { ConversationsComponent } from './components/conversations/conversations.component';
import { ChatWindowComponent } from './components/chat-window/chat-window.component';
import { ContactPanelComponent } from './components/contact-panel/contact-panel.component';
import { RightToolbarComponent } from './components/right-toolbar/right-toolbar.component';
import { MobileBottomNavComponent } from './components/mobile-bottom-nav/mobile-bottom-nav.component';

import { InboxStateService } from '../../services/inbox/inbox-state.service';

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
export class InboxComponent implements OnInit {

  state = inject(InboxStateService);

  ngOnInit(): void {

    // solo cargar mock si aún no existen conversaciones
    if (this.state.conversations().length === 0) {
      this.state.loadMockData();
    }

    // vista inicial mobile correcta
    const isMobile =
      window.matchMedia('(max-width: 992px)').matches;

    if (isMobile) {
      this.state.setMobileView('conversations');
    }

  }

}