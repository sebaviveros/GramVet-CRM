import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EtiquetaDto {
  id: number;
  nombre: string;
  color: string;
  descripcion?: string;
  veterinarioIds?: number[];
}

export interface CrearEtiquetaDto {
  nombre: string;
  color: string;
  descripcion?: string;
  veterinarioIds: number[];
}

@Injectable({
  providedIn: 'root'
})
export class EtiquetaService {

  private apiUrl = `${environment.apiUrl}/Etiqueta`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<EtiquetaDto[]> {
    return this.http.get<EtiquetaDto[]>(this.apiUrl);
  }

  crear(dto: CrearEtiquetaDto): Observable<EtiquetaDto> {
    return this.http.post<EtiquetaDto>(this.apiUrl, dto);
  }

  editar(id: number, dto: CrearEtiquetaDto): Observable<EtiquetaDto> {
    return this.http.put<EtiquetaDto>(`${this.apiUrl}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getByContacto(contactoId: number): Observable<EtiquetaDto[]> {
    return this.http.get<EtiquetaDto[]>(`${this.apiUrl}/contacto/${contactoId}`);
  }

  asignar(contactoId: number, etiquetaId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/asignar`, { contactoId, etiquetaId });
  }

  quitar(contactoId: number, etiquetaId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/quitar/${contactoId}/${etiquetaId}`);
  }
}