import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import CoursesPage from "./pages/CoursesPage";
import DashboardPage from "./pages/DashboardPage";

function App() {
  return (
    <Router>
      <div>
        {/* این بخش منوی ناوبری موقت برای تست است */}
        <nav style={{ display: "flex", gap: "10px", justifyContent: "center", marginTop: "20px", padding: "10px", background: "#f0f0f0" }}>
          <Link to="/login">Login</Link>
          <Link to="/register">Register</Link>
          <Link to="/dashboard">Dashboard</Link>
          <Link to="/courses">Courses</Link>
        </nav>

        {/* بخش اصلی مدیریت مسیرها */}
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/courses" element={<CoursesPage />} />
          {/* مسیر پیش‌فرض: اگر هیچکدام نبود برود به لاگین */}
          <Route path="/" element={<LoginPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
