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

  // state panel derecho

  private _activeRightPanel =
    signal<'contact' | 'calls' | 'notes' | null>('contact');


  // computed


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


  // panel derecho computed

  activeRightPanel = computed(() => this._activeRightPanel());

  isRightPanelOpen = computed(() =>
    this._activeRightPanel() !== null
  );

  setMobileView(view: 'inbox' | 'conversations' | 'chat') {

    this._mobileView.set(view);

  }

  // actions conversations

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


  // actions panel derecho

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

  // mock data


  loadMockData() {

    const conversations: Conversation[] = [

      {
        id: 1,
        clientId: 1,
        clientName: 'Juan Pérez',
        phone: '+56912345678',
        petName: 'Michi'
      },

      {
        id: 2,
        clientId: 2,
        clientName: 'María González',
        phone: '+56987654321',
        petName: 'Luna'
      },

      {
        id: 3,
        clientId: 3,
        clientName: 'Pedro Ramírez',
        phone: '+56911112222',
        petName: 'Rocky'
      },

      {
        id: 4,
        clientId: 4,
        clientName: 'Camila Torres',
        phone: '+56933334444',
        petName: 'Simba'
      },

      {
        id: 5,
        clientId: 5,
        clientName: 'Diego Soto',
        phone: '+56955556666',
        petName: 'Nala'
      },

      {
        id: 6,
        clientId: 6,
        clientName: 'Valentina Cruz',
        phone: '+56977778888',
        petName: 'Toby'
      },

      {
        id: 7,
        clientId: 7,
        clientName: 'Ricardo Fuentes',
        phone: '+56922223333',
        petName: 'Bobby'
      },

      {
        id: 8,
        clientId: 8,
        clientName: 'Daniela Vega',
        phone: '+56944445555',
        petName: 'Lola'
      },

      {
        id: 9,
        clientId: 9,
        clientName: 'Tomás Herrera',
        phone: '+56966667777',
        petName: 'Thor'
      },

      {
        id: 10,
        clientId: 10,
        clientName: 'Paula Díaz',
        phone: '+56999990000',
        petName: 'Coco'
      },

      {
        id: 11,
        clientId: 11,
        clientName: 'Luis Martínez',
        phone: '+56912121212',
        petName: 'Max'
      },

      {
        id: 12,
        clientId: 12,
        clientName: 'Andrea Castillo',
        phone: '+56934343434',
        petName: 'Kira'
      },

      {
        id: 13,
        clientId: 13,
        clientName: 'Felipe Soto',
        phone: '+56945454545',
        petName: 'Bruno'
      },

      {
        id: 14,
        clientId: 14,
        clientName: 'Carolina Vega',
        phone: '+56956565656',
        petName: 'Luna'
      },

      {
        id: 15,
        clientId: 15,
        clientName: 'Héctor Díaz',
        phone: '+56967676767',
        petName: 'Bobby'
      },

      {
        id: 16,
        clientId: 16,
        clientName: 'Patricia López',
        phone: '+56978787878',
        petName: 'Nina'
      },

      {
        id: 17,
        clientId: 17,
        clientName: 'Marco Silva',
        phone: '+56989898989',
        petName: 'Canelo'
      },

      {
        id: 18,
        clientId: 18,
        clientName: 'Claudia Rojas',
        phone: '+56923232323',
        petName: 'Pelusa'
      },

      {
        id: 19,
        clientId: 19,
        clientName: 'Javier Morales',
        phone: '+56945454512',
        petName: 'Zeus'
      },

      {
        id: 20,
        clientId: 20,
        clientName: 'Natalia Paredes',
        phone: '+56987871234',
        petName: 'Maya'
      }

    ];


    this.setConversations(conversations);

    // ================= MENSAJES MOCK =================

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
        text: 'Hola ¿en qué podemos ayudarte?',
        timestamp: new Date(),
        sender: 'agent'
      },
      {
        id: 3,
        conversationId: 1,
        text: 'Quiero vacunar a mi gato',
        timestamp: new Date(),
        sender: 'client'
      }
    ]);


    this.setMessages(2, [
      {
        id: 1,
        conversationId: 2,
        text: 'Gracias por la atención',
        timestamp: new Date(),
        sender: 'client'
      }
    ]);


    this.setMessages(3, [
      {
        id: 1,
        conversationId: 3,
        text: '¿Atienden hoy?',
        timestamp: new Date(),
        sender: 'client'
      }
    ]);

  }

}