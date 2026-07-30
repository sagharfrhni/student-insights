import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, toJalaliDate, formatMinutesToHours } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import ConfirmModal from '../components/ConfirmModal';
import CustomSelect from '../components/CustomSelect';
import { Plus, Trash2, Edit2, Clock, BookOpen, Eye, BarChart2 } from 'lucide-react';

const mockLogs = [
  { id: '1', courseId: 'c1', courseName: 'پایگاه داده‌ها', studyDateUtc: new Date().toISOString(), durationMinutes: 135, notes: 'مطالعه بخش نرمال‌سازی جدول‌ها و حل تمرینات فصل ۴' },
  { id: '2', courseId: 'c2', courseName: 'هوش مصنوعی', studyDateUtc: new Date(Date.now() - 86400000).toISOString(), durationMinutes: 90, notes: 'مرور الگوریتم A* و جست‌وجوی اول سطح' },
  { id: '3', courseId: 'c3', courseName: 'طراحی الگوریتم', studyDateUtc: new Date(Date.now() - 86400000 * 3).toISOString(), durationMinutes: 120, notes: 'تمرین الگوریتم‌های حریصانه و اثبات استقرا' },
];

export default function StudyLogs() {
  const [logs, setLogs] = useState([]);
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedCourse, setSelectedCourse] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [editingLog, setEditingLog] = useState(null);
  const [viewingLog, setViewingLog] = useState(null);

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
      const courseItems = coursesRes.data.items || coursesRes.data || [];
      setCourses(courseItems);

      const res = await api.get('/study-logs?pageNumber=1&pageSize=100');
      const logItems = res.data.items || res.data || [];
      setLogs(logItems.length > 0 ? logItems : mockLogs);
    } catch (err) {
      setLogs(mockLogs);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const getFilteredLogs = () => {
    let list = [...logs];
    if (selectedCourse) {
      list = list.filter((l) => String(l.courseId) === String(selectedCourse));
    }
    return list;
  };

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
        courseId: courses[0]?.id || 'c1',
        studyDateUtc: new Date().toISOString(),
        hours: 1,
        minutes: 30,
        notes: '',
      });
    }
    setShowModal(true);
  };

  const handleOpenViewModal = (log) => {
    setViewingLog(log);
    setShowViewModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const totalMinutes = parseInt(formData.hours) * 60 + parseInt(formData.minutes);
    if (totalMinutes <= 0 || totalMinutes > 720) {
      setError('مدت زمان مطالعه باید بین ۱ دقیقه تا ۱۲ ساعت (۷۲۰ دقیقه) باشد.');
      return;
    }

    const courseName = courses.find((c) => String(c.id) === String(formData.courseId))?.name || 'درس انتخابی';

    if (editingLog) {
      const updated = {
        ...editingLog,
        courseId: formData.courseId,
        courseName,
        studyDateUtc: formData.studyDateUtc,
        durationMinutes: totalMinutes,
        notes: formData.notes,
      };
      setLogs((prev) => prev.map((item) => (item.id === editingLog.id ? updated : item)));
    } else {
      const newLog = {
        id: Date.now().toString(),
        courseId: formData.courseId,
        courseName,
        studyDateUtc: formData.studyDateUtc,
        durationMinutes: totalMinutes,
        notes: formData.notes,
      };
      setLogs((prev) => [newLog, ...prev]);
    }

    setShowModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        const payload = {
          courseId: formData.courseId,
          studyDateUtc: formData.studyDateUtc,
          durationMinutes: totalMinutes,
          notes: formData.notes,
        };
        if (editingLog) {
          await api.put(`/study-logs/${editingLog.id}`, payload);
        } else {
          await api.post('/study-logs', payload);
        }
      }
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

  const displayLogs = getFilteredLogs();
  const totalMinutesAll = displayLogs.reduce((acc, curr) => acc + (curr.durationMinutes || 0), 0);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">جلسات مطالعه</h1>
          <p className="text-sm text-brand-dark/60 dark:text-[#F4F0FA]/60">ثبت، پایش و تحلیل کامل زمان‌های مطالعه دروس</p>
        </div>

        <button
          onClick={() => handleOpenModal()}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-bold flex items-center gap-2 self-start sm:self-auto shadow-xs hover:bg-brand-teal/90"
        >
          <Plus className="w-4 h-4" />
          ثبت جلسه جدید
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="bg-white dark:bg-[#221A32] p-5 rounded-3xl border border-brand-peach/80 dark:border-[#541532] flex items-center gap-4 shadow-xs">
          <div className="bg-brand-teal text-white p-3 rounded-2xl shrink-0">
            <Clock className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60 dark:text-[#F4F0FA]/60">مجموع زمان مطالعه نمایش داده‌شده</p>
            <p className="text-lg font-bold fa-num">{formatMinutesToHours(totalMinutesAll)}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-[#221A32] p-5 rounded-3xl border border-brand-peach/80 dark:border-[#541532] flex items-center gap-4 shadow-xs">
          <div className="bg-[#826F9D] text-white p-3 rounded-2xl shrink-0">
            <BarChart2 className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60 dark:text-[#F4F0FA]/60">تعداد کل جلسات ثبت‌شده</p>
            <p className="text-lg font-bold fa-num">{toPersianDigits(displayLogs.length)} جلسه</p>
          </div>
        </div>
      </div>

      <div className="w-full sm:w-60">
        <CustomSelect
          options={courseFilterOptions}
          value={selectedCourse}
          onChange={setSelectedCourse}
        />
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت جلسات مطالعه...</div>
      ) : displayLogs.length === 0 ? (
        <div className="text-center py-12 bg-white dark:bg-[#221A32] rounded-3xl border border-brand-peach/80 dark:border-[#541532] text-brand-dark/50">
          جلسه‌ای مطابق با فیلتر انتخابی یافت نشد.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {displayLogs.map((log) => (
            <div key={log.id} className="bg-white dark:bg-[#221A32] p-6 rounded-3xl border border-brand-peach/80 dark:border-[#541532] shadow-xs flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start mb-3">
                  <div className="flex items-center gap-2.5">
                    <div className="p-2.5 bg-brand-peach/40 dark:bg-[#541532] rounded-2xl text-brand-teal dark:text-[#BFABDE]">
                      <BookOpen className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="font-bold text-base text-brand-dark dark:text-[#F4F0FA]">{log.courseName}</h3>
                      <p className="text-xs text-brand-dark/50 dark:text-[#F4F0FA]/50 fa-num">{toJalaliDate(log.studyDateUtc)}</p>
                    </div>
                  </div>
                </div>

                <div className="inline-flex items-center gap-2 bg-brand-amber/20 dark:bg-[#722549]/40 text-brand-dark dark:text-[#F4F0FA] px-3 py-1 rounded-xl text-xs font-bold my-2 fa-num">
                  <Clock className="w-3.5 h-3.5 text-brand-teal" />
                  مدت مطالعه: {formatMinutesToHours(log.durationMinutes)}
                </div>

                {log.notes && (
                  <p className="text-xs text-brand-dark/70 dark:text-[#F4F0FA]/70 line-clamp-3 my-2 bg-brand-bg dark:bg-[#060407] p-3 rounded-2xl border border-brand-peach/30 dark:border-[#541532]">
                    {log.notes}
                  </p>
                )}
              </div>

              <div className="flex justify-between items-center pt-4 border-t border-brand-peach/30 dark:border-[#541532] mt-4">
                <button
                  onClick={() => handleOpenViewModal(log)}
                  className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl"
                  title="مشاهده جزئیات"
                >
                  <Eye className="w-4 h-4" />
                </button>

                <div className="flex gap-1">
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
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg dark:bg-[#221A32] rounded-3xl p-6 w-full max-w-md border border-brand-peach dark:border-[#541532] shadow-xl">
            <h2 className="text-lg font-bold mb-4">{editingLog ? 'ویرایش جلسه مطالعه' : 'ثبت جلسه مطالعه جدید'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">درس مربوطه</label>
                <CustomSelect
                  options={courseModalOptions}
                  value={formData.courseId}
                  placeholder="انتخاب درس..."
                  onChange={(val) => setFormData({ ...formData, courseId: val })}
                />
              </div>

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
                      className="w-full border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#060407] text-brand-dark dark:text-[#F4F0FA] rounded-2xl px-4 py-2 text-sm text-center fa-num"
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
                      className="w-full border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#060407] text-brand-dark dark:text-[#F4F0FA] rounded-2xl px-4 py-2 text-sm text-center fa-num"
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">یادداشت (اختیاری)</label>
                <textarea
                  value={formData.notes}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  className="w-full border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#060407] text-brand-dark dark:text-[#F4F0FA] rounded-2xl px-4 py-2.5 text-sm"
                  rows="3"
                />
              </div>

              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach dark:border-[#541532]">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white font-bold">
                  ذخیره
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showViewModal && viewingLog && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-bg dark:bg-[#221A32] rounded-3xl p-6 w-full max-w-md border border-brand-peach dark:border-[#541532] shadow-xl space-y-4">
            <h2 className="text-base font-bold text-brand-dark dark:text-[#F4F0FA]">جزئیات جلسه مطالعه</h2>
            <div className="space-y-2 text-xs">
              <div className="p-2 bg-white dark:bg-[#060407] rounded-xl flex justify-between">
                <span className="font-bold text-brand-dark/60">درس:</span>
                <span className="font-bold">{viewingLog.courseName}</span>
              </div>
              <div className="p-2 bg-white dark:bg-[#060407] rounded-xl flex justify-between">
                <span className="font-bold text-brand-dark/60">تاریخ:</span>
                <span className="font-bold fa-num">{toJalaliDate(viewingLog.studyDateUtc)}</span>
              </div>
              <div className="p-2 bg-white dark:bg-[#060407] rounded-xl flex justify-between">
                <span className="font-bold text-brand-dark/60">مدت:</span>
                <span className="font-bold fa-num">{formatMinutesToHours(viewingLog.durationMinutes)}</span>
              </div>
              {viewingLog.notes && (
                <div className="p-3 bg-white dark:bg-[#060407] rounded-2xl space-y-1">
                  <span className="font-bold block text-brand-dark/60">یادداشت:</span>
                  <p className="leading-relaxed">{viewingLog.notes}</p>
                </div>
              )}
            </div>
            <div className="flex justify-end">
              <button onClick={() => setShowViewModal(false)} className="px-4 py-2 text-xs font-bold bg-brand-teal text-white rounded-xl">
                بستن
              </button>
            </div>
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