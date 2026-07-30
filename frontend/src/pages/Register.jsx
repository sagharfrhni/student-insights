import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../services/api';
import { useTheme } from '../context/ThemeContext';
import { translateError } from '../utils/errorHandler';
import { GraduationCap, Check, X, Eye, EyeOff, Sun, Moon } from 'lucide-react';

export default function Register() {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [loading, setLoading] = useState(false);

  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();

  const rules = {
    minLength: formData.password.length >= 8,
    hasLetter: /[a-zA-Z]/.test(formData.password),
    hasDigit: /[0-9]/.test(formData.password),
    hasSpecial: /[^a-zA-Z0-9]/.test(formData.password),
    match: formData.password && formData.password === formData.confirmPassword,
  };

  const isFormValid =
    formData.firstName.trim() &&
    formData.lastName.trim() &&
    /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email) &&
    rules.minLength && rules.hasLetter && rules.hasDigit && rules.hasSpecial && rules.match;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isFormValid) return;

    setError('');
    setLoading(true);

    try {
      await api.post('/auth/register', {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        email: formData.email.trim(),
        password: formData.password,
      });

      setSuccessMsg('ثبت‌نام با موفقیت انجام شد. ایمیل تأیید برای شما ارسال گردید.');
      setTimeout(() => navigate('/login'), 3000);
    } catch (err) {
      setError(translateError(err));
    } finally {
      setLoading(false);
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
        <h2 className="text-2xl font-bold text-center mb-1">ایجاد حساب در آموینو</h2>
        <p className="text-sm text-brand-dark/60 text-center mb-6">مشخصات خود را وارد کنید</p>

        {error && <div className="bg-brand-rose/20 text-brand-dark border border-brand-rose p-3 rounded-2xl text-sm mb-4 text-center font-medium">{error}</div>}
        {successMsg && <div className="bg-emerald-100 text-emerald-800 p-3 rounded-2xl text-sm mb-4 text-center font-medium">{successMsg}</div>}

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-medium mb-1">نام</label>
              <input
                type="text"
                required
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                className="w-full px-3 py-2 rounded-xl border border-brand-peach bg-brand-bg text-brand-dark text-sm focus:outline-none focus:ring-2 focus:ring-brand-teal"
              />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1">نام خانوادگی</label>
              <input
                type="text"
                required
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                className="w-full px-3 py-2 rounded-xl border border-brand-peach bg-brand-bg text-brand-dark text-sm focus:outline-none focus:ring-2 focus:ring-brand-teal"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium mb-1">ایمیل</label>
            <input
              type="email"
              required
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              className="w-full px-3 py-2 rounded-xl border border-brand-peach bg-brand-bg text-brand-dark text-left dir-ltr text-sm focus:outline-none focus:ring-2 focus:ring-brand-teal"
              placeholder="example@mail.com"
            />
          </div>

          <div>
            <label className="block text-xs font-medium mb-1">رمز عبور</label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                required
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                className="w-full px-3 py-2 rounded-xl border border-brand-peach bg-brand-bg text-brand-dark text-left dir-ltr text-sm pl-9 focus:outline-none focus:ring-2 focus:ring-brand-teal"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute left-2.5 top-1/2 -translate-y-1/2 text-brand-dark/50 hover:text-brand-dark"
              >
                {showPassword ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
              </button>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium mb-1">تکرار رمز عبور</label>
            <div className="relative">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                required
                value={formData.confirmPassword}
                onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                className="w-full px-3 py-2 rounded-xl border border-brand-peach bg-brand-bg text-brand-dark text-left dir-ltr text-sm pl-9 focus:outline-none focus:ring-2 focus:ring-brand-teal"
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute left-2.5 top-1/2 -translate-y-1/2 text-brand-dark/50 hover:text-brand-dark"
              >
                {showConfirmPassword ? <EyeOff className="w-3.5 h-3.5" /> : <Eye className="w-3.5 h-3.5" />}
              </button>
            </div>
          </div>

          <div className="bg-brand-bg p-3 rounded-xl space-y-1 text-xs border border-brand-peach/40">
            <p className="font-bold mb-1">شرایط رمز عبور:</p>
            <RuleItem valid={rules.minLength} text="حداقل ۸ کاراکتر" />
            <RuleItem valid={rules.hasLetter} text="حداقل یک حرف انگلیسی" />
            <RuleItem valid={rules.hasDigit} text="حداقل یک عدد" />
            <RuleItem valid={rules.hasSpecial} text="حداقل یک کاراکتر خاص (@#$%)" />
            <RuleItem valid={rules.match} text="تطابق رمز عبور و تکرار آن" />
          </div>

          <button
            type="submit"
            disabled={!isFormValid || loading}
            className="w-full bg-brand-teal hover:bg-brand-teal/90 text-white py-3 rounded-2xl font-medium transition-all disabled:opacity-40 shadow-xs"
          >
            {loading ? 'در حال ثبت‌نام...' : 'ثبت‌نام'}
          </button>
        </form>

        <p className="text-center text-sm text-brand-dark/60 mt-4">
          قبلاً ثبت‌نام کرده‌اید؟{' '}
          <Link to="/login" className="text-brand-teal font-bold hover:underline">
            ورود به حساب
          </Link>
        </p>
      </div>
    </div>
  );
}

const RuleItem = ({ valid, text }) => (
  <div className={`flex items-center gap-1.5 ${valid ? 'text-emerald-500 font-medium' : 'text-brand-dark/50'}`}>
    {valid ? <Check className="w-3.5 h-3.5 text-emerald-500" /> : <X className="w-3.5 h-3.5 text-brand-rose" />}
    <span>{text}</span>
  </div>
);