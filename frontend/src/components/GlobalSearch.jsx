import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, X, BookOpen, GraduationCap, CheckSquare, Target, Calendar, Clock } from 'lucide-react';
import { toPersianDigits } from '../utils/formatters';

const mockGlobalData = {
  courses: [
    { id: 'c1', name: 'پایگاه داده‌ها', path: '/courses' },
    { id: 'c2', name: 'هوش مصنوعی', path: '/courses' },
    { id: 'c3', name: 'طراحی الگوریتم', path: '/courses' },
  ],
  exams: [
    { id: 'e1', title: 'امتحان میانترم پایگاه داده', courseName: 'پایگاه داده‌ها', path: '/exams' },
    { id: 'e2', title: 'امتحان پایانترم هوش مصنوعی', courseName: 'هوش مصنوعی', path: '/exams' },
  ],
  activities: [
    { id: 'a1', title: 'تمرین شماره ۳ - نرمال‌سازی', courseName: 'پایگاه داده‌ها', path: '/learning-activities' },
    { id: 'a2', title: 'پروژه فاز اول - پیاده‌سازی A*', courseName: 'هوش مصنوعی', path: '/learning-activities' },
  ],
  goals: [
    { id: 'g1', type: 'معدل بالای ۱۸', path: '/goals' },
    { id: 'g2', type: '۲۰ ساعت مطالعه هفتگی', path: '/goals' },
  ],
  events: [
    { id: 'v1', title: 'جلسه با استاد راهنما', path: '/calendar' },
  ],
  logs: [
    { id: 'l1', courseName: 'پایگاه داده‌ها', notes: 'مطالعه بخش نرمال‌سازی', path: '/study-logs' },
  ],
};

