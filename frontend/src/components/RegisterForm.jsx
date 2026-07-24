import React, { useState } from 'react';
// import { useNavigate } from 'react-router-dom'; // بعداً برای هدایت کاربر اضافه میشه
// import authService from '../services/authService'; // بعداً برای صدا زدن API استفاده میشه

export default function RegisterForm() {
  const [fullName, setFullName] = useState(''); // نام کامل
  const [email, setEmail] = useState(''); // ایمیل
  const [password, setPassword] = useState(''); // رمز عبور
  const [confirmPassword, setConfirmPassword] = useState(''); // تکرار رمز عبور
  const [isLoading, setIsLoading] = useState(false); // وضعیت بارگذاری
  const [error, setError] = useState(''); // پیام خطا

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
      // console.log('Attempting registration with:', { fullName, email, password });
      // alert(`در حال ثبت نام با اطلاعات: نام: ${fullName}, ایمیل: ${email}`); // پیام تست اولیه

      // *** اینجا تابع ثبت نام از authService صدا زده میشه ***
      // مثال: const response = await authService.registerUser({ name: fullName, email, password });
      // console.log('Registration response:', response);

      // شبیه سازی تاخیر پاسخ API
      await new Promise(resolve => setTimeout(resolve, 2000));

      alert('ثبت نام شما با موفقیت انجام شد!');
      // navigate('/login'); // هدایت به صفحه ورود بعد از ثبت نام موفق
      // فعلاً با alert نمایش داده میشه

    } catch (err) {
      console.error('Registration error:', err);
      // نمایش پیام خطای دقیق‌تر در صورت وجود در پاسخ API
      setError(err.response?.data?.message || 'خطا در ثبت نام. لطفاً دوباره امتحان کنید.');
    } finally {
      setIsLoading(false); // غیرفعال کردن دکمه و نمایش لودینگ
    }
  };

  return (
    <div className="register-card"> {/* کلاس CSS برای استایل دهی */}
      <h2>ایجاد حساب کاربری</h2> {/* عنوان صفحه به فارسی */}
      <p>به دنیای StudentHub AI بپیوندید</p> {/* متن زیر عنوان به فارسی */}

      {error && <p className="error-message" style={{ color: 'red', textAlign: 'center', marginBottom: '10px' }}>{error}</p>} {/* نمایش پیام خطا */}

      <form id="register-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="register-full-name">نام کامل</label> {/* لیبل به فارسی */}
          <input
            type="text"
            id="register-full-name"
            name="fullName"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="نام کامل خود را وارد کنید" // Placeholder به فارسی
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-email">آدرس ایمیل</label> {/* لیبل به فارسی */}
          <input
            type="email"
            id="register-email"
            name="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ایمیل خود را وارد کنید" // Placeholder به فارسی
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-password">رمز عبور</label> {/* لیبل به فارسی */}
          <input
            type="password"
            id="register-password"
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="رمز عبور خود را وارد کنید" // Placeholder به فارسی
            required
            minLength="6" // حداقل طول رمز عبور
          />
        </div>

        <div className="form-group">
          <label htmlFor="register-confirm-password">تأیید رمز عبور</label> {/* لیبل به فارسی */}
          <input
            type="password"
            id="register-confirm-password"
            name="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="رمز عبور خود را دوباره وارد کنید" // Placeholder به فارسی
            required
          />
        </div>

        <button type="submit" id="register-submit-btn" disabled={isLoading} style={{ width: '100%', padding: '10px', marginTop: '20px', backgroundColor: '#000', color: 'white', border: 'none', borderRadius: '5px', cursor: isLoading ? 'not-allowed' : 'pointer' }}>
          {isLoading ? 'در حال ایجاد حساب...' : 'ایجاد حساب کاربری'} {/* متن دکمه به فارسی */}
        </button>
      </form>

      <div className="auth-switch" style={{ textAlign: 'center', marginTop: '15px', fontSize: '0.9em' }}>
        <p>قبلاً حساب کاربری دارید؟</p> {/* متن به فارسی */}
        {/* <Link to="/login">Log in</Link> */}
        <a href="/login" style={{ color: '#007bff', textDecoration: 'none' }}>وارد شوید</a> {/* لینک ورود به فارسی */}
      </div>
    </div>
  );
}
