import React, { useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import Amuvino3DBackground from '../components/Amuvino3DBackground';
import { useTheme } from '../context/ThemeContext';
import { 
  GraduationCap, ArrowLeft, Sparkles, BookOpen, CheckSquare, Calendar, 
  Bell, Target, BarChart2, ChevronDown, Lock, Sun, Moon, Layers
} from 'lucide-react';

const StarryBackground = ({ isDark }) => {
  const stars = useMemo(() => {
    return Array.from({ length: 70 }).map((_, i) => ({
      id: i,
      top: `${(i * 17.3 + 5) % 100}%`,
      left: `${(i * 23.7 + 11) % 100}%`,
      size: (i % 3) === 0 ? 'large' : (i % 2 === 0 ? 'medium' : 'small'),
      isSvgStar: i % 4 === 0,
      delay: `${(i % 5) * 0.6}s`,
      duration: `${(i % 3) + 2}s`,
      darkColor: i % 3 === 0 ? '#FFFFFF' : i % 3 === 1 ? '#BFABDE' : '#38BDF8',
      lightColor: i % 3 === 0 ? '#826F9D' : i % 3 === 1 ? '#D97706' : '#0D9488'
    }));
  }, []);

  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden z-0">
      {stars.map((star) => {
        const color = isDark ? star.darkColor : star.lightColor;
        const opacity = isDark ? 0.85 : 0.55;

        if (star.isSvgStar) {
          return (
            <svg
              key={star.id}
              className="absolute animate-pulse transition-all duration-300"
              style={{
                top: star.top,
                left: star.left,
                width: star.size === 'large' ? '16px' : '11px',
                height: star.size === 'large' ? '16px' : '11px',
                animationDelay: star.delay,
                animationDuration: star.duration,
                color: color,
                opacity: opacity,
                filter: isDark ? `drop-shadow(0 0 5px ${color})` : 'none'
              }}
              viewBox="0 0 24 24"
              fill="currentColor"
            >
              <path d="M12 0L14.59 9.41L24 12L14.59 14.59L12 24L9.41 14.59L0 12L9.41 9.41L12 0Z" />
            </svg>
          );
        }

        const sizePx = star.size === 'large' ? '4px' : star.size === 'medium' ? '3px' : '2px';

        return (
          <div
            key={star.id}
            className="absolute rounded-full animate-pulse transition-all duration-300"
            style={{
              top: star.top,
              left: star.left,
              width: sizePx,
              height: sizePx,
              backgroundColor: color,
              opacity: opacity,
              animationDelay: star.delay,
              animationDuration: star.duration,
              boxShadow: isDark ? `0 0 8px ${color}` : 'none'
            }}
          />
        );
      })}
    </div>
  );
};

