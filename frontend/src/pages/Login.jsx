import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { translateError } from '../utils/errorHandler';
import { GraduationCap, Play, Eye, EyeOff, Sun, Moon } from 'lucide-react';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login, loginAsDemo } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login(email.trim(), password, rememberMe);
      navigate('/dashboard');
    } catch (err) {
      setError(translateError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDemoLogin = (isAdmin = false) => {
    loginAsDemo(isAdmin);
    if (isAdmin) {
      navigate('/admin/stats');
    } else {
      navigate('/dashboard');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-brand-bg p-4 text-brand-dark relative transition-colors duration-300">
      
      
      <div className="absolute top-4 left-4 z-20">
        <button
          onClick={toggleTheme}
          className="p-2.5 text-brand-dark/70 hover:text-brand-dark bg-white border border-brand-peach/80 rounded-2xl shadow-xs transition-all"
          title={theme === 'dark' ? 'تغییر به تم روشن' : 'تغییر به تم دارک'}
        >
          {theme === 'dark' ? <Sun className="w-5 h-5 text-brand-amber" /> : <Moon className="w-5 h-5 text-brand-teal" />}
        </button>
      </div>

      <div className="bg-white p-8 rounded-3xl shadow-lg w-full max-w-md border border-brand-peach/80 relative">
        <div className="flex justify-center mb-3">
          <div className="bg-brand-teal text-white p-3 rounded-2xl shadow-xs">
            <GraduationCap className="w-8 h-8" />
          </div>
        </div>
        
        <h2 className="text-2xl font-bold text-center mb-1">ورود به آموینو</h2>
        <p className="text-sm text-brand-dark/60 text-center mb-6">سامانه هوشمند مدیریت تحصیلی</p>

        
        <div className="bg-brand-amber/20 border border-brand-amber p-4 rounded-2xl mb-6 text-center space-y-2">
          <p className="text-xs font-bold text-brand-dark">سرور روشن نیست؟ از حالت دمو استفاده کنید:</p>
          <div className="flex gap-2 justify-center">
            <button
              onClick={() => handleDemoLogin(false)}
              className="bg-brand-teal hover:bg-brand-teal/90 text-white px-3 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1 shadow-xs"
            >
              <Play className="w-3 h-3" />
              ورود آزمایشی (دانشجو)
            </button>
            <button
              onClick={() => handleDemoLogin(true)}
              className="bg-brand-rose hover:bg-brand-rose/90 text-white px-3 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1 shadow-xs"
            >
              <Play className="w-3 h-3" />
              ورود آزمایشی (ادمین)
            </button>
          </div>
        </div>

        {error && <div className="bg-brand-rose/20 text-brand-dark border border-brand-rose p-3 rounded-2xl text-sm mb-4 text-center font-medium">{error}</div>}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">ایمیل</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 rounded-2xl border border-brand-peach bg-brand-bg text-brand-dark text-left dir-ltr text-sm focus:outline-none focus:ring-2 focus:ring-brand-teal"
              placeholder="example@mail.com"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">رمز عبور</label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-3 rounded-2xl border border-brand-peach bg-brand-bg text-brand-dark text-left dir-ltr text-sm pl-10 focus:outline-none focus:ring-2 focus:ring-brand-teal"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-brand-dark/50 hover:text-brand-dark"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
          </div>

          <div className="flex items-center justify-between text-sm">
            <label className="flex items-center gap-2 text-brand-dark/70 cursor-pointer">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="rounded border-brand-peach text-brand-teal focus:ring-brand-teal"
              />
              مرا به خاطر بسپار
            </label>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-brand-teal hover:bg-brand-teal/90 text-white py-3 rounded-2xl font-medium transition-all shadow-sm disabled:opacity-50"
          >
            {loading ? 'در حال ورود...' : 'ورود با سرور واقعی'}
          </button>
        </form>

        <p className="text-center text-sm text-brand-dark/60 mt-6">
          حساب کاربری ندارید؟{' '}
          <Link to="/register" className="text-brand-teal font-bold hover:underline">
            ثبت‌نام کنید
          </Link>
        </p>
      </div>
    </div>
  );
}