export default function GlobalSearch({ isOpen, onClose }) {
  const [searchTerm, setSearchTerm] = useState('');
  const [results, setResults] = useState([]);
  const navigate = useNavigate();
  const inputRef = useRef(null);

  useEffect(() => {
    if (isOpen) {
      setTimeout(() => inputRef.current?.focus(), 100);
    } else {
      setSearchTerm('');
      setResults([]);
    }
  }, [isOpen]);

  useEffect(() => {
    if (!searchTerm.trim()) {
      setResults([]);
      return;
    }

    const term = searchTerm.trim().toLowerCase();
    const matches = [];

    mockGlobalData.courses.forEach((c) => {
      if (c.name.toLowerCase().includes(term)) {
        matches.push({ id: c.id, category: 'درس', title: c.name, sub: 'درس ثبت‌شده', icon: BookOpen, path: '/courses' });
      }
    });

    mockGlobalData.exams.forEach((e) => {
      if (e.title.toLowerCase().includes(term) || e.courseName.toLowerCase().includes(term)) {
        matches.push({ id: e.id, category: 'امتحان', title: e.title, sub: e.courseName, icon: GraduationCap, path: '/exams' });
      }
    });

    mockGlobalData.activities.forEach((a) => {
      if (a.title.toLowerCase().includes(term) || a.courseName.toLowerCase().includes(term)) {
        matches.push({ id: a.id, category: 'تکلیف/پروژه', title: a.title, sub: a.courseName, icon: CheckSquare, path: '/learning-activities' });
      }
    });

    mockGlobalData.goals.forEach((g) => {
      if (g.type.toLowerCase().includes(term)) {
        matches.push({ id: g.id, category: 'هدف', title: g.type, sub: 'هدف تحصیلی', icon: Target, path: '/goals' });
      }
    });

    mockGlobalData.events.forEach((v) => {
      if (v.title.toLowerCase().includes(term)) {
        matches.push({ id: v.id, category: 'رویداد', title: v.title, sub: 'تقویم تحصیلی', icon: Calendar, path: '/calendar' });
      }
    });

    mockGlobalData.logs.forEach((l) => {
      if (l.courseName.toLowerCase().includes(term) || (l.notes && l.notes.toLowerCase().includes(term))) {
        matches.push({ id: l.id, category: 'جلسه مطالعه', title: l.courseName, sub: l.notes || 'جلسه مطالعه', icon: Clock, path: '/study-logs' });
      }
    });

    setResults(matches);
  }, [searchTerm]);

  if (!isOpen) return null;

  const handleSelectResult = (path) => {
    navigate(path);
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-brand-dark/50 backdrop-blur-xs z-50 flex items-start justify-center pt-16 sm:pt-24 p-4 animate-fade-in">
      <div className="bg-white dark:bg-[#221A32] border border-brand-peach dark:border-[#541532] w-full max-w-xl rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[80vh]">
        <div className="p-4 border-b border-brand-peach/60 dark:border-[#541532] flex items-center gap-3">
          <Search className="w-5 h-5 text-brand-teal shrink-0" />
          <input
            ref={inputRef}
            type="text"
            placeholder="جستجوی سراسری (درس، امتحان، تکلیف، هدف، تقویم، جلسه مطالعه)..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-transparent text-brand-dark dark:text-[#F4F0FA] text-sm focus:outline-none placeholder:text-brand-dark/40 dark:placeholder:text-[#F4F0FA]/40 font-medium"
          />
          {searchTerm && (
            <button onClick={() => setSearchTerm('')} className="p-1 text-brand-dark/40 dark:text-[#F4F0FA]/40 hover:text-brand-dark">
              <X className="w-4 h-4" />
            </button>
          )}
          <button onClick={onClose} className="p-1.5 bg-brand-bg dark:bg-[#060407] rounded-xl text-brand-dark/60 dark:text-[#F4F0FA]/60 hover:text-brand-dark">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="overflow-y-auto p-4 space-y-2 flex-1">
          {!searchTerm.trim() ? (
            <div className="text-center py-8 text-xs text-brand-dark/50 dark:text-[#F4F0FA]/50 space-y-1">
              <p className="font-bold">عبارت مورد نظر خود را تایپ کنید</p>
              <p>جستجو در تمام دروس، امتحانات، تکالیف، اهداف و جلسات مطالعه انجام می‌شود.</p>
            </div>
          ) : results.length === 0 ? (
            <div className="text-center py-8 text-xs text-brand-dark/50 dark:text-[#F4F0FA]/50">
              نتیجه‌ای برای «{searchTerm}» یافت نشد.
            </div>
          ) : (
            <div className="space-y-1.5">
              <p className="text-[11px] font-bold text-brand-dark/50 dark:text-[#F4F0FA]/50 px-2 fa-num">
                {toPersianDigits(results.length)} نتیجه یافت شد:
              </p>
              {results.map((res, idx) => {
                const Icon = res.icon;
                return (
                  <button
                    key={idx}
                    type="button"
                    onClick={() => handleSelectResult(res.path)}
                    className="w-full p-3 rounded-2xl bg-brand-bg/50 dark:bg-[#060407]/50 hover:bg-brand-teal hover:text-white dark:hover:bg-[#826F9D] transition-all flex items-center justify-between group text-right"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="p-2 bg-white dark:bg-[#221A32] rounded-xl text-brand-teal dark:text-[#BFABDE] group-hover:bg-white/20 group-hover:text-white shrink-0">
                        <Icon className="w-4 h-4" />
                      </div>
                      <div className="min-w-0">
                        <p className="font-bold text-xs truncate group-hover:text-white">{res.title}</p>
                        <p className="text-[10px] text-brand-dark/50 dark:text-[#F4F0FA]/50 group-hover:text-white/80 truncate">{res.sub}</p>
                      </div>
                    </div>
                    <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-white/60 dark:bg-[#221A32]/60 text-brand-dark dark:text-[#F4F0FA] group-hover:bg-white/20 group-hover:text-white shrink-0">
                      {res.category}
                    </span>
                  </button>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}