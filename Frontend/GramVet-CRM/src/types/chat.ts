export interface Message {
  id: string;
  text: string;
  fromMe: boolean;
  timestamp: Date;  
}

export interface Conversation {
  id: string;
  name: string;
  channel: "whatsapp" | "instagram" | "messenger";
  messages: Message[];
}
