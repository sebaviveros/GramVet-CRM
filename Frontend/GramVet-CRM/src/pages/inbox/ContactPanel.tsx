import { Box, Typography, Divider } from "@mui/material";
import type { Conversation } from "../../types/chat";

interface Props {
  conversation: Conversation | null;
}

export default function ContactPanel({ conversation }: Props) {
  if (!conversation) {
    return (
      <Box
        sx={{
          p: 2,
          color: "#6b7280"
        }}
      >
        Sin contacto seleccionado
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h6" color="white" sx={{ mb: 2 }}>
        Información de contacto
      </Typography>

      <Divider sx={{ mb: 2, borderColor: "#1f2937" }} />

      <Typography sx={{ color: "#d1d5db", mb: 1 }}>
        <strong>Nombre:</strong> {conversation.name}
      </Typography>

      <Typography sx={{ color: "#d1d5db" }}>
        <strong>Canal:</strong> {conversation.channel}
      </Typography>
    </Box>
  );
}
