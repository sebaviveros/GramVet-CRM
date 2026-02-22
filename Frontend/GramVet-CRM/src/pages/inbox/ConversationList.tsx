import { Box, Typography } from "@mui/material";
import type { Conversation } from "../../types/chat";

interface Props {
  conversations: Conversation[];
  activeId?: string;
  onSelect: (conversation: Conversation) => void;
}

export default function ConversationList({
  conversations,
  activeId,
  onSelect
}: Props) {
  return (
    <Box
      sx={{
        flexGrow: 1,
        overflowY: "auto"
      }}
    >
      {conversations.map((conv) => {
        const lastMessage =
          conv.messages[conv.messages.length - 1]?.text ?? "";

        const isActive = activeId === conv.id;

        return (
          <Box
            key={conv.id}
            onClick={() => onSelect(conv)}
            sx={{
              p: 2,
              cursor: "pointer",
              borderBottom: "1px solid #1f2937",
              backgroundColor: isActive ? "#1e293b" : "transparent",
              transition: "0.2s",
              "&:hover": {
                backgroundColor: "#111827"
              }
            }}
          >
            <Typography variant="subtitle1" color="white">
              {conv.name}
            </Typography>

            <Typography
              variant="body2"
              sx={{
                color: "#9ca3af",
                whiteSpace: "nowrap",
                overflow: "hidden",
                textOverflow: "ellipsis"
              }}
            >
              {lastMessage}
            </Typography>
          </Box>
        );
      })}
    </Box>
  );
}
