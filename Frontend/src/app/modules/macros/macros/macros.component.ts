import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  CardModule,
  ButtonModule,
  FormModule
} from '@coreui/angular';

@Component({
  selector: 'app-macros',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    FormModule
  ],
  templateUrl: './macros.component.html',
  styleUrl: './macros.component.scss'
})
export class MacrosComponent {}