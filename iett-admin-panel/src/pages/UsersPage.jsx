import { useEffect, useMemo, useState } from "react";
import "./UsersPage.css";

import { getUsers } from "../services/userService";

const roleLabels = { Admin: "Admin", Inspector: "Denetimci", Driver: "Şoför" };

export default function UsersPage() {
  const [users, setUsers] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadUsers() {
    try {
      setLoading(true);
      setError("");
      setUsers(await getUsers());
    } catch (requestError) {
      setError(requestError.message || "Kullanıcılar getirilemedi.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { loadUsers(); }, []);

  const filteredUsers = useMemo(() => {
    const value = search.trim().toLocaleLowerCase("tr-TR");
    if (!value) return users;
    return users.filter((user) => {
      const displayedRole = roleLabels[user.roleName] || user.roleName;
      return [user.fullName, user.userName, user.roleName, displayedRole].some(
        (field) => String(field ?? "").toLocaleLowerCase("tr-TR").includes(value),
      );
    });
  }, [search, users]);

  return (
    <section className="users-page">
      <header className="page-header">
        <div><h1>Kullanıcılar</h1><p>Sistemde kayıtlı kullanıcıları ve rollerini görüntüleyin.</p></div>
        <div className="user-count">Toplam {users.length} kullanıcı</div>
      </header>
      {error && <div className="alert-message">{error}</div>}
      <div className="users-toolbar">
        <input className="user-search" type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Ad soyad, kullanıcı adı veya rol ara..." aria-label="Kullanıcılarda ara" />
        <button type="button" className="users-refresh-button" onClick={loadUsers} disabled={loading}>{loading ? "Yenileniyor..." : "Listeyi Yenile"}</button>
      </div>
      <div className="table-card users-table-card">
        {loading ? <div className="table-state">Kullanıcılar yükleniyor...</div> : filteredUsers.length === 0 ? (
          <div className="table-state">{users.length === 0 ? "Kayıtlı kullanıcı bulunamadı." : "Arama sonucuna uygun kullanıcı bulunamadı."}</div>
        ) : (
          <table>
            <thead><tr><th>Ad Soyad</th><th>Kullanıcı Adı</th><th>Rol</th></tr></thead>
            <tbody>{filteredUsers.map((user) => (
              <tr key={user.id}>
                <td><div className="user-name-cell"><span className="user-avatar">{user.fullName?.charAt(0).toLocaleUpperCase("tr-TR")}</span><span>{user.fullName}</span></div></td>
                <td>{user.userName}</td>
                <td><span className={`role-badge role-${user.roleName?.toLocaleLowerCase("en-US")}`}>{roleLabels[user.roleName] || user.roleName}</span></td>
              </tr>
            ))}</tbody>
          </table>
        )}
      </div>
    </section>
  );
}
