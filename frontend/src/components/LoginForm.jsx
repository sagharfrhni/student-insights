import React, { useState } from 'react';
import "./LoginForm.css";




//import authService from '../services/authService';

export default function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      console.log('Login attempt with:', { email, password });

      // const response = await authService.loginUser({ email, password });
      // console.log('Login response:', response);

      await new Promise(resolve => setTimeout(resolve, 2000));
      alert('ورود موفقیت‌آمیز بود! (شبیه‌سازی)');

    } catch (error) {
      console.error('Login error:', error);
      setError(error.message || 'لطفاً دوباره امتحان کنید.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-card">
      <h2 className="login-title">ورود به حساب کاربری</h2>
      <p className="login-subtitle">لطفاً ایمیل و رمز عبور خود را وارد کنید</p>

      {error && <p className="error-message">{error}</p>}

      <form id="login-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="login-email">ایمیل دانشجویی</label>
          <input
            type="email"
            id="login-email"
            name="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="student@example.com"
            className="ltr-input"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="login-password">رمز عبور</label>
          <input
            type="password"
            id="login-password"
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            required
          />
        </div>

        <button
          type="submit"
          id="login-submit-btn"
          className="login-submit-btn"
          disabled={isLoading}
        >
          {isLoading ? 'در حال ورود...' : 'ورود به سامانه'}
        </button>
      </form>
    </div>
  );
}
