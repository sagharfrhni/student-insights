import React, { useCallback, useEffect, useRef, useState } from 'react';
import api from '../services/api';
import CustomSelect from '../components/CustomSelect';
import { toPersianDigits, formatMinutesToHours } from '../utils/formatters';
import { Play, Pause, Square, Trash2, CheckCircle2, BookOpen } from 'lucide-react';

const STORAGE_KEY = 'studyTimer:activeSession';
const TICK_MS = 1000;
const MAX_DURATION_MINUTES = 720;
const RADIUS = 88;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;
const MS_PER_LAP = 60 * 60 * 1000;

const STATUS = {
  IDLE: 'idle',
  RUNNING: 'running',
  PAUSED: 'paused',
};

const mockCourses = [
  { id: 'c1', name: 'پایگاه داده‌ها' },
  { id: 'c2', name: 'هوش مصنوعی' },
  { id: 'c3', name: 'طراحی الگوریتم' },
];

function readStoredSession() {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function writeStoredSession(session) {
  try {
    if (session) {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  } catch {}
}

function buildNotes({ title, notes, startPage, endPage }) {
  const parts = [];
  if (title && title.trim()) parts.push(title.trim());
  if (notes && notes.trim()) parts.push(notes.trim());

  const hasStart = startPage !== '' && startPage != null;
  const hasEnd = endPage !== '' && endPage != null;
  if (hasStart || hasEnd) {
    const range = hasStart && hasEnd
      ? `صفحات ${startPage} تا ${endPage}`
      : hasStart
      ? `از صفحه ${startPage}`
      : `تا صفحه ${endPage}`;
    parts.push(range);
  }

  return parts.length > 0 ? parts.join('\n\n') : null;
}

function emptyDetails() {
  return { title: '', notes: '', startPage: '', endPage: '' };
}

function formatElapsed(ms) {
  const totalSeconds = Math.floor(ms / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  const pad = (n) => String(n).padStart(2, '0');
  return hours > 0 ? `${pad(hours)}:${pad(minutes)}:${pad(seconds)}` : `${pad(minutes)}:${pad(seconds)}`;
}

export default function TimerPage() {
  const [courses, setCourses] = useState([]);
  const [selectedCourseId, setSelectedCourseId] = useState('');
  const [loadingCourses, setLoadingCourses] = useState(true);
  const [savedMessage, setSavedMessage] = useState(null);

  const [status, setStatus] = useState(STATUS.IDLE);
  const [activeCourse, setActiveCourse] = useState(null);
  const [details, setDetails] = useState(emptyDetails());
  const [sessionStartUtc, setSessionStartUtc] = useState(null);
  const [accumulatedMs, setAccumulatedMs] = useState(0);
  const [runningSinceMs, setRunningSinceMs] = useState(null);
  const [nowMs, setNowMs] = useState(Date.now());
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const tickRef = useRef(null);

  useEffect(() => {
    const fetchCoursesList = async () => {
      try {
        setLoadingCourses(true);
        const res = await api.get('/courses?pageNumber=1&pageSize=100');
        const items = res.data.items || res.data || [];
        setCourses(items);
        if (items.length > 0) setSelectedCourseId(items[0].id);
      } catch (err) {
        setCourses(mockCourses);
        setSelectedCourseId(mockCourses[0].id);
      } finally {
        setLoadingCourses(false);
      }
    };

    fetchCoursesList();

    const stored = readStoredSession();
    if (stored) {
      setStatus(stored.status);
      setActiveCourse(stored.course);
      setDetails(stored.details ?? emptyDetails());
      setSessionStartUtc(stored.sessionStartUtc);
      setAccumulatedMs(stored.accumulatedMs);
      setRunningSinceMs(stored.runningSinceMs);
    }
  }, []);

  useEffect(() => {
    if (status !== STATUS.RUNNING) {
      if (tickRef.current) clearInterval(tickRef.current);
      return;
    }
    tickRef.current = setInterval(() => setNowMs(Date.now()), TICK_MS);
    return () => clearInterval(tickRef.current);
  }, [status]);

  const persist = useCallback((patch) => {
    const stored = readStoredSession() ?? {};
    writeStoredSession({ ...stored, ...patch });
  }, []);

  const elapsedMs = accumulatedMs + (status === STATUS.RUNNING && runningSinceMs ? Math.max(0, nowMs - runningSinceMs) : 0);

  const updateDetails = (patch) => {
    setDetails((prev) => {
      const next = { ...prev, ...patch };
      persist({ details: next });
      return next;
    });
  };

  const handleStart = () => {
    const targetCourse = courses.find((c) => c.id === selectedCourseId) || courses[0];
    if (!targetCourse) return;
    const now = Date.now();
    const nowIso = new Date(now).toISOString();
    const courseObj = { id: targetCourse.id, name: targetCourse.name };

    setStatus(STATUS.RUNNING);
    setActiveCourse(courseObj);
    setDetails(emptyDetails());
    setSessionStartUtc(nowIso);
    setAccumulatedMs(0);
    setRunningSinceMs(now);
    setNowMs(now);
    setError(null);

    writeStoredSession({
      status: STATUS.RUNNING,
      course: courseObj,
      details: emptyDetails(),
      sessionStartUtc: nowIso,
      accumulatedMs: 0,
      runningSinceMs: now,
    });
  };

  const handlePause = () => {
    if (status !== STATUS.RUNNING) return;
    const now = Date.now();
    const nextAccumulated = accumulatedMs + Math.max(0, now - (runningSinceMs ?? now));
    setAccumulatedMs(nextAccumulated);
    setRunningSinceMs(null);
    setStatus(STATUS.PAUSED);
    persist({ status: STATUS.PAUSED, accumulatedMs: nextAccumulated, runningSinceMs: null });
  };

  const handleResume = () => {
    if (status !== STATUS.PAUSED) return;
    const now = Date.now();
    setRunningSinceMs(now);
    setNowMs(now);
    setStatus(STATUS.RUNNING);
    persist({ status: STATUS.RUNNING, runningSinceMs: now });
  };

  const handleReset = () => {
    setStatus(STATUS.IDLE);
    setActiveCourse(null);
    setDetails(emptyDetails());
    setSessionStartUtc(null);
    setAccumulatedMs(0);
    setRunningSinceMs(null);
    writeStoredSession(null);
  };

  const handleStop = async () => {
    if (status === STATUS.IDLE || !activeCourse) return;

    const finalElapsedMs = accumulatedMs + (status === STATUS.RUNNING && runningSinceMs ? Math.max(0, Date.now() - runningSinceMs) : 0);
    const durationMinutes = Math.min(MAX_DURATION_MINUTES, Math.max(1, Math.round(finalElapsedMs / 60000)));

    setIsSubmitting(true);
    setError(null);

    try {
      let createdLog;
      if (localStorage.getItem('accessToken') === 'demo-token') {
        createdLog = {
          id: Date.now().toString(),
          courseId: activeCourse.id,
          courseName: activeCourse.name,
          studyDateUtc: sessionStartUtc,
          durationMinutes,
          notes: buildNotes(details),
        };
      } else {
        const res = await api.post('/study-logs', {
          courseId: activeCourse.id,
          studyDateUtc: sessionStartUtc,
          durationMinutes,
          notes: buildNotes(details),
        });
        createdLog = res.data;
      }

      setSavedMessage(`جلسه مطالعه به مدت ${formatMinutesToHours(createdLog.durationMinutes)} برای درس «${createdLog.courseName || activeCourse.name}» با موفقیت ثبت شد.`);
      handleReset();
      setTimeout(() => setSavedMessage(null), 6000);
    } catch (err) {
      const message = err?.response?.data?.detail || err?.response?.data?.title || err?.message || 'خطا در ثبت جلسه مطالعه. لطفاً دوباره تلاش کنید.';
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const progress = (elapsedMs % MS_PER_LAP) / MS_PER_LAP;
  const dashOffset = CIRCUMFERENCE * (1 - progress);
  const courseOptions = courses.map((c) => ({ value: c.id, label: c.name }));

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-brand-dark dark:text-[#F4F0FA]">تایمر مطالعه زنده</h1>
        <p className="text-sm text-brand-dark/60 dark:text-[#F4F0FA]/60">زمان‌بندی و ثبت هوشمند جلسات مطالعه در زمان واقعی</p>
      </div>

      <div className="bg-white dark:bg-[#221A32] p-6 sm:p-8 rounded-3xl border-2 border-brand-peach dark:border-[#541532] shadow-xl space-y-6 text-center relative">
        <div className="relative w-52 h-52 mx-auto flex items-center justify-center">
          <svg className="w-full h-full -rotate-90 transform" viewBox="0 0 200 200">
            <circle
              className="text-brand-peach/40 dark:text-[#541532]/60 stroke-current"
              strokeWidth="10"
              fill="transparent"
              r={RADIUS}
              cx="100"
              cy="100"
            />
            <circle
              className={`transition-all duration-300 stroke-current ${
                status === STATUS.PAUSED ? 'text-amber-500' : 'text-[#826F9D] dark:text-[#BFABDE]'
              }`}
              strokeWidth="10"
              strokeDasharray={CIRCUMFERENCE}
              strokeDashoffset={dashOffset}
              strokeLinecap="round"
              fill="transparent"
              r={RADIUS}
              cx="100"
              cy="100"
            />
          </svg>

          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-4xl font-extrabold text-brand-dark dark:text-[#F4F0FA] fa-num tracking-wider">
              {toPersianDigits(formatElapsed(elapsedMs))}
            </span>
            <span className="text-xs font-bold mt-1 text-[#826F9D] dark:text-[#BFABDE]">
              {status === STATUS.RUNNING ? 'در حال مطالعه...' : status === STATUS.PAUSED ? 'توقف موقت' : 'آماده شروع'}
            </span>
          </div>
        </div>

        {status === STATUS.IDLE && (
          <div className="max-w-xs mx-auto text-right space-y-1.5 relative z-20">
            <label className="block text-xs font-bold text-brand-dark dark:text-[#F4F0FA]">انتخاب درس برای مطالعه:</label>
            <CustomSelect
              options={courseOptions}
              value={selectedCourseId}
              onChange={setSelectedCourseId}
              placeholder="انتخاب درس..."
            />
          </div>
        )}

        {status !== STATUS.IDLE && activeCourse && (
          <div className="inline-flex items-center gap-2 bg-brand-peach/30 dark:bg-[#541532]/40 px-4 py-2 rounded-2xl border border-brand-peach dark:border-[#541532]">
            <BookOpen className="w-4 h-4 text-[#826F9D] dark:text-[#BFABDE]" />
            <span className="text-xs font-bold text-brand-dark dark:text-[#F4F0FA]">
              در حال مطالعه درس: <strong className="text-[#826F9D] dark:text-[#BFABDE]">{activeCourse.name}</strong>
            </span>
          </div>
        )}

        {status !== STATUS.IDLE && (
          <div className="text-right bg-brand-bg dark:bg-[#060407] p-4 sm:p-5 rounded-2xl border border-brand-peach/60 dark:border-[#541532] space-y-3">
            <div>
              <label className="block text-xs font-bold mb-1 text-brand-dark dark:text-[#F4F0FA]">عنوان مطالعه (اختیاری)</label>
              <input
                type="text"
                placeholder="مثلاً: مطالعه فصل ۳ برای میانترم"
                maxLength={200}
                value={details.title}
                onChange={(e) => updateDetails({ title: e.target.value })}
                className="w-full px-3.5 py-2 rounded-xl border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#221A32] text-xs focus:outline-none focus:ring-2 focus:ring-[#826F9D]"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold mb-1 text-brand-dark dark:text-[#F4F0FA]">از صفحه (اختیاری)</label>
                <input
                  type="number"
                  min="0"
                  value={details.startPage}
                  onChange={(e) => updateDetails({ startPage: e.target.value })}
                  className="w-full px-3.5 py-2 rounded-xl border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#221A32] text-xs fa-num text-center focus:outline-none focus:ring-2 focus:ring-[#826F9D]"
                />
              </div>

              <div>
                <label className="block text-xs font-bold mb-1 text-brand-dark dark:text-[#F4F0FA]">تا صفحه (اختیاری)</label>
                <input
                  type="number"
                  min="0"
                  value={details.endPage}
                  onChange={(e) => updateDetails({ endPage: e.target.value })}
                  className="w-full px-3.5 py-2 rounded-xl border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#221A32] text-xs fa-num text-center focus:outline-none focus:ring-2 focus:ring-[#826F9D]"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold mb-1 text-brand-dark dark:text-[#F4F0FA]">یادداشت جلسه (اختیاری)</label>
              <textarea
                placeholder="یادداشت‌ها یا نکاتی که می‌خواهید ثبت شوند..."
                rows={2}
                maxLength={1800}
                value={details.notes}
                onChange={(e) => updateDetails({ notes: e.target.value })}
                className="w-full px-3.5 py-2 rounded-xl border border-brand-peach dark:border-[#541532] bg-white dark:bg-[#221A32] text-xs focus:outline-none focus:ring-2 focus:ring-[#826F9D]"
              />
            </div>
          </div>
        )}

        {error && (
          <div className="bg-brand-rose/20 text-brand-dark dark:text-rose-200 border border-brand-rose p-3 rounded-2xl text-xs font-bold">
            {error}
          </div>
        )}

        {savedMessage && (
          <div className="bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-200 border border-emerald-300 p-3 rounded-2xl text-xs font-bold flex items-center justify-center gap-2">
            <CheckCircle2 className="w-4 h-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
            <span>{savedMessage}</span>
          </div>
        )}

        <div className="flex flex-wrap gap-2.5 justify-center pt-2">
          {status === STATUS.IDLE && (
            <button
              type="button"
              onClick={handleStart}
              disabled={loadingCourses || courses.length === 0}
              className="bg-[#826F9D] hover:bg-[#826F9D]/90 text-white px-8 py-3 rounded-2xl text-xs font-bold flex items-center gap-2 shadow-md transition-all hover:scale-105 disabled:opacity-50"
            >
              <Play className="w-4 h-4" />
              شروع مطالعه
            </button>
          )}

          {status === STATUS.RUNNING && (
            <>
              <button
                type="button"
                onClick={handlePause}
                className="bg-amber-500 hover:bg-amber-600 text-white px-5 py-3 rounded-2xl text-xs font-bold flex items-center gap-1.5 shadow-sm transition-all"
              >
                <Pause className="w-4 h-4" />
                توقف موقت
              </button>

              <button
                type="button"
                onClick={handleStop}
                disabled={isSubmitting}
                className="bg-brand-rose hover:bg-brand-rose/90 text-white px-6 py-3 rounded-2xl text-xs font-bold flex items-center gap-1.5 shadow-sm transition-all disabled:opacity-50"
              >
                <Square className="w-4 h-4" />
                {isSubmitting ? 'در حال ثبت...' : 'پایان و ثبت جلسه'}
              </button>
            </>
          )}

          {status === STATUS.PAUSED && (
            <>
              <button
                type="button"
                onClick={handleResume}
                className="bg-[#826F9D] hover:bg-[#826F9D]/90 text-white px-5 py-3 rounded-2xl text-xs font-bold flex items-center gap-1.5 shadow-sm transition-all"
              >
                <Play className="w-4 h-4" />
                ادامه مطالعه
              </button>

              <button
                type="button"
                onClick={handleStop}
                disabled={isSubmitting}
                className="bg-brand-rose hover:bg-brand-rose/90 text-white px-6 py-3 rounded-2xl text-xs font-bold flex items-center gap-1.5 shadow-sm transition-all disabled:opacity-50"
              >
                <Square className="w-4 h-4" />
                {isSubmitting ? 'در حال ثبت...' : 'پایان و ثبت جلسه'}
              </button>
            </>
          )}
        </div>

        {status !== STATUS.IDLE && (
          <button
            type="button"
            onClick={handleReset}
            disabled={isSubmitting}
            className="text-xs font-bold text-brand-dark/50 hover:text-brand-rose transition-all inline-flex items-center gap-1 pt-2"
          >
            <Trash2 className="w-3.5 h-3.5" />
            انصراف و لغو این جلسه
          </button>
        )}
      </div>
    </div>
  );
}