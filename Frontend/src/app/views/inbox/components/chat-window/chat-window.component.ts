import {
  Component,
  inject,
  signal,
  ViewChild,
  ElementRef,
  effect,
  AfterViewInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.scss']
})
export class ChatWindowComponent implements AfterViewInit {

  state = inject(InboxStateService);
  conversacionService = inject(ConversacionService);

  messageInput = signal('');
  imagenSeleccionada = signal<File | null>(null);
  imagenPreview = signal<string | null>(null);
  lightboxUrl = signal<string | null>(null);

  @ViewChild('chatContainer')
  chatContainer?: ElementRef<HTMLDivElement>;

  constructor() {
    effect(() => {
      this.state.selectedMessages();
      setTimeout(() => this.scrollToBottom(), 100); 
    });
  }

  ngAfterViewInit() {
    this.scrollToBottom();
  }

  sendMessage() {
    const conversation = this.state.selectedConversation();
    if (!conversation) return;

    // Enviar imagen
    if (this.imagenSeleccionada()) {
      this.conversacionService.subirImagen(this.imagenSeleccionada()!).subscribe({
        next: ({ mediaId }) => {
          this.conversacionService.enviarMensaje({
            conversacionId: conversation.id,
            tipoMensaje: 'image',
            mediaId,
            caption: this.messageInput().trim() || undefined
          }).subscribe({
            next: (mensaje) => {
              this.state.addMessage(mensaje);
              this.messageInput.set('');
              this.cancelarImagen();
            },
            error: (err) => console.error('Error enviando imagen', err)
          });
        },
        error: (err) => console.error('Error subiendo imagen', err)
      });
      return;
    }

    // Enviar texto
    const text = this.messageInput().trim();
    if (!text) return;

    this.conversacionService.enviarMensaje({
      conversacionId: conversation.id,
      contenido: text,
      tipoMensaje: 'text'
    }).subscribe({
      next: (mensaje) => {
        this.state.addMessage(mensaje);
        this.messageInput.set('');
      },
      error: (err) => console.error('Error enviando mensaje', err)
    });
  }

  seleccionarImagen(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    this.imagenSeleccionada.set(file);

    const reader = new FileReader();
    reader.onload = (e) => this.imagenPreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  cancelarImagen() {
    this.imagenSeleccionada.set(null);
    this.imagenPreview.set(null);
  }

  abrirImagen(url: string) {
    this.lightboxUrl.set(url);
  }

  cerrarLightbox() {
    this.lightboxUrl.set(null);
  }

  goBackToConversations(event: MouseEvent) {
    event.stopPropagation();
    this.state.setMobileView('conversations');
  }

  openContactPanel() {
    this.state.openRightPanel('contact');
    if (window.innerWidth <= 992) {
      this.state.setMobileView('contact');
    }
  }

  private scrollToBottom() {
    if (!this.chatContainer?.nativeElement) return;
    const el = this.chatContainer.nativeElement.parentElement;
    if (!el) return;
    el.scrollTop = el.scrollHeight;
  }
}