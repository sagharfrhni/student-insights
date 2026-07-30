import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toJalaliDateTime } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import ConfirmModal from '../components/ConfirmModal';
import CustomSelect from '../components/CustomSelect';
import { Plus, Trash2, Edit2, Clock, CheckCircle2, Circle } from 'lucide-react';

const initialMockActivities = [
  { id: '1', courseId: 'c1', courseName: 'پایگاه داده‌ها', title: 'تمرین شماره ۳ - نرمال‌سازی', type: 'Assignment', dueDateUtc: new Date(Date.now() + 86400000 * 3).toISOString(), priority: 'High', status: 'InProgress', description: 'حل سؤالات ۱ تا ۵ فصل چهارم کتاب اصلی' },
  { id: '2', courseId: 'c2', courseName: 'هوش مصنوعی', title: 'پروژه فاز اول - پیاده‌سازی A*', type: 'Project', dueDateUtc: new Date(Date.now() + 86400000 * 7).toISOString(), priority: 'Medium', status: 'NotStarted', description: 'کدنویسی به زبان پایتون همراه با گزارش مستندات' },
];

const priorityOptions = [
  { value: 0, label: 'کم' },
  { value: 1, label: 'متوسط' },
  { value: 2, label: 'بالا' },
];

const typeOptions = [
  { value: 0, label: 'تکلیف' },
  { value: 1, label: 'پروژه' },
];

