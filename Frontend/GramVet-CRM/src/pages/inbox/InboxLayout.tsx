import { Box } from "@mui/material";
import ConversationList from "./ConversationList";
import ChatWindow from "./ChatWindow";
import ContactPanel from "./ContactPanel";
import { useState } from "react";
import type { Conversation } from "../../types/chat";

export default function InboxLayout() {
  const [conversations] = useState<Conversation[]>([
    {
      id: "1",
      name: "Mariela",
      channel: "whatsapp",
      messages: [
        {
          id: "m1",
          text: "Hola! Me gustaría saber más.",
          fromMe: false,
          timestamp: new Date()
        },
        {
          id: "m2",
          text: "Claro! ¿Consulta o vacunación?",
          fromMe: true,
          timestamp: new Date()
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
          text: "Tienen hora mañana?",
          fromMe: false,
          timestamp: new Date()
        }
      ]
    }
  ]);

  const [activeConversation, setActiveConversation] =
    useState<Conversation | null>(conversations[0]);

  return (
    <Box
      sx={{
        display: "flex",
        flexGrow: 1,
        height: "100%",
        overflow: "hidden"
      }}
    >
      {/* Lista */}
      <Box
        sx={{
          width: 350,
          borderRight: "1px solid #1f2937",
          overflowY: "auto"
        }}
      >
        <ConversationList
          conversations={conversations}
          activeId={activeConversation?.id}
          onSelect={setActiveConversation}
        />
      </Box>

      {/* Chat */}
      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          overflow: "hidden"
        }}
      >
        <ChatWindow conversation={activeConversation} />
      </Box>

      {/* Panel */}
      <Box
        sx={{
          width: 300,
          borderLeft: "1px solid #1f2937",
          overflowY: "auto"
        }}
      >
        <ContactPanel conversation={activeConversation} />
      </Box>
    </Box>
  );
}
