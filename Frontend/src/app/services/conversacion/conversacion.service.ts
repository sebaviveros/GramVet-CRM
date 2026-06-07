import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ConversacionDto {
  id: number;
  contactoId: number;
  nombreContacto: string;
  apellidoContacto?: string;
  telefono: string;
  estado: string;
  ultimoMensaje?: string;
  fechaUltimoMensaje?: Date;
  cantidadNoLeidos: number;
  usuarioAsignado?: string;
  canal: string;
}

export interface MensajeDto {
  id: number;
  conversacionId: number;
  contenido?: string;
  mediaUrl?: string;
  tipoMensaje?: string;
  direccion: string; // inbound / outbound
  fechaEnvio: Date;
  usuarioId?: number;
}

export interface EnviarMensajeDto {
  conversacionId: number;
  contenido?: string;
  tipoMensaje: string;
  mediaId?: string;
  mediaUrl?: string;
  caption?: string;
}

export interface SubirImagenResponse {
  mediaId: string;
  mediaUrl: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ConversacionService {

  private apiUrl = 'https://localhost:7101/api/Conversacion';

  constructor(private http: HttpClient) { }

  getAll(): Observable<ConversacionDto[]> {
    return this.http.get<ConversacionDto[]>(this.apiUrl);
  }

  getMensajes(conversacionId: number, page: number = 1, pageSize: number = 15): Observable<MensajeDto[]> {
    return this.http.get<MensajeDto[]>(
      `${this.apiUrl}/${conversacionId}/mensajes?page=${page}&pageSize=${pageSize}`
    );
  }

  enviarMensaje(dto: EnviarMensajeDto): Observable<MensajeDto> {
    return this.http.post<MensajeDto>(`${this.apiUrl}/mensaje`, dto);
  }

  subirImagen(file: File): Observable<SubirImagenResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<SubirImagenResponse>(`${this.apiUrl}/upload-imagen`, formData);
  }

  marcarLeida(conversacionId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${conversacionId}/leer`, {});
  }
}