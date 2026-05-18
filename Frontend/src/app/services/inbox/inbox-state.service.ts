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
  canal: string;
}

export interface Message {
  id: number;
  conversacionId: number;
  contenido?: string;
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

  // justo después de los signals, antes de los computed
  vets = ['Dr. Soto', 'Dra. Pérez', 'Dr. González'];

  // COMPUTED
  conversations = computed(() => this._conversations());
  mobileView = computed(() => this._mobileView());
  activeRightPanel = computed(() => this._activeRightPanel());
  loadingConversations = computed(() => this._loadingConversations());
  loadingMessages = computed(() => this._loadingMessages());

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

  setAssignedVet(conversacionId: number, vet: string) {
    this._conversations.update(convs =>
      convs.map(c =>
        c.id === conversacionId
          ? { ...c, usuarioAsignado: vet }
          : c
      )
    );
}

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

  selectConversation(conversationId: number) {
    this._selectedConversationId.set(conversationId);
  }

  setMessages(conversacionId: number, messages: Message[]) {
    this._messages.update(current => ({
      ...current,
      [conversacionId]: messages
    }));
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
}