import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';

import Layout from './components/Layout';
import Landing from './pages/Landing';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Courses from './pages/Courses';
import TimerPage from './pages/TimerPage';
import Exams from './pages/Exams';
import LearningActivities from './pages/LearningActivities';
import NotificationsPage from './pages/NotificationsPage';

import StudyLogs from './pages/StudyLogs';
import Goals from './pages/Goals';
import CalendarPage from './pages/CalendarPage';
import AnalyticsPage from './pages/AnalyticsPage';

import AdminStats from './pages/AdminStats';
import AdminUsers from './pages/AdminUsers';
import AdminSettings from './pages/AdminSettings';

const ProtectedRoute = ({ children }) => {
  const { user, loading } = useAuth();
  if (loading) return null;
  return user?.isAuthenticated ? children : <Navigate to="/login" />;
};

const AdminRoute = ({ children }) => {
  const { user, loading } = useAuth();
  if (loading) return null;
  return user?.isAuthenticated && user?.isAdmin ? children : <Navigate to="/dashboard" />;
};

const IndexRedirect = () => {
  const { user } = useAuth();
  if (user?.isAdmin) return <Navigate to="/admin/stats" replace />;
  return <Navigate to="/dashboard" replace />;
};

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Landing />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            <Route
              element={
                <ProtectedRoute>
                  <Layout />
                </ProtectedRoute>
              }
            >
              <Route path="/app" element={<IndexRedirect />} />

              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/courses" element={<Courses />} />
              <Route path="/timer" element={<TimerPage />} />
              <Route path="/exams" element={<Exams />} />
              <Route path="/learning-activities" element={<LearningActivities />} />
              <Route path="/notifications" element={<NotificationsPage />} />
              <Route path="/study-logs" element={<StudyLogs />} />
              <Route path="/goals" element={<Goals />} />
              <Route path="/calendar" element={<CalendarPage />} />
              <Route path="/analytics" element={<AnalyticsPage />} />
              
              <Route path="/admin/stats" element={<AdminRoute><AdminStats /></AdminRoute>} />
              <Route path="/admin/users" element={<AdminRoute><AdminUsers /></AdminRoute>} />
              <Route path="/admin/settings" element={<AdminRoute><AdminSettings /></AdminRoute>} />
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}