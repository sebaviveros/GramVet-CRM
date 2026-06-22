import {
  Component,
  inject,
  signal,
  computed,
  ViewChild,
  ElementRef,
  effect,
  AfterViewChecked,
  OnDestroy,
  afterNextRender,
  Injector
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';
import { RespuestaRapidaService, RespuestaRapidaDto } from '../../../../services/respuesta-rapida/respuesta-rapida.service';

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
  etiquetaService = inject(EtiquetaService);
  respuestaRapidaService = inject(RespuestaRapidaService);
  #injector = inject(Injector);

  messageInput = signal('');
  enviando = signal(false);
  imagenSeleccionada = signal<File | null>(null);
  imagenPreview = signal<string | null>(null);
  lightboxUrl = signal<string | null>(null);
  etiquetasContacto = signal<EtiquetaDto[]>([]);

  // Respuestas rápidas
  todasLasRespuestas = signal<RespuestaRapidaDto[]>([]);
  sugerencias = computed(() => {
    const input = this.messageInput();
    if (!input.startsWith('/') || input.length < 2) return [];
    const filtro = input.slice(1).toLowerCase();
    return this.todasLasRespuestas().filter(r =>
      r.comando.toLowerCase().includes(filtro)
    );
  });
  mostrarSugerencias = computed(() => this.sugerencias().length > 0);

  @ViewChild('scrollContainer')
  scrollContainer?: ElementRef<HTMLDivElement>;

  #scrollHeightBeforePrepend = 0;
  #isPrepending = false;

  constructor() {
    // Cargar etiquetas del contacto cuando cambia la conversación seleccionada
    effect(() => {
      const conv = this.state.selectedConversation();
      if (conv) {
        this.etiquetaService.getByContacto(conv.contactoId).subscribe(e => this.etiquetasContacto.set(e));
      } else {
        this.etiquetasContacto.set([]);
      }
    });

    // Scroll al fondo
    effect(() => {
      const counter = this.state.scrollToBottomCounter();
      if (counter === 0) return;

      afterNextRender(() => {
        const el = this.scrollContainer?.nativeElement;
        if (el) {
          el.scrollTop = el.scrollHeight;
        }
      }, { injector: this.#injector });
    });

    // Cargar respuestas rápidas una vez al iniciar
    this.respuestaRapidaService.getAll().subscribe(r => this.todasLasRespuestas.set(r));
  }

  ngAfterViewChecked() {
    const el = this.scrollContainer?.nativeElement;
    if (!el) return;

    if (this.#isPrepending && this.#scrollHeightBeforePrepend > 0) {
      const added = el.scrollHeight - this.#scrollHeightBeforePrepend;
      if (added > 0) {
        el.scrollTop = added;
        this.#isPrepending = false;
        this.#scrollHeightBeforePrepend = 0;
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

  // ── Respuestas rápidas ───────────────────────────────────────────

  seleccionarRespuesta(r: RespuestaRapidaDto) {
    this.messageInput.set(r.texto);
  }

  // ── Envío de mensajes ────────────────────────────────────────────

  sendMessage() {
    const conversation = this.state.selectedConversation();
    if (!conversation) return;

    // Evita envíos duplicados: si ya hay un envío en curso, ignorar
    if (this.enviando()) return;

    if (this.imagenSeleccionada()) {
      this.enviando.set(true);
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
              this.enviando.set(false);
            },
            error: (err) => {
              console.error('Error enviando imagen', err);
              this.enviando.set(false);
            }
          });
        },
        error: (err) => {
          console.error('Error subiendo imagen', err);
          this.enviando.set(false);
        }
      });
      return;
    }

    const text = this.messageInput().trim();
    if (!text) return;

    this.enviando.set(true);
    this.conversacionService.enviarMensaje({
      conversacionId: conversation.id,
      contenido: text,
      tipoMensaje: 'text'
    }).subscribe({
      next: () => {
        this.messageInput.set('');
        this.enviando.set(false);
      },
      error: (err) => {
        console.error('Error enviando mensaje', err);
        this.enviando.set(false);
      }
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
