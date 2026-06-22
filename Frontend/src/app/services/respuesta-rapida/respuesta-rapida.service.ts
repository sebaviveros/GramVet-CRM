import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RespuestaRapidaDto {
  id: number;
  comando: string;
  texto: string;
}

export interface CrearRespuestaRapidaDto {
  comando: string;
  texto: string;
}

@Injectable({ providedIn: 'root' })
export class RespuestaRapidaService {
  #http = inject(HttpClient);
  #base = 'https://localhost:7101/api/RespuestaRapida';

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
