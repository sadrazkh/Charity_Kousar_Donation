<script setup>
import { onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useTheme } from '@/composables/useTheme'
import { loadSiteConfig } from '@/composables/useSiteConfig'
import ToastHost from '@/components/ToastHost.vue'

const { locale } = useI18n()
const { initTheme, applySiteColors } = useTheme()

async function loadTheme() {
  const cfg = await loadSiteConfig(true)
  applySiteColors(cfg)
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
