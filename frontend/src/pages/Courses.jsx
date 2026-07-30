import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import ConfirmModal from '../components/ConfirmModal';
import { Plus, Trash2, Edit2, BookOpen, Award } from 'lucide-react';

const mockCoursesList = [
  { id: 'c1', name: 'پایگاه داده‌ها', credits: 3, instructorName: 'دکتر علوی', finalGrade: 18.5, createdAtUtc: new Date().toISOString() },
  { id: 'c2', name: 'هوش مصنوعی', credits: 3, instructorName: 'دکتر حسینی', finalGrade: 17.0, createdAtUtc: new Date().toISOString() },
  { id: 'c3', name: 'شبکه‌های کامپیوتری', credits: 3, instructorName: 'مهندس رضایی', finalGrade: null, createdAtUtc: new Date().toISOString() },
  { id: 'c4', name: 'طراحی الگوریتم', credits: 3, instructorName: 'دکتر محمدی', finalGrade: 19.25, createdAtUtc: new Date().toISOString() },
  { id: 'c5', name: 'زبان تخصصی', credits: 2, instructorName: 'استاد شریفی', finalGrade: null, createdAtUtc: new Date().toISOString() },
];

export default function Courses() {
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showModal, setShowModal] = useState(false);
  const [showGradeModal, setShowGradeModal] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState(null);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  const [formData, setFormData] = useState({ name: '', credits: 3, instructorName: '' });
  const [gradeValue, setGradeValue] = useState('');

  const fetchCourses = async () => {
    try {
      setLoading(true);
      const res = await api.get('/courses?pageNumber=1&pageSize=50');
      setCourses(res.data.items);
    } catch (err) {
      
      if (courses.length === 0) {
        setCourses(mockCoursesList);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCourses();
  }, []);

  const handleOpenModal = (course = null) => {
    if (course) {
      setSelectedCourse(course);
      setFormData({ name: course.name, credits: course.credits, instructorName: course.instructorName || '' });
    } else {
      setSelectedCourse(null);
      setFormData({ name: '', credits: 3, instructorName: '' });
    }
    setShowModal(true);
  };

  const handleOpenGradeModal = (course) => {
    setSelectedCourse(course);
    setGradeValue(course.finalGrade !== null ? course.finalGrade.toString() : '');
    setShowGradeModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (selectedCourse) {
      setCourses((prev) =>
        prev.map((c) => (c.id === selectedCourse.id ? { ...c, ...formData } : c))
      );
    } else {
      const newCourse = {
        id: `c_${Date.now()}`,
        name: formData.name,
        credits: formData.credits,
        instructorName: formData.instructorName,
        finalGrade: null,
        createdAtUtc: new Date().toISOString(),
      };
      setCourses((prev) => [newCourse, ...prev]);
    }

    setShowModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        if (selectedCourse) {
          await api.put(`/courses/${selectedCourse.id}`, formData);
        } else {
          await api.post('/courses', formData);
        }
        fetchCourses();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  const handleGradeSubmit = async (e) => {
    e.preventDefault();
    const grade = parseFloat(gradeValue);
    if (isNaN(grade) || grade < 0 || grade > 20) {
      setError('نمره باید عددی بین ۰ تا ۲۰ باشد.');
      return;
    }

    setCourses((prev) =>
      prev.map((c) => (c.id === selectedCourse.id ? { ...c, finalGrade: grade } : c))
    );
    setShowGradeModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.patch(`/courses/${selectedCourse.id}/grade`, { grade });
        fetchCourses();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setCourses((prev) => prev.filter((c) => c.id !== deletingId));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/courses/${deletingId}`);
        fetchCourses();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold">دروس من</h1>
          <p className="text-sm text-brand-dark/60">لیست دروس ثبت‌شده در ترم جاری</p>
        </div>
        <button
          onClick={() => handleOpenModal()}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 shadow-xs hover:bg-brand-teal/90"
        >
          <Plus className="w-4 h-4" />
          افزودن درس جدید
        </button>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl mb-6">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت دروس...</div>
      ) : courses.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-3xl border border-brand-peach/80 text-brand-dark/50">
          درسی ثبت نشده است. برای افزودن اولین درس دکمه بالا را بزنید.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {courses.map((course) => (
            <div key={course.id} className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start mb-3">
                  <div className="flex items-center gap-3">
                    <div className="p-2.5 bg-brand-peach/40 rounded-2xl text-brand-teal">
                      <BookOpen className="w-5 h-5" />
                    </div>
                    <h3 className="font-bold text-lg">{course.name}</h3>
                  </div>
                  <span className="bg-brand-amber/30 text-brand-dark text-xs px-3 py-1 rounded-full font-medium fa-num">
                    {toPersianDigits(course.credits)} واحد
                  </span>
                </div>

                <p className="text-sm text-brand-dark/70 my-3">
                  <span className="text-brand-dark/40 ml-1">استاد:</span>
                  {course.instructorName || 'ثبت نشده'}
                </p>

                <div className="flex items-center gap-2 mt-4">
                  <span className="text-xs text-brand-dark/50">نمره نهایی:</span>
                  {course.finalGrade !== null ? (
                    <span className="bg-emerald-100 text-emerald-800 text-xs px-2.5 py-1 rounded-xl font-bold fa-num">
                      {toPersianDigits(course.finalGrade)}
                    </span>
                  ) : (
                    <span className="bg-slate-100 text-slate-500 text-xs px-2 py-0.5 rounded-xl">ثبت‌نشده</span>
                  )}
                </div>
              </div>

              <div className="flex items-center justify-between pt-4 border-t border-brand-peach/30 mt-4">
                <button
                  onClick={() => handleOpenGradeModal(course)}
                  className="text-xs text-brand-teal font-bold flex items-center gap-1 hover:underline"
                >
                  <Award className="w-4 h-4" />
                  ثبت/ویرایش نمره
                </button>
                <div className="flex gap-1">
                  <button onClick={() => handleOpenModal(course)} className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl" title="ویرایش">
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => {
                      setDeletingId(course.id);
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
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-md border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">{selectedCourse ? 'ویرایش درس' : 'افزودن درس جدید'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">نام درس</label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">تعداد واحد</label>
                <input
                  type="number"
                  min="1"
                  required
                  value={formData.credits}
                  onChange={(e) => setFormData({ ...formData, credits: parseInt(e.target.value) })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">نام استاد</label>
                <input
                  type="text"
                  value={formData.instructorName}
                  onChange={(e) => setFormData({ ...formData, instructorName: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
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

      
      {showGradeModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-sm border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">ثبت نمره نهایی درس</h2>
            <form onSubmit={handleGradeSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">نمره (از ۲۰)</label>
                <input
                  type="number"
                  step="0.25"
                  min="0"
                  max="20"
                  required
                  value={gradeValue}
                  onChange={(e) => setGradeValue(e.target.value)}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm fa-num"
                />
              </div>
              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowGradeModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white">
                  ثبت نمره
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
        title="حذف درس"
        message="آیا از حذف این درس اطمینان دارید؟ تمام امتحانات، تکالیف و جلسات مطالعه مربوط به این درس نیز حذف خواهند شد."
      />
    </div>
  );
}