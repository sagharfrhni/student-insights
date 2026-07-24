import React from 'react';
import RegisterForm from '../components/RegisterForm'; // کامپوننت فرم ثبت نام را import میکنیم

// اگر Navbar و Footer را قبلا ساخته اید، آنها را هم import کنید
// import Navbar from '../components/Navbar';
// import Footer from '../components/Footer';

function RegisterPage() {
  return (
    <div className="page-container"> {/* یک کلاس wrapper برای کل صفحه */}
      {/* <Navbar /> */} {/* کامپوننت Navbar اگر موجود باشد */}

      <main className="main-content" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '70vh', padding: '20px' }}>
        {/* main-content را استایل دادیم تا فرم در وسط صفحه قرار گیرد */}
        <RegisterForm /> {/* کامپوننت فرم ثبت نام را اینجا رندر میکنیم */}
      </main>

      {/* <Footer /> */} {/* کامپوننت Footer اگر موجود باشد */}
    </div>
  );
}

export default RegisterPage;
