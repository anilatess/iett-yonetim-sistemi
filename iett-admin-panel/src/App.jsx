import { useState } from "react";
import "./App.css";

import Sidebar from "./components/layout/Sidebar";
import DriversPage from "./pages/DriversPage";
import VehiclesPage from "./pages/VehiclesPage";

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [activePage, setActivePage] = useState("vehicles");

  return (
    <div
      className={`admin-layout ${
        sidebarOpen ? "" : "sidebar-closed"
      }`}
    >
      <Sidebar
        sidebarOpen={sidebarOpen}
        activePage={activePage}
        setActivePage={setActivePage}
      />

      <main className="content">
        <button
          type="button"
          className="sidebar-toggle"
          onClick={() =>
            setSidebarOpen((current) => !current)
          }
        >
          {sidebarOpen ? "←" : "→"}
        </button>

        {activePage === "vehicles" && <VehiclesPage />}

        {activePage === "drivers" && <DriversPage />}
      </main>
    </div>
  );
}

export default App;