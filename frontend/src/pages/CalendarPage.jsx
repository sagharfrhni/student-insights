import React, { useState, useEffect } from "react";
import "./CalendarPage.css";

// 1. Mock Data: این بخش دقیقا همان ساختاری است که نرگس از بک‌اِند دریافت می‌کند
const MOCK_EVENTS = [
  { id: 1, title: "کلاس مهندسی نرم‌افزار", date: "2026-08-05", time: "09:00", type: "کلاس" },
  { id: 2, title: "تحویل فاز ۱ StudentHub", date: "2026-08-07", time: "23:59", type: "ددلاین" },
  { id: 3, title: "جلسه مشاوره", date: "2026-08-10", time: "14:00", type: "جلسه" },
];

export default function CalendarPage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);

  // 2. Logic: نرگس عزیز، برای اتصال به API، این بخش را تغییر بده:
  useEffect(() => {
    // شبیه‌سازی تاخیر شبکه
    const timer = setTimeout(() => {
      setEvents(MOCK_EVENTS);
      setLoading(false);
    }, 1000);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="calendar-page">
      <header className="calendar-header">
        <h1>تقویم برنامه‌ها</h1>
        <p>مشاهده کلاس‌ها و ددلاین‌های پیش رو</p>
      </header>

      <section className="calendar-content">
        {loading ? (
          <div className="status-message">در حال دریافت رویدادها...</div>
        ) : events.length === 0 ? (
          <div className="status-message">رویدادی برای نمایش وجود ندارد.</div>
        ) : (
          <div className="event-list">
            {events.map((event) => (
              <div key={event.id} className={`event-card type-${event.type}`}>
                <div className="event-date">
                  <span className="day">{new Date(event.date).getDate()}</span>
                  <span className="month">مرداد</span>
                </div>
                <div className="event-info">
                  <h3>{event.title}</h3>
                  <p>{event.time} | {event.type}</p>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
