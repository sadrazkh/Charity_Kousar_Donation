<script setup>
import { onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const { locale } = useI18n()

async function loadTheme() {
  try {
    const cfg = await api('/settings/public')
    document.documentElement.style.setProperty('--primary', cfg.primaryColor || '#0d9488')
    document.documentElement.style.setProperty('--accent', cfg.accentColor || '#f59e0b')
    document.documentElement.style.setProperty('--bg', cfg.backgroundColor || '#0f172a')
    document.body.style.background = cfg.backgroundColor || '#0f172a'
  } catch { /* defaults */ }
}

onMounted(loadTheme)
watch(locale, loadTheme)
</script>

<template>
  <router-view />
</template>
