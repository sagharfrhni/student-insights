import LoginForm from "../components/LoginForm";

export default function LoginPage() {
  return (
    <div className="login-page-container">
      <header className="brand-header">
        <h1>🎓 StudentHub</h1>
        <p>سامانه مدیریت تکالیف و امتحانات</p>
      </header>

      <main className="main-content">
        <LoginForm />
      </main>

      <footer className="footer">
        <p>© 2026 StudentHub - تمامی حقوق محفوظ است</p>
      </footer>
    </div>
  );
}
