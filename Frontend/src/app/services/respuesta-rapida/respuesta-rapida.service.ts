import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RespuestaRapidaDto {
  id: number;
  comando: string;
  texto: string;
  veterinarioIds?: number[];
}

export interface CrearRespuestaRapidaDto {
  comando: string;
  texto: string;
  veterinarioIds: number[];
}

@Injectable({ providedIn: 'root' })
export class RespuestaRapidaService {
  #http = inject(HttpClient);
  #base = `${environment.apiUrl}/RespuestaRapida`;

  getAll(): Observable<RespuestaRapidaDto[]> {
    return this.#http.get<RespuestaRapidaDto[]>(this.#base);
  }

  crear(dto: CrearRespuestaRapidaDto): Observable<RespuestaRapidaDto> {
    return this.#http.post<RespuestaRapidaDto>(this.#base, dto);
  }

  editar(id: number, dto: CrearRespuestaRapidaDto): Observable<RespuestaRapidaDto> {
    return this.#http.put<RespuestaRapidaDto>(`${this.#base}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#base}/${id}`);
  }
}
