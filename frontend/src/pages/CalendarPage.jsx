import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toJalaliDate } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import { Plus, GraduationCap, CheckSquare, Target, Calendar as CalendarIcon, User } from 'lucide-react';

const mockCalendarEvents = [
  { type: 'Exam', title: 'امتحان میانترم پایگاه داده', startAtUtc: new Date(Date.now() + 86400000 * 2).toISOString() },
  { type: 'Deadline', title: 'تحویل تمرین فصل ۳ هوش مصنوعی', startAtUtc: new Date(Date.now() + 86400000 * 4).toISOString() },
  { type: 'Personal', title: 'جلسه با استاد راهنما', startAtUtc: new Date(Date.now() + 86400000 * 6).toISOString() },
];

export default function CalendarPage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showEventModal, setShowEventModal] = useState(false);
  const [personalEvent, setPersonalEvent] = useState({
    title: '',
    startAtUtc: new Date().toISOString(),
    endAtUtc: new Date().toISOString(),
    isAllDay: false,
    description: '',
  });

  const fetchCalendar = async () => {
    try {
      setLoading(true);
      const from = new Date();
      const to = new Date();
      to.setDate(to.getDate() + 60);

      const url = `/calendar?from=${from.toISOString()}&to=${to.toISOString()}`;
      const res = await api.get(url);
      setEvents(res.data);
    } catch (err) {
      setEvents(mockCalendarEvents);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCalendar();
  }, []);

  const handleCreatePersonalEvent = async (e) => {
    e.preventDefault();
    try {
      await api.post('/personal-events', personalEvent);
      setShowEventModal(false);
      fetchCalendar();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const getEventBadge = (type) => {
    switch (type) {
      case 'Exam':
        return <span className="bg-purple-100 text-purple-800 text-xs px-2.5 py-1 rounded-xl font-bold flex items-center gap-1"><GraduationCap className="w-3.5 h-3.5" /> امتحان</span>;
      case 'Deadline':
        return <span className="bg-brand-amber/30 text-brand-dark text-xs px-2.5 py-1 rounded-xl font-bold flex items-center gap-1"><CheckSquare className="w-3.5 h-3.5" /> مهلت تحویل</span>;
      case 'Goal':
        return <span className="bg-brand-teal/20 text-brand-teal text-xs px-2.5 py-1 rounded-xl font-bold flex items-center gap-1"><Target className="w-3.5 h-3.5" /> هدف</span>;
      default:
        return <span className="bg-brand-peach/60 text-brand-dark text-xs px-2.5 py-1 rounded-xl font-bold flex items-center gap-1"><User className="w-3.5 h-3.5" /> رویداد شخصی</span>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">تقویم تحصیلی یکپارچه</h1>
          <p className="text-sm text-brand-dark/60">دید زمان‌بندی‌شده از امتحانات، مهلت‌ها و رویدادها</p>
        </div>

        <button
          onClick={() => {
            const now = new Date().toISOString();
            setPersonalEvent({ title: '', startAtUtc: now, endAtUtc: now, isAllDay: false, description: '' });
            setShowEventModal(true);
          }}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 self-start sm:self-auto"
        >
          <Plus className="w-4 h-4" />
          افزودن رویداد شخصی
        </button>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت رویدادهای تقویم...</div>
      ) : (
        <div className="space-y-3">
          {events.map((ev, idx) => (
            <div key={idx} className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center justify-between gap-4">
              <div className="flex items-center gap-3.5">
                <div className="p-3 bg-brand-bg rounded-2xl">
                  <CalendarIcon className="w-5 h-5 text-brand-teal" />
                </div>
                <div>
                  <p className="font-bold text-sm">{ev.title}</p>
                  <p className="text-xs text-brand-dark/50 fa-num mt-0.5">{toJalaliDate(ev.startAtUtc)}</p>
                </div>
              </div>

              <div>{getEventBadge(ev.type)}</div>
            </div>
          ))}
        </div>
      )}

      
      {showEventModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-md border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">افزودن رویداد شخصی جدید</h2>
            <form onSubmit={handleCreatePersonalEvent} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">عنوان رویداد</label>
                <input
                  type="text"
                  required
                  value={personalEvent.title}
                  onChange={(e) => setPersonalEvent({ ...personalEvent, title: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">زمان شروع</label>
                <JalaliDateTimePicker
                  value={personalEvent.startAtUtc}
                  onChange={(iso) => setPersonalEvent({ ...personalEvent, startAtUtc: iso })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">زمان پایان</label>
                <JalaliDateTimePicker
                  value={personalEvent.endAtUtc}
                  onChange={(iso) => setPersonalEvent({ ...personalEvent, endAtUtc: iso })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">توضیحات (اختیاری)</label>
                <textarea
                  value={personalEvent.description}
                  onChange={(e) => setPersonalEvent({ ...personalEvent, description: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                  rows="2"
                />
              </div>

              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowEventModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white">
                  ذخیره رویداد
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}