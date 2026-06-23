import { Component } from '@angular/core';
import { Router } from '@angular/router';
import {
  ButtonDirective,
  FormControlDirective,
  FormDirective,
} from '@coreui/angular';
import { AuthService } from '../../../services/auth/auth.service';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [
    FormDirective,
    FormControlDirective,
    ButtonDirective,
    FormsModule
  ],
})
export class LoginComponent {

  username: string = '';
  password: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {

    this.authService.login(this.username, this.password)
      .subscribe({
        next: (res) => {

          const token = res.token;
          this.authService.saveToken(token);

          Swal.fire({
            icon: 'success',
            title: 'Autenticado',
            text: 'Inicio de sesión exitoso',
            background: '#0d0d0d',
            color: '#e0e0e0',
            confirmButtonColor: '#5aad5a',
            iconColor: '#5aad5a',
            showConfirmButton: false,
            timer: 1500,
            customClass: {
              popup: 'swal-dark'
            }
          });

          setTimeout(() => {
            this.router.navigate(['/inbox']);
          }, 1500);
        },

        error: (err) => {

          console.error('Error login', err);

          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Usuario o contraseña incorrectos',
            background: '#0d0d0d',
            color: '#e0e0e0',
            confirmButtonColor: '#d33',
            iconColor: '#ff4d4f',
            customClass: {
              popup: 'swal-dark'
            }
          });
        }
      });
  }
}