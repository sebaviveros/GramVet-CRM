import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EtiquetaResumen {
  id: number;
  nombre: string;
  color?: string;
}

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
  usuarioAsignadoId?: number | null;
  canal: string;
  etiquetas?: EtiquetaResumen[];
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
  reaccion?: string;
  estadoEntrega?: string;
}

export interface EnviarMensajeDto {
  conversacionId: number;
  contenido?: string;
  tipoMensaje: string;
  mediaId?: string;
  mediaUrl?: string;
  caption?: string;
  latitud?: number;
  longitud?: number;
  nombreUbicacion?: string;
}

export interface SubirImagenResponse {
  mediaId: string;
  mediaUrl: string | null;
}

export interface MascotaCita {
  nombre: string;
  especie?: string;
  fechaNacimiento?: string;
}

export interface CitaExtraidaDto {
  nombreCliente?: string;
  telefono?: string;
  direccion?: string;
  comunaSector?: string;
  referenciasDireccion?: string;
  correo?: string;
  pacientes?: string;
  clienteSolicito?: string;
  cobros?: string;
  totalMinimo?: string;
  observaciones?: string;
  fechaHoraSugerida?: string;
  ubicacionGps?: string;
  seguroMascota?: boolean;
  seguroNota?: string;
  estacionamientoVisita?: boolean;
  estacionamientoNota?: string;
  mascotas?: MascotaCita[];
  slotSugerido?: number | null;
  tituloEvento: string;
  descripcionEvento: string;
  simulada: boolean;
}

export interface CrearCitaDto {
  fecha: string;            // ISO date (día elegido)
  movil: number;           // 1 | 2
  slotIndex: number;
  tituloEvento: string;
  descripcionEvento: string;
  ubicacion?: string;
  mascotas: MascotaCita[];
  // Datos para completar el perfil del cliente si está vacío
  nombreCliente?: string;
  direccion?: string;
  referenciasDireccion?: string;
  correo?: string;
}

export interface CitaCreadaDto {
  eventoId: string;
  eventoLink?: string;
  inicio: string;
  fin: string;
  mascotasCreadas: number;
  camposPerfilActualizados: number;
  simulada: boolean;
}

// Catálogo fijo de horarios (debe coincidir con AgendaSlots del backend).
export const SLOTS_AGENDA: { index: number; label: string }[] = [
  { index: 0, label: '10 - 11:30' },
  { index: 1, label: '11 - 1' },
  { index: 2, label: '12 - 2' },
  { index: 3, label: '1 - 3' },
  { index: 4, label: '2 - 4' },
  { index: 5, label: '3 - 5' },
  { index: 6, label: '4 - 6' },
  { index: 7, label: '5 - 7' },
  { index: 8, label: '6 - 7:30' },
  { index: 9, label: '6 - 7:30 (2º) — solo Móvil 1, miér a sáb' }
];

@Injectable({
  providedIn: 'root'
})
export class ConversacionService {

  private apiUrl = `${environment.apiUrl}/Conversacion`;

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

  asignarUsuario(conversacionId: number, usuarioAsignadoId: number | null): Observable<ConversacionDto> {
    return this.http.put<ConversacionDto>(`${this.apiUrl}/${conversacionId}/asignar`, { usuarioAsignadoId });
  }

  reaccionar(mensajeId: number, emoji: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/mensaje/${mensajeId}/reaccion`, { emoji });
  }

  extraerCita(conversacionId: number): Observable<CitaExtraidaDto> {
    return this.http.post<CitaExtraidaDto>(`${this.apiUrl}/${conversacionId}/extraer-cita`, {});
  }

  crearCita(conversacionId: number, dto: CrearCitaDto): Observable<CitaCreadaDto> {
    return this.http.post<CitaCreadaDto>(`${this.apiUrl}/${conversacionId}/crear-cita`, dto);
  }
}