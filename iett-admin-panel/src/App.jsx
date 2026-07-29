import { useState } from "react";
import "./App.css";

import Sidebar from "./components/layout/Sidebar";
import VehiclesPage from "./pages/VehiclesPage";
import DriversPage from "./pages/DriversPage";
import BusRoutesPage from "./pages/BusRoutesPage";
import BusStopsPage from "./pages/BusStopsPage";

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const validPages = [
    "vehicles",
    "drivers",
    "busRoutes",
    "busStops",
  ];

  const [activePage, setActivePage] = useState(() => {
    const savedPage = localStorage.getItem("activePage");

    return validPages.includes(savedPage)
      ? savedPage
      : "vehicles";
  });

  const [selectedRoute, setSelectedRoute] = useState(() => {
    const savedRoute = localStorage.getItem("selectedRoute");

    if (!savedRoute) {
      return null;
    }

    try {
      return JSON.parse(savedRoute);
    } catch {
      return null;
    }
  });

  function handlePageChange(page) {
    setActivePage(page);
    localStorage.setItem("activePage", page);
  }

  function handleSelectRoute(route) {
    setSelectedRoute(route);

    localStorage.setItem(
      "selectedRoute",
      JSON.stringify(route),
    );

    handlePageChange("busStops");
  }

  function handleBackToRoutes() {
    handlePageChange("busRoutes");
  }

  return (
    <div
      className={`admin-layout ${
        sidebarOpen ? "" : "sidebar-closed"
      }`}
    >
      <Sidebar
        sidebarOpen={sidebarOpen}
        activePage={activePage}
        setActivePage={handlePageChange}
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

        {activePage === "vehicles" && (
          <VehiclesPage />
        )}

        {activePage === "drivers" && (
          <DriversPage />
        )}

        {activePage === "busRoutes" && (
          <BusRoutesPage
            onSelectRoute={handleSelectRoute}
          />
        )}

        {activePage === "busStops" && (
          <BusStopsPage
            selectedRoute={selectedRoute}
            onBack={handleBackToRoutes}
          />
        )}
      </main>
    </div>
  );
}

export default App;