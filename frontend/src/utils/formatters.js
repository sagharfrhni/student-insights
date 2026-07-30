
export const toJalaliDate = (isoString) => {
  if (!isoString) return '-';
  const date = new Date(isoString);
  return new Intl.DateTimeFormat('fa-IR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date);
};

export const toJalaliDateTime = (isoString) => {
  if (!isoString) return '-';
  const date = new Date(isoString);
  return new Intl.DateTimeFormat('fa-IR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false, 
  }).format(date);
};

export const toPersianDigits = (num) => {
  if (num === null || num === undefined) return '';
  const persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
  return num.toString().replace(/\d/g, (x) => persianDigits[x]);
};

export const formatMinutesToHours = (totalMinutes) => {
  if (!totalMinutes || totalMinutes <= 0) return '۰ دقیقه';
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  if (hours === 0) return `${toPersianDigits(minutes)} دقیقه`;
  if (minutes === 0) return `${toPersianDigits(hours)} ساعت`;
  return `${toPersianDigits(hours)} ساعت و ${toPersianDigits(minutes)} دقیقه`;
};


export function toJalaliObject(gY, gM, gD) {
  const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
  let jy = (gY <= 1600) ? 0 : 979;
  gY -= (gY <= 1600) ? 621 : 1600;
  let gy2 = (gM > 2) ? (gY + 1) : gY;
  let days = (365 * gY) + (parseInt((gy2 + 3) / 4)) - (parseInt((gy2 + 99) / 100)) + (parseInt((gy2 + 399) / 400)) - 80 + gD + g_d_m[gM - 1];
  jy += 33 * (parseInt(days / 12053));
  days %= 12053;
  jy += 4 * (parseInt(days / 1461));
  days %= 1461;
  if (days > 365) {
    jy += parseInt((days - 1) / 365);
    days = (days - 1) % 365;
  }
  let jm = (days < 186) ? 1 + parseInt(days / 31) : 7 + parseInt((days - 186) / 30);
  let jd = 1 + ((days < 186) ? (days % 31) : ((days - 186) % 30));
  return { jy, jm, jd };
}

export function toGregorianObject(jY, jM, jD) {
  let gy = (jY <= 979) ? 621 : 1600;
  jY -= (jY <= 979) ? 0 : 979;
  let days = (365 * jY) + (parseInt(jY / 33) * 8) + (parseInt(((jY % 33) + 3) / 4)) + 78 + jD + ((jM < 7) ? (jM - 1) * 31 : ((jM - 7) * 30) + 186);
  gy += 400 * (parseInt(days / 146097));
  days %= 146097;
  if (days > 36524) {
    gy += 100 * (parseInt(--days / 36524));
    days %= 36524;
    if (days >= 365) days++;
  }
  gy += 4 * (parseInt(days / 1461));
  days %= 1461;
  if (days > 365) {
    gy += parseInt((days - 1) / 365);
    days = (days - 1) % 365;
  }
  let gm = 0;
  const g_d_m = [0, 31, (gy % 4 === 0 && gy % 100 !== 0) || (gy % 400 === 0) ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
  for (let i = 1; i <= 12; i++) {
    if (days < g_d_m[i]) {
      gm = i;
      break;
    }
    days -= g_d_m[i];
  }
  let gd = days + 1;
  return { gy, gm, gd };
}