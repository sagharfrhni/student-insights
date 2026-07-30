import React, { useState, useEffect } from 'react';
import { toJalaliObject, toGregorianObject, toPersianDigits } from '../utils/formatters';
import CustomSelect from './CustomSelect';

const jalaliMonths = [
  'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
  'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
];

export default function JalaliDateTimePicker({ value, onChange, includeTime = true }) {
  const [jYear, setJYear] = useState(1405);
  const [jMonth, setJMonth] = useState(1);
  const [jDay, setJDay] = useState(1);
  const [hour, setHour] = useState(10);
  const [minute, setMinute] = useState(0);

  useEffect(() => {
    const d = value ? new Date(value) : new Date();
    const validDate = isNaN(d.getTime()) ? new Date() : d;
    
    
    const gY = validDate.getUTCFullYear();
    const gM = validDate.getUTCMonth() + 1;
    const gD = validDate.getUTCDate();

    const jalali = toJalaliObject(gY, gM, gD);
    setJYear(jalali.jy);
    setJMonth(jalali.jm);
    setJDay(jalali.jd);
    setHour(validDate.getUTCHours());
    setMinute(validDate.getUTCMinutes());
  }, [value]);

  const updateIsoDate = (y, m, d, h, min) => {
    const greg = toGregorianObject(y, m, d);
    
    const date = new Date(Date.UTC(greg.gy, greg.gm - 1, greg.gd, includeTime ? h : 12, includeTime ? min : 0, 0));
    onChange(date.toISOString());
  };

  const handleYearChange = (y) => { const ny = parseInt(y); setJYear(ny); updateIsoDate(ny, jMonth, jDay, hour, minute); };
  const handleMonthChange = (m) => { const nm = parseInt(m); setJMonth(nm); updateIsoDate(jYear, nm, jDay, hour, minute); };
  const handleDayChange = (d) => { const nd = parseInt(d); setJDay(nd); updateIsoDate(jYear, jMonth, nd, hour, minute); };
  const handleHourChange = (h) => { const nh = parseInt(h); setHour(nh); updateIsoDate(jYear, jMonth, jDay, nh, minute); };
  const handleMinuteChange = (min) => { const nm = parseInt(min); setMinute(nm); updateIsoDate(jYear, jMonth, jDay, hour, nm); };

  const yearOptions = [1403, 1404, 1405, 1406, 1407].map((y) => ({ value: y, label: toPersianDigits(y) }));
  const monthOptions = jalaliMonths.map((m, idx) => ({ value: idx + 1, label: m }));
  const dayOptions = Array.from({ length: 31 }, (_, i) => i + 1).map((d) => ({ value: d, label: toPersianDigits(d) }));

  const hourOptions = Array.from({ length: 24 }, (_, i) => i).map((h) => ({
    value: h,
    label: toPersianDigits(h < 10 ? `0${h}` : h),
  }));

  const minuteOptions = Array.from({ length: 60 }, (_, i) => i).map((m) => ({
    value: m,
    label: toPersianDigits(m < 10 ? `0${m}` : m),
  }));

  return (
    <div className="bg-white dark:bg-[#221A32] p-3.5 rounded-2xl border border-brand-peach dark:border-[#521431] space-y-3">
      
      <div className="grid grid-cols-3 gap-2 text-xs">
        <div>
          <CustomSelect options={yearOptions} value={jYear} onChange={handleYearChange} />
        </div>
        <div>
          <CustomSelect options={monthOptions} value={jMonth} onChange={handleMonthChange} />
        </div>
        <div>
          <CustomSelect options={dayOptions} value={jDay} onChange={handleDayChange} />
        </div>
      </div>

      
      {includeTime && (
        <div className="pt-2.5 border-t border-brand-peach/40 dark:border-[#521431] flex items-center justify-between text-xs">
          <span className="text-[10px] text-brand-dark/60 dark:text-[#F4F0FA]/70 font-bold shrink-0">زمان:</span>

          <div className="flex items-center justify-center gap-2 w-48 dir-ltr mx-auto">
            
            <div className="w-20">
              <CustomSelect options={minuteOptions} value={minute} onChange={handleMinuteChange} />
            </div>

            <span className="font-bold text-brand-dark dark:text-[#F4F0FA] text-base shrink-0">:</span>

            
            <div className="w-20">
              <CustomSelect options={hourOptions} value={hour} onChange={handleHourChange} />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}