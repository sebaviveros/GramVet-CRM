import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';

@Component({
  selector: 'app-inbox-sidebar',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './inbox-sidebar.component.html',
  styleUrl: './inbox-sidebar.component.scss'
})
export class InboxSidebarComponent {}