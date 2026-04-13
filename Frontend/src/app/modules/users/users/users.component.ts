import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

/* 🔥 IMPORTAR COREUI */
import {
  CardModule,
  ButtonModule,
  FormModule
} from '@coreui/angular';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    FormModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {}