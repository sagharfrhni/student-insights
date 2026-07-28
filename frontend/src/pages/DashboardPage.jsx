import React, { useState } from 'react';
import './DashboardPage.css';

export default function DashboardPage() {
  // داده‌های فرضی (Mock Data) مطابق قراردادهای API پروژه
  const [dashboardData, setDashboardData] = useState({
    activeCoursesCount: 5,
    activeTasksCount: 3,
    upcomingExamsCount: 2,
    todayClasses: [
      { id: 1, name: "سیستم‌های دیجیتال", time: "۰۸:۰۰ - ۱۰:۰۰", location: "کلاس ۱۰۲" },
      { id: 2, name: "مهندسی نرم‌افزار", time: "۱۰:۳۰ - ۱۲:۳۰", location: "سایت کامپیوتر" }
    ],
    upcomingTasks: [
      { id: 101, title: "تحویل فاز اول پروژه StudentHub", type: "task", dueDate: "۱۴۰۵/۰۵/۱۰" },
      { id: 102, title: "امتحان میان‌ترم جبر خطی", type: "exam", dueDate: "۱۴۰۵/۰۵/۱۵" },
      { id: 103, title: "تمرین سری سوم سیستم‌های دیجیتال", type: "task", dueDate: "۱۴۰۵/۰۵/۱۸" }
    ]
  });

  return (
    <div className="dashboard-wrapper" dir="rtl">
      {/* هدر داشبورد */}
      <header className="dashboard-header">
        <h1 className="dashboard-title">داشبورد کاربری</h1>
        <p className="dashboard-subtitle">زهرا عزیز، به پنل خود خوش آمدید. در ادامه خلاصه وضعیت ترم جاری را مشاهده می‌کنید.</p>
      </header>

      {/* بخش کارت‌های آمار بالا - بخش ۳ و ۴ */}
      <section className="stats-grid">
        <div className="stat-card">
          <span className="stat-label">درس‌های فعال</span>
          <span id="stat-active-courses" className="stat-value">
            {dashboardData.activeCoursesCount}
          </span>
        </div>

        <div className="stat-card">
          <span className="stat-label">تکالیف فعال</span>
          <span id="stat-active-tasks" className="stat-value">
            {dashboardData.activeTasksCount}
          </span>
        </div>

        <div className="stat-card">
          <span className="stat-label">امتحان‌های پیش‌رو</span>
          <span id="stat-upcoming-exams" className="stat-value">
            {dashboardData.upcomingExamsCount}
          </span>
        </div>
      </section>

      {/* بخش پایین: کلاس‌های امروز و کارهای نزدیک - بخش ۵ و ۶ */}
      <div className="dashboard-content-layout">
        
        {/* باکس کلاس‌های امروز */}
        <section className="info-section-card">
          <h2 className="section-title">کلاس‌های امروز</h2>
          <div id="today-classes-container" className="list-wrapper">
            {dashboardData.todayClasses.length > 0 ? (
              dashboardData.todayClasses.map((cls) => (
                <div key={cls.id} className="schedule-item">
                  <div className="schedule-info">
                    <span className="schedule-name">{cls.name}</span>
                    <span className="schedule-meta">⏰ {cls.time} | 📍 {cls.location}</span>
                  </div>
                </div>
              ))
            ) : (
              <p className="empty-list-message">امروز هیچ کلاسی برای شما ثبت نشده است.</p>
            )}
          </div>
        </section>

        {/* باکس تکالیف و امتحان‌های نزدیک */}
        <section className="info-section-card">
          <h2 className="section-title">تکالیف و امتحانات نزدیک</h2>
          <div id="upcoming-tasks-container" className="list-wrapper">
            {dashboardData.upcomingTasks.length > 0 ? (
              dashboardData.upcomingTasks.map((item) => (
                <div key={item.id} className={`task-item-card ${item.type}`}>
                  <div className="task-info">
                    <span className="task-title">
                      <span className={`type-badge ${item.type}`}>
                        {item.type === 'exam' ? 'امتحان' : 'تکلیف'}
                      </span>
                      {item.title}
                    </span>
                    <span className="task-due-date">📅 مهلت تحویل: {item.dueDate}</span>
                  </div>
                </div>
              ))
            ) : (
              <p className="empty-list-message">هیچ تکلیف یا امتحانی در روزهای آینده ندارید.</p>
            )}
          </div>
        </section>

      </div>
    </div>
  );
}
