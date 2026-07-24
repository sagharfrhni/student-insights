export default function LoginForm() {
  return (
    <div className="login-card">
      <h2>ورود به حساب کاربری</h2>
      <p>لطفا ایمیل و رمز عبور خود را وارد کنید</p>

      {/* فرم ورود - شناسه دقیق طبق قرارداد پروژه */}
      <form id="login-form">
        <div className="form-group">
          <label htmlFor="login-email">ایمیل دانشجویی</label>
          <input
            type="email"
            id="login-email"
            name="email"
            placeholder="student@example.com"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="login-password">رمز عبور</label>
          <input
            type="password"
            id="login-password"
            name="password"
            placeholder="••••••••"
            required
          />
        </div>

        <button type="submit" id="login-submit-btn">
          ورود به سامانه
        </button>
      </form>
    </div>
  );
}
