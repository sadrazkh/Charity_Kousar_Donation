import { createI18n } from 'vue-i18n'
import fa from './locales/fa.json'
import en from './locales/en.json'

const saved = localStorage.getItem('locale') || 'fa'

const i18n = createI18n({
  legacy: false,
  locale: saved,
  fallbackLocale: 'fa',
  messages: { fa, en }
})

export function setLocale(locale) {
  i18n.global.locale.value = locale
  localStorage.setItem('locale', locale)
  document.documentElement.lang = locale
  document.documentElement.dir = locale === 'fa' ? 'rtl' : 'ltr'
}

setLocale(saved)
export default i18n
