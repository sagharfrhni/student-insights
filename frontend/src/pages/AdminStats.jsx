import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, formatMinutesToHours } from '../utils/formatters';
import { translateError } from '../utils/errorHandler';
import { Users, UserCheck, UserX, Shield, BookOpen, CheckSquare, GraduationCap, Target, Clock } from 'lucide-react';

const mockAdminStats = {
  totalUsers: 120,
  activeUsers: 112,
  inactiveUsers: 8,
  adminCount: 3,
  studentCount: 117,
  newUsersLast7Days: 14,
  newUsersLast30Days: 45,
  totalCourses: 340,
  totalLearningActivities: 890,
  assignmentCount: 620,
  projectCount: 270,
  totalExams: 410,
  totalGoals: 210,
  totalStudyLogs: 1540,
  totalStudyMinutes: 45200,
};

export default function AdminStats() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoading(true);
        const res = await api.get('/admin/stats');
        setStats(res.data);
      } catch (err) {
        
        setStats(mockAdminStats);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  if (loading) return <div className="text-center py-12 text-brand-dark/50">در حال دریافت آمار سیستم...</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">آمار کلی سیستم</h1>
        <p className="text-sm text-brand-dark/60">نمای کلی از وضعیت عملکرد و کاربران سامانه آموینو</p>
      </div>

      {error && <div className="bg-brand-rose/20 text-brand-dark p-4 rounded-2xl">{error}</div>}

      
      <h2 className="text-base font-bold text-brand-dark pt-2">وضعیت کاربران</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatBox title="کل کاربران" value={toPersianDigits(stats.totalUsers)} icon={Users} color="bg-brand-teal" />
        <StatBox title="کاربران فعال" value={toPersianDigits(stats.activeUsers)} icon={UserCheck} color="bg-emerald-600" />
        <StatBox title="کاربران غیرفعال" value={toPersianDigits(stats.inactiveUsers)} icon={UserX} color="bg-brand-rose" />
        <StatBox title="تعداد ادمین‌ها" value={toPersianDigits(stats.adminCount)} icon={Shield} color="bg-brand-amber" />
      </div>

      
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex justify-between items-center">
          <div>
            <p className="text-xs text-brand-dark/60">ثبت‌نامی‌های ۷ روز اخیر</p>
            <p className="text-xl font-bold text-brand-teal fa-num mt-1">{toPersianDigits(stats.newUsersLast7Days)} کاربر</p>
          </div>
          <span className="text-xs bg-brand-teal/10 text-brand-teal font-bold px-3 py-1 rounded-full">جدید</span>
        </div>

        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex justify-between items-center">
          <div>
            <p className="text-xs text-brand-dark/60">ثبت‌نامی‌های ۳۰ روز اخیر</p>
            <p className="text-xl font-bold text-brand-dark fa-num mt-1">{toPersianDigits(stats.newUsersLast30Days)} کاربر</p>
          </div>
          <span className="text-xs bg-brand-peach/40 text-brand-dark font-bold px-3 py-1 rounded-full">ماه جاری</span>
        </div>
      </div>

      
      <h2 className="text-base font-bold text-brand-dark pt-4">محتوا و فعالیت‌های تحصیلی</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatBox title="کل دروس ثبت‌شده" value={toPersianDigits(stats.totalCourses)} icon={BookOpen} color="bg-brand-teal" />
        <StatBox title="کل فعالیت‌ها (تکالیف/پروژه‌ها)" value={toPersianDigits(stats.totalLearningActivities)} icon={CheckSquare} color="bg-brand-amber" />
        <StatBox title="کل امتحانات" value={toPersianDigits(stats.totalExams)} icon={GraduationCap} color="bg-purple-600" />
        <StatBox title="مجموع ساعات مطالعه" value={formatMinutesToHours(stats.totalStudyMinutes)} icon={Clock} color="bg-brand-peach" />
      </div>
    </div>
  );
}

const StatBox = ({ title, value, icon: Icon, color }) => (
  <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-4">
    <div className={`${color} text-white p-3 rounded-2xl shrink-0`}>
      <Icon className="w-6 h-6" />
    </div>
    <div className="min-w-0 flex-1">
      <p className="text-xs text-brand-dark/60 truncate">{title}</p>
      <p className="text-lg font-bold text-brand-dark fa-num truncate mt-0.5">{value}</p>
    </div>
  </div>
);