import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ContactoDto {
  id: number;
  nombre: string;
  apellido?: string;
  telefono: string;
  email?: string;
}

export interface EditarContactoDto {
  nombre: string;
  apellido?: string;
  email?: string;
}

export interface MascotaDto {
  id: number;
  contactoId: number;
  nombre: string;
  especie?: string;
  raza?: string;
  fechaNacimiento?: string;
}

export interface CrearMascotaDto {
  contactoId: number;
  nombre: string;
  especie?: string;
  raza?: string;
  fechaNacimiento?: string;
}

export interface EditarMascotaDto {
  nombre: string;
  especie?: string;
  raza?: string;
  fechaNacimiento?: string;
}

@Injectable({ providedIn: 'root' })
export class ContactoService {
  #http = inject(HttpClient);
  #base = 'https://localhost:7101/api/Contacto';
  #mascotaBase = 'https://localhost:7101/api/Mascota';

  getById(id: number): Observable<ContactoDto> {
    return this.#http.get<ContactoDto>(`${this.#base}/${id}`);
  }

  editar(id: number, dto: EditarContactoDto): Observable<ContactoDto> {
    return this.#http.put<ContactoDto>(`${this.#base}/${id}`, dto);
  }

  getMascotas(contactoId: number): Observable<MascotaDto[]> {
    return this.#http.get<MascotaDto[]>(`${this.#base}/${contactoId}/mascotas`);
  }

  crearMascota(dto: CrearMascotaDto): Observable<MascotaDto> {
    return this.#http.post<MascotaDto>(this.#mascotaBase, dto);
  }

  editarMascota(id: number, dto: EditarMascotaDto): Observable<MascotaDto> {
    return this.#http.put<MascotaDto>(`${this.#mascotaBase}/${id}`, dto);
  }

  eliminarMascota(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#mascotaBase}/${id}`);
  }
}
