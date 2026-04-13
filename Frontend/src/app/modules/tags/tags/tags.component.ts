import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  CardModule,
  ButtonModule,
  FormModule
} from '@coreui/angular';

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    FormModule
  ],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.scss'
})
export class TagsComponent {}