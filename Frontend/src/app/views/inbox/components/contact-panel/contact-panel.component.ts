import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';

import { InboxStateService } from '../../../../services/inbox/inbox-state.service';

@Component({
  selector: 'app-contact-panel',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './contact-panel.component.html',
  styleUrls: ['./contact-panel.component.scss']
})
export class ContactPanelComponent {

  state = inject(InboxStateService);

}