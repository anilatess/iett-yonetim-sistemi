import PlaceholderPage from "../components/common/PlaceholderPage";

export default function ProfilePage({ role }) {
  const tables = role === "Driver" ? ["Users", "Drivers"] : ["Users", "Inspectors"];
  return <PlaceholderPage title="Profilim" description="Kullanıcı ve personel bilgileriniz burada gösterilecek." tables={tables} />;
}
