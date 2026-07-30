import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, toJalaliDate } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import JalaliDateTimePicker from '../components/JalaliDateTimePicker';
import ConfirmModal from '../components/ConfirmModal';
import CustomSelect from '../components/CustomSelect';
import { Plus, Trash2, Target, RefreshCw } from 'lucide-react';

const mockGoals = [
  { id: '1', type: 'GradePointAverage', targetValue: 18.5, currentValue: 17.8, progressPercentage: 96, progressStatus: 'Available', targetDateUtc: new Date().toISOString() },
  { id: '2', type: 'StudyHours', targetValue: 50, currentValue: 30, progressPercentage: 60, progressStatus: 'Available', targetDateUtc: new Date().toISOString() },
  { id: '3', type: 'ChapterCount', targetValue: 12, currentValue: 5, progressPercentage: 41, progressStatus: 'Available', targetDateUtc: new Date().toISOString() },
  { id: '4', type: 'ProjectDeadline', targetValue: 1, currentValue: 0, progressPercentage: 0, progressStatus: 'NotYetAvailable', targetDateUtc: new Date().toISOString() },
];

const goalTypeOptions = [
  { value: 0, label: 'میانگین معدل' },
  { value: 1, label: 'ساعات مطالعه' },
  { value: 2, label: 'سررسید پروژه' },
  { value: 3, label: 'تعداد فصل مطالعه‌شده' },
];