export default function LearningActivities() {
  const [activities, setActivities] = useState([]);
  const [masterActivities, setMasterActivities] = useState(initialMockActivities);
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedCourse, setSelectedCourse] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');
  const [selectedType, setSelectedType] = useState('');

  const [showModal, setShowModal] = useState(false);
  const [editingActivity, setEditingActivity] = useState(null);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  const [formData, setFormData] = useState({
    courseId: '',
    title: '',
    type: 0,
    dueDateUtc: new Date().toISOString(),
    priority: 1,
    description: '',
    resourceLink: '',
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const coursesRes = await api.get('/courses?pageNumber=1&pageSize=100');
      setCourses(coursesRes.data.items);

      let url = '/learning-activities?pageNumber=1&pageSize=50';
      if (selectedCourse) url += `&courseId=${selectedCourse}`;
      if (selectedStatus !== '') url += `&status=${selectedStatus}`;
      if (selectedType !== '') url += `&type=${selectedType}`;

      const res = await api.get(url);
      setActivities(res.data.items);
    } catch (err) {
      
      let filtered = [...masterActivities];
      if (selectedCourse) {
        filtered = filtered.filter((a) => a.courseId === selectedCourse);
      }
      if (selectedStatus !== '') {
        const statusStr = selectedStatus === '2' ? 'Completed' : selectedStatus === '1' ? 'InProgress' : 'NotStarted';
        filtered = filtered.filter((a) => a.status === statusStr);
      }
      if (selectedType !== '') {
        const typeStr = selectedType === '1' ? 'Project' : 'Assignment';
        filtered = filtered.filter((a) => a.type === typeStr);
      }
      setActivities(filtered);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedCourse, selectedStatus, selectedType]);

  const handleOpenModal = (act = null) => {
    if (act) {
      setEditingActivity(act);
      setFormData({
        courseId: act.courseId,
        title: act.title,
        type: act.type === 'Project' ? 1 : 0,
        dueDateUtc: act.dueDateUtc,
        priority: act.priority === 'High' ? 2 : act.priority === 'Medium' ? 1 : 0,
        description: act.description || '',
        resourceLink: act.resourceLink || '',
      });
    } else {
      setEditingActivity(null);
      setFormData({
        courseId: courses[0]?.id || '',
        title: '',
        type: 0,
        dueDateUtc: new Date().toISOString(),
        priority: 1,
        description: '',
        resourceLink: '',
      });
    }
    setShowModal(true);
  };

  const handleStatusChange = async (id, newStatus) => {
    const statusString = newStatus === 2 ? 'Completed' : newStatus === 1 ? 'InProgress' : 'NotStarted';

    setMasterActivities((prev) =>
      prev.map((item) => (item.id === id ? { ...item, status: statusString } : item))
    );
    setActivities((prev) =>
      prev.map((item) => (item.id === id ? { ...item, status: statusString } : item))
    );

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.patch(`/learning-activities/${id}/status`, { newStatus });
      }
    } catch (err) {
      fetchData();
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const priorityString = formData.priority === 2 ? 'High' : formData.priority === 1 ? 'Medium' : 'Low';
    const typeString = formData.type === 1 ? 'Project' : 'Assignment';
    const courseName = courses.find((c) => c.id === formData.courseId)?.name || 'درس انتخابی';

    if (editingActivity) {
      const updated = {
        ...editingActivity,
        title: formData.title,
        dueDateUtc: formData.dueDateUtc,
        priority: priorityString,
        description: formData.description,
        resourceLink: formData.resourceLink,
      };
      setMasterActivities((prev) => prev.map((item) => (item.id === editingActivity.id ? updated : item)));
      setActivities((prev) => prev.map((item) => (item.id === editingActivity.id ? updated : item)));
    } else {
      const newAct = {
        id: Date.now().toString(),
        courseId: formData.courseId,
        courseName,
        title: formData.title,
        type: typeString,
        dueDateUtc: formData.dueDateUtc,
        priority: priorityString,
        status: 'NotStarted',
        description: formData.description,
        resourceLink: formData.resourceLink,
      };
      setMasterActivities((prev) => [newAct, ...prev]);
      setActivities((prev) => [newAct, ...prev]);
    }

    setShowModal(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        if (editingActivity) {
          await api.put(`/learning-activities/${editingActivity.id}`, {
            title: formData.title,
            dueDateUtc: formData.dueDateUtc,
            priority: formData.priority,
            description: formData.description,
            resourceLink: formData.resourceLink,
          });
        } else {
          await api.post('/learning-activities', formData);
        }
        fetchData();
      }
    } catch (err) {
      alert(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setMasterActivities((prev) => prev.filter((item) => item.id !== deletingId));
    setActivities((prev) => prev.filter((item) => item.id !== deletingId));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/learning-activities/${deletingId}`);
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

  const statusFilterOptions = [
    { value: '', label: 'همه وضعیت‌ها' },
    { value: '0', label: 'شروع‌نشده' },
    { value: '1', label: 'در حال انجام' },
    { value: '2', label: 'تکمیل‌شده' },
  ];

  const typeFilterOptions = [
    { value: '', label: 'همه انواع' },
    { value: '0', label: 'تکلیف' },
    { value: '1', label: 'پروژه' },
  ];

  const courseModalOptions = courses.map((c) => ({ value: c.id, label: c.name }));

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">تکالیف و پروژه‌ها</h1>
          <p className="text-sm text-brand-dark/60">مدیریت و پیگیری فعالیت‌های تحصیلی</p>
        </div>

        <button
          onClick={() => handleOpenModal()}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 self-start sm:self-auto shadow-xs"
        >
          <Plus className="w-4 h-4" />
          افزودن فعالیت
        </button>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <div className="w-full sm:w-48">
          <CustomSelect
            options={courseFilterOptions}
            value={selectedCourse}
            onChange={setSelectedCourse}
          />
        </div>

        <div className="w-full sm:w-40">
          <CustomSelect
            options={statusFilterOptions}
            value={selectedStatus}
            onChange={setSelectedStatus}
          />
        </div>

        <div className="w-full sm:w-40">
          <CustomSelect
            options={typeFilterOptions}
            value={selectedType}
            onChange={setSelectedType}
          />
        </div>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت فعالیت‌ها...</div>
      ) : activities.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-3xl border border-brand-peach/80 text-brand-dark/50">
          فعالیتی مطابق با فیلترهای انتخابی یافت نشد.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {activities.map((act) => (
            <div key={act.id} className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start mb-2">
                  <div className="flex items-center gap-2">
                    <span className={`text-[10px] px-2.5 py-0.5 rounded-full font-bold ${
                      act.type === 'Project' ? 'bg-brand-rose/20 text-brand-dark' : 'bg-brand-amber/30 text-brand-dark'
                    }`}>
                      {act.type === 'Project' ? 'پروژه' : 'تکلیف'}
                    </span>
                    <span className="text-xs text-brand-dark/60">{act.courseName}</span>
                  </div>

                  <StatusBadge status={act.status} />
                </div>

                <h3 className="font-bold text-lg my-2">{act.title}</h3>

                <p className="text-xs text-brand-dark/70 my-2 fa-num">
                  مهلت: {toJalaliDateTime(act.dueDateUtc)}
                </p>

                {act.description && (
                  <p className="text-xs text-brand-dark/60 line-clamp-2 my-2 bg-brand-bg p-2.5 rounded-xl border border-brand-peach/30">{act.description}</p>
                )}
              </div>

              <div className="pt-4 border-t border-brand-peach/30 mt-4 flex items-center justify-between">
                <span className="text-xs text-brand-dark/50 font-bold">وضعیت:</span>

                <div className="flex items-center gap-1.5 bg-brand-bg p-1 rounded-2xl border border-brand-peach/60">
                  <button
                    type="button"
                    onClick={() => handleStatusChange(act.id, 0)}
                    className={`p-2 rounded-xl transition-all ${
                      act.status === 'NotStarted'
                        ? 'bg-slate-600 text-white shadow-xs scale-105'
                        : 'text-brand-dark/40 hover:text-brand-dark hover:bg-slate-200/50'
                    }`}
                    title="شروع‌نشده"
                  >
                    <Circle className="w-4 h-4" />
                  </button>

                  <button
                    type="button"
                    onClick={() => handleStatusChange(act.id, 1)}
                    className={`p-2 rounded-xl transition-all ${
                      act.status === 'InProgress'
                        ? 'bg-brand-amber text-brand-dark font-bold shadow-xs scale-105'
                        : 'text-brand-dark/40 hover:text-brand-dark hover:bg-brand-amber/20'
                    }`}
                    title="در حال انجام"
                  >
                    <Clock className="w-4 h-4" />
                  </button>

                  <button
                    type="button"
                    onClick={() => handleStatusChange(act.id, 2)}
                    className={`p-2 rounded-xl transition-all ${
                      act.status === 'Completed'
                        ? 'bg-brand-teal text-white font-bold shadow-xs scale-105'
                        : 'text-brand-dark/40 hover:text-brand-dark hover:bg-brand-teal/20'
                    }`}
                    title="تکمیل‌شده"
                  >
                    <CheckCircle2 className="w-4 h-4" />
                  </button>
                </div>

                <div className="flex gap-1">
                  <button onClick={() => handleOpenModal(act)} className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl" title="ویرایش">
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => {
                      setDeletingId(act.id);
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
            <h2 className="text-lg font-bold mb-4">{editingActivity ? 'ویرایش فعالیت' : 'افزودن فعالیت جدید'}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              {!editingActivity && (
                <>
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
                    <label className="block text-sm font-medium mb-1">نوع فعالیت</label>
                    <CustomSelect
                      options={typeOptions}
                      value={formData.type}
                      onChange={(val) => setFormData({ ...formData, type: val })}
                    />
                  </div>
                </>
              )}

              <div>
                <label className="block text-sm font-medium mb-1">عنوان</label>
                <input
                  type="text"
                  required
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">مهلت تحویل (شمسی)</label>
                <JalaliDateTimePicker
                  value={formData.dueDateUtc}
                  onChange={(iso) => setFormData({ ...formData, dueDateUtc: iso })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">اولویت</label>
                <CustomSelect
                  options={priorityOptions}
                  value={formData.priority}
                  onChange={(val) => setFormData({ ...formData, priority: val })}
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

      <ConfirmModal
        isOpen={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={confirmDelete}
        title="حذف فعالیت"
        message="آیا از حذف این تکلیف/پروژه اطمینان دارید؟ این عملیات قابل بازگشت نیست."
      />
    </div>
  );
}

const StatusBadge = ({ status }) => {
  if (status === 'Completed') {
    return <span className="bg-brand-teal text-white border border-brand-teal/30 text-xs px-2.5 py-1 rounded-xl font-bold shadow-xs">تکمیل‌شده</span>;
  }
  if (status === 'InProgress') {
    return <span className="bg-brand-amber text-brand-dark border border-brand-amber/40 text-xs px-2.5 py-1 rounded-xl font-bold shadow-xs">در حال انجام</span>;
  }
  return <span className="bg-slate-200 dark:bg-slate-800 text-slate-700 dark:text-slate-200 border border-slate-300 dark:border-slate-700 text-xs px-2.5 py-1 rounded-xl font-bold">شروع‌نشده</span>;
};