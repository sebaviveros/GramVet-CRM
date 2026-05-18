import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { InboxStateService, Message } from '../inbox/inbox-state.service';

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

    this.hubConnection.on('NuevoMensaje', (mensaje: Message) => {
      console.log('Mensaje recibido via SignalR:', mensaje);
      this.state.addMessage(mensaje);
    });
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }
}