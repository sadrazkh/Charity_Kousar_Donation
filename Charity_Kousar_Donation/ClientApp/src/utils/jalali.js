import jalaali from 'jalaali-js'

export const JALALI_MONTHS = [
  'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
  'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
]

const PERSIAN_DIGITS = '۰۱۲۳۴۵۶۷۸۹'
const ARABIC_DIGITS = '٠١٢٣٤٥٦٧٨٩'

export function toEnglishDigits(value) {
  if (value == null) return ''
  let s = String(value)
  for (let i = 0; i < 10; i++) {
    s = s.replaceAll(PERSIAN_DIGITS[i], String(i)).replaceAll(ARABIC_DIGITS[i], String(i))
  }
  return s
}

export function toPersianDigits(value) {
  return String(value).replace(/\d/g, (d) => PERSIAN_DIGITS[d])
}

export function daysInJalaliMonth(jy, jm) {
  if (jm <= 6) return 31
  if (jm <= 11) return 30
  return jalaali.isLeapJalaaliYear(jy) ? 30 : 29
}

/** Gregorian date in Tehran → Jalali parts */
export function dateToJalaliParts(date) {
  const d = date instanceof Date ? date : new Date(date)
  if (Number.isNaN(d.getTime())) return null

  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Asia/Tehran',
    year: 'numeric', month: 'numeric', day: 'numeric',
    hour: 'numeric', minute: 'numeric', hour12: false
  }).formatToParts(d)

  const get = (type) => Number(parts.find((p) => p.type === type)?.value || 0)
  const gy = get('year')
  const gm = get('month')
  const gd = get('day')
  const { jy, jm, jd } = jalaali.toJalaali(gy, gm, gd)
  return { jy, jm, jd, hh: get('hour'), mm: get('minute') }
}

export function isoToJalaliParts(iso) {
  if (!iso) return null
  return dateToJalaliParts(new Date(iso))
}

/** Jalali datetime (Tehran) → UTC ISO */
export function jalaliPartsToIso(jy, jm, jd, hh = 0, mm = 0) {
  const { gy, gm, gd } = jalaali.toGregorian(jy, jm, jd)
  const pad = (n) => String(n).padStart(2, '0')
  const iso = new Date(`${gy}-${pad(gm)}-${pad(gd)}T${pad(hh)}:${pad(mm)}:00+03:30`).toISOString()
  return Number.isNaN(new Date(iso).getTime()) ? null : iso
}

/** Parse picker/API string → ISO (handles Jalali & accidental Gregorian years) */
export function parseDateTimeToIso(val) {
  if (!val || !String(val).trim()) return null
  const s = toEnglishDigits(String(val).trim()).replace(/\//g, '-')

  const m = s.match(/^(\d{4})-(\d{1,2})-(\d{1,2})(?:[T\s](\d{1,2}):(\d{1,2}))?/)
  if (!m) return null

  const y = +m[1]
  const mo = +m[2]
  const d = +m[3]
  const hh = +(m[4] ?? 0)
  const mm = +(m[5] ?? 0)

  if (y >= 1900 && y <= 2100) {
    const pad = (n) => String(n).padStart(2, '0')
    const iso = new Date(`${y}-${pad(mo)}-${pad(d)}T${pad(hh)}:${pad(mm)}:00+03:30`).toISOString()
    return Number.isNaN(new Date(iso).getTime()) ? null : iso
  }

  if (y >= 1300 && y <= 1500) {
    return jalaliPartsToIso(y, mo, d, hh, mm)
  }

  return null
}

export function formatJalaliDisplay(iso, usePersianDigits = true) {
  const p = isoToJalaliParts(iso)
  if (!p) return ''
  const text = `${p.jy}/${String(p.jm).padStart(2, '0')}/${String(p.jd).padStart(2, '0')} — ${String(p.hh).padStart(2, '0')}:${String(p.mm).padStart(2, '0')}`
  return usePersianDigits ? toPersianDigits(text) : text
}

export function currentJalaliParts() {
  return dateToJalaliParts(new Date())
}

/** First weekday of Jalali month (0=Sat … 6=Fri) */
export function jalaliMonthStartWeekday(jy, jm) {
  const { gy, gm, gd } = jalaali.toGregorian(jy, jm, 1)
  const dow = new Date(gy, gm - 1, gd).getDay()
  return (dow + 1) % 7
}

export const WEEKDAYS_FA = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج']
