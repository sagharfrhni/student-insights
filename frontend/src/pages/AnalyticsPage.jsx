import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { toPersianDigits } from '../utils/formatters';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement,
} from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import { Sparkles, Flame, Clock, Award, CheckCircle } from 'lucide-react';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement
);

// داده‌های زاپاس در صورت خاموش بودن پایتون
const fallbackData = {
  summary_cards: {
    total_study_hours: 7.5,
    average_grade: 16.25,
    task_completion_rate: 66.7,
  },
  charts: {
    weekly_trend: {
      labels: ['2026-07-20', '2026-07-21', '2026-07-22', '2026-07-23'],
      datasets: [{ label: 'Study Hours', data: [2, 1.5, 1, 3] }],
    },
    course_distribution: {
      labels: ['ریاضی عمومی', 'هوش مصنوعی', 'پایگاه داده'],
      datasets: [{ data: [3.5, 3, 1] }],
    },
    task_status: {
      labels: ['Completed', 'Pending'],
      datasets: [{ data: [2, 1] }],
    },
    grade_trend: {
      labels: ['2026-07-10', '2026-07-15'],
      datasets: [{ label: 'Grade', data: [18.5, 14.0] }],
    },
  },
  analytics: { study_streak: { current_streak: 4 } },
  ai_insights: ['آمار تحصیلی شما خوب است. به مطالعه منظم ادامه دهید!'],
};

export default function AnalyticsPage() {
  const { user } = useAuth();
  const { theme } = useTheme();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  const isDark = theme === 'dark';
  const textColor = isDark ? '#F4F0FA' : '#2D3E40';
  const gridColor = isDark ? 'rgba(84, 21, 50, 0.4)' : 'rgba(244, 218, 203, 0.4)';

  const userId = user?.userId || 1;

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setLoading(true);
        // اتصال به API پایتون (FastAPI) دوستتون
        const res = await axios.get(`http://localhost:8000/api/dashboard/${userId}`);
        setData(res.data);
      } catch (err) {
        console.warn("پایتون روشن نیست یا انپوینت پاسخ نداد، استفاده از داده‌های پیش‌فرض.");
        setData(fallbackData);
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, [userId]);

  if (loading) {
    return <div className="text-center py-12 text-brand-dark/50">در حال دریافت و تحلیل داده‌ها از موتور پایتون...</div>;
  }

  const summary = data?.summary_cards || fallbackData.summary_cards;
  const charts = data?.charts || fallbackData.charts;
  const streak = data?.analytics?.study_streak?.current_streak || 0;
  const insights = data?.ai_insights || [];

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        labels: { color: textColor, font: { family: 'Vazirmatn' } },
      },
    },
    scales: {
      x: {
        ticks: { color: textColor, font: { family: 'Vazirmatn' } },
        grid: { color: gridColor },
      },
      y: {
        ticks: { color: textColor, font: { family: 'Vazirmatn' } },
        grid: { color: gridColor },
      },
    },
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">تحلیل‌های هوشمند و نمودارها</h1>
        <p className="text-sm text-brand-dark/60">گزارشات آنالیز شده توسط سرویس تحلیل پایتون</p>
      </div>

      {/* کارت‌های شاخص‌های کلیدی (KPIs) */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-3">
          <div className="bg-brand-teal text-white p-3 rounded-2xl">
            <Clock className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60">کل ساعات مطالعه</p>
            <p className="text-xl font-bold fa-num">{toPersianDigits(summary.total_study_hours)} ساعت</p>
          </div>
        </div>

        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-3">
          <div className="bg-[#826F9D] text-white p-3 rounded-2xl">
            <Award className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60">میانگین نمرات</p>
            <p className="text-xl font-bold fa-num">{toPersianDigits(summary.average_grade)}</p>
          </div>
        </div>

        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-3">
          <div className="bg-brand-amber text-brand-dark p-3 rounded-2xl">
            <CheckCircle className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60">درصد انجام تکالیف</p>
            <p className="text-xl font-bold fa-num">٪{toPersianDigits(summary.task_completion_rate)}</p>
          </div>
        </div>

        <div className="bg-white p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-3">
          <div className="bg-orange-500 text-white p-3 rounded-2xl">
            <Flame className="w-6 h-6" />
          </div>
          <div>
            <p className="text-xs text-brand-dark/60">زنجیره مطالعه </p>
            <p className="text-xl font-bold fa-num">{toPersianDigits(streak)} روز متوالی</p>
          </div>
        </div>
      </div>

      {/* بخش پیشنهادات هوش مصنوعی (AI Insights) */}
      <div className="bg-gradient-to-r from-[#826F9D]/15 via-brand-peach/30 to-[#826F9D]/15 border border-[#826F9D]/40 p-5 rounded-3xl space-y-3">
        <div className="flex items-center gap-2 font-bold text-sm text-[#826F9D] dark:text-[#BFABDE]">
          <Sparkles className="w-5 h-5" />
          <span>پیشنهادات و تحلیل هوشمند (AI Insights)</span>
        </div>
        <ul className="space-y-1.5 text-xs text-brand-dark font-medium list-disc list-inside">
          {insights.map((item, index) => (
            <li key={index} className="leading-relaxed">{item}</li>
          ))}
        </ul>
      </div>

      {/* نمودارها */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* نمودار روند روند هفتگی مطالعه */}
        <div className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <h3 className="font-bold text-sm">روند ساعات مطالعه اخیر</h3>
          <div className="h-64">
            <Line data={charts.weekly_trend} options={chartOptions} />
          </div>
        </div>

        {/* نمودار توزیع مطالعه دروس */}
        <div className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <h3 className="font-bold text-sm">توزیع مطالعه بر اساس درس</h3>
          <div className="h-64">
            <Bar data={charts.course_distribution} options={chartOptions} />
          </div>
        </div>

        {/* وضعیت تکالیف */}
        <div className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <h3 className="font-bold text-sm">وضعیت انجام فعالیت‌ها</h3>
          <div className="h-64 flex justify-center items-center">
            <Doughnut data={charts.task_status} options={{ plugins: { legend: { labels: { color: textColor } } } }} />
          </div>
        </div>

        {/* روند نمرات */}
        <div className="bg-white p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <h3 className="font-bold text-sm">روند نمرات امتحانات</h3>
          <div className="h-64">
            <Line data={charts.grade_trend} options={chartOptions} />
          </div>
        </div>
      </div>
    </div>
  );
}