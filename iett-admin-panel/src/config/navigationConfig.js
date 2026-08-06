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
      ["drivers", "👤", "Şoförler"],
      ["vehicles", "🚌", "Araçlar"],
      ["busRoutes", "🛣️", "Hatlar"],
      ["trips", "🗓️", "Şoför Görev Yönetimi"],
      ["inspectorComplaints", "💬", "Şikâyet İncelemeleri"],
      ["inspectorCertificates", "📄", "Sertifikalar"],
      ["performanceEvaluation", "📊", "Performans Değerlendirme"],
      ["investigationHistory", "🕘", "Performans Değerlendirme Geçmişi"],
      ["profile", "👤", "Profilim"],
    ],
    additionalPages: ["busStops"],
  },
  Driver: {
    title: "İETT Şoför Paneli",
    startPage: "driverDashboard",
    menuItems: [
      ["driverDashboard", "🏠", "Dashboard"],
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
