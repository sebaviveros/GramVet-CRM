import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { InboxStateService, Message, Conversation } from '../inbox/inbox-state.service';
import { AuthService } from '../auth/auth.service';
import { ConversacionService } from '../conversacion/conversacion.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection: signalR.HubConnection | null = null;
  private readonly hubUrl = 'https://localhost:7101/hubs/chat';

  constructor(
    private state: InboxStateService,
    private auth: AuthService,
    private conversacionService: ConversacionService
  ) {}

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR conectado'))
      .catch(err => console.error('Error conectando SignalR:', err));

    this.registrarEventos();
  }

  private registrarEventos(): void {
    if (!this.hubConnection) return;

    // Mensaje nuevo
    this.hubConnection.on('NuevoMensaje', (mensaje: Message) => {
      console.log('Mensaje recibido via SignalR:', mensaje);
      this.state.addMessage(mensaje);

      // Si el mensaje es de la conversación activa: scrollear al fondo y
      // marcarlo como leído (no debe acumularse como no leído estando abierta)
      const convActiva = this.state.selectedConversation();
      if (convActiva && mensaje.conversacionId === convActiva.id) {
        this.state.triggerScrollToBottom();
        this.state.resetearNoLeidos(convActiva.id);
        this.conversacionService.marcarLeida(convActiva.id).subscribe();
      }
    });

    // Conversación actualizada o nueva
    this.hubConnection.on('ConversacionActualizada', (conversacion: Conversation) => {
      console.log('Conversación actualizada via SignalR:', conversacion);
      this.state.upsertConversacion(conversacion);
    });

    // Conversación que dejó de estar asignada a este usuario (reasignada a otro)
    this.hubConnection.on('ConversacionDesasignada', (conversacionId: number) => {
      console.log('Conversación desasignada via SignalR:', conversacionId);
      this.state.removeConversacion(conversacionId);
    });

    // Reacción aplicada/quitada a un mensaje
    this.hubConnection.on('MensajeReaccionado',
      (data: { mensajeId: number; reaccion: string | null; conversacionId: number }) => {
        this.state.updateReaccion(data.mensajeId, data.conversacionId, data.reaccion);
      });

    // Estado de entrega (visto) de un mensaje saliente
    this.hubConnection.on('MensajeEstado',
      (data: { mensajeId: number; estadoEntrega: string; conversacionId: number }) => {
        this.state.updateEstadoMensaje(data.mensajeId, data.conversacionId, data.estadoEntrega);
      });
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }
}