import React, { useState } from 'react'; // import useState

//import authService from '../services/authService';

export default function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false); // state برای مدیریت حالت بارگذاری

  const handleSubmit = async (e) => {
    e.preventDefault(); // جلوگیری از ریفرش صفحه هنگام submit
    setIsLoading(true); // فعال کردن حالت بارگذاری

    try {
      console.log('Login attempt with:', { email, password });
      // alert(`در حال تلاش برای ورود با ایمیل: ${email}`); // برای تست اولیه


      // const response = await authService.loginUser({ email, password });
      // console.log('Login response:', response);
      // اگر response موفقیت آمیز بود (مثلا token داشت) کاربر را هدایت کن
      // مثال: navigate('/dashboard');

      // شبیه سازی تاخیر برای دیدن حالت بارگذاری
      await new Promise(resolve => setTimeout(resolve, 2000));
      alert('ورود موفقیت آمیز بود! (شبیه سازی)'); // پیام شبیه سازی شده

    } catch (error) {
      console.error('Login error:', error);
      // نمایش پیام خطا به کاربر
      alert(`خطا در ورود: ${error.message || 'لطفا دوباره امتحان کنید.'}`);
    } finally {
      setIsLoading(false); // غیر فعال کردن حالت بارگذاری، چه موفق چه خطا
    }
  };

  return (
    <div className="login-card"> {/* والد برای کل JSX */}
      <h2>ورود به حساب کاربری</h2>
      <p>لطفا ایمیل و رمز عبور خود را وارد کنید</p>
      {/* فرم ورود - شناسه دقیق قرارداد */}
      <form id="login-form" onSubmit={handleSubmit}> {/* اضافه کردن onSubmit */}
        <div className="form-group">
          <label htmlFor="login-email">ایمیل دانشجوئی</label>
          <input
            type="email"
            id="login-email"
            name="email"
            value={email} // اتصال مقدار input به state email
            onChange={(e) => setEmail(e.target.value)} // آپدیت state email با هر تغییر
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
            value={password} // اتصال مقدار input به state password
            onChange={(e) => setPassword(e.target.value)} // آپدیت state password با هر تغییر
            placeholder="••••••••"
            required
          />
        </div>
        {/* دکمه ورود با مدیریت isLoading */}
        <button type="submit" id="login-submit-btn" disabled={isLoading}>
          {isLoading ? 'درحال ورود...' : 'ورود به سامانه'}
        </button>
      </form>
    </div>
  );
}
