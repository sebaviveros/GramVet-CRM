import { Box } from "@mui/material";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";
import { Outlet } from "react-router-dom";

export default function MainLayout() {
  return (
    <Box
      sx={{
        display: "flex",
        height: "100vh",
        width: "100vw",
        overflow: "hidden"
      }}
    >
      <Sidebar />

      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          minWidth: 0
        }}
      >
        {/* Topbar fija */}
        <Topbar />

        {/* Área contenido */}
        <Box
          sx={{
            flexGrow: 1,
            overflow: "hidden",
            display: "flex",
            bgcolor: "#0f172a"
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
