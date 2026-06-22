import { Injectable, signal, computed } from '@angular/core';

export interface Conversation {
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
  etiquetas?: { id: number; nombre: string; color?: string }[];
}

export interface Message {
  id: number;
  conversacionId: number;
  contenido?: string;
  mediaUrl?: string;
  tipoMensaje?: string;
  direccion: string; // 'inbound' | 'outbound'
  fechaEnvio: Date;
  usuarioId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class InboxStateService {

  // STATE
  private _conversations = signal<Conversation[]>([]);
  private _selectedConversationId = signal<number | null>(null);
  private _messages = signal<Record<number, Message[]>>({});
  private _mobileView = signal<'conversations' | 'chat' | 'contact'>('conversations');
  private _activeRightPanel = signal<'contact' | 'calls' | 'notes' | null>('contact');
  private _loadingConversations = signal<boolean>(false);
  private _loadingMessages = signal<boolean>(false);
  private _loadingMoreMessages = signal<boolean>(false);

  // Página actual por conversación — para saber qué página pedir en el scroll
  private _messagePage = signal<Record<number, number>>({});
  // Si ya no hay más páginas para una conversación
  private _noMoreMessages = signal<Record<number, boolean>>({});

  // Trigger para scroll al fondo: emite un contador que se incrementa cada vez
  // que hay que scrollear (permite disparar aunque el convId sea el mismo)
  private _scrollToBottomCounter = signal<number>(0);

  // COMPUTED
  conversations = computed(() => this._conversations());
  mobileView = computed(() => this._mobileView());
  activeRightPanel = computed(() => this._activeRightPanel());
  loadingConversations = computed(() => this._loadingConversations());
  loadingMessages = computed(() => this._loadingMessages());
  loadingMoreMessages = computed(() => this._loadingMoreMessages());
  scrollToBottomCounter = computed(() => this._scrollToBottomCounter());

  isRightPanelOpen = computed(() => this._activeRightPanel() !== null);

  selectedConversation = computed(() => {
    const id = this._selectedConversationId();
    if (!id) return null;
    return this._conversations().find(c => c.id === id) ?? null;
  });

  selectedMessages = computed(() => {
    const id = this._selectedConversationId();
    if (!id) return [];
    return this._messages()[id] ?? [];
  });

  selectedConversationPage = computed(() => {
    const id = this._selectedConversationId();
    if (!id) return 1;
    return this._messagePage()[id] ?? 1;
  });

  selectedConversationHasMore = computed(() => {
    const id = this._selectedConversationId();
    if (!id) return false;
    return !this._noMoreMessages()[id];
  });

  // ACTIONS
  setConversations(conversations: Conversation[]) {
    this._conversations.set(conversations);
  }

  setLoadingConversations(value: boolean) {
    this._loadingConversations.set(value);
  }

  setLoadingMessages(value: boolean) {
    this._loadingMessages.set(value);
  }

  setLoadingMoreMessages(value: boolean) {
    this._loadingMoreMessages.set(value);
  }

  selectConversation(conversationId: number) {
    this._selectedConversationId.set(conversationId);
  }

  setMessages(conversacionId: number, messages: Message[]) {
    this._messages.update(current => ({
      ...current,
      [conversacionId]: messages
    }));
    // Resetear página y flag de más mensajes al cargar la primera página
    this._messagePage.update(p => ({ ...p, [conversacionId]: 1 }));
    this._noMoreMessages.update(n => ({ ...n, [conversacionId]: false }));
  }

  // Agrega mensajes al INICIO (scroll infinito hacia arriba)
  prependMessages(conversacionId: number, messages: Message[], hasMore: boolean) {
    this._messages.update(current => {
      const existing = current[conversacionId] ?? [];
      return {
        ...current,
        [conversacionId]: [...messages, ...existing]
      };
    });

    const currentPage = this._messagePage()[conversacionId] ?? 1;
    this._messagePage.update(p => ({ ...p, [conversacionId]: currentPage + 1 }));

    if (!hasMore) {
      this._noMoreMessages.update(n => ({ ...n, [conversacionId]: true }));
    }
  }

  addMessage(message: Message) {
    this._messages.update(current => {
      const existing = current[message.conversacionId] ?? [];
      return {
        ...current,
        [message.conversacionId]: [...existing, message]
      };
    });
  }

  // Dispara scroll al fondo en el chat-window activo
  triggerScrollToBottom() {
    this._scrollToBottomCounter.update(n => n + 1);
  }

  setMobileView(view: 'conversations' | 'chat' | 'contact') {
    this._mobileView.set(view);
  }

  setRightPanel(panel: 'contact' | 'calls' | 'notes') {
    if (this._activeRightPanel() === panel) {
      this._activeRightPanel.set(null);
    } else {
      this._activeRightPanel.set(panel);
    }
  }

  openRightPanel(panel: 'contact' | 'calls' | 'notes') {
    this._activeRightPanel.set(panel);
  }

  closeRightPanel() {
    this._activeRightPanel.set(null);
  }

  upsertConversacion(conversacion: Conversation) {
    this._conversations.update(convs => {
      const index = convs.findIndex(c => c.id === conversacion.id);
      if (index >= 0) {
        const updated = [...convs];
        // Preservar etiquetas existentes si el evento de SignalR no las trae
        // (las emisiones en tiempo real no incluyen etiquetas)
        if (!conversacion.etiquetas?.length && convs[index].etiquetas?.length) {
          conversacion = { ...conversacion, etiquetas: convs[index].etiquetas };
        }
        updated[index] = conversacion;
        return updated.sort((a, b) => {
          const dateA = a.fechaUltimoMensaje ? new Date(a.fechaUltimoMensaje).getTime() : 0;
          const dateB = b.fechaUltimoMensaje ? new Date(b.fechaUltimoMensaje).getTime() : 0;
          return dateB - dateA;
        });
      } else {
        return [conversacion, ...convs];
      }
    });
  }

  resetearNoLeidos(conversacionId: number) {
    this._conversations.update(convs =>
      convs.map(c =>
        c.id === conversacionId
          ? { ...c, cantidadNoLeidos: 0 }
          : c
      )
    );
  }

  removeConversacion(conversacionId: number) {
    this._conversations.update(convs => convs.filter(c => c.id !== conversacionId));
    // Si era la conversación seleccionada, deseleccionarla
    if (this._selectedConversationId() === conversacionId) {
      this._selectedConversationId.set(null);
    }
  }

  setAssignedVet(conversacionId: number, usuarioAsignadoId: number | null, usuarioAsignado?: string) {
    this._conversations.update(convs =>
      convs.map(c =>
        c.id === conversacionId
          ? { ...c, usuarioAsignadoId, usuarioAsignado }
          : c
      )
    );
  }
}