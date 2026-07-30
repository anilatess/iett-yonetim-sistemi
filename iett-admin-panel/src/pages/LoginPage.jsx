import { useState } from "react";
import { login } from "../services/authService";
import iettLogo from "../assets/iett-logo.png";
import "./LoginPage.css";

function LoginPage({ onLogin }) {
  const [form, setForm] = useState({
    userName: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  function handleChange(event) {
    const { name, value } = event.target;

    setForm((previousForm) => ({
      ...previousForm,
      [name]: value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setError("");
    setLoading(true);

    try {
      const result = await login(form);
      onLogin(result);
    } catch (error) {
      setError(error.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-brand">
        <div className="login-logo">
        <img src={iettLogo} alt="İETT Logo" />
        </div>
          <div>
            <h1>Yönetim Sistemi</h1>
            <p>Hesabınıza giriş yapın</p>
          </div>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <div className="login-field">
            <label htmlFor="userName">Kullanıcı Adı</label>

            <input
              id="userName"
              name="userName"
              type="text"
              placeholder="Kullanıcı adınızı girin"
              value={form.userName}
              onChange={handleChange}
              autoComplete="username"
              required
            />
          </div>

          <div className="login-field">
            <label htmlFor="password">Şifre</label>

            <input
              id="password"
              name="password"
              type="password"
              placeholder="Şifrenizi girin"
              value={form.password}
              onChange={handleChange}
              autoComplete="current-password"
              required
            />
          </div>

          {error && (
            <div className="login-error">
              {error}
            </div>
          )}

          <button
            className="login-button"
            type="submit"
            disabled={loading}
          >
            {loading ? "Giriş yapılıyor..." : "Giriş Yap"}
          </button>
        </form>

        <p className="login-footer">
          İETT Ulaşım Yönetim Sistemi
        </p>
      </div>
    </div>
  );
}

export default LoginPage;