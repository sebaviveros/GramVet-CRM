import type { Conversation } from "../types/chat";

export const mockConversations: Conversation[] = [
  {
    id: "1",
    name: "Mariela",
    channel: "whatsapp",
    messages: [
      {
        id: "m1",
        text: "Hola! Me gustaría saber más sobre el servicio.",
        fromMe: false,
        timestamp: new Date("2026-02-14T18:30:00")
      },
      {
        id: "m2",
        text: "Claro! ¿Es para consulta o vacunación?",
        fromMe: true,
        timestamp: new Date("2026-02-14T18:31:00")
      }
    ]
  },
  {
    id: "2",
    name: "Daisy",
    channel: "instagram",
    messages: [
      {
        id: "m3",
        text: "Consulta tiene una hora disponible mañana?",
        fromMe: false,
        timestamp: new Date("2026-02-14T19:10:00")
      }
    ]
  }
];
