import { Injectable, signal, computed } from '@angular/core';


// MODELOS

export interface Conversation {

  id: number;

  clientId: number;

  clientName: string;

  phone: string;

  petName?: string;

  lastMessage?: string;

  lastMessageDate?: Date;

  unreadCount?: number;

}

export interface Message {

  id: number;

  conversationId: number;

  text: string;

  timestamp: Date;

  sender: 'client' | 'agent';

}


// SERVICE

@Injectable({
  providedIn: 'root'
})
export class InboxStateService {


  // STATE PRINCIPAL

  private _conversations = signal<Conversation[]>([]);

  private _selectedConversationId = signal<number | null>(null);

  private _messages = signal<Record<number, Message[]>>({});

  private _mobileView =
  signal<'inbox' | 'conversations' | 'chat'>('chat');
  mobileView = computed(() => this._mobileView());

  // STATE PANEL DERECHO 

  private _activeRightPanel =
    signal<'contact' | 'calls' | 'notes' | null>('contact');


  // COMPUTED


  conversations = computed(() => this._conversations());

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


  // PANEL DERECHO COMPUTED

  activeRightPanel = computed(() => this._activeRightPanel());

  isRightPanelOpen = computed(() =>
    this._activeRightPanel() !== null
  );

  setMobileView(view: 'inbox' | 'conversations' | 'chat') {

  this._mobileView.set(view);

}

  // ACTIONS CONVERSACIONES

  setConversations(conversations: Conversation[]) {

    this._conversations.set(conversations);

  }

  selectConversation(conversationId: number) {

    this._selectedConversationId.set(conversationId);

  }

  setMessages(conversationId: number, messages: Message[]) {

    this._messages.update(current => ({

      ...current,

      [conversationId]: messages

    }));

  }

  addMessage(message: Message) {

    this._messages.update(current => {

      const existing =
        current[message.conversationId] ?? [];

      return {

        ...current,

        [message.conversationId]:
          [...existing, message]

      };

    });

  }


  // ACTIONS PANEL DERECHO

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

  // MOCK DATA


  loadMockData() {

    const conversations: Conversation[] = [

      {
        id: 1,
        clientId: 1,
        clientName: 'Juan Pérez',
        phone: '+56912345678',
        petName: 'Michi',
        lastMessage: 'Hola',
        lastMessageDate: new Date(),
        unreadCount: 0
      },

      {
        id: 2,
        clientId: 2,
        clientName: 'María González',
        phone: '+56987654321',
        petName: 'Luna',
        lastMessage: 'Gracias',
        lastMessageDate: new Date(),
        unreadCount: 2
      }

    ];

    this.setConversations(conversations);

    this.setMessages(1, [

      {
        id: 1,
        conversationId: 1,
        text: 'Hola',
        timestamp: new Date(),
        sender: 'client'
      },

      {
        id: 2,
        conversationId: 1,
        text: 'Hola, ¿en qué podemos ayudarte?',
        timestamp: new Date(),
        sender: 'agent'
      }

    ]);

  }

}