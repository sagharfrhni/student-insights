import React, { useState } from 'react';

export default function RegisterForm() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    if (!fullName || !email || !password || !confirmPassword) {
      setError('لطفاً تمام فیلدها را پر کنید.');
      setIsLoading(false);
      return;
    }

    if (password !== confirmPassword) {
      setError('رمز عبور و تکرار رمز عبور یکسان نیستند.');
      setIsLoading(false);
      return;
    }

    try {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      alert('ثبت‌نام شما با موفقیت انجام شد!');
    } catch (err) {
      setError(err.response?.data?.message || 'خطا در ثبت‌نام. لطفاً دوباره امتحان کنید.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="register-card">
      <h2>ایجاد حساب کاربری</h2>
      <p className="register-subtitle">همین حالا در StudentHub AI ثبت‌نام کنید</p>

      {error && <p className="error-message">{error}</p>}

      <form id="register-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="register-full-name">نام کامل</label>
          <input
            type="text"
            id="register-full-name"
            name="fullName"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="نام کامل خود را وارد کنید"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-email">آدرس ایمیل</label>
          <input
            type="email"
            id="register-email"
            name="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ایمیل خود را وارد کنید"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-password">رمز عبور</label>
          <input
            type="password"
            id="register-password"
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="رمز عبور خود را وارد کنید"
            required
            minLength="6"
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-confirm-password">تأیید رمز عبور</label>
          <input
            type="password"
            id="register-confirm-password"
            name="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="رمز عبور را دوباره وارد کنید"
            required
          />
        </div>

        <button type="submit" className="register-submit-btn" disabled={isLoading}>
          {isLoading ? 'در حال ایجاد حساب...' : 'ایجاد حساب کاربری'}
        </button>
      </form>

      <div className="auth-switch">
        <p>قبلاً حساب کاربری دارید؟</p>
        <a href="/login">وارد شوید</a>
      </div>
    </div>
  );
}
