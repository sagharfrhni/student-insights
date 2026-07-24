    // ... (داخل App.jsx)
    import React, { useState } from 'react'; // useState را import کنید
    import LoginForm from './components/LoginForm';
    import RegisterPage from './pages/RegisterPage'; // اینجا import شد

    function App() {
      const [currentPage, setCurrentPage] = useState('register'); // یا login، بسته به اینکه کدوم اول نمایش داده بشه

      return (
        <div className="App">
          {/* <Navbar /> */}
          <main className="main-content">
            {currentPage === 'login' && <LoginForm />}
            {currentPage === 'register' && <RegisterPage />}
          </main>
          {/* <Footer /> */}

          {/* دکمه برای جابجایی */}
          <div style={{ textAlign: 'center', marginTop: '20px' }}>
            {currentPage === 'login' ? (
              <button onClick={() => setCurrentPage('register')}>رفتن به ثبت نام</button>
            ) : (
              <button onClick={() => setCurrentPage('login')}>رفتن به ورود</button>
            )}
          </div>
        </div>
      );
    }

    export default App;

