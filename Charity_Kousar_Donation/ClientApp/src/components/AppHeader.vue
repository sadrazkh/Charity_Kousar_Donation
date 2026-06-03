<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { setLocale } from '@/i18n'
import { api } from '@/api/client'
import { useTheme } from '@/composables/useTheme'

const { t, locale } = useI18n()
const { isDark, toggleTheme } = useTheme()
const config = ref({ siteNameFa: '', siteNameEn: '', taglineFa: '', taglineEn: '', logoUrl: null })

onMounted(async () => {
  try { config.value = await api('/settings/public') } catch { /* */ }
})

function siteName() {
  return locale.value === 'fa' ? config.value.siteNameFa : config.value.siteNameEn
}
function tagline() {
  return locale.value === 'fa' ? config.value.taglineFa : config.value.taglineEn
}
function toggleLang() {
  setLocale(locale.value === 'fa' ? 'en' : 'fa')
}
</script>

<template>
  <header class="header">
    <div class="container header-inner">
      <router-link to="/" class="brand">
        <img v-if="config.logoUrl" :src="config.logoUrl" alt="" class="logo" />
        <span v-else class="logo-icon">♥</span>
        <div>
          <strong>{{ siteName() || t('site') }}</strong>
          <small>{{ tagline() || t('tagline') }}</small>
        </div>
      </router-link>
      <nav>
        <router-link to="/">{{ t('home') }}</router-link>
        <button class="btn btn-ghost btn-sm" @click="toggleLang">{{ locale === 'fa' ? 'EN' : 'FA' }}</button>
        <button class="btn btn-ghost btn-sm" @click="toggleTheme" :title="isDark ? t('themeLight') : t('themeDark')">
          {{ isDark ? '☀️' : '🌙' }}
        </button>
        <router-link to="/admin" class="btn btn-ghost btn-sm">{{ t('admin') }}</router-link>
      </nav>
    </div>
  </header>
</template>

<style scoped>
.header {
  padding: 1rem 0;
  border-bottom: 1px solid rgba(148,163,184,0.1);
  margin-bottom: 2rem;
}
.header-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: inherit;
  text-decoration: none;
}
.brand strong { display: block; font-size: 1.15rem; }
.brand small { color: var(--muted); font-size: 0.8rem; }
.logo { width: 48px; height: 48px; border-radius: 12px; object-fit: cover; }
.logo-icon {
  width: 48px; height: 48px;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, var(--primary), var(--accent));
  border-radius: 12px;
  font-size: 1.5rem;
}
nav { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
nav a { color: var(--muted); font-size: 0.9rem; }
@media (max-width: 480px) {
  .header { margin-bottom: 1.25rem; padding: 0.75rem 0; }
  .brand strong { font-size: 1rem; }
  .logo, .logo-icon { width: 40px; height: 40px; }
  nav .btn-sm { padding: 0.4rem 0.65rem; font-size: 0.8rem; }
}
</style>
