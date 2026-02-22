import { Box, Typography } from "@mui/material";

export default function Topbar() {
  return (
    <Box
      sx={{
        height: 64,
        flexShrink: 0,
        bgcolor: "#111827",
        display: "flex",
        alignItems: "center",
        px: 3,
        borderBottom: "1px solid #1f2937"
      }}
    >
      <Typography variant="h6">
        Buzón
      </Typography>
    </Box>
  );
}
