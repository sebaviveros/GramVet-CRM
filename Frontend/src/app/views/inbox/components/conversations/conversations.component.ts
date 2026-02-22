import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from 'src/app/services/inbox/inbox-state.service';

@Component({
  selector: 'app-conversations',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './conversations.component.html',
  styleUrl: './conversations.component.scss'
})
export class ConversationsComponent {
  
  state = inject(InboxStateService);
}