import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UsuarioDto {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  username: string;
  rolId: number;
  rolNombre?: string;
  fotoUrl?: string;
}

export interface CrearUsuarioDto {
  nombre: string;
  apellido: string;
  email: string;
  username: string;
  rolId: number;
}

export interface EditarUsuarioDto {
  nombre: string;
  apellido: string;
  email: string;
  rolId: number;
}

export interface RolDto {
  id: number;
  nombre: string;
  descripcion?: string;
}

export interface CambiarPasswordDto {
  passwordActual: string;
  passwordNueva: string;
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  #http = inject(HttpClient);
  #base = 'https://localhost:7101/api/Usuario';
  #rolBase = 'https://localhost:7101/api/Rol';

  getAll(): Observable<UsuarioDto[]> {
    return this.#http.get<UsuarioDto[]>(this.#base);
  }

  crear(dto: CrearUsuarioDto): Observable<UsuarioDto> {
    return this.#http.post<UsuarioDto>(this.#base, dto);
  }

  editar(id: number, dto: EditarUsuarioDto): Observable<UsuarioDto> {
    return this.#http.put<UsuarioDto>(`${this.#base}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#base}/${id}`);
  }

  resetPassword(id: number): Observable<{ mensaje: string }> {
    return this.#http.post<{ mensaje: string }>(`${this.#base}/${id}/reset-password`, {});
  }

  cambiarPassword(dto: CambiarPasswordDto): Observable<{ mensaje: string }> {
    return this.#http.post<{ mensaje: string }>(`${this.#base}/cambiar-password`, dto);
  }

  getRoles(): Observable<RolDto[]> {
    return this.#http.get<RolDto[]>(this.#rolBase);
  }

  getVeterinarios(): Observable<UsuarioDto[]> {
    return this.#http.get<UsuarioDto[]>(`${this.#base}/veterinarios`);
  }

  getMe(): Observable<UsuarioDto> {
    return this.#http.get<UsuarioDto>(`${this.#base}/me`);
  }

  subirFotoPerfil(file: File): Observable<{ fotoUrl: string }> {
    const fd = new FormData();
    fd.append('file', file);
    return this.#http.post<{ fotoUrl: string }>(`${this.#base}/foto`, fd);
  }

  // Sube/cambia la foto de un usuario específico (gestión de usuarios)
  subirFotoUsuario(id: number, file: File): Observable<{ fotoUrl: string }> {
    const fd = new FormData();
    fd.append('file', file);
    return this.#http.post<{ fotoUrl: string }>(`${this.#base}/${id}/foto`, fd);
  }
}
