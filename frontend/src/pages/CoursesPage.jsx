import React, { useState } from 'react';
import './CoursesPage.css';

const CoursesPage = () => {
  // داده‌های تستی (Mock Data) بر اساس نیازهای پروژه
  const [courses] = useState([
    { id: 101, name: 'سیستم‌های دیجیتال', instructor: 'دکتر محمدی', day: 'شنبه', time: '۰۸:۰۰ - ۱۰:۰۰', units: 3 },
    { id: 102, name: 'مهندسی نرم‌افزار', instructor: 'دکتر علوی', day: 'دوشنبه', time: '۱۳:۳۰ - ۱۵:۳۰', units: 3 },
    { id: 103, name: 'طراحی الگوریتم', instructor: 'دکتر سهرابی', day: 'چهارشنبه', time: '۱۰:۰۰ - ۱۲:۰۰', units: 3 },
  ]);

  return (
    <div className="courses-page-container" id="courses-page-root">
      <header className="page-header">
        <h1 id="courses-title">مدیریت دروس و برنامه هفتگی</h1>
        <p>لیست دروسی که در این ترم اخذ کرده‌اید</p>
      </header>

      <section className="courses-section">
        <h2 className="section-title">لیست دروس</h2>
        <div className="courses-grid" id="courses-list-container">
          {courses.map((course) => (
            <div className="course-card" key={course.id} id={`course-card-${course.id}`}>
              <div className="course-info">
                <h3 className="course-name">{course.name}</h3>
                <p className="course-instructor">استاد: {course.instructor}</p>
                <p className="course-time">زمان: {course.day} | {course.time}</p>
              </div>
              <div className="course-badge" id={`units-badge-${course.id}`}>
                {course.units} واحد
              </div>
              <button className="details-btn" id={`btn-details-${course.id}`}>
                مشاهده جزئیات
              </button>
            </div>
          ))}
        </div>
      </section>

      <section className="schedule-section">
        <h2 className="section-title">تقویم هفتگی</h2>
        <div className="weekly-schedule-grid" id="weekly-schedule-table">
          {/* اینجا یک نمای ساده از جدول زمان‌بندی برای جلوگیری از تداخل (Conflict) */}
          <div className="schedule-helper-text">
            نمای تقویم بر اساس زمان‌بندی دروس بالا تنظیم شده است.
          </div>
          <div className="schedule-placeholder">
             (در فاز بعدی، این بخش به صورت جدول گرافیکی پیاده‌سازی می‌شود)
          </div>
        </div>
      </section>
    </div>
  );
};

export default CoursesPage;
