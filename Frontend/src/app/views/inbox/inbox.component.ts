import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { InboxSidebarComponent } from './components/inbox-sidebar/inbox-sidebar.component';
import { ConversationsComponent } from './components/conversations/conversations.component';
import { ChatWindowComponent } from './components/chat-window/chat-window.component';
import { ContactPanelComponent } from './components/contact-panel/contact-panel.component';
import { RightToolbarComponent } from './components/right-toolbar/right-toolbar.component';

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
    RightToolbarComponent
  ],
  templateUrl: './inbox.component.html',
  styleUrls: ['./inbox.component.scss']
})
export class InboxComponent {

  state = inject(InboxStateService);

  constructor() {

    this.state.loadMockData();

  }

}