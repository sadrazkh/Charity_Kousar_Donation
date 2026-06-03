<script setup>
import { onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useTheme } from '@/composables/useTheme'
import ToastHost from '@/components/ToastHost.vue'

const { locale } = useI18n()
const { initTheme, applySiteColors } = useTheme()

async function loadTheme() {
  try {
    const cfg = await api('/settings/public')
    applySiteColors(cfg)
  } catch {
    applySiteColors({})
  }
}

onMounted(() => {
  initTheme()
  loadTheme()
})
watch(locale, loadTheme)
</script>

<template>
  <router-view />
  <ToastHost />
</template>
