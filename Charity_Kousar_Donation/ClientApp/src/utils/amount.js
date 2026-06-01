const PERSIAN = '۰۱۲۳۴۵۶۷۸۹'
const ARABIC = '٠١٢٣٤٥٦٧٨٩'

export function toEnglishDigits(str) {
  if (!str) return ''
  let s = String(str)
  for (let i = 0; i < 10; i++) {
    s = s.replaceAll(PERSIAN[i], String(i)).replaceAll(ARABIC[i], String(i))
  }
  return s
}

/** Format number with thousand separators for display */
export function formatAmount(n, locale = 'fa') {
  const num = Number(n)
  if (!Number.isFinite(num)) return '0'
  return num.toLocaleString(locale === 'fa' ? 'fa-IR' : 'en-US', {
    maximumFractionDigits: 0
  })
}

/** Parse formatted or raw input to number */
export function parseAmount(value) {
  if (typeof value === 'number') return Number.isFinite(value) ? value : 0
  const digits = toEnglishDigits(String(value ?? '')).replace(/[^\d]/g, '')
  return digits ? parseInt(digits, 10) : 0
}

/** Format while user types — keeps only digits, returns display + numeric */
export function formatAmountTyping(raw, locale = 'fa') {
  const numeric = parseAmount(raw)
  return {
    numeric,
    display: numeric ? formatAmount(numeric, locale) : ''
  }
}

export function whatsAppShareUrl(text) {
  return `https://api.whatsapp.com/send?text=${encodeURIComponent(text)}`
}

export function telegramShareUrl(pageUrl, text) {
  return `https://t.me/share/url?url=${encodeURIComponent(pageUrl)}&text=${encodeURIComponent(text)}`
}
