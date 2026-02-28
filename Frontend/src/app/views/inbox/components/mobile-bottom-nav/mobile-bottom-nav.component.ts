import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';

@Component({
  selector: 'app-mobile-bottom-nav',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mobile-bottom-nav.component.html',
  styleUrls: ['./mobile-bottom-nav.component.scss']
})
export class MobileBottomNavComponent {

  state = inject(InboxStateService);

}