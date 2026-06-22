import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://localhost:7101/api/Auth';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, {
      username,
      password
    });
  }

  saveToken(token: string) {
    sessionStorage.setItem('token', token);
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  logout() {
    sessionStorage.removeItem('token');
  }

  isLoggedIn(): boolean {
    return !!sessionStorage.getItem('token');
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
}
