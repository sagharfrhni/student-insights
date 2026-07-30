import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { translateError } from '../utils/errorHandler';
import ConfirmModal from '../components/ConfirmModal';
import { Plus, Trash2, Edit2 } from 'lucide-react';

const mockSettings = [
  { id: '1', key: 'MaxLoginAttempts', value: '5', description: 'حداکثر تعداد تلاش‌های ناموفق ورود قبل از قفل موقت' },
  { id: '2', key: 'SystemMaintenanceMode', value: 'false', description: 'وضعیت حالت تعمیر و نگهداری سیستم' },
  { id: '3', key: 'DefaultPageSize', value: '10', description: 'تعداد پیش‌فرض آیتم‌ها در صفحه‌بندی‌ها' },
];

export default function AdminSettings() {
  const [settings, setSettings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedSetting, setSelectedSetting] = useState(null);

  
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [deletingKey, setDeletingKey] = useState(null);

  const [newSetting, setNewSetting] = useState({ key: '', value: '', description: '' });
  const [editValue, setEditValue] = useState('');

  const fetchSettings = async () => {
    try {
      setLoading(true);
      const res = await api.get('/admin/settings');
      setSettings(res.data);
    } catch (err) {
      setSettings(mockSettings);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSettings();
  }, []);

  const handleAddSetting = async (e) => {
    e.preventDefault();
    try {
      await api.post('/admin/settings', newSetting);
      setShowAddModal(false);
      setNewSetting({ key: '', value: '', description: '' });
      fetchSettings();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const handleEditSettingValue = async (e) => {
    e.preventDefault();
    try {
      await api.patch(`/admin/settings/${selectedSetting.key}/value`, { value: editValue });
      setShowEditModal(false);
      fetchSettings();
    } catch (err) {
      setError(translateError(err));
    }
  };

  const confirmDelete = async () => {
    setSettings((prev) => prev.filter((s) => s.key !== deletingKey));
    setDeleteModalOpen(false);

    try {
      if (localStorage.getItem('accessToken') !== 'demo-token') {
        await api.delete(`/admin/settings/${deletingKey}`);
        fetchSettings();
      }
    } catch (err) {
      setError(translateError(err));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">تنظیمات سیستم</h1>
          <p className="text-sm text-brand-dark/60">مدیریت متغیرها و تنظیمات کلید-مقدار (Key-Value) سامانه</p>
        </div>

        <button
          onClick={() => setShowAddModal(true)}
          className="bg-brand-teal text-white px-4 py-2.5 rounded-2xl text-sm font-medium flex items-center gap-2"
        >
          <Plus className="w-4 h-4" />
          افزودن تنظیم جدید
        </button>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت تنظیمات...</div>
      ) : (
        <div className="bg-white rounded-3xl border border-brand-peach/80 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-right text-sm">
              <thead className="bg-brand-bg text-brand-dark/70 text-xs border-b border-brand-peach/60">
                <tr>
                  <th className="p-4">کلید (Key)</th>
                  <th className="p-4">مقدار (Value)</th>
                  <th className="p-4">توضیحات</th>
                  <th className="p-4 text-left">عملیات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-brand-peach/30">
                {settings.map((s) => (
                  <tr key={s.id} className="hover:bg-brand-bg/40 transition-colors">
                    <td className="p-4 font-mono text-xs font-bold text-brand-teal dir-ltr text-right">{s.key}</td>
                    <td className="p-4 font-bold">{s.value}</td>
                    <td className="p-4 text-xs text-brand-dark/60">{s.description || '-'}</td>
                    <td className="p-4 text-left flex justify-end gap-1">
                      <button
                        onClick={() => {
                          setSelectedSetting(s);
                          setEditValue(s.value);
                          setShowEditModal(true);
                        }}
                        className="p-2 text-brand-dark/50 hover:text-brand-teal rounded-xl"
                        title="ویرایش مقدار"
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => {
                          setDeletingKey(s.key);
                          setDeleteModalOpen(true);
                        }}
                        className="p-2 text-brand-dark/50 hover:text-brand-rose rounded-xl"
                        title="حذف"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      
      {showAddModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-md border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-4">افزودن تنظیم جدید</h2>
            <form onSubmit={handleAddSetting} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">کلید (Key)</label>
                <input
                  type="text"
                  required
                  value={newSetting.key}
                  onChange={(e) => setNewSetting({ ...newSetting, key: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm dir-ltr"
                  placeholder="مثال: MaxLoginAttempts"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">مقدار (Value)</label>
                <input
                  type="text"
                  required
                  value={newSetting.value}
                  onChange={(e) => setNewSetting({ ...newSetting, value: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">توضیحات </label>
                <textarea
                  value={newSetting.description}
                  onChange={(e) => setNewSetting({ ...newSetting, description: e.target.value })}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                  rows="2"
                />
              </div>
              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowAddModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
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

      
      {showEditModal && (
        <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-md border border-brand-peach shadow-xl">
            <h2 className="text-lg font-bold mb-2">ویرایش مقدار تنظیم</h2>
            <p className="text-xs text-brand-dark/60 font-mono mb-4">{selectedSetting?.key}</p>

            <form onSubmit={handleEditSettingValue} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">مقدار جدید (Value)</label>
                <input
                  type="text"
                  required
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  className="w-full border border-brand-peach bg-white rounded-2xl px-4 py-2.5 text-sm"
                />
              </div>
              <div className="flex gap-2 justify-end pt-4">
                <button type="button" onClick={() => setShowEditModal(false)} className="px-4 py-2 text-sm rounded-xl border border-brand-peach">
                  انصراف
                </button>
                <button type="submit" className="px-5 py-2 text-sm rounded-xl bg-brand-teal text-white">
                  به‌روزرسانی
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
        title="حذف تنظیم سیستم"
        message={`آیا از حذف تنظیم «${deletingKey}» اطمینان دارید؟`}
      />
    </div>
  );
}