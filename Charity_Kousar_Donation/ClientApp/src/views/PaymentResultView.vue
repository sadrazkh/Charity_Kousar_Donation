<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'

defineProps({ success: Boolean })
const route = useRoute()
const { t } = useI18n()
const message = computed(() => route.query.message || '')
</script>

<template>
  <AppHeader />
  <main class="container result">
    <div class="card" style="text-align:center; max-width:480px; margin:2rem auto;">
      <div class="icon" :class="success ? 'ok' : 'fail'">{{ success ? '✓' : '✕' }}</div>
      <h1>{{ success ? t('paymentSuccess') : t('paymentFailed') }}</h1>
      <p v-if="message" class="msg">{{ message }}</p>
      <router-link to="/" class="btn btn-primary" style="margin-top:1.5rem">{{ t('backHome') }}</router-link>
    </div>
  </main>
</template>

<style scoped>
.icon { width: 72px; height: 72px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 2rem; margin: 0 auto 1rem; }
.ok { background: #065f4633; color: #34d399; }
.fail { background: #7f1d1d33; color: #f87171; }
.msg { color: var(--muted); margin-top: 0.75rem; }
</style>
