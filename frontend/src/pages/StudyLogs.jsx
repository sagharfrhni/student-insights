import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, toJalaliDate, formatMinutesToHours } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import ConfirmModal from '../components/ConfirmModal';
import CustomSelect from '../components/CustomSelect';
import { Plus, Trash2, Edit2, Clock, BookOpen } from 'lucide-react';

const mockLogs = [
  { id: '1', courseId: 'c1', courseName: 'پایگاه داده‌ها', studyDateUtc: new Date().toISOString(), durationMinutes: 135, notes: 'مطالعه بخش نرمال‌سازی جدول‌ها و حل تمرینات فصل ۴' },
  { id: '2', courseId: 'c2', courseName: 'هوش مصنوعی', studyDateUtc: new Date(Date.now() - 86400000).toISOString(), durationMinutes: 90, notes: 'مرور الگوریتم A* و جست‌وجوی اول سطح' },
];

export default function StudyLogs() {
  const [logs, setLogs] = useState([]);
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedCourse, setSelectedCourse] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingLog, setEditingLog] = useState(null);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  const [formData, setFormData] = useState({
    courseId: '',
    studyDateUtc: new Date().toISOString(),
    hours: 1,
    minutes: 30,
    notes: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const coursesRes = await api.get('/courses?pageNumber=1&pageSize=100');
      setCourses(coursesRes.data.items);

      let url = '/study-logs?pageNumber=1&pageSize=50';
      if (selectedCourse) url += `&courseId=${selectedCourse}`;

      const res = await api.get(url);
      setLogs(res.data.items);
    } catch (err) {
      setLogs(mockLogs);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedCourse]);

  const handleOpenModal = (log = null) => {
    if (log) {
      setEditingLog(log);
      const h = Math.floor(log.durationMinutes / 60);
      const m = log.durationMinutes % 60;
      setFormData({
        courseId: log.courseId,
        studyDateUtc: log.studyDateUtc,
        hours: h,
        minutes: m,
        notes: log.notes || '',
      });
    } else {
      setEditingLog(null);
      setFormData({
        courseId: courses[0]?.id || '',
        studyDateUtc: new Date().toISOString(),
        hours: 1,
        minutes: 30,
        notes: '',
      });
    }
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const totalMinutes = parseInt(formData.hours) * 60 + parseInt(formData.minutes);
    if (totalMinutes <= 0 || totalMinutes > 720) {
      setError('مدت زمان مطالعه باید بین ۱ دقیقه تا ۱۲ ساعت (۷۲۰ دقیقه) باشد.');
      return;
    }

    try {
      const payload = {
        courseId: formData.courseId,
        studyDateUtc: formData.studyDateUtc,
        durationMinutes: totalMinutes,
        notes: formData.notes,
      };

      if (editingLog) {
        await api.put(`/study-logs/${editingLog.id}`, {
          studyDateUtc: payload.studyDateUtc,
          durationMinutes: payload.durationMinutes,
          notes: payload.notes,
        });
      } else {
        await api.post('/study-logs', payload);
      }
      setShowModal(false);
      fetchData();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setLogs((prev) => prev.filter((l) => l.id !== deletingId));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/study-logs/${deletingId}`);
        fetchData();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  
  const courseFilterOptions = [
    { value: '', label: 'همه درس‌ها' },
    ...courses.map((c) => ({ value: c.id, label: c.name })),
  ];

  const courseModalOptions = courses.map((c) => ({ value: c.id, label: c.name }));

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">جلسات مطالعه</h1>
          <p className="text-sm text-brand-dark/60">ثبت و پایش زمان‌های مطالعه دروس</p>
        </div>

        <button
          onClick={() => handleOpenModal()}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 self-start sm:self-auto shadow-xs hover:bg-brand-teal/90"
        >
          <Plus className="w-4 h-4" />
          ثبت جلسه جدید
        </button>
      </div>

      <div className="w-56">
        <CustomSelect
          options={courseFilterOptions}
          value={selectedCourse}
          onChange={setSelectedCourse}
        />
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت جلسات مطالعه...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {logs.map((log) => (
            <div key={log.id} className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start mb-3">
                  <div className="flex items-center gap-2.5">
                    <div className="p-2.5 bg-brand-peach/40 rounded-2xl text-brand-teal">
                      <BookOpen className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="font-bold text-base">{log.courseName}</h3>
                      <p className="text-xs text-brand-dark/50 fa-num">{toJalaliDate(log.studyDateUtc)}</p>
                    </div>
                  </div>
                </div>

                <div className="inline-flex items-center gap-2 bg-brand-amber/20 text-brand-dark px-3 py-1 rounded-xl text-xs font-bold my-2 fa-num">
                  <Clock className="w-3.5 h-3.5 text-brand-teal" />
                  مدت مطالعه: {formatMinutesToHours(log.durationMinutes)}
                </div>

                {log.notes && (
                  <p className="text-xs text-brand-dark/70 line-clamp-3 my-2 bg-brand-bg p-3 rounded-2xl border border-brand-peach/30">
                    {log.notes}
                  </p>
                )}
              </div>

              <div className="flex justify-end gap-1 pt-4 border-t border-brand-peach/30 mt-4">
                <button onClick={() => handleOpenModal(log)} className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl" title="ویرایش">
                  <Edit2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => {
                    setDeletingId(log.id);
                    setDeleteModalOpen(true);
                  }}
                  className="p-2 text-brand-dark/50 hover:text-brand-rose rounded-xl"
                  title="حذف"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      
      {showModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-md border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">{editingLog ? 'ویرایش جلسه مطالعه' : 'ثبت جلسه مطالعه جدید'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              {!editingLog && (
                <div>
                  <label className="block text-sm font-medium mb-1">درس مربوطه</label>
                  <CustomSelect
                    options={courseModalOptions}
                    value={formData.courseId}
                    placeholder="انتخاب درس..."
                    onChange={(val) => setFormData({ ...formData, courseId: val })}
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium mb-1">تاریخ مطالعه (شمسی)</label>
                <JalaliDateTimePicker
                  value={formData.studyDateUtc}
                  includeTime={false}
                  onChange={(iso) => setFormData({ ...formData, studyDateUtc: iso })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">مدت زمان مطالعه (حداکثر ۱۲ ساعت)</label>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <span className="text-xs text-brand-dark/60 block mb-1">ساعت</span>
                    <input
                      type="number"
                      min="0"
                      max="12"
                      value={formData.hours}
                      onChange={(e) => setFormData({ ...formData, hours: parseInt(e.target.value) || 0 })}
                      className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2 text-sm text-center fa-num"
                    />
                  </div>
                  <div>
                    <span className="text-xs text-brand-dark/60 block mb-1">دقیقه</span>
                    <input
                      type="number"
                      min="0"
                      max="59"
                      value={formData.minutes}
                      onChange={(e) => setFormData({ ...formData, minutes: parseInt(e.target.value) || 0 })}
                      className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2 text-sm text-center fa-num"
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">یادداشت (اختیاری)</label>
                <textarea
                  value={formData.notes}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                  rows="3"
                />
              </div>

              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white">
                  ذخیره
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <ConfirmModal
        isOpen={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={confirmDelete}
        title="حذف جلسه مطالعه"
        message="آیا از حذف این جلسه مطالعه اطمینان دارید؟"
      />
    </div>
  );
}