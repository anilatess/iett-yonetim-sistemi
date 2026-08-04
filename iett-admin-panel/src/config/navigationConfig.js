export const navigationByRole = {
  Admin: {
    title: "İETT Admin Paneli",
    startPage: "adminDashboard",
    menuItems: [
      ["adminDashboard", "🏠", "Dashboard"],
      ["vehicles", "🚌", "Araçlar"],
      ["drivers", "👤", "Şoförler"],
      ["busRoutes", "🛣️", "Hatlar"],
      ["busStops", "📍", "Duraklar"],
      ["trips", "🗓️", "Seferler"],
      ["taskAssignment", "📌", "Görev Atama"],
      ["complaints", "💬", "Şikâyetler"],
      ["inspections", "🔎", "Denetimler"],
      ["users", "👥", "Kullanıcılar"],
    ],
  },
  Inspector: {
    title: "İETT Denetim Paneli",
    startPage: "inspectorDashboard",
    menuItems: [
      ["inspectorDashboard", "🏠", "Dashboard"],
      ["inspectorTasks", "📋", "Görevlerim"],
      ["drivers", "👤", "Şoförler"],
      ["vehicles", "🚌", "Araçlar"],
      ["busRoutes", "🛣️", "Hatlar"],
      ["trips", "🗓️", "Seferler"],
      ["inspectorComplaints", "💬", "Şikâyet İncelemeleri"],
      ["performanceEvaluation", "📊", "Performans Değerlendirme"],
      ["investigationHistory", "🕘", "Değerlendirme Geçmişim"],
      ["profile", "👤", "Profilim"],
    ],
    additionalPages: ["busStops"],
  },
  Driver: {
    title: "İETT Şoför Paneli",
    startPage: "driverDashboard",
    menuItems: [
      ["driverDashboard", "🏠", "Dashboard"],
      ["driverTasks", "📋", "Görevlerim"],
      ["driverTrips", "🚌", "Seferlerim"],
      ["driverComplaints", "💬", "Şikâyetlerim"],
      ["driverCertificates", "📄", "Sertifikalarım"],
      ["driverPerformance", "📊", "Performansım"],
      ["profile", "👤", "Profilim"],
    ],
  },
};

export function getRoleNavigation(role) {
  return navigationByRole[role] || null;
}

export function getStartPage(role) {
  return getRoleNavigation(role)?.startPage || "adminDashboard";
}

export function isPageAllowed(role, page) {
  const navigation = getRoleNavigation(role);

  if (!navigation) {
    return false;
  }

  const menuPages = navigation.menuItems.map(([pageKey]) => pageKey);
  return [
    ...menuPages,
    ...(navigation.additionalPages || []),
  ].includes(page);
}
