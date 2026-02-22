import { Box, Typography } from "@mui/material";
import type { Conversation } from "../../types/chat";

interface Props {
  conversation: Conversation | null;
}

export default function ChatWindow({ conversation }: Props) {
  if (!conversation) {
    return (
      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: "#6b7280"
        }}
      >
        Selecciona una conversación
      </Box>
    );
  }

  return (
    <Box
      sx={{
        flexGrow: 1,
        display: "flex",
        flexDirection: "column",
        overflow: "hidden"
      }}
    >
      {/* Header del chat */}
      <Box
        sx={{
          p: 2,
          borderBottom: "1px solid #1f2937"
        }}
      >
        <Typography variant="h6" color="white">
          {conversation.name}
        </Typography>
      </Box>

      {/* Mensajes */}
      <Box
        sx={{
          flexGrow: 1,
          overflowY: "auto",
          p: 3,
          display: "flex",
          flexDirection: "column"
        }}
      >
        {conversation.messages.map((msg) => (
          <Box
            key={msg.id}
            sx={{
              display: "flex",
              justifyContent: msg.fromMe ? "flex-end" : "flex-start",
              mb: 1
            }}
          >
            <Box
              sx={{
                px: 2,
                py: 1,
                borderRadius: 2,
                backgroundColor: msg.fromMe ? "#2563eb" : "#374151",
                maxWidth: "60%"
              }}
            >
              <Typography variant="body2" color="white">
                {msg.text}
              </Typography>
            </Box>
          </Box>
        ))}
      </Box>
    </Box>
  );
}
