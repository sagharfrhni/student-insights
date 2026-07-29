import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import LoginPage from './pages/LoginPage.jsx';
import RegisterPage from './pages/RegisterPage.jsx';
import CoursesPage from './pages/CoursesPage.jsx';
import DashboardPage from './pages/DashboardPage.jsx';
import TaskPage from './pages/TaskPage.jsx';
import CalendarPage from './pages/CalendarPage.jsx';

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
          <Link to="/tasks">Tasks</Link>
          <Link to="/calendar">Calendar</Link>
        </nav>

        {/* بخش اصلی مدیریت مسیرها */}
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/courses" element={<CoursesPage />} />
          <Route path="/tasks" element={<TaskPage />} />
          <Route path="/calendar" element={<CalendarPage />} />
          {/* مسیر پیش‌فرض: اگر هیچ‌کدام نبود برو به لاگین */}
          <Route path="/" element={<LoginPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
