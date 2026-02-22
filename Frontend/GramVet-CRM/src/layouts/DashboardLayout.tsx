import { Box, Toolbar } from "@mui/material";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

interface Props {
  children: React.ReactNode;
}

export default function DashboardLayout({ children }: Props) {
  return (
    <Box sx={{ display: "flex" }}>
      <Sidebar />
      <Topbar />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          ml: "80px",
          backgroundColor: "#111827",
          minHeight: "100vh",
          color: "white",
        }}
      >
        <Toolbar />
        {children}
      </Box>
    </Box>
  );
}