export default function Goals() {
  const [goals, setGoals] = useState([]);
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showModal, setShowModal] = useState(false);
  const [showProgressModal, setShowProgressModal] = useState(false);
  const [selectedGoal, setSelectedGoal] = useState(null);

  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  const [formData, setFormData] = useState({
    type: 0,
    targetValue: 18,
    targetDateUtc: new Date().toISOString(),
    relatedActivityId: '',
  });
  const [manualProgress, setManualProgress] = useState(0);

  const fetchData = async () => {
    try {
      setLoading(true);
      const res = await api.get('/goals?pageNumber=1&pageSize=50');
      setGoals(res.data.items);

      const projectsRes = await api.get('/learning-activities?type=1&pageNumber=1&pageSize=100');
      setProjects(projectsRes.data.items);
    } catch (err) {
      setGoals(mockGoals);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleOpenProgressModal = (goal) => {
    setSelectedGoal(goal);
    setManualProgress(goal.currentValue || 0);
    setShowProgressModal(true);
  };

  const handleCreateGoal = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        type: parseInt(formData.type),
        targetValue: parseFloat(formData.targetValue),
        targetDateUtc: formData.targetDateUtc,
        relatedActivityId: formData.type === 2 ? formData.relatedActivityId : null,
      };

      await api.post('/goals', payload);
      setShowModal(false);
      fetchData();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const handleUpdateProgress = async (e) => {
    e.preventDefault();
    try {
      await api.patch(`/goals/${selectedGoal.id}/progress`, {
        currentValue: parseFloat(manualProgress),
      });
      setShowProgressModal(false);
      fetchData();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setGoals((prev) => prev.filter((g) => g.id !== deletingId));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/goals/${deletingId}`);
        fetchData();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  const getGoalTypeTitle = (type) => {
    switch (type) {
      case 'GradePointAverage': return 'میانگین معدل';
      case 'StudyHours': return 'مجموع ساعات مطالعه';
      case 'ProjectDeadline': return 'سررسید پروژه تحصیلی';
      case 'ChapterCount': return 'تعداد فصل مطالعه‌شده';
      default: return 'هدف تحصیلی';
    }
  };

  const projectOptions = projects.map((p) => ({
    value: p.id,
    label: `${p.title} (${p.courseName})`,
  }));

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">اهداف تحصیلی</h1>
          <p className="text-sm text-brand-dark/60">تعریف و پایش اهداف تحصیلی ترم جاری</p>
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="bg-brand-teal text-white px-5 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2 self-start sm:self-auto"
        >
          <Plus className="w-4 h-4" />
          تعریف هدف جدید
        </button>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت اهداف...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {goals.map((goal) => (
            <div key={goal.id} className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs flex flex-col justify-between space-y-4">
              <div>
                <div className="flex justify-between items-start mb-2">
                  <div className="flex items-center gap-2">
                    <div className="p-2 bg-brand-peach/40 rounded-xl text-brand-teal">
                      <Target className="w-5 h-5" />
                    </div>
                    <h3 className="font-bold text-sm">{getGoalTypeTitle(goal.type)}</h3>
                  </div>
                </div>

                <div className="text-xs text-brand-dark/70 my-3 space-y-1">
                  <p>مقدار هدف: <span className="font-bold fa-num">{toPersianDigits(goal.targetValue)}</span></p>
                  {goal.targetDateUtc && (
                    <p className="text-brand-dark/50 fa-num">مهلت: {toJalaliDate(goal.targetDateUtc)}</p>
                  )}
                </div>

                {goal.progressStatus === 'Available' ? (
                  <div className="space-y-1.5 mt-4">
                    <div className="flex justify-between text-xs font-bold">
                      <span>پیشرفت</span>
                      <span className="fa-num">{toPersianDigits(Math.round(goal.progressPercentage))}%</span>
                    </div>
                    <div className="w-full bg-brand-bg h-2.5 rounded-full overflow-hidden">
                      <div className="bg-brand-teal h-full rounded-full transition-all duration-500" style={{ width: `${goal.progressPercentage}%` }} />
                    </div>
                  </div>
                ) : (
                  <div className="bg-brand-amber/20 border border-brand-amber/40 text-brand-dark text-xs p-3 rounded-2xl text-center font-medium my-3">
                    هنوز داده‌ای برای محاسبه پیشرفت وجود ندارد
                  </div>
                )}
              </div>

              <div className="flex items-center justify-between pt-4 border-t border-brand-peach/30">
                {goal.type === 'ChapterCount' ? (
                  <button
                    onClick={() => handleOpenProgressModal(goal)}
                    className="text-xs text-brand-teal font-bold flex items-center gap-1 hover:underline"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    ثبت پیشرفت
                  </button>
                ) : <span />}

                <button
                  onClick={() => {
                    setDeletingId(goal.id);
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
            <h2 className="text-lg font-bold mb-4">تعریف هدف تحصیلی جدید</h2>
            <form onSubmit={handleCreateGoal} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">نوع هدف</label>
                <CustomSelect
                  options={goalTypeOptions}
                  value={formData.type}
                  onChange={(val) => setFormData({ ...formData, type: val })}
                />
              </div>

              {formData.type === 2 && (
                <div>
                  <label className="block text-sm font-medium mb-1">پروژه مرتبط</label>
                  <CustomSelect
                    options={projectOptions}
                    value={formData.relatedActivityId}
                    placeholder="انتخاب پروژه مرتبط..."
                    onChange={(val) => setFormData({ ...formData, relatedActivityId: val })}
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium mb-1">مقدار هدف</label>
                <input
                  type="number"
                  step="0.1"
                  min="0.1"
                  required
                  value={formData.targetValue}
                  onChange={(e) => setFormData({ ...formData, targetValue: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm fa-num"
                />
              </div>

              
              <div>
                <label className="block text-sm font-medium mb-1">تاریخ هدف</label>
                <JalaliDateTimePicker
                  value={formData.targetDateUtc}
                  includeTime={false}
                  onChange={(iso) => setFormData({ ...formData, targetDateUtc: iso })}
                />
              </div>

              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white">
                  ایجاد هدف
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      
      {showProgressModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-sm border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">ثبت پیشرفت دستی</h2>
            <form onSubmit={handleUpdateProgress} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">مقدار فعلی مطالعه‌شده</label>
                <input
                  type="number"
                  step="0.5"
                  min="0"
                  required
                  value={manualProgress}
                  onChange={(e) => setManualProgress(e.target.value)}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm fa-num"
                />
              </div>
              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowProgressModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
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
        title="حذف هدف تحصیلی"
        message="آیا از حذف این هدف تحصیلی اطمینان دارید؟"
      />
    </div>
  );
}