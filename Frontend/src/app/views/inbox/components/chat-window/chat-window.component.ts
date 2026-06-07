import {
  Component,
  inject,
  signal,
  ViewChild,
  ElementRef,
  effect,
  AfterViewChecked,
  OnDestroy,
  NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';

const PAGE_SIZE = 15;

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.scss']
})
export class ChatWindowComponent implements AfterViewChecked, OnDestroy {

  state = inject(InboxStateService);
  conversacionService = inject(ConversacionService);
  #zone = inject(NgZone);

  messageInput = signal('');
  imagenSeleccionada = signal<File | null>(null);
  imagenPreview = signal<string | null>(null);
  lightboxUrl = signal<string | null>(null);

  @ViewChild('scrollContainer')
  scrollContainer?: ElementRef<HTMLDivElement>;

  // ID de conversación para la cual hay que scrollear al fondo.
  // Se setea cuando cambia la conversación seleccionada o llega un mensaje nuevo.
  // Se limpia SOLO cuando el DOM ya muestra mensajes de ESA conversación.
  #pendingScrollConvId: number | null = null;

  #scrollHeightBeforePrepend = 0;
  #isPrepending = false;

  constructor() {
    // Observar cambios en los mensajes seleccionados
    effect(() => {
      const msgs = this.state.selectedMessages();
      const conv = this.state.selectedConversation();

      if (!conv || this.#isPrepending) return;

      // Hay mensajes de la conversación actual → pedir scroll al fondo
      if (msgs.length > 0) {
        this.#pendingScrollConvId = conv.id;
      }
    });
  }

  ngAfterViewChecked() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    // Restaurar posición después de un prepend
    if (this.#isPrepending && this.#scrollHeightBeforePrepend > 0) {
      const added = el.scrollHeight - this.#scrollHeightBeforePrepend;
      if (added > 0) {
        el.scrollTop = added;
        this.#isPrepending = false;
        this.#scrollHeightBeforePrepend = 0;
      }
      return;
    }

    // Scroll al fondo: solo si los mensajes del DOM corresponden
    // a la conversación pendiente (evita scrollear con mensajes viejos)
    if (this.#pendingScrollConvId !== null) {
      const conv = this.state.selectedConversation();
      const msgs = this.state.selectedMessages();

      // Verificar que el DOM ya tiene los mensajes de ESTA conversación
      // (msgs.length > 0 y la conversación activa coincide con la pendiente)
      if (conv?.id === this.#pendingScrollConvId && msgs.length > 0 && el.scrollHeight > 100) {
        el.scrollTop = el.scrollHeight;
        this.#pendingScrollConvId = null;
      }
    }
  }

  ngOnDestroy() {}

  // ── Scroll infinito ──────────────────────────────────────────────

  onScroll() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;
    if (el.scrollTop > 60) return;

    const conv = this.state.selectedConversation();
    if (!conv) return;
    if (!this.state.selectedConversationHasMore()) return;
    if (this.state.loadingMoreMessages()) return;

    this.#cargarMasAntiguos(conv.id);
  }

  #cargarMasAntiguos(conversacionId: number) {
    const nextPage = this.state.selectedConversationPage() + 1;
    const el = this.scrollContainer?.nativeElement;

    this.#scrollHeightBeforePrepend = el?.scrollHeight ?? 0;
    this.#isPrepending = true;

    this.state.setLoadingMoreMessages(true);

    this.conversacionService.getMensajes(conversacionId, nextPage, PAGE_SIZE).subscribe({
      next: (mensajes) => {
        const hasMore = mensajes.length === PAGE_SIZE;
        this.state.prependMessages(conversacionId, mensajes, hasMore);
        this.state.setLoadingMoreMessages(false);
      },
      error: (err) => {
        console.error('Error cargando más mensajes', err);
        this.state.setLoadingMoreMessages(false);
        this.#isPrepending = false;
      }
    });
  }

  // ── Envío de mensajes ────────────────────────────────────────────

  sendMessage() {
    const conversation = this.state.selectedConversation();
    if (!conversation) return;

    if (this.imagenSeleccionada()) {
      this.conversacionService.subirImagen(this.imagenSeleccionada()!).subscribe({
        next: ({ mediaId, mediaUrl }) => {
          this.conversacionService.enviarMensaje({
            conversacionId: conversation.id,
            tipoMensaje: 'image',
            mediaId,
            mediaUrl: mediaUrl ?? undefined,
            caption: this.messageInput().trim() || undefined
          }).subscribe({
            next: () => {
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

    const text = this.messageInput().trim();
    if (!text) return;

    this.conversacionService.enviarMensaje({
      conversacionId: conversation.id,
      contenido: text,
      tipoMensaje: 'text'
    }).subscribe({
      next: () => this.messageInput.set(''),
      error: (err) => console.error('Error enviando mensaje', err)
    });
  }

  // ── Imagen ───────────────────────────────────────────────────────

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

  abrirImagen(url: string) { this.lightboxUrl.set(url); }
  cerrarLightbox() { this.lightboxUrl.set(null); }

  // ── Navegación mobile ────────────────────────────────────────────

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
}