import React, { useState } from 'react';
// import { useNavigate } from 'react-router-dom'; // بعداً برای هدایت کاربر اضافه میشه
// import authService from '../services/authService'; // بعداً برای صدا زدن API استفاده میشه

export default function RegisterForm() {
  const [fullName, setFullName] = useState(''); // تغییر نام state به مطابق فیلد Full Name
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // const navigate = useNavigate(); // برای هدایت کاربر

  const handleSubmit = async (e) => {
    e.preventDefault(); // جلوگیری از رفرش صفحه
    setError(''); // پاک کردن خطاهای قبلی
    setIsLoading(true);

    // --- اعتبارسنجی اولیه سمت کلاینت ---
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
    // --- پایان اعتبارسنجی ---

    try {
      // console.log('Register attempt with:', { fullName, email, password });
      // alert(`در حال ثبت نام با اطلاعات: نام: ${fullName}, ایمیل: ${email}`); // پیام تست اولیه

      // *** اینجا تابع ثبت نام از authService صدا زده میشه ***
      // مثال: const response = await authService.registerUser({ name: fullName, email, password });
      // console.log('Registration response:', response);

      // شبیه سازی تاخیر پاسخ API
      await new Promise(resolve => setTimeout(resolve, 2000));

      alert('ثبت نام با موفقیت انجام شد!');
      // navigate('/login'); // هدایت به صفحه ورود بعد از ثبت نام موفق
      // فعلاً با alert نمایش داده میشه

    } catch (err) {
      console.error('Registration error:', err);
      // نمایش پیام خطای دقیق‌تر در صورت وجود در پاسخ API
      setError(err.response?.data?.message || 'خطا در ثبت نام. لطفا دوباره امتحان کنید.');
    } finally {
      setIsLoading(false); // غیرفعال کردن دکمه و نمایش لودینگ
    }
  };

  return (
    <div className="register-card"> {/* کلاس CSS برای استایل دهی */}
      <h2>Create Account</h2> {/* عنوان صفحه بر اساس Figma */}
      <p>Join StudentHub AI today</p> {/* متن زیر عنوان بر اساس Figma */}

      {error && <p className="error-message" style={{ color: 'red', textAlign: 'center', marginBottom: '10px' }}>{error}</p>} {/* نمایش پیام خطا */}

      <form id="register-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="register-full-name">FULL NAME</label> {/* لیبل مطابق Figma */}
          <input
            type="text"
            id="register-full-name"
            name="fullName"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Enter your full name" // Placeholder مطابق Figma
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-email">EMAIL ADDRESS</label> {/* لیبل مطابق Figma */}
          <input
            type="email"
            id="register-email"
            name="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Enter your email" // Placeholder مطابق Figma
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-password">PASSWORD</label> {/* لیبل مطابق Figma */}
          <input
            type="password"
            id="register-password"
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter your password" // Placeholder مطابق Figma
            required
            minLength="6" // حداقل طول رمز عبور
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-confirm-password">CONFIRM PASSWORD</label> {/* لیبل مطابق Figma */}
          <input
            type="password"
            id="register-confirm-password"
            name="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Confirm your password" // Placeholder مطابق Figma
            required
          />
        </div>

        <button type="submit" id="register-submit-btn" disabled={isLoading} style={{ width: '100%', padding: '10px', marginTop: '20px', backgroundColor: '#000', color: 'white', border: 'none', borderRadius: '5px', cursor: isLoading ? 'not-allowed' : 'pointer' }}>
          {isLoading ? 'Creating Account...' : 'Create Account'} {/* متن دکمه مطابق Figma */}
        </button>
      </form>

      <div className="auth-switch" style={{ textAlign: 'center', marginTop: '15px', fontSize: '0.9em' }}>
        <p>Already have an account?</p> {/* متن مطابق Figma */}
        {/* <Link to="/login">Log in</Link> */}
        <a href="/login" style={{ color: '#007bff', textDecoration: 'none' }}>Log in</a> {/* فعلاً از a استفاده می‌کنیم */}
      </div>
    </div>
  );
}
