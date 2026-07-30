import { useEffect, useState } from "react";
import "./VehiclesPage.css";

import {
  createBusRoute,
  deleteBusRoute,
  getBusRoutes,
  updateBusRoute,
} from "../services/busRouteService";

const EMPTY_FORM = {
  routeCode: "",
  routeName: "",
  estimatedDuration: "",
};

function BusRoutesPage({ onSelectRoute, canEdit = true }) {
  const [busRoutes, setBusRoutes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");
  const [modalError, setModalError] = useState("");

  const [modalOpen, setModalOpen] = useState(false);
  const [editingBusRoute, setEditingBusRoute] =
    useState(null);

  const [formData, setFormData] = useState(EMPTY_FORM);

  async function loadBusRoutes() {
    try {
      setLoading(true);
      setMessage("");

      const data = await getBusRoutes();
      setBusRoutes(data);
    } catch (error) {
      setMessage(
        error.message || "Hatlar getirilemedi.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadBusRoutes();
  }, []);

  function openCreateModal() {
    setEditingBusRoute(null);
    setFormData(EMPTY_FORM);
    setModalError("");
    setModalOpen(true);
  }

  function openEditModal(busRoute) {
    setEditingBusRoute(busRoute);

    setFormData({
      routeCode: busRoute.routeCode,
      routeName: busRoute.routeName,
      estimatedDuration: busRoute.estimatedDuration,
    });

    setModalError("");
    setModalOpen(true);
  }

  function closeModal() {
    setModalOpen(false);
    setEditingBusRoute(null);
    setModalError("");
    setFormData(EMPTY_FORM);
  }

  function handleInputChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    const routeCode = formData.routeCode.trim();
    const routeName = formData.routeName.trim();
    const estimatedDuration = Number(
      formData.estimatedDuration,
    );

    if (!routeCode) {
      setModalError("Hat kodu boş bırakılamaz.");
      return;
    }

    if (!routeName) {
      setModalError("Hat adı boş bırakılamaz.");
      return;
    }

    if (
      !Number.isInteger(estimatedDuration) ||
      estimatedDuration <= 0
    ) {
      setModalError(
        "Tahmini süre sıfırdan büyük bir tam sayı olmalıdır.",
      );
      return;
    }

    const requestData = {
      routeCode,
      routeName,
      estimatedDuration,
    };

    try {
      setModalError("");

      if (editingBusRoute) {
        await updateBusRoute({
          id: editingBusRoute.id,
          ...requestData,
        });
      } else {
        await createBusRoute(requestData);
      }

      closeModal();
      await loadBusRoutes();
    } catch (error) {
      setModalError(
        error.message ||
          "Hat kaydedilirken hata oluştu.",
      );
    }
  }

  async function handleDelete(busRoute) {
    const confirmed = window.confirm(
      `${busRoute.routeCode} kodlu hattı silmek istediğine emin misin?`,
    );

    if (!confirmed) {
      return;
    }

    try {
      setMessage("");

      await deleteBusRoute(busRoute.id);
      await loadBusRoutes();
    } catch (error) {
      setMessage(
        error.message || "Hat silinirken hata oluştu.",
      );
    }
  }

  return (
    <div className="vehicles-page">
      <div className="page-header">
        <div>
          <h1>{canEdit ? "Hat Yönetimi" : "Hatlar"}</h1>
          <p>
            Veritabanında kayıtlı hatları {canEdit ? "yönetebilirsiniz" : "görüntüleyebilirsiniz"}.
          </p>
        </div>

        {canEdit && (
          <button type="button" className="add-button" onClick={openCreateModal}>
            Yeni Hat
          </button>
        )}
      </div>

      {message && (
        <div className="alert-message">{message}</div>
      )}

      <div className="table-card">
        {loading ? (
          <div className="table-state">
            Hatlar yükleniyor...
          </div>
        ) : busRoutes.length === 0 ? (
          <div className="table-state">
            Kayıtlı hat bulunamadı.
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Hat Kodu</th>
                <th>Hat Adı</th>
                <th>Tahmini Süre</th>
                {canEdit && <th>İşlemler</th>}
              </tr>
            </thead>

            <tbody>
              {busRoutes.map((busRoute) => (
                <tr key={busRoute.id}>
                  <td>{busRoute.id}</td>
                  <td>
                    <button
                      type="button"
                      className="route-link"
                      onClick={() => onSelectRoute(busRoute)}
                    >
                      {busRoute.routeCode}
                    </button>
                  </td>
                  <td>{busRoute.routeName}</td>
                  <td>
                    {busRoute.estimatedDuration} dakika
                  </td>
                  {canEdit && <td>
                    <button
                      type="button"
                      className="edit-button"
                      onClick={() =>
                        openEditModal(busRoute)
                      }
                    >
                      Düzenle
                    </button>

                    <button
                      type="button"
                      className="delete-button"
                      onClick={() =>
                        handleDelete(busRoute)
                      }
                    >
                      Sil
                    </button>
                  </td>}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {canEdit && modalOpen && (
        <div
          className="modal-backdrop"
          onMouseDown={closeModal}
        >
          <div
            className="modal-card"
            onMouseDown={(event) =>
              event.stopPropagation()
            }
          >
            <div className="modal-header">
              <div>
                <h2>
                  {editingBusRoute
                    ? "Hattı Düzenle"
                    : "Yeni Hat Ekle"}
                </h2>

                <p>Hat bilgilerini doldurun.</p>
              </div>

              <button
                type="button"
                className="modal-close"
                onClick={closeModal}
              >
                ×
              </button>
            </div>

            {modalError && (
              <div className="modal-error">
                {modalError}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label htmlFor="routeCode">
                  Hat Kodu
                </label>

                <input
                  id="routeCode"
                  name="routeCode"
                  type="text"
                  value={formData.routeCode}
                  onChange={handleInputChange}
                  placeholder="Örnek: 500T"
                  autoFocus
                />
              </div>

              <div className="form-group">
                <label htmlFor="routeName">
                  Hat Adı
                </label>

                <input
                  id="routeName"
                  name="routeName"
                  type="text"
                  value={formData.routeName}
                  onChange={handleInputChange}
                  placeholder="Örnek: Tuzla - Cevizlibağ"
                />
              </div>

              <div className="form-group">
                <label htmlFor="estimatedDuration">
                  Tahmini Süre
                </label>

                <input
                  id="estimatedDuration"
                  name="estimatedDuration"
                  type="number"
                  min="1"
                  step="1"
                  value={formData.estimatedDuration}
                  onChange={handleInputChange}
                  placeholder="Dakika olarak girin"
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="cancel-button"
                  onClick={closeModal}
                >
                  Vazgeç
                </button>

                <button
                  type="submit"
                  className="save-button"
                >
                  {editingBusRoute
                    ? "Değişiklikleri Kaydet"
                    : "Hattı Kaydet"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default BusRoutesPage;
