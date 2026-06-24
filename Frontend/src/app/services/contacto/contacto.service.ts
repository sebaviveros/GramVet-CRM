import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ContactoDto {
  id: number;
  nombre: string;
  apellido?: string;
  telefono: string;
  email?: string;
  direccion?: string;
  referenciaDireccion?: string;
}

export interface EditarContactoDto {
  nombre: string;
  apellido?: string;
  email?: string;
  direccion?: string;
  referenciaDireccion?: string;
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

export interface BitacoraEntradaDto {
  id: number;
  mascotaId: number;
  contenido: string;
  fecha: string;
  autor?: string;
}

export interface CrearBitacoraDto {
  mascotaId: number;
  contenido: string;
}

export interface MascotaFotoDto {
  id: number;
  mascotaId: number;
  url: string;
  descripcion?: string;
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

  // ── Bitácora ───────────────────────────────────────────────────────

  getBitacora(mascotaId: number): Observable<BitacoraEntradaDto[]> {
    return this.#http.get<BitacoraEntradaDto[]>(`${this.#mascotaBase}/${mascotaId}/bitacora`);
  }

  crearBitacora(dto: CrearBitacoraDto): Observable<BitacoraEntradaDto> {
    return this.#http.post<BitacoraEntradaDto>(`${this.#mascotaBase}/bitacora`, dto);
  }

  eliminarBitacora(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#mascotaBase}/bitacora/${id}`);
  }

  // ── Fotos de mascota ───────────────────────────────────────────────

  getFotos(mascotaId: number): Observable<MascotaFotoDto[]> {
    return this.#http.get<MascotaFotoDto[]>(`${this.#mascotaBase}/${mascotaId}/fotos`);
  }

  subirFoto(mascotaId: number, file: File, descripcion?: string): Observable<MascotaFotoDto> {
    const fd = new FormData();
    fd.append('file', file);
    if (descripcion) fd.append('descripcion', descripcion);
    return this.#http.post<MascotaFotoDto>(`${this.#mascotaBase}/${mascotaId}/fotos`, fd);
  }

  eliminarFoto(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#mascotaBase}/fotos/${id}`);
  }
}
