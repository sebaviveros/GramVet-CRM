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
  Injector,
  HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from '@coreui/angular';
import { InboxStateService } from '../../../../services/inbox/inbox-state.service';
import { ConversacionService } from '../../../../services/conversacion/conversacion.service';
import { EtiquetaService, EtiquetaDto } from '../../../../services/etiqueta/etiqueta.service';
import { RespuestaRapidaService, RespuestaRapidaDto } from '../../../../services/respuesta-rapida/respuesta-rapida.service';
import { ChannelBadgeComponent } from '../../../../shared/channel-badge/channel-badge.component';
import { colorEtiquetaTexto } from '../../../../shared/color.util';
import Swal from 'sweetalert2';

const PAGE_SIZE = 15;

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, CardModule, ChannelBadgeComponent],
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

  // Emojis del composer
  mostrarEmojis = signal(false);
  emojis = ['😀','😁','😂','🤣','😊','😍','😘','😎','🤗','🤔','😅','🙂','😉','😴','😢','😭','😡','😱','👍','👎','🙏','👏','🙌','💪','❤️','🧡','💛','💚','💙','💜','✅','❌','⚠️','📍','🎉','🔥','💉','🩺','🐶','🐱','🐾','🦴'];

  // Reacciones rápidas a un mensaje
  emojisReaccion = ['👍','❤️','😂','😮','😢','🙏'];
  reaccionAbiertaMensajeId = signal<number | null>(null);

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
  // El panel se abre apenas se escribe `/x`, aunque no haya coincidencias:
  // así se muestra el empty-state en vez de no pasar nada.
  mostrarSugerencias = computed(() => {
    const input = this.messageInput();
    return input.startsWith('/') && input.length >= 2;
  });

  /** Recorte para la lista de sugerencias: una línea, máximo 90 caracteres. */
  previewRespuesta(texto: string): string {
    const unaLinea = texto.replace(/\s+/g, ' ').trim();
    return unaLinea.length > 90 ? unaLinea.slice(0, 90).trimEnd() + '…' : unaLinea;
  }

  // Las reacciones por API solo están disponibles en WhatsApp
  esWhatsApp = computed(() =>
    (this.state.selectedConversation()?.canal ?? '').toLowerCase().includes('whatsapp'));

  // ── Ventana de atención de WhatsApp (24h) ────────────────────────
  // WhatsApp solo deja mandar texto libre dentro de las 24h desde el último
  // mensaje DEL CLIENTE. Fuera de eso, Meta rechaza el mensaje. Acá se detecta
  // para avisar y bloquear el composer (solo WhatsApp; otros canales sin cambio).

  // Reloj que avanza cada 60s para que la ventana se recalcule al pasar el tiempo.
  #ahora = signal(Date.now());
  #relojId = setInterval(() => this.#ahora.set(Date.now()), 60_000);

  /** Momento en que expira la ventana (último inbound conocido + 24h), o null. */
  ventanaExpiraEn = computed<number | null>(() => {
    // Del backend: cubre el caso en que el último inbound no está en la página cargada.
    const hint = this.state.selectedConversation()?.ventanaExpiraEn;
    let expira = hint ? new Date(hint).getTime() : null;

    // De los mensajes cargados: cubre lo que llega en tiempo real por SignalR.
    for (const m of this.state.selectedMessages()) {
      if (m.direccion !== 'inbound') continue;
      const t = new Date(m.fechaEnvio).getTime() + 24 * 60 * 60 * 1000;
      if (expira === null || t > expira) expira = t;
    }
    return expira;
  });

  /** La ventana solo aplica a WhatsApp. Cerrada = no se puede mandar texto libre. */
  ventanaCerrada = computed(() => {
    if (!this.esWhatsApp()) return false;      // otros canales: sin restricción
    const expira = this.ventanaExpiraEn();
    if (expira === null) return true;          // el cliente nunca escribió
    return this.#ahora() >= expira;
  });

  /** Aviso discreto cuando la ventana está por cerrarse (menos de 2h). */
  ventanaPorCerrar = computed(() => {
    if (this.ventanaCerrada() || !this.esWhatsApp()) return null;
    const expira = this.ventanaExpiraEn();
    if (expira === null) return null;
    const restanteMin = Math.floor((expira - this.#ahora()) / 60000);
    if (restanteMin > 120) return null;        // solo avisar en las últimas 2h
    const h = Math.floor(restanteMin / 60);
    const m = restanteMin % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  });


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

  ngOnDestroy() {
    clearInterval(this.#relojId);
  }

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
    this.#programarAjusteComposer();
  }

  // ── Composer (textarea con auto-resize) ──────────────────────────

  @ViewChild('composer')
  composerRef?: ElementRef<HTMLTextAreaElement>;

  /** Alto máximo del composer antes de que scrollee por dentro (px). */
  readonly #maxAltoComposer = 140;

  onComposerInput(event: Event) {
    this.messageInput.set((event.target as HTMLTextAreaElement).value);
    // Al tipear el textarea ya tiene el valor nuevo: se puede medir en el acto.
    this.#ajustarAltoComposer();
  }

  /**
   * Cuando el texto cambia por código (respuesta rápida, emoji, limpiar al
   * enviar), el binding `[value]` recién se aplica en el próximo ciclo de
   * detección de cambios. Medir antes leería el valor VIEJO: por eso el
   * composer no crecía al elegir una respuesta larga y quedaba grande al enviar.
   */
  #programarAjusteComposer() {
    setTimeout(() => this.#ajustarAltoComposer());
  }

  /** Enter envía. Shift+Enter inserta un salto de línea. */
  onComposerEnter(event: Event) {
    const e = event as KeyboardEvent;
    if (e.shiftKey) return;
    e.preventDefault();
    this.sendMessage();
  }

  #ajustarAltoComposer() {
    const el = this.composerRef?.nativeElement;
    if (!el) return;
    // Se resetea antes de medir: si no, scrollHeight nunca decrece al borrar texto.
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, this.#maxAltoComposer) + 'px';
  }

  // ── Emojis ───────────────────────────────────────────────────────

  toggleEmojis() {
    this.mostrarEmojis.update(v => !v);
  }

  agregarEmoji(emoji: string) {
    // No se cierra al elegir (como WhatsApp): se pueden agregar varios
    this.messageInput.update(t => t + emoji);
    this.#programarAjusteComposer();
  }

  // Cierra el panel de emojis al hacer click fuera de él.
  // Los clicks en el botón y dentro del panel hacen stopPropagation, así que no llegan acá.
  @HostListener('document:click')
  cerrarEmojisAfuera() {
    if (this.mostrarEmojis()) this.mostrarEmojis.set(false);
  }

  // Símbolo de "visto" según el estado de entrega
  tickSymbol(estado?: string): string {
    switch (estado) {
      case 'read':
      case 'delivered': return '✓✓';
      case 'sent': return '✓';
      case 'failed': return '⚠';
      default: return '';
    }
  }

  /** Color legible del texto del chip de etiqueta (aclara los colores oscuros). */
  colorTexto = (color?: string | null) => colorEtiquetaTexto(color);

  // ── Separadores de fecha (estilo WhatsApp) ───────────────────────

  /** Medianoche local de una fecha, para comparar días sin que influya la hora. */
  #inicioDelDia(fecha: string | Date): Date {
    const d = new Date(fecha);
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  }

  /** True si el mensaje en `i` abre un día distinto al del mensaje anterior. */
  esInicioDeDia(i: number): boolean {
    const mensajes = this.state.selectedMessages();
    if (i === 0) return true;
    const actual = this.#inicioDelDia(mensajes[i].fechaEnvio);
    const previo = this.#inicioDelDia(mensajes[i - 1].fechaEnvio);
    return actual.getTime() !== previo.getTime();
  }

  /**
   * "Hoy" / "Ayer" / nombre del día (hasta 6 días atrás) / fecha d/m/aaaa.
   * A partir de una semana el nombre del día deja de ser útil porque se repite.
   */
  etiquetaDia(fecha: string | Date): string {
    const dia = this.#inicioDelDia(fecha);
    const hoy = this.#inicioDelDia(new Date());
    const diffDias = Math.round((hoy.getTime() - dia.getTime()) / 86_400_000);

    if (diffDias === 0) return 'Hoy';
    if (diffDias === 1) return 'Ayer';
    if (diffDias > 1 && diffDias < 7) {
      const nombre = dia.toLocaleDateString('es-CL', { weekday: 'long' });
      return nombre.charAt(0).toUpperCase() + nombre.slice(1);
    }
    return dia.toLocaleDateString('es-CL');
  }

  // ── Reacciones ───────────────────────────────────────────────────

  toggleReaccionPicker(mensajeId: number) {
    this.reaccionAbiertaMensajeId.update(id => id === mensajeId ? null : mensajeId);
  }

  reaccionar(mensajeId: number, emoji: string) {
    this.reaccionAbiertaMensajeId.set(null);
    this.conversacionService.reaccionar(mensajeId, emoji).subscribe({
      next: () => { /* la UI se actualiza por SignalR (MensajeReaccionado) */ },
      error: (err) => {
        console.error('Error reaccionando', err);
        Swal.fire({
          icon: 'warning',
          title: 'No se pudo reaccionar',
          text: err?.error?.mensaje ?? ''
        });
      }
    });
  }

  // ── Envío de mensajes ────────────────────────────────────────────

  sendMessage() {
    const conversation = this.state.selectedConversation();
    if (!conversation) return;

    // Evita envíos duplicados: si ya hay un envío en curso, ignorar
    if (this.enviando()) return;

    // Red de seguridad: fuera de la ventana de 24h WhatsApp rechaza el mensaje.
    // El composer ya está deshabilitado, pero esto cubre atajos de teclado.
    if (this.ventanaCerrada()) return;

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
              this.#programarAjusteComposer();
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
        this.#programarAjusteComposer();
        this.enviando.set(false);
      },
      error: (err) => {
        console.error('Error enviando mensaje', err);
        this.enviando.set(false);
      }
    });
  }

  // ── Ubicación ────────────────────────────────────────────────────

  enviarUbicacion() {
    const conversation = this.state.selectedConversation();
    if (!conversation) return;
    if (this.enviando()) return;

    if (!navigator.geolocation) {
      Swal.fire({ icon: 'error', title: 'Tu navegador no soporta geolocalización' });
      return;
    }

    this.enviando.set(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.conversacionService.enviarMensaje({
          conversacionId: conversation.id,
          tipoMensaje: 'location',
          latitud: pos.coords.latitude,
          longitud: pos.coords.longitude
        }).subscribe({
          next: () => this.enviando.set(false),
          error: (err) => {
            console.error('Error enviando ubicación', err);
            this.enviando.set(false);
          }
        });
      },
      (err) => {
        console.error('Error obteniendo ubicación', err);
        this.enviando.set(false);
        Swal.fire({
          icon: 'warning',
          title: 'No se pudo obtener tu ubicación',
          text: 'Revisa que el navegador tenga permiso de ubicación.'
        });
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
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
    // Solo teléfono (<768px): navega a la vista de contacto de una columna.
    // Tablet (≥768px) y escritorio usan el mismo layout de 2 columnas, así que
    // alternan (pliegan/despliegan) el panel de contacto como en escritorio.
    if (window.innerWidth < 768) {
      this.state.openRightPanel('contact');
      this.state.setMobileView('contact');
      return;
    }
    // Tablet + escritorio: alterna (pliega/despliega) el panel de contacto
    this.state.setRightPanel('contact');
  }
}
