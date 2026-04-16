import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ButtonDirective,
  FormControlDirective,
  FormDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [
    FormDirective,
    FormControlDirective,
    ButtonDirective,
    RouterLink,
  ],
})
export class LoginComponent {
  onSubmit() {
    // Tu lógica de backend sigue igual aquí
  }
}