import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import { toPersianDigits, toJalaliDate } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import { Search, Shield, UserCheck, UserX, ShieldAlert } from 'lucide-react';

const mockUsersList = [
  { id: '00000000-0000-0000-0000-000000000001', firstName: 'علی', lastName: 'محمدی', email: 'student@demo.local', role: 'Student', isActive: true, emailConfirmed: true, createdAtUtc: new Date().toISOString() },
  { id: '00000000-0000-0000-0000-000000000002', firstName: 'مدیر', lastName: 'سیستم', email: 'admin@demo.local', role: 'Admin', isActive: true, emailConfirmed: true, createdAtUtc: new Date().toISOString() },
  { id: '00000000-0000-0000-0000-000000000003', firstName: 'زهرا', lastName: 'احمدی', email: 'zahra@demo.local', role: 'Student', isActive: false, emailConfirmed: false, createdAtUtc: new Date().toISOString() },
];

export default function AdminUsers() {
  const { user } = useAuth();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [activeFilter, setActiveFilter] = useState('');

  const fetchUsers = async () => {
    try {
      setLoading(true);
      let url = '/admin/users?pageNumber=1&pageSize=50';
      if (search) url += `&search=${encodeURIComponent(search)}`;
      if (roleFilter) url += `&role=${roleFilter}`;
      if (activeFilter !== '') url += `&isActive=${activeFilter}`;

      const res = await api.get(url);
      setUsers(res.data.items);
    } catch (err) {
      setUsers(mockUsersList);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [search, roleFilter, activeFilter]);

  const handleToggleActivate = async (targetUser) => {
    if (targetUser.id === user?.userId) {
      alert('شما نمی‌توانید حساب کاربری خودتان را غیرفعال کنید.');
      return;
    }

    try {
      if (targetUser.isActive) {
        await api.patch(`/admin/users/${targetUser.id}/deactivate`);
      } else {
        await api.patch(`/admin/users/${targetUser.id}/activate`);
      }
      fetchUsers();
    } catch (err) {
      alert(translateError(err));
    }
  };

  const handleChangeRole = async (targetUser, newRole) => {
    if (targetUser.id === user?.userId && newRole === 'Student') {
      alert('شما نمی‌توانید نقش حساب خودتان را از ادمین به دانشجو تنزل دهید.');
      return;
    }

    try {
      await api.patch(`/admin/users/${targetUser.id}/role`, { newRole });
      fetchUsers();
    } catch (err) {
      alert(translateError(err));
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">مدیریت کاربران</h1>
        <p className="text-sm text-brand-dark/60">جست‌وجو، تغییر نقش و مدیریت وضعیت دسترسی کاربران</p>
      </div>

      
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="w-4 h-4 text-brand-dark/40 absolute right-3.5 top-3" />
          <input
            type="text"
            placeholder="جست‌وجو با نام یا ایمیل..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full border border-brand-peach bg-white rounded-2xl pr-10 pl-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-brand-teal"
          />
        </div>

        <select
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
          className="border border-brand-peach bg-white rounded-2xl px-4 py-2 text-sm"
        >
          <option value="">همه نقش‌ها</option>
          <option value="Student">دانشجو</option>
          <option value="Admin">ادمین</option>
        </select>

        <select
          value={activeFilter}
          onChange={(e) => setActiveFilter(e.target.value)}
          className="border border-brand-peach bg-white rounded-2xl px-4 py-2 text-sm"
        >
          <option value="">همه وضعیت‌ها</option>
          <option value="true">فعال</option>
          <option value="false">غیرفعال</option>
        </select>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      
      {loading ? (
        <div className="text-center py-12 text-brand-dark/50">در حال دریافت لیست کاربران...</div>
      ) : (
        <div className="bg-white rounded-3xl border border-brand-peach/80 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-right text-sm">
              <thead className="bg-brand-bg text-brand-dark/70 text-xs border-b border-brand-peach/60">
                <tr>
                  <th className="p-4">نام و نام خانوادگی</th>
                  <th className="p-4">ایمیل</th>
                  <th className="p-4">نقش</th>
                  <th className="p-4">وضعیت</th>
                  <th className="p-4">تاریخ ثبت‌نام</th>
                  <th className="p-4 text-left">عملیات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-brand-peach/30">
                {users.map((u) => {
                  const isSelf = u.id === user?.userId;
                  return (
                    <tr key={u.id} className="hover:bg-brand-bg/40 transition-colors">
                      <td className="p-4 font-bold flex items-center gap-2">
                        {u.firstName} {u.lastName}
                        {isSelf && <span className="text-[10px] bg-brand-teal text-white px-2 py-0.5 rounded-full">(حساب شما)</span>}
                      </td>
                      <td className="p-4 text-brand-dark/70 dir-ltr text-right">{u.email}</td>
                      <td className="p-4">
                        <select
                          value={u.role}
                          disabled={isSelf} 
                          onChange={(e) => handleChangeRole(u, e.target.value)}
                          className="bg-brand-bg border border-brand-peach rounded-xl px-2.5 py-1 text-xs font-bold"
                        >
                          <option value="Student">دانشجو</option>
                          <option value="Admin">ادمین</option>
                        </select>
                      </td>
                      <td className="p-4">
                        {u.isActive ? (
                          <span className="bg-emerald-100 text-emerald-800 text-xs px-2.5 py-1 rounded-full font-bold inline-flex items-center gap-1">
                            <UserCheck className="w-3 h-3" /> فعال
                          </span>
                        ) : (
                          <span className="bg-brand-rose/20 text-brand-dark text-xs px-2.5 py-1 rounded-full font-bold inline-flex items-center gap-1">
                            <UserX className="w-3 h-3" /> غیرفعال
                          </span>
                        )}
                      </td>
                      <td className="p-4 text-xs text-brand-dark/60 fa-num">{toJalaliDate(u.createdAtUtc)}</td>
                      <td className="p-4 text-left">
                        <button
                          onClick={() => handleToggleActivate(u)}
                          disabled={isSelf} 
                          className={`text-xs px-3 py-1.5 rounded-xl font-bold transition-all disabled:opacity-30 disabled:cursor-not-allowed ${
                            u.isActive
                              ? 'bg-brand-rose/20 text-brand-dark hover:bg-brand-rose/30'
                              : 'bg-emerald-100 text-emerald-800 hover:bg-emerald-200'
                          }`}
                        >
                          {u.isActive ? 'غیرفعال‌سازی' : 'فعال‌سازی'}
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}