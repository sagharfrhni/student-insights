import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, toJalaliDateTime } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import ConfirmModal from '../components/ConfirmModal';
import CustomSelect from '../components/CustomSelect';
import { Plus, Trash2, Edit2, GraduationCap, Award } from 'lucide-react';

const mockExamsList = [
  { id: '1', courseId: 'c1', courseName: 'پایگاه داده‌ها', title: 'امتحان میانترم پایگاه داده', examDateUtc: new Date(Date.now() + 86400000 * 2).toISOString(), grade: 18.5, description: 'فصل‌های ۱ تا ۴ کتاب مرجع' },
  { id: '2', courseId: 'c2', courseName: 'هوش مصنوعی', title: 'امتحان پایانترم هوش مصنوعی', examDateUtc: new Date(Date.now() + 86400000 * 10).toISOString(), grade: null, description: 'شامل الگوریتم‌های جست‌وجو و منطق فازی' },
];

export default function Exams() {
  const [exams, setExams] = useState([]);
  const [masterExams, setMasterExams] = useState(mockExamsList);
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedCourseFilter, setSelectedCourseFilter] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [showGradeModal, setShowGradeModal] = useState(false);
  const [selectedExam, setSelectedExam] = useState(null);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingExamId, setDeletingExamId] = useState(null);

  const [formData, setFormData] = useState({
    courseId: '',
    title: '',
    examDateUtc: new Date().toISOString(),
    description: '',
  });
  const [gradeValue, setGradeValue] = useState('');

  const fetchData = async () => {
    try {
      setLoading(true);
      const coursesRes = await api.get('/courses?pageNumber=1&pageSize=100');
      setCourses(coursesRes.data.items);

      let url = '/exams?pageNumber=1&pageSize=50';
      if (selectedCourseFilter) url += `&courseId=${selectedCourseFilter}`;
      
      const examsRes = await api.get(url);
      setExams(examsRes.data.items);
    } catch (err) {
      let filtered = [...masterExams];
      if (selectedCourseFilter) {
        filtered = filtered.filter((e) => e.courseId === selectedCourseFilter);
      }
      setExams(filtered);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedCourseFilter]);

  const handleOpenModal = (exam = null) => {
    if (exam) {
      setSelectedExam(exam);
      setFormData({
        courseId: exam.courseId,
        title: exam.title,
        examDateUtc: exam.examDateUtc,
        description: exam.description || '',
      });
    } else {
      setSelectedExam(null);
      setFormData({
        courseId: courses[0]?.id || '',
        title: '',
        examDateUtc: new Date().toISOString(),
        description: '',
      });
    }
    setShowModal(true);
  };

  const handleOpenGradeModal = (exam) => {
    setSelectedExam(exam);
    setGradeValue(exam.grade !== null ? exam.grade.toString() : '');
    setShowGradeModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const courseName = courses.find((c) => c.id === formData.courseId)?.name || 'درس انتخابی';

    if (selectedExam) {
      const updated = {
        ...selectedExam,
        title: formData.title,
        examDateUtc: formData.examDateUtc,
        description: formData.description,
      };
      setMasterExams((prev) => prev.map((item) => (item.id === selectedExam.id ? updated : item)));
      setExams((prev) => prev.map((item) => (item.id === selectedExam.id ? updated : item)));
    } else {
      const newExam = {
        id: Date.now().toString(),
        courseId: formData.courseId,
        courseName,
        title: formData.title,
        examDateUtc: formData.examDateUtc,
        grade: null,
        description: formData.description,
      };
      setMasterExams((prev) => [newExam, ...prev]);
      setExams((prev) => [newExam, ...prev]);
    }

    setShowModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        if (selectedExam) {
          await api.put(`/exams/${selectedExam.id}`, {
            title: formData.title,
            examDateUtc: formData.examDateUtc,
            description: formData.description,
          });
        } else {
          await api.post('/exams', formData);
        }
        fetchData();
      }
    } catch (err) {
      alert(translateError(err));
    }
  };

  const handleGradeSubmit = async (e) => {
    e.preventDefault();
    const grade = parseFloat(gradeValue);
    if (isNaN(grade) || grade < 0 || grade > 20) {
      alert('نمره باید عددی بین ۰ تا ۲۰ باشد.');
      return;
    }

    setMasterExams((prev) => prev.map((ex) => (ex.id === selectedExam.id ? { ...ex, grade } : ex)));
    setExams((prev) => prev.map((ex) => (ex.id === selectedExam.id ? { ...ex, grade } : ex)));
    setShowGradeModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.patch(`/exams/${selectedExam.id}/grade`, { grade });
        fetchData();
      }
    } catch (err) {
      alert(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setMasterExams((prev) => prev.filter((e) => e.id !== deletingExamId));
    setExams((prev) => prev.filter((e) => e.id !== deletingExamId));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/exams/${deletingExamId}`);
        fetchData();
      }
    } catch (err) {
      alert(translateError(err));
    }
  };

  const courseFilterOptions = [
    { value: '', label: 'همه درس‌ها' },
    ...courses.map((c) => ({ value: c.id, label: c.name })),
  ];

  const courseModalOptions = courses.map((c) => ({ value: c.id, label: c.name }));

  return (
    <div>
      <div className="flex flex-col md:flex-row md:items-center justify-between mb-8 gap-4">
        <div>
          <h1 className="text-2xl font-bold">مدیریت امتحانات</h1>
          <p className="text-sm text-brand-dark/60">امتحانات و ارزیابی‌های تحصیلی</p>
        </div>

        <div className="flex items-center gap-3">
          <div className="w-full sm:w-48">
            <CustomSelect
              options={courseFilterOptions}
              value={selectedCourseFilter}
              onChange={setSelectedCourseFilter}
            />
          </div>

          <button
            onClick={() => handleOpenModal()}
            className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 shrink-0 shadow-xs"
          >
            <Plus className="w-4 h-4" />
            افزودن امتحان
          </button>
        </div>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl mb-6">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت امتحانات...</div>
      ) : exams.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-3xl border border-brand-peach/80 text-brand-dark/50">
          امتحانی مطابق با فیلتر انتخابی یافت نشد.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {exams.map((exam) => (
            <div key={exam.id} className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-sm flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start mb-2">
                  <div className="flex items-center gap-3">
                    <div className="p-2.5 bg-brand-amber/30 rounded-2xl text-brand-dark">
                      <GraduationCap className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="font-bold text-lg">{exam.title}</h3>
                      <p className="text-xs text-brand-dark/60">{exam.courseName}</p>
                    </div>
                  </div>
                </div>

                <p className="text-xs text-brand-dark/70 my-3 fa-num">
                  زمان: {toJalaliDateTime(exam.examDateUtc)}
                </p>

                {exam.description && (
                  <p className="text-xs text-brand-dark/60 line-clamp-2 my-2 bg-brand-bg p-2.5 rounded-xl border border-brand-peach/30">{exam.description}</p>
                )}

                <div className="flex items-center gap-2 mt-4">
                  <span className="text-xs text-brand-dark/50">نمره امتحان:</span>
                  {exam.grade !== null ? (
                    <span className="bg-emerald-100 text-emerald-800 text-xs px-2.5 py-1 rounded-xl font-bold fa-num">
                      {toPersianDigits(exam.grade)}
                    </span>
                  ) : (
                    <span className="bg-slate-100 text-slate-500 text-xs px-2 py-0.5 rounded-xl">ثبت‌نشده</span>
                  )}
                </div>
              </div>

              <div className="flex items-center justify-between pt-4 border-t border-brand-peach/30 mt-4">
                <button
                  onClick={() => handleOpenGradeModal(exam)}
                  className="text-xs text-brand-teal flex items-center gap-1 hover:underline font-bold"
                >
                  <Award className="w-4 h-4" />
                  ثبت نمره
                </button>
                <div className="flex gap-1">
                  <button onClick={() => handleOpenModal(exam)} className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl" title="ویرایش">
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => {
                      setDeletingExamId(exam.id);
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
            <h2 className="text-lg font-bold mb-4">{selectedExam ? 'ویرایش امتحان' : 'افزودن امتحان جدید'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              {!selectedExam && (
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
                <label className="block text-sm font-medium mb-1">عنوان امتحان</label>
                <input
                  type="text"
                  required
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">تاریخ و زمان امتحان (شمسی)</label>
                <JalaliDateTimePicker
                  value={formData.examDateUtc}
                  onChange={(iso) => setFormData({ ...formData, examDateUtc: iso })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">توضیحات (اختیاری)</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
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

      
      {showGradeModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-sm border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">ثبت نمره امتحان</h2>
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
        title="حذف امتحان"
        message="آیا از حذف این امتحان اطمینان دارید؟ این عملیات قابل بازگشت نیست."
      />
    </div>
  );
}