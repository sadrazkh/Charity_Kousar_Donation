<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { setLocale } from '@/i18n'
import { useTheme } from '@/composables/useTheme'
import { useSiteConfig } from '@/composables/useSiteConfig'

const { t, locale } = useI18n()
const { isDark, toggleTheme } = useTheme()
const { config } = useSiteConfig()

const logoSize = computed(() => (Number(config.logoHeight) || 48) + 'px')
const showText = computed(() => config.showLogoText !== false)

function siteName() {
  return locale.value === 'fa' ? config.siteNameFa : config.siteNameEn
}
function tagline() {
  return locale.value === 'fa' ? config.taglineFa : config.taglineEn
}
function toggleLang() {
  setLocale(locale.value === 'fa' ? 'en' : 'fa')
}
</script>

<template>
  <header class="header">
    <div class="container header-inner">
      <router-link to="/" class="brand">
        <img v-if="config.logoUrl" :src="config.logoUrl" alt="" class="logo"
          :style="{ width: logoSize, height: logoSize }" />
        <span v-else class="logo-icon" :style="{ width: logoSize, height: logoSize }" aria-hidden="true">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
        </span>
        <div v-if="showText">
          <strong>{{ siteName() || t('site') }}</strong>
          <small>{{ tagline() || t('tagline') }}</small>
        </div>
      </router-link>
      <nav>
        <router-link to="/" class="btn btn-ghost btn-sm home-link">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 10.5 12 3l9 7.5"/><path d="M5 9.5V21h14V9.5"/></svg>
          <span>{{ t('home') }}</span>
        </router-link>
        <button class="icon-btn" @click="toggleLang" :aria-label="locale === 'fa' ? 'Switch to English' : 'تغییر به فارسی'">
          <span class="lang-txt">{{ locale === 'fa' ? 'EN' : 'FA' }}</span>
        </button>
        <button class="icon-btn" @click="toggleTheme" :aria-label="isDark ? t('themeLight') : t('themeDark')" :title="isDark ? t('themeLight') : t('themeDark')">
          <svg v-if="isDark" class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="4.5"/><path d="M12 2v2M12 20v2M4 12H2M22 12h-2M5 5l1.5 1.5M17.5 17.5 19 19M19 5l-1.5 1.5M6.5 17.5 5 19"/></svg>
          <svg v-else class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.8A8.5 8.5 0 1 1 11.2 3a6.6 6.6 0 0 0 9.8 9.8z"/></svg>
        </button>
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
  border-radius: 14px;
  color: #fff;
  box-shadow: 0 8px 20px -6px color-mix(in srgb, var(--primary) 55%, transparent);
}
.logo-icon .icon { width: 55%; height: 55%; }
.lang-txt { font-size: 0.85rem; font-weight: 700; letter-spacing: 0.3px; }
nav { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
nav a { color: var(--muted); font-size: 0.9rem; }
.home-link { color: var(--text); text-decoration: none; font-weight: 600; }
.home-link:hover { text-decoration: none; color: var(--primary); }
@media (max-width: 480px) {
  .header { margin-bottom: 1.25rem; padding: 0.75rem 0; }
  .brand strong { font-size: 1rem; }
  .logo, .logo-icon { width: 40px; height: 40px; }
  nav .btn-sm { padding: 0.4rem 0.65rem; font-size: 0.8rem; }
}
</style>
