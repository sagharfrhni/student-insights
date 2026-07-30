import React, { useState } from 'react';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { 
  BookOpen, GraduationCap, CheckSquare, LayoutDashboard, 
  Clock, Target, Calendar, Bell, BarChart2, Shield, LogOut, Menu, X, Users, Settings, Activity, Sun, Moon, Timer, Search 
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import GlobalSearch from './GlobalSearch';

export default function Layout() {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const location = useLocation();
  const navigate = useNavigate();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);

  const studentMenu = [
    { path: '/dashboard', label: 'داشبورد تحصیلی', icon: LayoutDashboard },
    { path: '/courses', label: 'دروس من', icon: BookOpen },
    { path: '/timer', label: 'تایمر مطالعه', icon: Timer },
    { path: '/exams', label: 'امتحانات', icon: GraduationCap },
    { path: '/learning-activities', label: 'تکالیف و پروژه‌ها', icon: CheckSquare },
    { path: '/study-logs', label: 'جلسات مطالعه', icon: Clock },
    { path: '/goals', label: 'اهداف تحصیلی', icon: Target },
    { path: '/calendar', label: 'تقویم تحصیلی', icon: Calendar },
    { path: '/analytics', label: 'تحلیل‌ها و نمودارها', icon: BarChart2 },
    { path: '/notifications', label: 'مرکز اعلان‌ها', icon: Bell },
  ];

  const adminMenu = [
    { path: '/admin/stats', label: 'آمار کلی سیستم', icon: Activity },
    { path: '/admin/users', label: 'مدیریت کاربران', icon: Users },
    { path: '/admin/settings', label: 'تنظیمات سیستم', icon: Settings },
  ];

  const currentMenu = user?.isAdmin ? adminMenu : studentMenu;

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-brand-bg text-brand-dark flex flex-col md:flex-row w-full overflow-x-hidden transition-colors duration-300">
      <GlobalSearch isOpen={isSearchOpen} onClose={() => setIsSearchOpen(false)} />

      <header className="md:hidden bg-white border-b border-brand-peach/60 px-4 py-2.5 flex items-center justify-between sticky top-0 z-30 shadow-xs">
        <div className="flex items-center gap-2">
          <div className={`${user?.isAdmin ? 'bg-brand-rose' : 'bg-brand-teal'} text-white p-1.5 rounded-xl shadow-xs`}>
            {user?.isAdmin ? <Shield className="w-5 h-5" /> : <GraduationCap className="w-5 h-5" />}
          </div>
          <span className="font-bold text-sm text-brand-dark">
            {user?.isAdmin ? 'آموینو | ادمین' : 'آموینو'}
          </span>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsSearchOpen(true)}
            className="p-1.5 text-brand-dark hover:bg-brand-peach/30 rounded-xl transition-all"
            title="جستجوی سراسری"
          >
            <Search className="w-5 h-5 text-brand-teal" />
          </button>

          <button
            onClick={toggleTheme}
            className="p-1.5 text-brand-dark hover:bg-brand-peach/30 rounded-xl transition-all"
            title={theme === 'dark' ? 'تم روشن' : 'تم دارک'}
          >
            {theme === 'dark' ? <Sun className="w-5 h-5 text-brand-amber" /> : <Moon className="w-5 h-5 text-brand-teal" />}
          </button>

          <button
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            className="p-1.5 text-brand-dark hover:bg-brand-peach/30 rounded-xl transition-all"
          >
            {isMobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
          </button>
        </div>
      </header>

      {isMobileMenuOpen && (
        <div
          onClick={() => setIsMobileMenuOpen(false)}
          className="fixed inset-0 bg-brand-dark/30 z-40 md:hidden backdrop-blur-xs transition-opacity duration-300"
        />
      )}

      <aside
        className={`fixed md:static inset-y-0 right-0 z-50 w-[240px] max-w-[80vw] md:w-60 bg-white border-l border-brand-peach/60 flex flex-col justify-between p-3.5 shadow-xl md:shadow-xs transition-transform duration-300 ease-in-out md:rounded-none shrink-0 ${
          isMobileMenuOpen ? 'translate-x-0 rounded-l-3xl' : 'translate-x-full md:translate-x-0'
        }`}
      >
        <div>
          <div className="flex items-center justify-between px-2 py-2 border-b border-brand-peach/40 mb-3 md:mb-4">
            <div className="flex items-center gap-2.5">
              <div className={`${user?.isAdmin ? 'bg-brand-rose' : 'bg-brand-teal'} text-white p-2 rounded-xl shadow-xs`}>
                {user?.isAdmin ? <Shield className="w-5 h-5" /> : <GraduationCap className="w-5 h-5" />}
              </div>
              <div>
                <h1 className="font-bold text-brand-dark text-sm">آموینو</h1>
                <p className="text-[10px] text-brand-dark/50">
                  {user?.isAdmin ? 'پنل مدیریت ارشد' : 'سامانه تحصیلی دانشجو'}
                </p>
              </div>
            </div>

            <div className="flex items-center gap-1">
              <button
                onClick={toggleTheme}
                className="p-1.5 text-brand-dark/70 hover:text-brand-dark hover:bg-brand-peach/30 rounded-xl transition-all"
                title={theme === 'dark' ? 'تغییر به تم روشن' : 'تغییر به تم دارک'}
              >
                {theme === 'dark' ? <Sun className="w-4 h-4 text-brand-amber" /> : <Moon className="w-4 h-4 text-brand-teal" />}
              </button>

              <button
                onClick={() => setIsMobileMenuOpen(false)}
                className="md:hidden p-1 text-brand-dark/50 hover:text-brand-dark rounded-lg"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>

          <div className="px-2 mb-3">
            <button
              onClick={() => setIsSearchOpen(true)}
              className="w-full flex items-center justify-between px-3 py-2 bg-brand-bg dark:bg-[#060407] hover:bg-brand-peach/40 rounded-2xl border border-brand-peach/60 text-xs text-brand-dark/60 dark:text-[#F4F0FA]/60 font-bold transition-all shadow-2xs"
            >
              <div className="flex items-center gap-2">
                <Search className="w-3.5 h-3.5 text-brand-teal" />
                <span>جستجوی سراسری...</span>
              </div>
            </button>
          </div>

          <nav className="space-y-0.5 overflow-y-auto max-h-[calc(100vh-250px)] pr-0.5">
            {currentMenu.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  onClick={() => setIsMobileMenuOpen(false)}
                  className={`flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-medium transition-all ${
                    isActive
                      ? user?.isAdmin 
                        ? 'bg-brand-rose text-white shadow-xs font-semibold'
                        : 'bg-brand-teal text-white shadow-xs font-semibold'
                      : 'text-brand-dark/80 hover:bg-brand-peach/30 hover:text-brand-dark'
                  }`}
                >
                  <Icon className="w-4 h-4 shrink-0" />
                  <span className="truncate">{item.label}</span>
                </Link>
              );
            })}
          </nav>
        </div>

        <div className="border-t border-brand-peach/40 pt-3 bg-white space-y-2.5">
          <div className="px-2">
            <div className="flex items-center justify-between">
              <p className="text-xs font-bold truncate text-brand-dark">{user?.firstName} {user?.lastName}</p>
              {user?.isAdmin && (
                <span className="text-[9px] bg-brand-rose/20 text-brand-dark px-1.5 py-0.5 rounded font-bold">ادمین</span>
              )}
            </div>
            <p className="text-[10px] text-brand-dark/50 truncate">{user?.email}</p>
          </div>

          <button
            onClick={handleLogout}
            className="w-full flex items-center justify-center gap-2 px-3 py-2.5 text-xs font-bold text-rose-600 dark:text-rose-300 bg-rose-500/10 dark:bg-rose-500/20 hover:bg-rose-600 hover:text-white dark:hover:bg-rose-600 dark:hover:text-white rounded-2xl transition-all border border-rose-500/20 shadow-xs"
          >
            <LogOut className="w-4 h-4 shrink-0" />
            <span>خروج از حساب</span>
          </button>
        </div>
      </aside>

      <main className="flex-1 min-w-0 p-3.5 sm:p-6 md:p-8 overflow-y-auto w-full">
        <Outlet />
      </main>
    </div>
  );
}