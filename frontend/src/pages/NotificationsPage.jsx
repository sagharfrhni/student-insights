import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toJalaliDateTime } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import { Bell, CheckCheck } from 'lucide-react';

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchNotifications = async () => {
    try {
      setLoading(true);
      const res = await api.get('/notifications?pageNumber=1&pageSize=50');
      setNotifications(res.data.items);
    } catch (err) {
      setError(translateError(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, []);

  const handleMarkAsRead = async (id) => {
    try {
      await api.patch(`/notifications/${id}/read`);
      fetchNotifications();
    } catch (err) {
      alert(translateError(err));
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await api.patch('/notifications/read-all');
      fetchNotifications();
    } catch (err) {
      alert(translateError(err));
    }
  };

  
  const renderNotificationMessage = (notif) => {
    switch (notif.type) {
      case 'ExamTomorrow':
        return 'فردا یک امتحان در پیش دارید. لطفاً آمادگی لازم را داشته باشید.';
      case 'DeadlineApproaching':
        return 'مهلت تحویل یکی از تکالیف یا پروژه‌های شما رو به پایان است.';
      case 'OverdueActivity':
        return 'مهلت تحویل یکی از فعالیت‌های تحصیلی شما گذشته است.';
      case 'GoalBehindSchedule':
        return 'پیشرفت یکی از اهداف تحصیلی شما از برنامه زمان‌بندی عقب‌تر است.';
      default:
        return 'یک اعلان جدید دریافت کردید.';
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">مرکز اعلان‌ها</h1>
          <p className="text-sm text-brand-dark/60">یادآوری‌ها و هشدارهای تحصیلی سیستم آموینو</p>
        </div>

        <button
          onClick={handleMarkAllAsRead}
          className="bg-white border border-brand-peach text-brand-teal px-4 py-2 rounded-2xl text-xs font-bold flex items-center gap-2 hover:bg-brand-peach/20 transition-all self-start sm:self-auto"
        >
          <CheckCheck className="w-4 h-4" />
          علامت‌گذاری همه به عنوان خوانده‌شده
        </button>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت اعلان‌ها...</div>
      ) : notifications.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-3xl border border-brand-peach/80 text-brand-dark/50">
          اعلا‌نی برای نمایش وجود ندارد.
        </div>
      ) : (
        <div className="space-y-3">
          {notifications.map((notif) => (
            <div
              key={notif.id}
              onClick={() => !notif.isRead && handleMarkAsRead(notif.id)}
              className={`p-4 sm:p-5 rounded-3xl border transition-all cursor-pointer flex items-center justify-between gap-4 ${
                notif.isRead
                  ? 'bg-white border-brand-peach/60 opacity-70'
                  : 'bg-brand-peach/30 border-brand-teal shadow-xs font-semibold'
              }`}
            >
              <div className="flex items-center gap-3.5">
                <div className={`p-3 rounded-2xl shrink-0 ${notif.isRead ? 'bg-slate-100 text-slate-500' : 'bg-brand-teal text-white'}`}>
                  <Bell className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-xs sm:text-sm text-brand-dark leading-relaxed">{renderNotificationMessage(notif)}</p>
                  <p className="text-[10px] text-brand-dark/50 mt-1 fa-num">{toJalaliDateTime(notif.createdAtUtc)}</p>
                </div>
              </div>

              {!notif.isRead && (
                <span className="w-2.5 h-2.5 bg-brand-rose rounded-full shrink-0" title="خوانده‌نشده" />
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}