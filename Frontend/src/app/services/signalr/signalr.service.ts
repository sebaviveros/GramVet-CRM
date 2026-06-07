import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { InboxStateService, Message, Conversation } from '../inbox/inbox-state.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection: signalR.HubConnection | null = null;
  private readonly hubUrl = 'https://localhost:7101/hubs/chat';

  constructor(private state: InboxStateService) {}

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
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

    // Mensaje nuevo en conversación abierta
    this.hubConnection.on('NuevoMensaje', (mensaje: Message) => {
      console.log('Mensaje recibido via SignalR:', mensaje);
      this.state.addMessage(mensaje);
    });

    // Conversación actualizada o nueva
    this.hubConnection.on('ConversacionActualizada', (conversacion: Conversation) => {
      console.log('Conversación actualizada via SignalR:', conversacion);
      this.state.upsertConversacion(conversacion);
    });
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }
}