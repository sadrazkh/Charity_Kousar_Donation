import { ref, computed } from 'vue'

const theme = ref(localStorage.getItem('theme') || 'dark')
let lastSiteConfig = {}

export function useTheme() {
  const isDark = computed(() => theme.value === 'dark')

  function applyThemeAttr() {
    document.documentElement.setAttribute('data-theme', theme.value)
  }

  function setTheme(mode) {
    theme.value = mode === 'light' ? 'light' : 'dark'
    localStorage.setItem('theme', theme.value)
    applyThemeAttr()
    applySiteColors(lastSiteConfig)
  }

  function toggleTheme() {
    setTheme(theme.value === 'dark' ? 'light' : 'dark')
  }

  function applySiteColors(cfg = {}) {
    if (Object.keys(cfg).length) lastSiteConfig = cfg
    const c = lastSiteConfig
    document.documentElement.style.setProperty('--primary', c.primaryColor || '#0d9488')
    document.documentElement.style.setProperty('--accent', c.accentColor || '#f59e0b')
    if (theme.value === 'dark') {
      const bg = c.backgroundColor || '#0f172a'
      document.documentElement.style.setProperty('--bg', bg)
      document.body.style.background = bg
    } else {
      document.documentElement.style.setProperty('--bg', '#f1f5f9')
      document.body.style.background = '#f1f5f9'
    }
  }

  function initTheme() {
    applyThemeAttr()
  }

  return { theme, isDark, setTheme, toggleTheme, applySiteColors, initTheme }
}
