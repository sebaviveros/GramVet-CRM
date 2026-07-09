import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) { }

  login(username: string, password: string, captchaToken?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, {
      username,
      password,
      captchaToken
    });
  }

  // localStorage y no sessionStorage: en el celular el navegador descarta la
  // pestaña cuando el veterinario pasa un rato en otra app, y con sessionStorage
  // volvía deslogueado aunque el token siguiera vigente.
  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout() {
    localStorage.removeItem('token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken() && !this.isTokenExpirado();
  }

  /**
   * Como el token ahora persiste, hay que mirar el claim `exp`: si no,
   * un token vencido dejaría entrar al buzón y todas las llamadas fallarían.
   */
  isTokenExpirado(): boolean {
    const exp = this.decodeToken()?.['exp'];
    if (!exp) return true;
    return Date.now() >= exp * 1000;
  }

  // ── Decodificación del JWT ──────────────────────────────────────

  private decodeToken(): any | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  getRolNombre(): string {
    const payload = this.decodeToken();
    return (payload?.['rolNombre'] ?? '').toString();
  }

  getUserId(): number | null {
    const payload = this.decodeToken();
    const id = payload?.['nameid'] ??
               payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    return id ? Number(id) : null;
  }

  isAdmin(): boolean {
    return this.getRolNombre().toLowerCase().includes('admin');
  }

  isVeterinario(): boolean {
    return this.getRolNombre().toLowerCase().includes('veterinario');
  }

  // Admin o Secretario: acceso a los módulos de gestión
  isStaff(): boolean {
    const rol = this.getRolNombre().toLowerCase();
    return rol.includes('admin') || rol.includes('secretario');
  }
}
