import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { toPersianDigits, formatMinutesToHours, toJalaliDate } from '../utils/formatters';
import { BookOpen, CheckSquare, GraduationCap, Clock, Bell, Target, ArrowLeft, Bot, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';

const mockDashboardData = {
  totalCourses: 5,
  activeAssignmentsCount: 3,
  activeProjectsCount: 2,
  weeklyStudyMinutes: 345,
  upcomingExams: [
    { id: '1', title: 'میانترم پایگاه داده', courseName: 'پایگاه داده‌ها', examDateUtc: new Date(Date.now() + 86400000 * 2).toISOString() },
    { id: '2', title: 'پایانترم هوش مصنوعی', courseName: 'هوش مصنوعی', examDateUtc: new Date(Date.now() + 86400000 * 5).toISOString() }
  ],
  goalsProgress: [
    { id: '1', type: 'معدل بالای ۱۸', progressPercentage: 85 },
    { id: '2', type: '۲۰ ساعت مطالعه هفتگی', progressPercentage: 60 }
  ],
  recentActivities: [
    { id: '1', title: 'تمرین شماره ۳ تحویل داده شد', sourceType: 'LearningActivity', occurredAtUtc: new Date().toISOString() },
    { id: '2', title: 'درس جدید اضافه شد', sourceType: 'Course', occurredAtUtc: new Date().toISOString() }
  ]
};


const aiTools = [
  { name: 'ChatGPT', url: 'https://chatgpt.com', domain: 'chatgpt.com' },
  { name: 'Claude', url: 'https://claude.ai', domain: 'claude.ai' },
  { name: 'Gemini', url: 'https://gemini.google.com', domain: 'gemini.google.com' },
  { name: 'DeepSeek', url: 'https://chat.deepseek.com', domain: 'deepseek.com' },
  { 
    name: 'NotebookLM', 
    url: 'https://notebooklm.google.com',
    customIcon: (
      <svg className="w-6 h-6 text-[#1A73E8]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>
        <circle cx="12" cy="10" r="2" fill="currentColor" />
      </svg>
    )
  },
  { name: 'Perplexity', url: 'https://www.perplexity.ai', domain: 'perplexity.ai' },
  { name: 'Grok', url: 'https://grok.x.ai', domain: 'x.ai' },
];

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const res = await api.get('/dashboard');
        setData(res.data);
      } catch (err) {
        setData(mockDashboardData);
      } finally {
        setLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (loading) return <div className="text-center py-12">در حال بارگذاری داشبورد...</div>;

  return (
    <div className="space-y-6 w-full max-w-full">
      <div className="flex flex-wrap justify-between items-center gap-2">
        <h1 className="text-xl sm:text-2xl font-bold">خلاصه وضعیت ترم جاری</h1>
        {localStorage.getItem('accessToken') === 'demo-token' && (
          <span className="bg-brand-amber/30 text-brand-dark text-xs px-3 py-1 rounded-full font-bold">
            حالت دمو (آزمایشی)
          </span>
        )}
      </div>

      
      <div className="bg-white p-4 sm:p-5 rounded-3xl border border-brand-peach/80 shadow-xs space-y-3">
        <div className="flex items-center justify-between border-b border-brand-peach/40 pb-2.5">
          <div className="flex items-center gap-2">
            <div className="bg-brand-teal text-white p-2 rounded-2xl shadow-xs">
              <Bot className="w-5 h-5" />
            </div>
            <div>
              <h2 className="font-bold text-sm sm:text-base">میانبر هوش مصنوعی</h2>
              <p className="text-[10px] text-brand-dark/50">دسترسی سریع به ابزارهای هوش مصنوعی برای یادگیری</p>
            </div>
          </div>
          <ExternalLink className="w-4 h-4 text-brand-dark/40" />
        </div>

        
        <div className="grid grid-cols-4 sm:grid-cols-7 gap-2.5 sm:gap-3 pt-1">
          {aiTools.map((tool) => (
            <a
              key={tool.name}
              href={tool.url}
              target="_blank"
              rel="noopener noreferrer"
              className="flex flex-col items-center justify-center p-3 rounded-2xl border border-brand-peach/50 bg-brand-bg/50 hover:bg-brand-teal hover:border-brand-teal hover:scale-105 transition-all group shadow-2xs"
              title={tool.name}
            >
              {tool.customIcon ? (
                <div className="group-hover:text-white transition-colors">
                  {tool.customIcon}
                </div>
              ) : (
                <img
                  src={`https://www.google.com/s2/favicons?domain=${tool.domain}&sz=64`}
                  alt={tool.name}
                  className="w-6 h-6 rounded-md object-contain group-hover:brightness-200 transition-all"
                  loading="lazy"
                />
              )}
              <span className="text-[10px] font-bold mt-1.5 truncate max-w-full text-brand-dark group-hover:text-white">
                {tool.name}
              </span>
            </a>
          ))}
        </div>
      </div>

      
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
        <StatCard title="تعداد دروس" value={toPersianDigits(data.totalCourses)} icon={BookOpen} color="bg-brand-teal" />
        <StatCard title="تکالیف فعال" value={toPersianDigits(data.activeAssignmentsCount)} icon={CheckSquare} color="bg-brand-amber" />
        <StatCard title="پروژه‌های فعال" value={toPersianDigits(data.activeProjectsCount)} icon={CheckSquare} color="bg-brand-rose" />
        <StatCard title="مطالعه این هفته" value={formatMinutesToHours(data.weeklyStudyMinutes)} icon={Clock} color="bg-brand-peach" />
      </div>

      
      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4 sm:gap-6">
        
        <div className="bg-white p-4 sm:p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <div className="flex justify-between items-center border-b border-brand-peach/40 pb-3">
            <h2 className="font-bold text-sm sm:text-base flex items-center gap-2">
              <GraduationCap className="w-5 h-5 text-brand-teal shrink-0" />
              <span>امتحانات پیش‌رو</span>
            </h2>
            <Link to="/exams" className="text-xs text-brand-teal flex items-center gap-1 hover:underline shrink-0">
              مشاهده همه <ArrowLeft className="w-3 h-3" />
            </Link>
          </div>
          {data.upcomingExams?.length === 0 ? (
            <p className="text-xs text-brand-dark/50 text-center py-4">امتحانی در پیش نیست</p>
          ) : (
            <div className="space-y-2">
              {data.upcomingExams?.map((exam) => (
                <div key={exam.id} className="p-3 bg-brand-bg rounded-2xl flex items-center justify-between gap-2">
                  <div className="min-w-0 flex-1">
                    <p className="font-bold text-xs sm:text-sm truncate">{exam.title}</p>
                    <p className="text-[11px] text-brand-dark/60 truncate">{exam.courseName}</p>
                  </div>
                  <span className="text-[11px] bg-white px-2 py-1 rounded-xl border border-brand-peach fa-num shrink-0">
                    {toJalaliDate(exam.examDateUtc)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        
        <div className="bg-white p-4 sm:p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4">
          <div className="flex justify-between items-center border-b border-brand-peach/40 pb-3">
            <h2 className="font-bold text-sm sm:text-base flex items-center gap-2">
              <Target className="w-5 h-5 text-brand-teal shrink-0" />
              <span>پیشرفت اهداف</span>
            </h2>
            <Link to="/goals" className="text-xs text-brand-teal flex items-center gap-1 hover:underline shrink-0">
              مشاهده همه <ArrowLeft className="w-3 h-3" />
            </Link>
          </div>
          {data.goalsProgress?.length === 0 ? (
            <p className="text-xs text-brand-dark/50 text-center py-4">هدفی ثبت نشده است</p>
          ) : (
            <div className="space-y-3">
              {data.goalsProgress?.map((goal) => (
                <div key={goal.id} className="space-y-1">
                  <div className="flex justify-between text-xs">
                    <span className="truncate">{goal.type}</span>
                    <span className="fa-num shrink-0">{toPersianDigits(Math.round(goal.progressPercentage))}%</span>
                  </div>
                  <div className="w-full bg-brand-bg h-2 rounded-full overflow-hidden">
                    <div className="bg-brand-teal h-full rounded-full transition-all duration-500" style={{ width: `${goal.progressPercentage}%` }} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        
        <div className="bg-white p-4 sm:p-6 rounded-3xl border border-brand-peach/80 shadow-xs space-y-4 lg:col-span-2 xl:col-span-1">
          <div className="border-b border-brand-peach/40 pb-3">
            <h2 className="font-bold text-sm sm:text-base flex items-center gap-2">
              <Bell className="w-5 h-5 text-brand-teal shrink-0" />
              <span>فعالیت‌های اخیر</span>
            </h2>
          </div>
          {data.recentActivities?.length === 0 ? (
            <p className="text-xs text-brand-dark/50 text-center py-4">فعالیتی ثبت نشده است</p>
          ) : (
            <div className="space-y-2">
              {data.recentActivities?.map((act) => (
                <div key={act.id} className="p-3 bg-brand-bg rounded-2xl flex items-center justify-between gap-2">
                  <div className="min-w-0 flex-1">
                    <p className="font-medium text-xs truncate">{act.title}</p>
                    <p className="text-[10px] text-brand-dark/50 truncate">{act.sourceType}</p>
                  </div>
                  <span className="text-[10px] text-brand-dark/60 fa-num shrink-0">
                    {toJalaliDate(act.occurredAtUtc)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

const StatCard = ({ title, value, icon: Icon, color }) => (
  <div className="bg-white p-4 sm:p-5 rounded-3xl border border-brand-peach/80 shadow-xs flex items-center gap-3.5 min-w-0">
    <div className={`${color} text-white p-2.5 sm:p-3 rounded-2xl shrink-0`}>
      <Icon className="w-5 h-5 sm:w-6 sm:h-6" />
    </div>
    <div className="min-w-0 flex-1">
      <p className="text-[11px] sm:text-xs text-brand-dark/60 truncate">{title}</p>
      <p className="text-base sm:text-lg font-bold text-brand-dark fa-num truncate">{value}</p>
    </div>
  </div>
);