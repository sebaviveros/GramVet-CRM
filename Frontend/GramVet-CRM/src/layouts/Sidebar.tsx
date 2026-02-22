import { Box, Typography } from "@mui/material";

export default function Sidebar() {
  return (
    <Box
      sx={{
        width: 80,
        bgcolor: "#111827",
        borderRight: "1px solid #1f2937",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        pt: 2
      }}
    >
      <Typography variant="h6">M</Typography>
    </Box>
  );
}