export default function Landing() {
  const { theme, toggleTheme } = useTheme();
  const [openFaq, setOpenFaq] = useState(null);

  const isDark = theme === 'dark';

  const scrollToSection = (id) => {
    const element = document.getElementById(id);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  };

  const faqs = [
    {
      q: 'آیا استفاده از آموینو رایگان است؟',
      a: 'بله. در حال حاضر، تمام امکانات آموینو کاملاً رایگان است.'
    },
    {
      q: 'آیا اطلاعاتم در اختیار دانشگاه یا استادم قرار می‌گیرد؟',
      a: 'خیر، اطلاعات شما کاملاً شخصی بوده و در اختیار هیچ شخص یا نهادی قرار نمی‌گیرد.'
    },
    {
      q: 'اگر یک ترم از آموینو استفاده نکنم، اطلاعاتم از بین می‌رود؟',
      a: 'خیر، تمام اطلاعات شما با امنیت کامل حفظ می‌شود و با ورود مجدد در دسترس خواهد بود.'
    }
  ];

  return (
    <div className={`font-sans select-none dir-rtl min-h-screen w-full overflow-x-hidden transition-colors duration-300 ${
      isDark ? 'bg-[#060407] text-[#F4F0FA]' : 'bg-[#FAF9F5] text-[#2D3E40]'
    }`}>
      
      <section className={`relative min-h-screen w-full overflow-hidden [transform:translateZ(0)] border-b transition-colors duration-300 ${
        isDark ? 'bg-[#060407] border-[#541532]' : 'bg-[#FAF9F5] border-[#F4DACB]'
      }`}>
        
        <Amuvino3DBackground />

        <div className={`absolute inset-0 z-10 pointer-events-none ${
          isDark 
            ? 'bg-gradient-to-l from-[#060407]/80 via-[#060407]/40 to-transparent' 
            : 'bg-gradient-to-l from-[#FAF9F5]/90 via-[#FAF9F5]/50 to-transparent'
        }`} />

        <div className="relative z-20 min-h-screen w-full flex flex-col justify-between p-4 sm:p-10 max-w-7xl mx-auto pointer-events-none">
          
          <header className="flex items-center justify-between pointer-events-none py-2">
            <div className="flex items-center gap-2 sm:gap-3">
              <div className="bg-[#826F9D] text-white p-2 sm:p-2.5 rounded-2xl shadow-md border border-white/20">
                <GraduationCap className="w-5 h-5 sm:w-6 sm:h-6" />
              </div>
              <div>
                <span className={`font-extrabold text-lg sm:text-2xl tracking-tight ${isDark ? 'text-white' : 'text-[#2D3E40]'}`}>آموینو</span>
                <span className="text-[9px] sm:text-[10px] block text-brand-teal font-bold">Amuvino Insights</span>
              </div>
            </div>

            <nav className={`hidden md:flex items-center gap-1 text-xs font-bold pointer-events-auto ${isDark ? 'text-white/80' : 'text-[#2D3E40]'}`}>
              <button onClick={() => scrollToSection('features')} className="px-3 py-1.5 rounded-xl hover:text-brand-teal hover:bg-brand-peach/30 transition-all">امکانات</button>
              <button onClick={() => scrollToSection('how-it-works')} className="px-3 py-1.5 rounded-xl hover:text-brand-teal hover:bg-brand-peach/30 transition-all">نحوه کار</button>
              <button onClick={() => scrollToSection('privacy')} className="px-3 py-1.5 rounded-xl hover:text-brand-teal hover:bg-brand-peach/30 transition-all">حریم خصوصی</button>
              <button onClick={() => scrollToSection('faq')} className="px-3 py-1.5 rounded-xl hover:text-brand-teal hover:bg-brand-peach/30 transition-all">سوالات متداول</button>
            </nav>

            <div className="flex items-center gap-2 sm:gap-3 pointer-events-auto">
              <button
                onClick={toggleTheme}
                className={`p-2 rounded-2xl border transition-all ${
                  isDark ? 'bg-white/10 text-brand-amber border-white/20 hover:bg-white/20' : 'bg-white/80 text-brand-teal border-brand-peach shadow-xs hover:bg-brand-peach/30'
                }`}
                title={isDark ? 'تغییر به تم روشن' : 'تغییر به تم دارک'}
              >
                {isDark ? <Sun className="w-4 h-4 sm:w-4.5 sm:h-4.5" /> : <Moon className="w-4 h-4 sm:w-4.5 sm:h-4.5" />}
              </button>

              <Link to="/login" className={`text-xs sm:text-sm font-bold transition-all px-3 py-1.5 rounded-xl ${isDark ? 'text-white/90 hover:text-[#BFABDE]' : 'text-[#2D3E40] hover:text-brand-teal hover:bg-brand-peach/30'}`}>
                ورود
              </Link>
              <Link to="/register" className="bg-[#826F9D] hover:bg-[#826F9D]/90 text-white text-xs sm:text-sm font-bold px-3 sm:px-4 py-2 sm:py-2.5 rounded-2xl shadow-lg transition-all border border-white/20">
                ساخت حساب
              </Link>
            </div>
          </header>

          <main className="my-auto py-8 max-w-xl space-y-4 sm:space-y-6 pointer-events-none">
            
            <div className={`inline-flex items-center gap-2 border px-3 sm:px-4 py-1.5 rounded-full text-[11px] sm:text-xs font-bold backdrop-blur-md ${
              isDark ? 'bg-[#221A32]/70 border-[#722549] text-[#BFABDE]' : 'bg-[#F4DACB]/40 border-[#F4DACB] text-[#2D3E40]'
            }`}>
              <GraduationCap className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-brand-teal" />
              دستیار هوشمند زندگی تحصیلی دانشجویان
            </div>

            <h1 className={`text-3xl sm:text-5xl font-black leading-tight sm:leading-tight ${isDark ? 'text-white' : 'text-[#2D3E40]'}`}>
              ترم تحصیلی‌ات را <br />
              <span className="text-brand-teal underline decoration-[#826F9D] decoration-wavy decoration-2">منظم‌تر و هدفمندتر</span> مدیریت کن.
            </h1>

            <p className={`text-xs sm:text-sm leading-relaxed font-medium p-4 sm:p-5 rounded-3xl border backdrop-blur-md transition-all ${
              isDark ? 'bg-[#221A32]/60 text-white/90 border-white/10' : 'bg-[#F4DACB]/25 text-[#2D3E40] border-[#F4DACB]/60 shadow-xs'
            }`}>
              آموینو دروس، تکالیف، امتحان‌ها و اهداف ترمت را در یک سامانه‌ی واحد کنار هم می‌آورد؛ همان چیزی که تا امروز بین چند اپلیکیشن پخش بود. طراحی‌شده برای زندگی واقعی دانشجویی.
            </p>

            <div className="flex flex-col sm:flex-row gap-3 pt-2 pointer-events-auto">
              <Link
                to="/register"
                className="bg-[#826F9D] hover:bg-[#826F9D]/90 text-white px-6 sm:px-8 py-3.5 rounded-2xl font-bold text-xs sm:text-sm shadow-xl flex items-center justify-center gap-2 transition-all hover:scale-105"
              >
                ساخت حساب رایگان
                <ArrowLeft className="w-4 h-4" />
              </Link>

              <button
                onClick={() => scrollToSection('features')}
                className={`border px-6 py-3.5 rounded-2xl font-bold text-xs sm:text-sm backdrop-blur-md text-center transition-all ${
                  isDark ? 'bg-[#221A32]/80 hover:bg-[#221A32] text-white border-[#541532]' : 'bg-[#FAF9F5]/80 hover:bg-[#FAF9F5] text-[#2D3E40] border-[#F4DACB] shadow-xs'
                }`}
              >
                آشنایی با امکانات ↓
              </button>
            </div>
          </main>

          <div />

        </div>
      </section>

      <div className={`relative transition-colors duration-300 ${
        isDark ? 'bg-[#060407]' : 'bg-[#FAF9F5]'
      }`}>

        <StarryBackground isDark={isDark} />

        <section className="relative z-10 py-16 sm:py-20 px-4 sm:px-6 max-w-7xl mx-auto space-y-10 sm:space-y-12">
          <div className="text-center space-y-3 max-w-2xl mx-auto">
            <span className={`text-xs font-bold px-4 py-1.5 rounded-full border ${
              isDark ? 'text-[#BFABDE] bg-[#722549]/20 border-[#722549]' : 'text-brand-dark bg-brand-amber/30 border-brand-amber'
            }`}>چالش</span>
            <h2 className={`text-2xl sm:text-4xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>سه چالش آشنا برای هر دانشجو</h2>
            <p className="text-xs sm:text-sm opacity-80 leading-relaxed">
              تکلیف‌ها فراموش می‌شوند، زمان امتحان‌ها دقیق مشخص نیست و برنامه‌ریزی منسجمی نداری — تا وقتی یادت بیفتد، معمولاً دیر شده است.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className={`p-6 rounded-3xl border space-y-3 shadow-md backdrop-blur-md transition-all ${
              isDark ? 'bg-[#221A32]/70 border-[#541532]' : 'bg-white/70 border-brand-peach'
            }`}>
              <div className="p-3 bg-[#541532] text-[#BFABDE] rounded-2xl w-fit">
                <Layers className="w-5 h-5" />
              </div>
              <h3 className="font-bold text-base">پراکندگی برنامه‌ها و یادداشت‌ها</h3>
              <p className="text-xs opacity-80 leading-relaxed">اطلاعات درسی، تکالیف و برنامه‌ریزی‌هایت بین چندین دفترچه و اپلیکیشن پخش شده و مدیریت آن‌ها سخت است.</p>
            </div>

            <div className={`p-6 rounded-3xl border space-y-3 shadow-md backdrop-blur-md transition-all ${
              isDark ? 'bg-[#221A32]/70 border-[#541532]' : 'bg-white/70 border-brand-peach'
            }`}>
              <div className="p-3 bg-[#541532] text-[#BFABDE] rounded-2xl w-fit">⏰</div>
              <h3 className="font-bold text-base">ددلاین‌هایی که غافلگیرت می‌کنند</h3>
              <p className="text-xs opacity-80 leading-relaxed">تاریخ تحویل پروژه یا امتحان را در ذهن سپرده‌ای اما چون جایی یکپارچه ثبت نشده، ناگهان غافلگیر می‌شوی.</p>
            </div>

            <div className={`p-6 rounded-3xl border space-y-3 shadow-md backdrop-blur-md transition-all ${
              isDark ? 'bg-[#221A32]/70 border-[#541532]' : 'bg-white/70 border-brand-peach'
            }`}>
              <div className="p-3 bg-[#541532] text-[#BFABDE] rounded-2xl w-fit">📊</div>
              <h3 className="font-bold text-base">تصویر روشنی از وضعیت ترم نداری</h3>
              <p className="text-xs opacity-80 leading-relaxed">نمی‌دانی این ترم واقعاً چقدر طبق اهدافت پیش رفته‌ای و وضعیت کلی مطالعه‌ات چگونه است.</p>
            </div>
          </div>
        </section>

        <section className={`relative z-10 py-16 border-y px-4 sm:px-6 backdrop-blur-md ${
          isDark ? 'bg-[#221A32]/40 border-[#541532]' : 'bg-white/60 border-brand-peach'
        }`}>
          <div className="max-w-4xl mx-auto text-center space-y-6">
            <span className="text-xs font-bold text-brand-teal bg-brand-teal/20 px-4 py-1.5 rounded-full border border-brand-teal/40">راه‌حل</span>
            <h2 className={`text-2xl sm:text-4xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>یک سامانه، به‌جای ابزارهای پراکنده</h2>
            <p className="text-xs sm:text-sm leading-relaxed max-w-xl mx-auto font-medium opacity-90">
              آموینو تقویم، تکالیف، امتحان‌ها، اهداف و تحلیل پیشرفت تحصیلی‌ات را در یک داشبورد ساده کنار هم می‌آورد.
            </p>
            <div className={`inline-block border px-5 py-3 rounded-2xl text-xs sm:text-sm font-bold shadow-sm backdrop-blur-md ${
              isDark ? 'bg-[#060407]/80 border-[#541532] text-[#BFABDE]' : 'bg-brand-bg/80 border-brand-peach text-brand-dark'
            }`}>
              تقویم + لیست کارها + تحلیل تحصیلی ← <span className="text-brand-teal">همه در آموینو</span>
            </div>
          </div>
        </section>

        <section id="features" className="relative z-10 py-16 sm:py-20 px-4 sm:px-6 max-w-7xl mx-auto space-y-10 sm:space-y-12">
          <div className="text-center space-y-3 max-w-2xl mx-auto">
            <span className="text-xs font-bold text-brand-teal bg-brand-teal/20 px-4 py-1.5 rounded-full border border-brand-teal/40">امکانات</span>
            <h2 className={`text-2xl sm:text-4xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>ابزارهایی که واقعاً به کارت می‌آیند</h2>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            <FeatureCard isDark={isDark} icon={BookOpen} title="مدیریت درس‌ها" desc="با ثبت دروس، واحدها و برنامه‌ی هفتگی، ساختار کامل ترمت را در یک نگاه داشته باش." />
            <FeatureCard isDark={isDark} icon={CheckSquare} title="تکالیف و پروژه‌ها" desc="با ثبت ددلاین و اولویت هر تکلیف، دیگر هیچ‌کدام از قلم نمی‌افتد." />
            <FeatureCard isDark={isDark} icon={Calendar} title="تقویم تحصیلی یکپارچه" desc="کلاس، امتحان، ددلاین و هدف، همه در یک تقویم — بدون نیاز به چند برنامه‌ی جداگانه." />
            <FeatureCard isDark={isDark} icon={Bell} title="یادآوری هوشمند" desc="پیش از آن‌که دیر شود، از نزدیک‌شدن ددلاین یا عقب‌افتادن از هدف باخبر می‌شوی." />
            <FeatureCard isDark={isDark} icon={Target} title="هدف‌گذاری ترم" desc="معدل، ساعت مطالعه یا زمان پایان پروژه را هدف‌گذاری کن و پیشرفت واقعی‌ات را دنبال کن." />
            <FeatureCard isDark={isDark} icon={BarChart2} title="تحلیل عملکرد" desc="با نمودارهای ساده می‌بینی کجای مسیر ترم هستی و کجا باید بیشتر تلاش کنی." />
          </div>
        </section>

        <section id="how-it-works" className={`relative z-10 py-16 sm:py-20 border-y px-4 sm:px-6 backdrop-blur-md ${
          isDark ? 'bg-[#221A32]/30 border-[#541532]' : 'bg-white/60 border-brand-peach'
        }`}>
          <div className="max-w-7xl mx-auto space-y-10 sm:space-y-12">
            <div className="text-center space-y-3 max-w-2xl mx-auto">
              <span className="text-xs font-bold text-brand-teal bg-brand-teal/20 px-4 py-1.5 rounded-full border border-brand-teal/40">نحوه کار</span>
              <h2 className={`text-2xl sm:text-4xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>شروع در سه قدم ساده</h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 sm:gap-8">
              <StepCard isDark={isDark} step="۱" title="ثبت‌نام کن" desc="کمتر از یک دقیقه، بدون نیاز به اطلاعات اضافه." />
              <StepCard isDark={isDark} step="۲" title="دروس و اهدافت را وارد کن" desc="درس‌ها، واحدها، تکالیف و هدف ترم را ثبت کن." />
              <StepCard isDark={isDark} step="۳" title="پیشرفتت را دنبال کن" desc="با تحلیل هوشمند آموینو، همیشه می‌دانی دقیقاً کجای مسیر ترم هستی." />
            </div>
          </div>
        </section>

        <section className="relative z-10 py-16 px-4 sm:px-6 max-w-4xl mx-auto text-center space-y-8">
          <span className="text-xs font-bold text-brand-rose bg-brand-rose/20 px-4 py-1.5 rounded-full border border-brand-rose/40">در راه است</span>
          <h2 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-brand-dark'}`}>به‌زودی در آموینو</h2>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-xs font-bold">
            <div className={`p-4 border rounded-2xl flex items-center justify-center gap-2 backdrop-blur-md ${
              isDark ? 'bg-[#221A32]/80 border-[#541532]' : 'bg-white/80 border-brand-peach'
            }`}>
              🤖 دستیار هوشمند برنامه‌ریزی مطالعه
            </div>
            <div className={`p-4 border rounded-2xl flex items-center justify-center gap-2 backdrop-blur-md ${
              isDark ? 'bg-[#221A32]/80 border-[#541532]' : 'bg-white/80 border-brand-peach'
            }`}>
              🔔 سیستم یادآوری پیشرفته
            </div>
            <div className={`p-4 border rounded-2xl flex items-center justify-center gap-2 backdrop-blur-md ${
              isDark ? 'bg-[#221A32]/80 border-[#541532]' : 'bg-white/80 border-brand-peach'
            }`}>
              ⚡ پیش‌بینی هوشمند فشار کاری ترم
            </div>
          </div>
        </section>

        <section id="privacy" className={`relative z-10 py-16 sm:py-20 border-t px-4 sm:px-6 backdrop-blur-md ${
          isDark ? 'bg-[#221A32]/40 border-[#541532]' : 'bg-white/60 border-brand-peach'
        }`}>
          <div className="max-w-7xl mx-auto space-y-10 sm:space-y-12">
            <div className="text-center space-y-3 max-w-2xl mx-auto">
              <span className="text-xs font-bold text-brand-teal bg-brand-teal/20 px-4 py-1.5 rounded-full border border-brand-teal/40">اعتماد و حریم خصوصی</span>
              <h2 className={`text-2xl sm:text-4xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>اطلاعاتت نزد ما امن می‌ماند</h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className={`p-6 rounded-3xl border space-y-3 backdrop-blur-md ${
                isDark ? 'bg-[#060407]/70 border-[#541532]' : 'bg-brand-bg/70 border-brand-peach'
              }`}>
                <Lock className="w-6 h-6 text-brand-teal" />
                <h3 className="font-bold text-sm">فقط مخصوص خودت</h3>
                <p className="text-xs opacity-80 leading-relaxed">اطلاعات تحصیلی‌ات فقط در اختیار خودت است؛ نه دانشگاه، نه استاد و نه هیچ شخص ثالثی به آن دسترسی ندارد.</p>
              </div>

              <div className={`p-6 rounded-3xl border space-y-3 backdrop-blur-md ${
                isDark ? 'bg-[#060407]/70 border-[#541532]' : 'bg-brand-bg/70 border-brand-peach'
              }`}>
                <GraduationCap className="w-6 h-6 text-brand-teal" />
                <h3 className="font-bold text-sm">تمرکز کاملاً تحصیلی</h3>
                <p className="text-xs opacity-80 leading-relaxed">آموینو یک شبکه‌ی اجتماعی نیست؛ ابزاری شخصی برای مدیریت بهتر زندگی تحصیلی توست.</p>
              </div>

              <div className={`p-6 rounded-3xl border space-y-3 backdrop-blur-md ${
                isDark ? 'bg-[#060407]/70 border-[#541532]' : 'bg-brand-bg/70 border-brand-peach'
              }`}>
                <Sparkles className="w-6 h-6 text-brand-teal" />
                <h3 className="font-bold text-sm">صادقانه در حال رشد</h3>
                <p className="text-xs opacity-80 leading-relaxed">این نسخه از آموینو بر اساس بازخورد دانشجوها، پیوسته در حال بهینه‌تر شدن است.</p>
              </div>
            </div>
          </div>
        </section>

        <section id="faq" className="relative z-10 py-16 sm:py-20 px-4 sm:px-6 max-w-4xl mx-auto space-y-8">
          <div className="text-center space-y-3">
            <span className="text-xs font-bold text-brand-teal bg-brand-teal/20 px-4 py-1.5 rounded-full border border-brand-teal/40">سوالات متداول</span>
            <h2 className={`text-2xl sm:text-3xl font-extrabold ${isDark ? 'text-white' : 'text-brand-dark'}`}>پاسخ به پرسش‌های شما</h2>
          </div>

          <div className="space-y-3">
            {faqs.map((faq, idx) => (
              <div
                key={idx}
                className={`border rounded-2xl overflow-hidden transition-all backdrop-blur-md ${
                  isDark ? 'bg-[#221A32]/80 border-[#541532]' : 'bg-white/80 border-brand-peach'
                }`}
              >
                <button
                  onClick={() => setOpenFaq(openFaq === idx ? null : idx)}
                  className="w-full p-4 text-right flex items-center justify-between text-xs sm:text-sm font-bold hover:opacity-80 transition-all"
                >
                  <span>{faq.q}</span>
                  <ChevronDown className={`w-4 h-4 text-brand-teal transition-transform ${openFaq === idx ? 'rotate-180' : ''}`} />
                </button>
                {openFaq === idx && (
                  <div className={`p-4 pt-0 text-xs border-t leading-relaxed opacity-80 ${
                    isDark ? 'border-[#541532]/40 bg-[#060407]/40' : 'border-brand-peach/40 bg-brand-bg/50'
                  }`}>
                    {faq.a}
                  </div>
                )}
              </div>
            ))}
          </div>
        </section>

        <section className={`relative z-10 py-16 sm:py-20 border-t text-center px-4 sm:px-6 space-y-6 ${
          isDark ? 'bg-gradient-to-b from-[#221A32]/60 to-[#060407] border-[#541532]' : 'bg-gradient-to-b from-white/80 to-[#FAF9F5] border-brand-peach'
        }`}>
          <h2 className={`text-2xl sm:text-4xl font-black ${isDark ? 'text-white' : 'text-brand-dark'}`}>آماده‌ای این ترم را منظم‌تر شروع کنی؟</h2>
          <p className="text-xs sm:text-sm opacity-80">ساخت حساب رایگان کمتر از یک دقیقه زمان می‌برد.</p>

          <div className="flex justify-center">
            <Link
              to="/register"
              className="bg-[#826F9D] hover:bg-[#826F9D]/90 text-white px-8 sm:px-10 py-3.5 rounded-2xl font-bold text-xs sm:text-sm shadow-xl transition-all hover:scale-105"
            >
              ساخت حساب رایگان
            </Link>
          </div>
        </section>

        <footer className={`relative z-10 border-t py-8 px-6 text-xs text-center space-y-3 ${
          isDark ? 'bg-[#060407]/90 border-[#541532] text-white/60' : 'bg-[#FAF9F5]/90 border-brand-peach/60 text-brand-dark/60'
        }`}>
          <p className="font-bold opacity-90">آموینو — طراحی‌شده برای دانشجویان دانشگاه‌های ایران.</p>
          <div className="flex justify-center text-[11px]">
            <a href="#privacy" className="hover:opacity-100 transition-all">حریم خصوصی</a>
          </div>
          <p className="text-[10px] opacity-60">© ۱۴۰۵ آموینو</p>
        </footer>

      </div>
    </div>
  );
}

const FeatureCard = ({ icon: Icon, title, desc, isDark }) => (
  <div className={`p-6 rounded-3xl border space-y-3 shadow-md hover:border-brand-teal transition-all backdrop-blur-md ${
    isDark ? 'bg-[#221A32]/70 border-[#541532]' : 'bg-white/70 border-brand-peach'
  }`}>
    <div className={`p-3 rounded-2xl w-fit ${isDark ? 'bg-[#541532] text-[#826F9D]' : 'bg-brand-peach/40 text-brand-teal'}`}>
      <Icon className="w-5 h-5" />
    </div>
    <h3 className="font-bold text-base">{title}</h3>
    <p className="text-xs opacity-80 leading-relaxed">{desc}</p>
  </div>
);

const StepCard = ({ step, title, desc, isDark }) => (
  <div className={`p-6 rounded-3xl border space-y-3 relative overflow-hidden backdrop-blur-md ${
    isDark ? 'bg-[#2B2141]/70 border-[#541532]' : 'bg-white/70 border-brand-peach'
  }`}>
    <span className="text-4xl font-black text-brand-teal/20 absolute left-4 top-2">{step}</span>
    <h3 className="font-bold text-base relative z-10">{title}</h3>
    <p className="text-xs opacity-80 leading-relaxed relative z-10">{desc}</p>
  </div>
);