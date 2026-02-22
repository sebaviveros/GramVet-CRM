import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';

@Component({
  selector: 'app-right-toolbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './right-toolbar.component.html',
  styleUrls: ['./right-toolbar.component.scss']
})
export class RightToolbarComponent {

  state = inject(InboxStateService);

}